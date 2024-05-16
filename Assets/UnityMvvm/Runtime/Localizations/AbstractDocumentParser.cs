

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Fusion.Mvvm
{
    public abstract class AbstractDocumentParser : IDocumentParser
    {
        private readonly List<ITypeConverter> converters = new List<ITypeConverter>();

        public AbstractDocumentParser() : this(null)
        {
        }

        public AbstractDocumentParser(List<ITypeConverter> converters)
        {
            if (converters != null)
                this.converters.AddRange(converters);
            this.converters.Add(new ColorTypeConverter());
            this.converters.Add(new VectorTypeConverter());
            this.converters.Add(new RectTypeConverter());
            this.converters.Add(new PrimitiveTypeConverter());
            this.converters.Add(new VersionTypeConverter());
        }

        public abstract Dictionary<string, object> Parse(Stream input, CultureInfo cultureInfo);

        protected virtual object Parse(string typeName, string value)
        {
            // foreach (ITypeConverter converter in converters)
            // {
            //     if (!converter.Support(typeName))
            //         continue;
            //
            //     Type type = converter.GetType(typeName);
            //     return converter.Convert(type, value);
            // }

            throw new NotSupportedException($"The '{typeName}' is not supported.");
        }

        protected virtual object Parse(string typeName, IList<string> values)
        {
            // foreach (ITypeConverter converter in converters)
            // {
            //     if (!converter.Support(typeName))
            //         continue;
            //
            //     Type type = converter.GetType(typeName);
            //     Array array = Array.CreateInstance(type, values.Count);
            //     for (int i = 0; i < values.Count; i++)
            //     {
            //         object value = converter.Convert(type, values[i]);
            //         array.SetValue(value, i);
            //     }
            //     return array;
            // }

            throw new NotSupportedException($"The '{typeName}' is not supported.");
        }
    }
}
