

using System;

namespace Fusion.Mvvm
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class AllowedMembersAttribute : Attribute
    {
        private readonly Type type;
        private readonly string[] names;

        public AllowedMembersAttribute(Type type, params string[] names)
        {
            this.type = type;
            this.names = names;
            if (this.names == null)
                this.names = new string[0];
        }

        public Type Type => type;

        public string[] Names => names;
    }
}
