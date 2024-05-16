using System;
using System.Text;

namespace Fusion.Mvvm
{
    [Serializable]
    public class TargetDescription
    {
        public string TargetName { get; set; }

        public Type TargetType { get; set; }

        public string UpdateTrigger { get; set; }

        public IConverter Converter { get; set; }

        public BindingMode Mode { get; set; }

        public SourceDescription Source { get; set; }

        public object CommandParameter { get; set; }

        public TargetDescription()
        {
        }

        public TargetDescription(string targetName, Path path, IConverter converter = null, BindingMode mode = BindingMode.Default)
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