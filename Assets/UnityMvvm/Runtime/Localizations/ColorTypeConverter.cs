

using System;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class ColorTypeConverter : ITypeConverter
    {
        public bool Support(string typeName)
        {
            switch (typeName)
            {
                case "color":
                    return true;
                default:
                    return false;
            }
        }

        public Type GetType(string typeName)
        {
            switch (typeName)
            {
                case "color":
                    return typeof(Color);
                default:
                    throw new NotSupportedException();
            }
        }

        public bool Support(Type type)
        {
            throw new NotImplementedException();
        }

        public object Convert(Type type, object value)
        {
            if (type == null)
                throw new NotSupportedException();

            Color color;
            if (ColorUtility.TryParseHtmlString((string)value, out color))
                return color;

            throw new FormatException($"The '{value}' is illegal Color.");
        }
    }
}
