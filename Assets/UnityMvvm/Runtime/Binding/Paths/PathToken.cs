using System;
using System.Text;

namespace Fusion.Mvvm
{
    [Serializable]
    public struct PathToken
    {
        private Path path;
        private int index;
        public PathToken(Path path, int index)
        {
            this.path = path;
            this.index = index;
        }

        public Path Path => path;

        public int Index => index;

        public IPathNode Current => path[index];

        public bool HasNext()
        {
            if (index + 1 < path.Count)
                return true;
            return false;
        }

        public PathToken NextToken()
        {
            if (!HasNext())
                throw new IndexOutOfRangeException();
            return new PathToken(path, index + 1);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(Current).Append(" Index:").Append(index);
            return buf.ToString();
        }
    }
}
