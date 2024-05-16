using System;
using System.Collections.Generic;
using UnityEngine;
namespace Fusion.Mvvm
{
    public class ConfigurationExample : MonoBehaviour
    {
        private void Start()
        {
            IConfiguration conf = CreateConfiguration();
            Version appVersion = conf.GetVersion("application.app.version");
            Version dataVersion = conf.GetVersion("application.data.version");

            Debug.LogFormat("application.app.version:{0}", appVersion);
            Debug.LogFormat("application.data.version:{0}", dataVersion);

            string groupName = conf.GetString("application.config-group");
            IConfiguration currentGroupConf = conf.Subset("application." + groupName);

            string upgradeUrl = currentGroupConf.GetString("upgrade.url");
            string username = currentGroupConf.GetString("username");
            string password = currentGroupConf.GetString("password");
            string[] gatewayArray = currentGroupConf.GetArray<string>("gateway");

            Debug.LogFormat("upgrade.url:{0}", upgradeUrl);
            Debug.LogFormat("username:{0}", username);
            Debug.LogFormat("password:{0}", password);

            int i = 1;
            foreach (string gateway in gatewayArray)
            {
                Debug.LogFormat("gateway {0}:{1}", i++, gateway);
            }
        }

        private IConfiguration CreateConfiguration()
        {
            List<IConfiguration> list = new List<IConfiguration>();

            //Load default configuration file
            TextAsset text = Resources.Load<TextAsset>("application.properties");
            list.Add(new PropertiesConfiguration(text.text));

            //Load configuration files based on platform information. Configuration files loaded later 
            //have a higher priority than configuration files loaded first.
            text = Resources.Load<TextAsset>($"application.{Application.platform.ToString().ToLower()}.properties");
            if (text != null)
                list.Add(new PropertiesConfiguration(text.text));

            if (list.Count == 1)
                return list[0];

            return new CompositeConfiguration(list);
        }
    }
}
