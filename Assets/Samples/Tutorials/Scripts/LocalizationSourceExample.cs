using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Mvvm
{
    public class LocalizationSourceExample : MonoBehaviour
    {
        public Dropdown dropdown;

        private Localization localization;

        void Awake()
        {
            localization = Localization.Current;
            localization.CultureInfo = new CultureInfo("en-CA");
            // localization.AddDataProvider(new DefaultDataProvider("LocalizationTutorials", new XmlDocumentParser()));

            dropdown.value = 0;
            dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        void OnValueChanged(int value)
        {
            switch (value)
            {
                case 0:
                    localization.CultureInfo = new CultureInfo("en-CA");
                    break;
                case 1:
                    localization.CultureInfo = new CultureInfo("zh-CN");
                    break;
                case 2:
                    localization.CultureInfo = new CultureInfo("ko-KR");
                    break;
                case 3:
                    localization.CultureInfo = new CultureInfo("ja-JP");
                    break;
                default:
                    localization.CultureInfo = new CultureInfo("zh-CN");
                    break;
            }
        }

        void OnDestroy()
        {
            dropdown.onValueChanged.RemoveListener(OnValueChanged);
        }
    }
}