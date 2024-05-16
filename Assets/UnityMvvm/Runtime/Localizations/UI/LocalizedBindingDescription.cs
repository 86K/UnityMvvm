using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Fusion.Mvvm
{
    [Serializable]
    public class LocalizedBindingDescriptionSet
    {
        public List<LocalizedBindingDescription> descriptions = new List<LocalizedBindingDescription>();
    }

    [Serializable]
    public class LocalizedBindingDescription
    {
        [SerializeField]
        public string TypeName;

        [SerializeField]
        public string PropertyName;

        [SerializeField]
        public string Key;

        [SerializeField]
        public BindingMode Mode = BindingMode.OneWay;

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(TypeName).Append(" ");
            buf.Append("{binding ").Append(PropertyName);
            buf.Append(" Key:").Append(Key);
            buf.Append(" Mode:").Append(Mode);
            buf.Append(" }");
            return buf.ToString();
        }
    }
}
