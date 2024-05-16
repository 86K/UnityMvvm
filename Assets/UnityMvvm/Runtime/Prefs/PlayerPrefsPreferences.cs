

using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace Fusion.Mvvm
{
    /// <summary>
    /// 
    /// </summary>
    public class PlayerPrefsPreferencesFactory : AbstractFactory
    {
        /// <summary>
        /// 
        /// </summary>
        public PlayerPrefsPreferencesFactory() : this(null, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializer"></param>
        public PlayerPrefsPreferencesFactory(ISerializer serializer) : this(serializer, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="encryptor"></param>
        public PlayerPrefsPreferencesFactory(ISerializer serializer, IPrefsEncryptor encryptor) : base(serializer, encryptor)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override Preferences Create(string name)
        {
            return new PlayerPrefsPreferences(name, Serializer, Encryptor);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PlayerPrefsPreferences : Preferences
    {
        /// <summary>
        /// Default key
        /// </summary>
        protected static readonly string KEYS = "_KEYS_";

        /// <summary>
        /// 
        /// </summary>
        protected readonly ISerializer serializer;
        /// <summary>
        /// 
        /// </summary>
        protected readonly IPrefsEncryptor encryptor;

        /// <summary>
        /// 
        /// </summary>
        protected readonly List<string> keys = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="serializer"></param>
        /// <param name="encryptor"></param>
        public PlayerPrefsPreferences(string name, ISerializer serializer, IPrefsEncryptor encryptor) : base(name)
        {
            this.serializer = serializer;
            this.encryptor = encryptor;
            Load();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Load()
        {
            LoadKeys();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string Key(string key)
        {
            StringBuilder buf = new StringBuilder(Name);
            buf.Append(".").Append(key);
            return buf.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void LoadKeys()
        {
            if (!PlayerPrefs.HasKey(Key(KEYS)))
                return;

            string value = PlayerPrefs.GetString(Key(KEYS));
            if (string.IsNullOrEmpty(value))
                return;

            string[] keyValues = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string key in keyValues)
            {
                if (string.IsNullOrEmpty(key))
                    continue;

                keys.Add(key);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void SaveKeys()
        {
            if (keys == null || keys.Count <= 0)
            {
                PlayerPrefs.DeleteKey(Key(KEYS));
                return;
            }

            string[] values = keys.ToArray();

            StringBuilder buf = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                if (string.IsNullOrEmpty(values[i]))
                    continue;

                buf.Append(values[i]);
                if (i < values.Length - 1)
                    buf.Append(",");
            }

            PlayerPrefs.SetString(Key(KEYS), buf.ToString());
        }

        public override object GetObject(string key, Type type, object defaultValue)
        {
            if (!PlayerPrefs.HasKey(Key(key)))
                return defaultValue;

            string str = PlayerPrefs.GetString(Key(key));
            if (string.IsNullOrEmpty(str))
                return defaultValue;

            if (encryptor != null)
            {
                byte[] data = Convert.FromBase64String(str);
                data = encryptor.Decode(data);
                str = Encoding.UTF8.GetString(data);
            }

            return serializer.Deserialize(str, type);
        }

        public override void SetObject(string key, object value)
        {
            string str = value == null ? "" : serializer.Serialize(value);
            if (encryptor != null)
            {
                byte[] data = Encoding.UTF8.GetBytes(str);
                data = encryptor.Encode(data);
                str = Convert.ToBase64String(data);
            }

            PlayerPrefs.SetString(Key(key), str);

            if (!keys.Contains(key))
            {
                keys.Add(key);
                SaveKeys();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public override T GetObject<T>(string key, T defaultValue)
        {
            if (!PlayerPrefs.HasKey(Key(key)))
                return defaultValue;

            string str = PlayerPrefs.GetString(Key(key));
            if (string.IsNullOrEmpty(str))
                return defaultValue;

            if (encryptor != null)
            {
                byte[] data = Convert.FromBase64String(str);
                data = encryptor.Decode(data);
                str = Encoding.UTF8.GetString(data);
            }

            return (T)serializer.Deserialize(str, typeof(T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public override void SetObject<T>(string key, T value)
        {
            string str = value == null ? "" : serializer.Serialize(value);
            if (encryptor != null)
            {
                byte[] data = Encoding.UTF8.GetBytes(str);
                data = encryptor.Encode(data);
                str = Convert.ToBase64String(data);
            }

            PlayerPrefs.SetString(Key(key), str);

            if (!keys.Contains(key))
            {
                keys.Add(key);
                SaveKeys();
            }
        }

        public override object[] GetArray(string key, Type type, object[] defaultValue)
        {
            if (!PlayerPrefs.HasKey(Key(key)))
                return defaultValue;

            string str = PlayerPrefs.GetString(Key(key));
            if (string.IsNullOrEmpty(str))
                return defaultValue;

            if (encryptor != null)
            {
                byte[] data = Convert.FromBase64String(str);
                data = encryptor.Decode(data);
                str = Encoding.UTF8.GetString(data);
            }

            string[] items = str.Split(ARRAY_SEPARATOR);
            List<object> list = new List<object>();
            for (int i = 0; i < items.Length; i++)
            {
                string item = items[i];
                if (string.IsNullOrEmpty(item))
                    list.Add(null);
                else
                {
                    list.Add(serializer.Deserialize(items[i], type));
                }
            }
            return list.ToArray();
        }

        public override void SetArray(string key, object[] values)
        {
            StringBuilder buf = new StringBuilder();
            if (values != null && values.Length > 0)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    var value = values[i];
                    buf.Append(serializer.Serialize(value));
                    if (i < values.Length - 1)
                        buf.Append(ARRAY_SEPARATOR);
                }
            }
            string str = buf.ToString();
            if (encryptor != null)
            {
                byte[] data = Encoding.UTF8.GetBytes(str);
                data = encryptor.Encode(data);
                str = Convert.ToBase64String(data);
            }

            PlayerPrefs.SetString(Key(key), str);

            if (!keys.Contains(key))
            {
                keys.Add(key);
                SaveKeys();
            }
        }

        public override T[] GetArray<T>(string key, T[] defaultValue)
        {
            if (!PlayerPrefs.HasKey(Key(key)))
                return defaultValue;

            string str = PlayerPrefs.GetString(Key(key));
            if (string.IsNullOrEmpty(str))
                return defaultValue;

            if (encryptor != null)
            {
                byte[] data = Convert.FromBase64String(str);
                data = encryptor.Decode(data);
                str = Encoding.UTF8.GetString(data);
            }

            string[] items = str.Split(ARRAY_SEPARATOR);
            List<T> list = new List<T>();
            for (int i = 0; i < items.Length; i++)
            {
                string item = items[i];
                if (string.IsNullOrEmpty(item))
                    list.Add(default(T));
                else
                {
                    list.Add((T)serializer.Deserialize(items[i], typeof(T)));
                }
            }
            return list.ToArray();
        }

        public override void SetArray<T>(string key, T[] values)
        {
            StringBuilder buf = new StringBuilder();
            if (values != null && values.Length > 0)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    var value = values[i];
                    buf.Append(serializer.Serialize(value));
                    if (i < values.Length - 1)
                        buf.Append(ARRAY_SEPARATOR);
                }
            }
            string str = buf.ToString();
            if (encryptor != null)
            {
                byte[] data = Encoding.UTF8.GetBytes(str);
                data = encryptor.Encode(data);
                str = Convert.ToBase64String(data);
            }

            PlayerPrefs.SetString(Key(key), str);

            if (!keys.Contains(key))
            {
                keys.Add(key);
                SaveKeys();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override bool ContainsKey(string key)
        {
            return PlayerPrefs.HasKey(Key(key));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public override void Remove(string key)
        {
            PlayerPrefs.DeleteKey(Key(key));
            if (keys.Contains(key))
            {
                keys.Remove(key);
                SaveKeys();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void RemoveAll()
        {
            foreach (string key in keys)
            {
                PlayerPrefs.DeleteKey(Key(key));
            }
            PlayerPrefs.DeleteKey(Key(KEYS));
            keys.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Save()
        {
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Delete()
        {
            RemoveAll();
            PlayerPrefs.Save();
        }
    }
}
