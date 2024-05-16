using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Mvvm
{

    public class DatabindingForLocalizationViewModel : ViewModelBase
    {
        private readonly Localization localization;

        public DatabindingForLocalizationViewModel(Localization localization)
        {
            this.localization = localization;
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

    public class DatabindingForLocalizationExample : MonoBehaviour
    {
        public Dropdown dropdown;

        public Text text;

        private Localization localization;

        void Awake()
        {
            ApplicationContext context = Context.GetApplicationContext();
            BindingServiceBundle bindingService = new BindingServiceBundle(context.GetContainer());
            bindingService.Start();

            localization = Localization.Current;
            CultureInfo cultureInfo = Locale.GetCultureInfoByLanguage(SystemLanguage.English);
            localization.CultureInfo = cultureInfo;
            //this.localization.AddDataProvider(new DefaultDataProvider("LocalizationTutorials", new XmlDocumentParser()));
            // localization.AddDataProvider(new DefaultLocalizationSourceDataProvider("LocalizationTutorials", "LocalizationModule.asset"));
        }

        void Start()
        {
            BindingSet<DatabindingForLocalizationExample, DatabindingForLocalizationViewModel> bindingSet = this.CreateBindingSet(new DatabindingForLocalizationViewModel(localization));
            bindingSet.Bind(dropdown).For(v => v.onValueChanged).To<int>(vm => vm.OnValueChanged);
            bindingSet.Build();

            BindingSet<DatabindingForLocalizationExample> staticBindingSet = this.CreateBindingSet();
            //staticBindingSet.Bind(this.text).For(v => v.text).To(() => Res.localization_tutorials_content).OneWay();
            staticBindingSet.Bind(text).For(v => v.text).ToValue(localization.GetValue("localization.tutorials.content")).OneWay();
            staticBindingSet.Build();
        }
    }
}