

using System;

namespace Fusion.Mvvm
{
    public class VersionTypeConverter : ITypeConverter
    {
        public bool Support(string typeName)
        {
            switch (typeName)
            {
                case "version":
                    return true;
                default:
                    return false;
            }
        }

        public Type GetType(string typeName)
        {
            switch (typeName)
            {
                case "version":
                    return typeof(Version);
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

            string version = (string)value;
            if (string.IsNullOrEmpty(version))
                return new Version("0.0.0");
            return new Version(version.Trim());
        }
    }
}
