

using System;

namespace Fusion.Mvvm
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Enum)]
    public class RemarkAttribute : Attribute
    {
        private readonly string remark;
        public RemarkAttribute(string remark)
        {
            this.remark = remark;
        }

        public string Remark => remark;
    }
}
