

using System.Collections.Generic;

namespace Fusion.Mvvm
{
    public class LocalizedString : LocalizedObject<string>
    {
        public LocalizedString() : base(null, Localization.Current)
        {
        }

        public LocalizedString(IDictionary<string, string> source) : base(source, Localization.Current)
        {
        }

        public LocalizedString(IDictionary<string, string> source, Localization localization) : base(source, localization)
        {
        }
    }
}
