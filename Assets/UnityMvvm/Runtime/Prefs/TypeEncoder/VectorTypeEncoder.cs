



using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class VectorTypeEncoder : ITypeEncoder
    {
        private static readonly char[] COMMA_SEPARATOR = new char[] { ',' };
        private static readonly string PATTERN = @"(^\()|(\)$)";

        private int priority = 996;

        public int Priority
        {
            get => priority;
            set => priority = value;
        }

        public bool IsSupport(Type type)
        {
            if (type.Equals(typeof(Vector2)) || type.Equals(typeof(Vector3)) || type.Equals(typeof(Vector4)))
                return true;
            return false;
        }

        public object Decode(Type type, string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var val = Regex.Replace((value).Trim(), PATTERN, "");
            if (type.Equals(typeof(Vector2)))
            {
                try
                {
                    string[] s = val.Split(COMMA_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
                    if (s.Length == 2)
                        return new Vector2(float.Parse(s[0]), float.Parse(s[1]));
                }
                catch (Exception e)
                {
                    throw new FormatException($"The '{value}' is illegal Vector2.", e);
                }
                throw new FormatException($"The '{value}' is illegal Vector2.");
            }

            if (type.Equals(typeof(Vector3)))
            {
                try
                {
                    string[] s = val.Split(COMMA_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
                    if (s.Length == 3)
                        return new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]));
                }
                catch (Exception e)
                {
                    throw new FormatException($"The '{value}' is illegal Vector3.", e);
                }
                throw new FormatException($"The '{value}' is illegal Vector3.");
            }

            if (type.Equals(typeof(Vector4)))
            {
                try
                {
                    string[] s = val.Split(COMMA_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
                    if (s.Length == 4)
                        return new Vector4(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3]));
                }
                catch (Exception e)
                {
                    throw new FormatException($"The '{value}' is illegal Vector4.", e);
                }
                throw new FormatException($"The '{value}' is illegal Vector4.");
            }

            throw new NotSupportedException();
        }

        public string Encode(object value)
        {
            return value.ToString();
        }
    }
}
