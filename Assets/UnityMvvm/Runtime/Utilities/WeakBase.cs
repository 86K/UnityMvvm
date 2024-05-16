/*
 * MIT License
 *
 * Copyright (c) 2018 Clark Yang
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
 * of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 */

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Loxodon.Framework.Utilities
{
    public interface IExecute
    {
        object Execute(params object[] parameters);
    }

    public abstract class WeakBase<TDelegate> : IExecute where TDelegate : class
    {
        private readonly bool isStatic;
        private readonly int hashCode = 0;

        protected TDelegate del;
        protected WeakReference targetReference;
        protected MethodInfo targetMethod;

        public WeakBase(TDelegate del) : this(null, del)
        {
        }

        public WeakBase(object target, TDelegate del)
        {
            hashCode = del.GetHashCode();
            var dd = del as Delegate;

#if NETFX_CORE
            this.isStatic = dd.GetMethodInfo().IsStatic;
#else
            isStatic = dd.Method.IsStatic;
#endif
            if (isStatic || (target != null && !target.Equals(dd.Target)) || IsClosure(dd))
            {
                this.del = del;
                if (target != null)
                    targetReference = new WeakReference(target);
            }
            else
            {
#if NETFX_CORE
                this.targetMethod = dd.GetMethodInfo();
#else
                targetMethod = dd.Method;
#endif
                targetReference = new WeakReference(dd.Target);
            }
        }

        protected bool IsStatic => isStatic;

        public bool IsAlive
        {
            get
            {
                if (del != null)
                {
                    if (targetReference != null && !targetReference.IsAlive)
                    {
                        targetReference = null;
                        del = null;
                        return false;
                    }
                    return true;
                }

                if (targetReference != null)
                    return targetReference.IsAlive;

                return false;
            }
        }

        protected bool IsClosure(Delegate del)
        {
#if NETFX_CORE
            if (del == null || del.GetMethodInfo().IsStatic || del.Target == null)
                return false;

            var type = del.Target.GetType();
            var isCompilerGenerated = type.GetTypeInfo().GetCustomAttribute(typeof(CompilerGeneratedAttribute), false) !=null;
            var isNested = type.IsNested;
            return isNested && isCompilerGenerated;
#else
            if (del == null || del.Method.IsStatic || del.Target == null)
                return false;

            var type = del.Target.GetType();
            var isInvisible = !type.IsVisible;
            var isCompilerGenerated = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length != 0;
            var isNested = type.IsNested && type.MemberType == MemberTypes.NestedType;
            return isNested && isCompilerGenerated && isInvisible;
#endif
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null || !(obj is WeakBase<TDelegate>))
                return false;

            WeakBase<TDelegate> other = (WeakBase<TDelegate>)obj;
            if (isStatic != other.isStatic)
                return false;

            if (del != null)
            {
                if ((targetReference == null && other.targetReference == null) || (targetReference != null && other.targetReference != null && targetReference.Target == other.targetReference.Target))
                    return del.Equals(other.del);

                return false;
            }

            return targetMethod.Equals(other.targetMethod) && targetReference.Target == other.targetReference.Target;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public abstract object Execute(params object[] parameters);
    }
}
