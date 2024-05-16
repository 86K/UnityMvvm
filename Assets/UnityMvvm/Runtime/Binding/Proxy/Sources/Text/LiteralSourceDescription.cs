

using System;

namespace Fusion.Mvvm
{
    [Serializable]
    public class LiteralSourceDescription : SourceDescription
    {
        public object Literal { get; set; }

        public LiteralSourceDescription()
        {
            IsStatic = true;
        }

        public override string ToString()
        {
            return Literal == null ? "Literal:null" : "Literal:" + Literal.ToString();
        }
    }
}