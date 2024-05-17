using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Fusion.Mvvm
{
    public class Path : IEnumerator<IPathNode>
    {
        private readonly List<IPathNode> _pathNodes = new List<IPathNode>();

        public IPathNode this[int index] => _pathNodes[index];
        
        public int Count => _pathNodes.Count;

        public bool IsStatic { get { return _pathNodes.Exists(n => n.IsStatic); } }

        public void Append(IPathNode node)
        {
            _pathNodes.Add(node);
        }

        public void Prepend(IPathNode node)
        {
            _pathNodes.Insert(0, node);
        }

        public void PrependIndexed(string indexValue)
        {
            Prepend(new StringIndexedNode(indexValue));
        }

        public void PrependIndexed(int indexValue)
        {
            Prepend(new IntegerIndexedNode(indexValue));
        }

        public void AppendIndexed(string indexValue)
        {
            Append(new StringIndexedNode(indexValue));
        }

        public void AppendIndexed(int indexValue)
        {
            Append(new IntegerIndexedNode(indexValue));
        }

        public PathToken AsPathToken()
        {
            if (_pathNodes.Count <= 0)
                throw new InvalidOperationException("The path node is empty");
            return new PathToken(this, 0);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            foreach (var node in _pathNodes)
            {
                node.AppendTo(buf);
            }
            return buf.ToString();
        }

        #region IEnumerator<IPathNode> Support
        private int _index = -1;
        public IPathNode Current => _pathNodes[_index];

        object IEnumerator.Current => _pathNodes[_index];

        public bool MoveNext()
        {
            _index++;
            return _index >= 0 && _index < _pathNodes.Count;
        }

        public void Reset()
        {
            _index = -1;
        }
        #endregion

        #region IDisposable Support
        private bool disposed;

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _pathNodes.Clear();
                    _index = -1;
                }
                disposed = true;
            }
        }

        ~Path()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    public interface IPathNode
    {
        bool IsStatic { get; }

        void AppendTo(StringBuilder output);
    }

    [Serializable]
    public class MemberNode : IPathNode
    {
        private readonly MemberInfo memberInfo;
        private readonly string name;
        private readonly Type type;
        private readonly bool isStatic;
        public MemberNode(string name) : this(null, name, false)
        {
        }

        public MemberNode(Type type, string name, bool isStatic)
        {
            this.name = name;
            this.type = type;
            this.isStatic = isStatic;
        }

        public MemberNode(MemberInfo memberInfo)
        {
            this.memberInfo = memberInfo;
            name = memberInfo.Name;
            type = memberInfo.DeclaringType;
            isStatic = memberInfo.IsStatic();
        }

        public bool IsStatic => isStatic;

        public Type Type => type;

        public string Name => name;

        public MemberInfo MemberInfo => memberInfo;

        public void AppendTo(StringBuilder output)
        {
            if (output.Length > 0)
                output.Append(".");
            if (IsStatic)
                output.Append(type.FullName).Append(".");
            output.Append(Name);
        }

        public override string ToString()
        {
            return "MemberNode:" + (Name ?? "null");
        }
    }

    [Serializable]
    public abstract class IndexedNode : IPathNode
    {
        private object _value;

        protected IndexedNode(object value)
        {
            _value = value;
        }

        public bool IsStatic => false;

        public object Value
        {
            get => _value;
            private set => _value = value;
        }

        public abstract void AppendTo(StringBuilder output);

        public override string ToString()
        {
            return "IndexedNode:" + (_value == null ? "null" : _value.ToString());
        }
    }

    [Serializable]
    public abstract class IndexedNode<T> : IndexedNode
    {
        protected IndexedNode(T value) : base(value)
        {
        }

        public new T Value => (T)base.Value;
    }

    [Serializable]
    public class StringIndexedNode : IndexedNode<string>
    {
        public StringIndexedNode(string indexValue) : base(indexValue)
        {
        }

        public override void AppendTo(StringBuilder output)
        {
            output.AppendFormat("[\"{0}\"]", Value);
        }
    }

    [Serializable]
    public class IntegerIndexedNode : IndexedNode<int>
    {
        public IntegerIndexedNode(int indexValue) : base(indexValue)
        {
        }

        public override void AppendTo(StringBuilder output)
        {
            output.AppendFormat("[{0}]", Value);
        }
    }
}
