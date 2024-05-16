

using System;

namespace Fusion.Mvvm
{
    [Serializable]
    public class ObjectSourceDescription : SourceDescription
    {
        private Path path;

        public ObjectSourceDescription()
        {
            IsStatic = false;
        }

        public ObjectSourceDescription(Path path)
        {
            Path = path;
        }

        public virtual Path Path
        {
            get => path;
            set
            {
                path = value;
                if (path != null)
                    IsStatic = path.IsStatic;
            }
        }

        public override string ToString()
        {
            return path == null ? "Path:null" : "Path:" + path.ToString();
        }
    }
}
