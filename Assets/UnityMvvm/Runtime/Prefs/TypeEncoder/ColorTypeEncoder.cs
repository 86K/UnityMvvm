



using System;
using UnityEngine;

namespace Fusion.Mvvm
{

    public class ColorTypeEncoder : ITypeEncoder
    {
        private int priority = 997;

        public int Priority
        {
            get => priority;
            set => priority = value;
        }

        public bool IsSupport(Type type)
        {
            if (type.Equals(typeof(Color)))
                return true;
            return false;
        }

        public object Decode(Type type, string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            Color color;
            if (ColorUtility.TryParseHtmlString(value, out color))
                return color;

            throw new FormatException($"The '{value}' is illegal Color.");
        }

        public string Encode(object value)
        {
            return $"#{ColorUtility.ToHtmlStringRGBA((Color)value)}";
        }
    }
}
