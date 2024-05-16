

using System;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Fusion.Mvvm
{
    public class VectorTypeConverter : ITypeConverter
    {
        private static readonly char[] COMMA_SEPARATOR = new char[] { ',' };
        private static readonly string PATTERN = @"(^\()|(\)$)";
        public bool Support(string typeName)
        {
            switch (typeName)
            {
                case "vector2":
                case "vector3":
                case "vector4":
                    return true;
                default:
                    return false;
            }
        }

        public Type GetType(string typeName)
        {
            switch (typeName)
            {
                case "vector2":
                    return typeof(Vector2);
                case "vector3":
                    return typeof(Vector3);
                case "vector4":
                    return typeof(Vector4);
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

            var val = Regex.Replace(((string)value).Trim(), PATTERN, "");
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
    }
}
