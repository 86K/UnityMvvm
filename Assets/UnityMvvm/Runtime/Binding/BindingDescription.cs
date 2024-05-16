/*
 * MIT License
 *
 * Copyright (c) 2018 Clark Yang
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
 * of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 */

using System;
using System.Text;

using Loxodon.Framework.Binding.Paths;
using Loxodon.Framework.Binding.Proxy.Sources;
using Loxodon.Framework.Binding.Proxy.Sources.Object;
using Loxodon.Framework.Binding.Converters;

namespace Loxodon.Framework.Binding
{
    [Serializable]
    public class BindingDescription
    {
        public string TargetName { get; set; }

        public Type TargetType { get; set; }

        public string UpdateTrigger { get; set; }

        public IConverter Converter { get; set; }

        public BindingMode Mode { get; set; }

        public SourceDescription Source { get; set; }

        public object CommandParameter { get; set; }

        public BindingDescription()
        {
        }

        public BindingDescription(string targetName, Path path, IConverter converter = null, BindingMode mode = BindingMode.Default)
        {
            TargetName = targetName;
            Mode = mode;
            Converter = converter;
            Source = new ObjectSourceDescription
            {
                Path = path
            };
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("{binding ").Append(TargetName);

            if (!string.IsNullOrEmpty(UpdateTrigger))
                buf.Append(" UpdateTrigger:").Append(UpdateTrigger);

            if (Converter != null)
                buf.Append(" Converter:").Append(Converter.GetType().Name);

            if (Source != null)
                buf.Append(" ").Append(Source.ToString());

            if (CommandParameter != null)
                buf.Append(" CommandParameter:").Append(CommandParameter);

            buf.Append(" Mode:").Append(Mode.ToString());
            buf.Append(" }");
            return buf.ToString();
        }
    }
}