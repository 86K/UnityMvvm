

using System;

namespace Fusion.Mvvm
{
    [Serializable]
    public abstract class SourceDescription
    {
        private bool isStatic;
        public virtual bool IsStatic
        {
            get => isStatic;
            set => isStatic = value;
        }
    }
}