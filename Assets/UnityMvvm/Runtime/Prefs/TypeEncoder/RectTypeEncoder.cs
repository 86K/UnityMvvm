



using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class RectTypeEncoder : ITypeEncoder
    {
        private int priority = 995;

        public int Priority
        {
            get => priority;
            set => priority = value;
        }

        public bool IsSupport(Type type)
        {
            if (type.Equals(typeof(Rect)))
                return true;
            return false;
        }

        public object Decode(Type type, string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            try
            {
                var val = Regex.Replace((value).Trim(), @"(^\()|(\)$)", "");
                string[] s = val.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (s.Length == 4)
                    return new Rect(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3]));
            }
            catch (Exception e)
            {
                throw new FormatException($"The '{value}' is illegal Rect.", e);
            }
            throw new FormatException($"The '{value}' is illegal Rect.");
        }

        public string Encode(object value)
        {
            Rect rect = (Rect)value;
            return $"({rect.x:F2}, {rect.y:F2}, {rect.width:F2}, {rect.height:F2})";
        }
    }
}
