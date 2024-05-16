using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Mvvm
{
    public class LocalizedDataBinderExample : MonoBehaviour
    {
        public Dropdown dropdown;
        private Localization localization;
        void Awake()
        {
            ApplicationContext context = Context.GetApplicationContext();
            BindingServiceBundle bindingService = new BindingServiceBundle(context.GetContainer());
            bindingService.Start();

            localization = Localization.Current;
            CultureInfo cultureInfo = Locale.GetCultureInfoByLanguage(SystemLanguage.English);
            localization.CultureInfo = cultureInfo;
            // localization.AddDataProvider(new DefaultDataProvider("LocalizationTutorials", new XmlDocumentParser()));
            //this.localization.AddDataProvider(new DefaultLocalizationSourceDataProvider("LocalizationTutorials", "LocalizationModule.asset"));

            dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        public void OnValueChanged(int value)
        {
            switch (value)
            {
                case 0:
                    localization.CultureInfo = Locale.GetCultureInfoByLanguage(SystemLanguage.English);
                    break;
                case 1:
                    localization.CultureInfo = Locale.GetCultureInfoByLanguage(SystemLanguage.ChineseSimplified);
                    break;
                default:
                    localization.CultureInfo = Locale.GetCultureInfoByLanguage(SystemLanguage.English);
                    break;
            }
        }

    }
}