using System;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class UnityWebRequestFileDownloader : FileDownloaderBase
    {
        public UnityWebRequestFileDownloader() : base()
        {
        }

        public UnityWebRequestFileDownloader(Uri baseUri, int maxTaskCount) : base(baseUri, maxTaskCount)
        {
        }

        public override IProgressResult<ProgressInfo, FileInfo> DownloadFileAsync(Uri path, FileInfo fileInfo)
        {
            return Executors.RunOnCoroutine<ProgressInfo, FileInfo>((promise) => DoDownloadFileAsync(path, fileInfo, promise));
        }

        protected virtual IEnumerator DoDownloadFileAsync(Uri path, FileInfo fileInfo, IProgressPromise<ProgressInfo> promise)
        {
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();

            ProgressInfo progressInfo = new ProgressInfo();
            progressInfo.TotalCount = 1;
            using (UnityWebRequest www = new UnityWebRequest(GetAbsoluteUri(path).AbsoluteUri))
            {
                DownloadFileHandler downloadHandler = new DownloadFileHandler(www, fileInfo);
                www.downloadHandler = downloadHandler;
#if UNITY_2018_1_OR_NEWER
                www.SendWebRequest();
#else
                www.Send();
#endif
                while (!www.isDone)
                {
                    if (downloadHandler.DownloadProgress > 0)
                    {
                        if (progressInfo.TotalSize <= 0)
                            progressInfo.TotalSize = downloadHandler.TotalSize;
                        progressInfo.CompletedSize = downloadHandler.DownloadedSize;
                        promise.UpdateProgress(progressInfo);
                    }
                    yield return null;
                }

#if UNITY_2018_1_OR_NEWER
                if (www.isNetworkError)
#else
                if (www.isError)
#endif
                {
                    promise.SetException(www.error);
                    yield break;
                }

                progressInfo.CompletedCount = 1;
                progressInfo.CompletedSize = progressInfo.TotalSize;
                promise.UpdateProgress(progressInfo);
                promise.SetResult(fileInfo);
            }
        }

        public override IProgressResult<ProgressInfo, ResourceInfo[]> DownloadFileAsync(ResourceInfo[] infos)
        {
            return Executors.RunOnCoroutine<ProgressInfo, ResourceInfo[]>((promise) => DoDownloadFileAsync(infos, promise));
        }

        protected virtual IEnumerator DoDownloadFileAsync(ResourceInfo[] infos, IProgressPromise<ProgressInfo> promise)
        {
            long totalSize = 0;
            long downloadedSize = 0;
            List<ResourceInfo> downloadList = new List<ResourceInfo>();
            for (int i = 0; i < infos.Length; i++)
            {
                var info = infos[i];
                var fileInfo = info.FileInfo;

                if (info.FileSize < 0)
                {
                    if (fileInfo.Exists)
                    {
                        info.FileSize = fileInfo.Length;
                    }
                    else
                    {
                        using (UnityWebRequest www = UnityWebRequest.Head(GetAbsoluteUri(info.Path).AbsoluteUri))
                        {
#if UNITY_2018_1_OR_NEWER
                            yield return www.SendWebRequest();
#else
                            yield return www.Send();
#endif
                            string contentLength = www.GetResponseHeader("Content-Length");
                            info.FileSize = long.Parse(contentLength);
                        }
                    }
                }

                totalSize += info.FileSize;
                if (fileInfo.Exists)
                    downloadedSize += info.FileSize;
                else
                    downloadList.Add(info);
            }

            ProgressInfo progressInfo = new ProgressInfo();
            progressInfo.TotalCount = infos.Length;
            progressInfo.CompletedCount = infos.Length - downloadList.Count;
            progressInfo.TotalSize = totalSize;
            progressInfo.CompletedSize = downloadedSize;

            yield return null;

            List<KeyValuePair<ResourceInfo, UnityWebRequest>> tasks = new List<KeyValuePair<ResourceInfo, UnityWebRequest>>();
            for (int i = 0; i < downloadList.Count; i++)
            {
                ResourceInfo info = downloadList[i];
                Uri path = info.Path;
                FileInfo fileInfo = info.FileInfo;
                if (!fileInfo.Directory.Exists)
                    fileInfo.Directory.Create();

                UnityWebRequest www = new UnityWebRequest(GetAbsoluteUri(path).AbsoluteUri);
                www.downloadHandler = new DownloadFileHandler(www, fileInfo);

#if UNITY_2018_1_OR_NEWER
                www.SendWebRequest();
#else
                www.Send();
#endif
                tasks.Add(new KeyValuePair<ResourceInfo, UnityWebRequest>(info, www));

                while (tasks.Count >= MaxTaskCount || (i == downloadList.Count - 1 && tasks.Count > 0))
                {
                    long tmpSize = 0;
                    for (int j = tasks.Count - 1; j >= 0; j--)
                    {
                        var task = tasks[j];
                        ResourceInfo _info = task.Key;
                        UnityWebRequest _www = task.Value;

                        if (!_www.isDone)
                        {
                            //tmpSize += (long)Math.Max(0, _www.downloadedBytes);//the UnityWebRequest.downloadedProgress has a bug in android platform
                            tmpSize += (long)Math.Max(0, ((DownloadFileHandler)_www.downloadHandler).DownloadedSize);
                            continue;
                        }

                        progressInfo.CompletedCount += 1;
                        tasks.RemoveAt(j);
                        downloadedSize += _info.FileSize;
#if UNITY_2018_1_OR_NEWER
                        if (_www.isNetworkError)
#else
                        if (_www.isError)
#endif
                        {
                            promise.SetException(new Exception(_www.error));
                            Debug.LogWarning(string.Format("Downloads file '{0}' failure from the address '{1}'.Reason:{2}", _info.FileInfo.FullName, GetAbsoluteUri(_info.Path), _www.error));
                            _www.Dispose();

                            try
                            {
                                foreach (var kv in tasks)
                                {
                                    kv.Value.Dispose();
                                }
                            }
                            catch (Exception) { }
                            yield break;
                        }
                        _www.Dispose();
                    }

                    progressInfo.CompletedSize = downloadedSize + tmpSize;
                    promise.UpdateProgress(progressInfo);
                    yield return null;
                }
            }
            promise.SetResult(infos);
        }
    }
}
