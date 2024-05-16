﻿/*
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
using UnityEngine;

namespace Loxodon.Framework.Asynchronous
{
    public interface ICallbackable
    {
        /// <summary>
        /// Called when the task is finished.
        /// </summary>
        /// <param name="callback"></param>
        void OnCallback(Action<IAsyncResult> callback);
    }

    public interface ICallbackable<TResult>
    {
        /// <summary>
        /// Called when the task is finished.
        /// </summary>
        /// <param name="callback"></param>
        void OnCallback(Action<IAsyncResult<TResult>> callback);
    }

    public interface IProgressCallbackable<TProgress>
    {
        /// <summary>
        /// Called when the task is finished.
        /// </summary>
        /// <param name="callback"></param>
        void OnCallback(Action<IProgressResult<TProgress>> callback);

        /// <summary>
        /// Called when the progress update.
        /// </summary>
        /// <param name="callback"></param>
        void OnProgressCallback(Action<TProgress> callback);
    }

    public interface IProgressCallbackable<TProgress, TResult>
    {
        /// <summary>
        /// Called when the task is finished.
        /// </summary>
        /// <param name="callback"></param>
        void OnCallback(Action<IProgressResult<TProgress, TResult>> callback);

        /// <summary>
        /// Called when the progress update.
        /// </summary>
        /// <param name="callback"></param>
        void OnProgressCallback(Action<TProgress> callback);
    }

    internal class Callbackable : ICallbackable
    {
        private readonly IAsyncResult result;
        private readonly object _lock = new object();
        private Action<IAsyncResult> callback;
        public Callbackable(IAsyncResult result)
        {
            this.result = result;
        }

        public void RaiseOnCallback()
        {
            lock (_lock)
            {
                try
                {
                    if (callback == null)
                        return;

                    var list = callback.GetInvocationList();
                    callback = null;

                    foreach (Action<IAsyncResult> action in list)
                    {
                        try
                        {
                            action(result);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Class[{GetType()}] callback exception.Error:{e}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Class[{GetType()}] callback exception.Error:{e}");
                }
            }
        }

        public void OnCallback(Action<IAsyncResult> callback)
        {
            lock (_lock)
            {
                if (callback == null)
                    return;

                if (result.IsDone)
                {
                    try
                    {
                        callback(result);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Class[{GetType()}] callback exception.Error:{e}");
                    }
                    return;
                }

                this.callback += callback;
            }
        }
    }

    internal class Callbackable<TResult> : ICallbackable<TResult>
    {
        private readonly IAsyncResult<TResult> result;
        private readonly object _lock = new object();
        private Action<IAsyncResult<TResult>> callback;
        public Callbackable(IAsyncResult<TResult> result)
        {
            this.result = result;
        }

        public void RaiseOnCallback()
        {
            lock (_lock)
            {
                try
                {
                    if (callback == null)
                        return;

                    var list = callback.GetInvocationList();
                    callback = null;

                    foreach (Action<IAsyncResult<TResult>> action in list)
                    {
                        try
                        {
                            action(result);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Class[{GetType()}] callback exception.Error:{e}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Class[{GetType()}] callback exception.Error:{e}");
                }
            }
        }

        public void OnCallback(Action<IAsyncResult<TResult>> callback)
        {
            lock (_lock)
            {
                if (callback == null)
                    return;

                if (result.IsDone)
                {
                    try
                    {
                        callback(result);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Class[{GetType()}] callback exception.Error:{e}");
                    }
                    return;
                }

                this.callback += callback;
            }
        }
    }

    internal class ProgressCallbackable<TProgress> : IProgressCallbackable<TProgress>
    {
        private readonly IProgressResult<TProgress> result;
        private readonly object _lock = new object();
        private Action<IProgressResult<TProgress>> callback;
        private Action<TProgress> progressCallback;
        public ProgressCallbackable(IProgressResult<TProgress> result)
        {
            this.result = result;
        }

        public void RaiseOnCallback()
        {
            lock (_lock)
            {
                try
                {
                    if (callback == null)
                        return;

                    var list = callback.GetInvocationList();
                    callback = null;

                    foreach (Action<IProgressResult<TProgress>> action in list)
                    {
                        try
                        {
                            action(result);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Class[{GetType()}] callback exception.Error:{e}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Class[{GetType()}] callback exception.Error:{e}");
                }
                finally
                {
                    progressCallback = null;
                }
            }
        }

        public void RaiseOnProgressCallback(TProgress progress)
        {
            lock (_lock)
            {
                try
                {
                    if (progressCallback == null)
                        return;

                    var list = progressCallback.GetInvocationList();
                    foreach (Action<TProgress> action in list)
                    {
                        try
                        {
                            action(progress);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Class[{GetType()}] progress callback exception.Error:{e}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Class[{GetType()}] progress callback exception.Error:{e}");
                }
            }
        }

        public void OnCallback(Action<IProgressResult<TProgress>> callback)
        {
            lock (_lock)
            {
                if (callback == null)
                    return;

                if (result.IsDone)
                {
                    try
                    {
                        callback(result);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Class[{GetType()}] callback exception.Error:{e}");
                    }
                    return;
                }

                this.callback += callback;
            }
        }

        public void OnProgressCallback(Action<TProgress> callback)
        {
            lock (_lock)
            {
                if (callback == null)
                    return;

                if (result.IsDone)
                {
                    try
                    {
                        callback(result.Progress);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Class[{GetType()}] progress callback exception.Error:{e}");
                    }
                    return;
                }

                progressCallback += callback;
            }
        }
    }

    internal class ProgressCallbackable<TProgress, TResult> : IProgressCallbackable<TProgress, TResult>
    {
        private readonly IProgressResult<TProgress, TResult> result;
        private readonly object _lock = new object();
        private Action<IProgressResult<TProgress, TResult>> callback;
        private Action<TProgress> progressCallback;
        public ProgressCallbackable(IProgressResult<TProgress, TResult> result)
        {
            this.result = result;
        }

        public void RaiseOnCallback()
        {
            lock (_lock)
            {
                try
                {
                    if (callback == null)
                        return;

                    var list = callback.GetInvocationList();
                    callback = null;

                    foreach (Action<IProgressResult<TProgress, TResult>> action in list)
                    {
                        try
                        {
                            action(result);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Class[{GetType()}] callback exception.Error:{e}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Class[{GetType()}] callback exception.Error:{e}");
                }
                finally
                {
                    progressCallback = null;
                }
            }
        }

        public void RaiseOnProgressCallback(TProgress progress)
        {
            lock (_lock)
            {
                try
                {
                    if (progressCallback == null)
                        return;

                    var list = progressCallback.GetInvocationList();
                    foreach (Action<TProgress> action in list)
                    {
                        try
                        {
                            action(progress);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Class[{GetType()}] progress callback exception.Error:{e}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Class[{GetType()}] progress callback exception.Error:{e}");
                }
            }
        }

        public void OnCallback(Action<IProgressResult<TProgress, TResult>> callback)
        {
            lock (_lock)
            {
                if (callback == null)
                    return;

                if (result.IsDone)
                {
                    try
                    {
                        callback(result);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Class[{GetType()}] callback exception.Error:{e}");
                    }
                    return;
                }

                this.callback += callback;
            }
        }

        public void OnProgressCallback(Action<TProgress> callback)
        {
            lock (_lock)
            {
                if (callback == null)
                    return;

                if (result.IsDone)
                {
                    try
                    {
                        callback(result.Progress);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Class[{GetType()}] progress callback exception.Error:{e}");
                    }
                    return;
                }

                progressCallback += callback;
            }
        }
    }
}
