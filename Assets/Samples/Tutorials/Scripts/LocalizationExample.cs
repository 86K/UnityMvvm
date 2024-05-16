using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Mvvm
{
    public class LocalizationExample : MonoBehaviour
	{
		public Dropdown dropdown;

		private Localization localization;

		void Awake ()
		{
            localization = Localization.Current;
            localization.CultureInfo = Locale.GetCultureInfoByLanguage(SystemLanguage.English);

            //Use files in xml format
            // localization.AddDataProvider(new DefaultDataProvider("LocalizationTutorials", new XmlDocumentParser()));

            //Use files in asset format
            //this.localization.AddDataProvider(new DefaultLocalizationSourceDataProvider("LocalizationTutorials", "LocalizationModule.asset"));

            dropdown.onValueChanged.AddListener (OnValueChanged);
		}

		void OnValueChanged (int value)
		{
			switch (value) {
			case 0:
				localization.CultureInfo = Locale.GetCultureInfoByLanguage (SystemLanguage.English);
				break;
			case 1:
				localization.CultureInfo = Locale.GetCultureInfoByLanguage (SystemLanguage.ChineseSimplified);
				break;
			default:
				localization.CultureInfo = Locale.GetCultureInfoByLanguage (SystemLanguage.English);
				break;
			}
		}

		void OnDestroy ()
		{
			dropdown.onValueChanged.RemoveListener (OnValueChanged);
		}
	}
}