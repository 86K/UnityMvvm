

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Fusion.Mvvm
{
    public class PropertiesConfiguration : ConfigurationBase
    {
        private readonly Dictionary<string, object> dict = new Dictionary<string, object>();
        public PropertiesConfiguration(string text)
        {
            Load(text);
        }

        protected void Load(string text)
        {
            StringReader reader = new StringReader(text);
            string line = null;
            while (null != (line = reader.ReadLine()))
            {
                line = line.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                if (Regex.IsMatch(line, @"^((#)|(//))"))
                    continue;

                int index = line.IndexOf("=");
                if (index <= 0 || (index + 1) >= line.Length)
                    throw new FormatException($"This line is not formatted correctly.line:{line}");

                string key = line.Substring(0, index).Trim();
                string value = line.Substring(index + 1).Trim();
                if (string.IsNullOrEmpty(key))
                    throw new FormatException($"The key is null or empty.line:{line}");

                if (dict.ContainsKey(key))
                    throw new AlreadyExistsException($"This key already exists.line:{line}");

                dict.Add(key, value);
            }
        }

        public override bool IsEmpty => dict.Count == 0;

        public override bool ContainsKey(string key)
        {
            return dict.ContainsKey(key);
        }

        public override IEnumerator<string> GetKeys()
        {
            return dict.Keys.GetEnumerator();
        }

        public override object GetProperty(string key)
        {
            object value = null;
            dict.TryGetValue(key, out value);
            return value;
        }

        public override void AddProperty(string key, object value)
        {
            if (dict.ContainsKey(key))
                throw new AlreadyExistsException(key);

            dict.Add(key, value);
        }

        public override void SetProperty(string key, object value)
        {
            dict[key] = value;
        }

        public override void RemoveProperty(string key)
        {
            dict.Remove(key);
        }

        public override void Clear()
        {
            dict.Clear();
        }
    }
}
