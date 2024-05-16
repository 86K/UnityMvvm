

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Fusion.Mvvm
{
    [Serializable]
    public class Path : IEnumerator<IPathNode>
    {
        private readonly List<IPathNode> nodes = new List<IPathNode>();
        public Path() : this(null)
        {
        }

        public Path(IPathNode root)
        {
            if (root != null)
                Prepend(root);
        }

        public IPathNode this[int index] => nodes[index];

        public bool IsEmpty => nodes.Count == 0;

        public int Count => nodes.Count;

        public bool IsStatic { get { return nodes.Exists(n => n.IsStatic); } }

        public List<IPathNode> ToList()
        {
            return new List<IPathNode>(nodes);
        }

        public void Append(IPathNode node)
        {
            nodes.Add(node);
        }

        public void Prepend(IPathNode node)
        {
            nodes.Insert(0, node);
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
            if (nodes.Count <= 0)
                throw new InvalidOperationException("The path node is empty");
            return new PathToken(this, 0);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            foreach (var node in nodes)
            {
                node.AppendTo(buf);
            }
            return buf.ToString();
        }

        #region IEnumerator<IPathNode> Support
        private int index = -1;
        public IPathNode Current => nodes[index];

        object IEnumerator.Current => nodes[index];

        public bool MoveNext()
        {
            index++;
            return index >= 0 && index < nodes.Count;
        }

        public void Reset()
        {
            index = -1;
        }
        #endregion

        #region IDisposable Support
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    nodes.Clear();
                    index = -1;
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
            return "MemberNode:" + (Name == null ? "null" : Name);
        }
    }

    [Serializable]
    public abstract class IndexedNode : IPathNode
    {
        private object _value;
        public IndexedNode(object value)
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
    public abstract class IndexedNode<T> : IndexedNode, IPathNode
    {
        public IndexedNode(T value) : base(value)
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
