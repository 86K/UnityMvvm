using System;
using System.IO;
using UnityEngine;

namespace Fusion.Mvvm
{
    public abstract class FileDownloaderBase : IFileDownloader
    {
        private Uri baseUri;
        private int maxTaskCount;

        public FileDownloaderBase() : this(null, SystemInfo.processorCount * 2)
        {
        }

        public FileDownloaderBase(Uri baseUri, int maxTaskCount)
        {
            BaseUri = baseUri;
            MaxTaskCount = maxTaskCount;
        }

        public virtual Uri BaseUri
        {
            get => baseUri;
            set
            {
                if (value != null && !IsAllowedAbsoluteUri(value))
                    throw new NotSupportedException($"Invalid uri:{(value == null ? "null" : value.OriginalString)}");

                baseUri = value;
            }
        }

        public virtual int MaxTaskCount
        {
            get => maxTaskCount;
            set => maxTaskCount = Mathf.Max(value > 0 ? value : SystemInfo.processorCount * 2, 1);
        }

        protected virtual bool IsAllowedAbsoluteUri(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
                return false;

            if ("http".Equals(uri.Scheme) || "https".Equals(uri.Scheme) || "ftp".Equals(uri.Scheme))
                return true;

            if ("file".Equals(uri.Scheme) && uri.OriginalString.IndexOf("jar:") < 0)
                return true;

            return false;
        }

        protected virtual Uri GetAbsoluteUri(Uri relativePath)
        {
            if (baseUri == null || IsAllowedAbsoluteUri(relativePath))
                return relativePath;

            return new Uri(baseUri, relativePath);
        }

        public virtual IProgressResult<ProgressInfo, FileInfo> DownloadFileAsync(Uri path, string fileName)
        {
            return DownloadFileAsync(path, new FileInfo(fileName));
        }

        public abstract IProgressResult<ProgressInfo, FileInfo> DownloadFileAsync(Uri path, FileInfo fileInfo);

        public abstract IProgressResult<ProgressInfo, ResourceInfo[]> DownloadFileAsync(ResourceInfo[] infos);
    }
}
