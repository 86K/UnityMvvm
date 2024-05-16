using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Fusion.Mvvm
{
    public class AssetBundleLocalizationSourceDataProvider : IDataProvider
    {
        protected string[] filenames;
        protected string assetBundleUrl;

        public AssetBundleLocalizationSourceDataProvider(string assetBundleUrl, params string[] filenames)
        {
            if (string.IsNullOrEmpty(assetBundleUrl))
                throw new ArgumentNullException("assetBundleUrl");

            this.assetBundleUrl = assetBundleUrl;
            this.filenames = filenames;
        }

        public virtual async Task<Dictionary<string, object>> Load(CultureInfo cultureInfo)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(assetBundleUrl))
            {
                await www.SendWebRequest();

                DownloadHandlerAssetBundle handler = (DownloadHandlerAssetBundle)www.downloadHandler;
                AssetBundle bundle = handler.assetBundle;
                if (bundle == null)
                {
                    Debug.LogWarning(string.Format("Failed to load Assetbundle from \"{0}\".", assetBundleUrl));
                    return dict;
                }
                try
                {
                    List<string> assetNames = new List<string>(bundle.GetAllAssetNames());
                    foreach (string filename in filenames)
                    {
                        try
                        {
                            string defaultPath = assetNames.Find(p => p.Contains($"/default/{filename}"));//eg:default
                            string twoLetterISOpath = assetNames.Find(p => p.Contains($"/{cultureInfo.TwoLetterISOLanguageName}/{filename}"));//eg:zh  en
                            string path = cultureInfo.Name.Equals(cultureInfo.TwoLetterISOLanguageName) ? null : assetNames.Find(p => p.Contains(
                                $"/{cultureInfo.Name}/{filename}"));//eg:zh-CN  en-US

                            FillData(dict, bundle, defaultPath);
                            FillData(dict, bundle, twoLetterISOpath);
                            FillData(dict, bundle, path);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(string.Format("An error occurred when loading localized data from \"{0}\".Error:{1}", filename, e));
                        }
                    }
                }
                finally
                {
                    try
                    {
                        if (bundle != null)
                            bundle.Unload(true);
                    }
                    catch (Exception) { }
                }
                return dict;
            }
        }

        private void FillData(Dictionary<string, object> dict, AssetBundle bundle, string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            LocalizationSourceAsset sourceAsset = bundle.LoadAsset<LocalizationSourceAsset>(path);
            if (sourceAsset == null)
                return;

            MonolingualSource source = sourceAsset.Source;
            if (source == null)
                return;

            foreach (KeyValuePair<string, object> kv in source.GetData())
            {
                dict[kv.Key] = kv.Value;
            }
        }
    }
}
