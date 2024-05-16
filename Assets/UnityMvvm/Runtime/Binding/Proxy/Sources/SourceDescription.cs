

using System;

namespace Fusion.Mvvm
{
    [Serializable]
    public abstract class SourceDescription
    {
        private bool isStatic = false;
        public virtual bool IsStatic
        {
            get => isStatic;
            set => isStatic = value;
        }
    }
}