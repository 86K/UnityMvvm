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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;

namespace Loxodon.Framework.Execution
{
    /// <summary>
    /// Interceptable enumerator
    /// Pooled the InterceptableEnumerator and the promise related features built in to optimize GC  
    /// </summary>
    public class InterceptableEnumerator : IEnumerator
    {
        private const int CAPACITY = 100;
        private static readonly ConcurrentQueue<InterceptableEnumerator> pools = new ConcurrentQueue<InterceptableEnumerator>();

        public static InterceptableEnumerator Create(IEnumerator routine)
        {
            InterceptableEnumerator enumerator;
            if (pools.TryDequeue(out enumerator))
            {
                enumerator.stack.Push(routine);
                return enumerator;
            }
            return new InterceptableEnumerator(routine);
        }

        private static void Free(InterceptableEnumerator enumerator)
        {
            if (pools.Count > CAPACITY)
                return;

            enumerator.Clear();
            pools.Enqueue(enumerator);
        }

        private object current;
        private readonly Stack<IEnumerator> stack = new Stack<IEnumerator>();
        private readonly List<Func<bool>> hasNext = new List<Func<bool>>();
        private Action<Exception> onException;
        private Action onFinally;

        public InterceptableEnumerator(IEnumerator routine)
        {
            stack.Push(routine);
        }

        public object Current => current;

        public bool MoveNext()
        {
            try
            {
                if (!HasNext())
                {
                    OnFinally();
                    return false;
                }

                if (stack.Count <= 0)
                {
                    OnFinally();
                    return false;
                }

                IEnumerator ie = stack.Peek();
                bool hasNext = ie.MoveNext();
                if (!hasNext)
                {
                    stack.Pop();
                    return MoveNext();
                }

                current = ie.Current;
                if (current is IEnumerator)
                {
                    stack.Push(current as IEnumerator);
                    return MoveNext();
                }

                if (current is Coroutine)
                    Debug.LogWarning(string.Format("The Enumerator's results contains the 'UnityEngine.Coroutine' type,If occurs an exception,it can't be catched.It is recommended to use 'yield return routine',rather than 'yield return StartCoroutine(routine)'."));

                return true;
            }
            catch (Exception e)
            {
                OnException(e);
                OnFinally();
                return false;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        private void OnException(Exception e)
        {
            try
            {
                if (onException == null)
                    return;

                onException(e);
            }
            catch (Exception) { }
        }

        private void OnFinally()
        {
            try
            {
                if (onFinally == null)
                    return;

                onFinally();
            }
            catch (Exception) { }
            finally
            {
                Free(this);
            }
        }

        private void Clear()
        {
            current = null;
            onException = null;
            onFinally = null;
            hasNext.Clear();
            stack.Clear();
        }

        private bool HasNext()
        {
            if (hasNext.Count > 0)
            {
                foreach (Func<bool> action in hasNext)
                {
                    if (!action())
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Register a condition code block.
        /// </summary>
        /// <param name="hasNext"></param>
        public virtual void RegisterConditionBlock(Func<bool> hasNext)
        {
            if (hasNext != null)
                this.hasNext.Add(hasNext);
        }

        /// <summary>
        /// Register a code block, when an exception occurs it will be executed.
        /// </summary>
        /// <param name="onException"></param>
        public virtual void RegisterCatchBlock(Action<Exception> onException)
        {
            if (onException != null)
                this.onException += onException;
        }

        /// <summary>
        /// Register a code block, when the end of the operation is executed.
        /// </summary>
        /// <param name="onFinally"></param>
        public virtual void RegisterFinallyBlock(Action onFinally)
        {
            if (onFinally != null)
                this.onFinally += onFinally;
        }
    }
}
