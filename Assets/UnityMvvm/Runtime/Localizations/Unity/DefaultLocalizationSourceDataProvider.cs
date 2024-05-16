

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Fusion.Mvvm
{
    /// <summary>
    /// Resources data provider.
    /// dir:
    /// root/default/
    /// 
    /// root/zh/
    /// root/zh-CN/
    /// root/zh-TW/
    /// root/zh-HK/
    /// 
    /// root/en/
    /// root/en-US/
    /// root/en-CA/
    /// root/en-AU/
    /// </summary>
    public class DefaultLocalizationSourceDataProvider : IDataProvider
    {
        protected string[] filenames;
        protected string root;

        public DefaultLocalizationSourceDataProvider(string root, params string[] filenames)
        {
            this.root = root;
            this.filenames = filenames;
        }

        protected string GetDefaultPath(string filename)
        {
            return GetPath("default", filename);
        }

        protected string GetPath(string dir, string filename)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(root);
            if (!root.EndsWith("/"))
                buf.Append("/");
            buf.Append(dir).Append("/").Append(filename.Replace(".asset", ""));
            return buf.ToString();
        }

        public virtual async Task<Dictionary<string, object>> Load(CultureInfo cultureInfo)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            List<Task> tasks = new List<Task>();
            foreach (string filename in filenames)
            {
                tasks.Add(Load(dict, filename, cultureInfo));
            }
            await Task.WhenAll(tasks);
            return dict;
        }

        protected virtual async Task Load(Dictionary<string, object> dict, string filename, CultureInfo cultureInfo)
        {
            LocalizationSourceAsset defaultSourceAsset = (LocalizationSourceAsset)await Resources.LoadAsync<LocalizationSourceAsset>(GetDefaultPath(filename)); //eg:default            
            LocalizationSourceAsset twoLetterISOSourceAsset = (LocalizationSourceAsset)await Resources.LoadAsync<LocalizationSourceAsset>(GetPath(cultureInfo.TwoLetterISOLanguageName, filename));//eg:zh  en
            LocalizationSourceAsset sourceAsset = cultureInfo.Name.Equals(cultureInfo.TwoLetterISOLanguageName) ? null : (LocalizationSourceAsset)await Resources.LoadAsync<LocalizationSourceAsset>(GetPath(cultureInfo.Name, filename));//eg:zh-CN  en-US

            if (defaultSourceAsset == null && twoLetterISOSourceAsset == null && sourceAsset == null)
            {
                Debug.LogWarning(string.Format("Not found the localized file \"{0}\".", filename));
                return;
            }

            if (defaultSourceAsset != null)
                FillData(dict, defaultSourceAsset.Source);
            if (twoLetterISOSourceAsset != null)
                FillData(dict, twoLetterISOSourceAsset.Source);
            if (sourceAsset != null)
                FillData(dict, sourceAsset.Source);
        }

        private void FillData(Dictionary<string, object> dict, MonolingualSource source)
        {
            if (source == null)
                return;

            foreach (KeyValuePair<string, object> kv in source.GetData())
            {
                dict[kv.Key] = kv.Value;
            }
        }
    }
}
