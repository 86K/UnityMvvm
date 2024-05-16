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
using System.Threading;
using System.Collections;

#if NETFX_CORE
using System.Threading.Tasks;
#endif

using Loxodon.Framework.Execution;
using UnityEngine;

namespace Loxodon.Framework.Asynchronous
{
    [Obsolete("This type will be removed in version 3.0")]
    public class ProgressTask<TProgress> : IProgressTask<TProgress>
    {
        private readonly Action action;

        private Action preCallbackOnMainThread;
        private Action preCallbackOnWorkerThread;

        private Action postCallbackOnMainThread;
        private Action postCallbackOnWorkerThread;

        private Action<TProgress> progressCallbackOnMainThread;
        private Action<TProgress> progressCallbackOnWorkerThread;

        private Action<Exception> errorCallbackOnMainThread;
        private Action<Exception> errorCallbackOnWorkerThread;

        private Action finishCallbackOnMainThread;
        private Action finishCallbackOnWorkerThread;

        private int running = 0;
        private readonly ProgressResult<TProgress> result;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <param name="runOnMainThread"></param>
        /// <param name="cancelable"></param>
        public ProgressTask(Action<IProgressPromise<TProgress>> task, bool runOnMainThread = false, bool cancelable = false)
        {
            if (task == null)
                throw new ArgumentNullException();

            result = new ProgressResult<TProgress>(!runOnMainThread && cancelable);
            result.Callbackable().OnProgressCallback(OnProgressChanged);
            if (runOnMainThread)
            {
                action = WrapAction(() =>
                {
                    Executors.RunOnMainThread(() => task(result), true);
                    result.Synchronized().WaitForResult();
                });
            }
            else
            {
                action = WrapAction(() =>
                {
                    task(result);
                    result.Synchronized().WaitForResult();
                });
            }
        }

        /// <summary>
        /// run on main thread.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="cancelable"></param>
        public ProgressTask(Func<IProgressPromise<TProgress>, IEnumerator> task, bool cancelable = false)
        {
            if (task == null)
                throw new ArgumentNullException();

            result = new ProgressResult<TProgress>(cancelable);
            result.Callbackable().OnProgressCallback(OnProgressChanged);
            action = WrapAction(() =>
            {
                Executors.RunOnCoroutine(task(result), result);
                result.Synchronized().WaitForResult();
            });
        }

        public virtual object Result => result.Result;

        public virtual Exception Exception => result.Exception;

        public virtual bool IsDone => result.IsDone;

        public virtual bool IsCancelled => result.IsCancelled;

        public virtual TProgress Progress => result.Progress;

        protected virtual Action WrapAction(Action action)
        {
            Action wrapAction = () =>
            {
                try
                {
                    try
                    {
                        if (preCallbackOnWorkerThread != null)
                            preCallbackOnWorkerThread();
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"{e}");
                    }

                    if (result.IsCancellationRequested)
                    {
                        result.SetCancelled();
                        return;
                    }

                    action();
                }
                catch (Exception e)
                {
                    result.SetException(e);
                }
                finally
                {
                    try
                    {
                        if (Exception != null)
                        {
                            if (errorCallbackOnMainThread != null)
                                Executors.RunOnMainThread(() => errorCallbackOnMainThread(Exception), true);

                            if (errorCallbackOnWorkerThread != null)
                                errorCallbackOnWorkerThread(Exception);
                        }
                        else
                        {
                            if (postCallbackOnMainThread != null)
                                Executors.RunOnMainThread(postCallbackOnMainThread, true);

                            if (postCallbackOnWorkerThread != null)
                                postCallbackOnWorkerThread();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"{e}");
                    }

                    try
                    {
                        if (finishCallbackOnMainThread != null)
                            Executors.RunOnMainThread(finishCallbackOnMainThread, true);

                        if (finishCallbackOnWorkerThread != null)
                            finishCallbackOnWorkerThread();
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"{e}");
                    }

                    Interlocked.Exchange(ref running, 0);
                }
            };
            return wrapAction;
        }

        protected virtual IEnumerator DoUpdateProgressOnMainThread()
        {
            while (!result.IsDone)
            {
                try
                {
                    if (progressCallbackOnMainThread != null)
                        progressCallbackOnMainThread(result.Progress);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"{e}");
                }
                yield return null;
            }
        }

        protected virtual void OnProgressChanged(TProgress progress)
        {
            try
            {
                if (result.IsDone || progressCallbackOnWorkerThread == null)
                    return;

                progressCallbackOnWorkerThread(progress);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
        }

        public virtual bool Cancel()
        {
            return result.Cancel();
        }

        public virtual IProgressCallbackable<TProgress> Callbackable()
        {
            return result.Callbackable();
        }

        ICallbackable IAsyncResult.Callbackable()
        {
            return (result as IAsyncResult).Callbackable();
        }

        public virtual ISynchronizable Synchronized()
        {
            return result.Synchronized();
        }

        public virtual object WaitForDone()
        {
            return Executors.WaitWhile(() => !IsDone);
        }

        public IProgressTask<TProgress> OnPreExecute(Action callback, bool runOnMainThread = true)
        {
            if (runOnMainThread)
                preCallbackOnMainThread += callback;
            else
                preCallbackOnWorkerThread += callback;
            return this;
        }

        public IProgressTask<TProgress> OnPostExecute(Action callback, bool runOnMainThread = true)
        {
            if (runOnMainThread)
                postCallbackOnMainThread += callback;
            else
                postCallbackOnWorkerThread += callback;
            return this;
        }

        public IProgressTask<TProgress> OnError(Action<Exception> callback, bool runOnMainThread = true)
        {
            if (runOnMainThread)
                errorCallbackOnMainThread += callback;
            else
                errorCallbackOnWorkerThread += callback;
            return this;
        }

        public IProgressTask<TProgress> OnProgressUpdate(Action<TProgress> callback, bool runOnMainThread = true)
        {
            if (runOnMainThread)
                progressCallbackOnMainThread += callback;
            else
                progressCallbackOnWorkerThread += callback;
            return this;
        }

        public IProgressTask<TProgress> OnFinish(Action callback, bool runOnMainThread = true)
        {
            if (runOnMainThread)
                finishCallbackOnMainThread += callback;
            else
                finishCallbackOnWorkerThread += callback;
            return this;
        }

        public IProgressTask<TProgress> Start(int delay)
        {
            if (delay <= 0)
                return Start();

            Executors.RunAsyncNoReturn(() =>
            {
#if NETFX_CORE
                Task.Delay(delay).Wait();
#else
                Thread.Sleep(delay);
#endif
                if (IsDone || running == 1)
                    return;

                Start();
            });
            return this;
        }

        public IProgressTask<TProgress> Start()
        {
            if (IsDone)
            {
                Debug.Log("The task has been done!");
                return this;
            }

            if (Interlocked.CompareExchange(ref running, 1, 0) == 1)
            {
                Debug.Log("The task is running!");
                return this;
            }

            try
            {
                if (preCallbackOnMainThread != null)
                    Executors.RunOnMainThread(preCallbackOnMainThread, true);

                if (progressCallbackOnMainThread != null)
                    Executors.RunOnCoroutineNoReturn(DoUpdateProgressOnMainThread());
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            Executors.RunAsync(action);

            return this;
        }


    }

    [Obsolete("This type will be removed in version 3.0")]
    public class ProgressTask<TProgress, TResult> : IProgressTask<TProgress, TResult>
    {
        private readonly Action action;

        private Action preCallbackOnMainThread;
        private Action preCallbackOnWorkerThread;

        private Action<TResult> postCallbackOnMainThread;
        private Action<TResult> postCallbackOnWorkerThread;

        private Action<TProgress> progressCallbackOnMainThread;
        private Action<TProgress> progressCallbackOnWorkerThread;

        private Action<Exception> errorCallbackOnMainThread;
        private Action<Exception> errorCallbackOnWorkerThread;

        private Action finishCallbackOnMainThread;
        private Action finishCallbackOnWorkerThread;

        private int running = 0;
        private readonly ProgressResult<TProgress, TResult> result;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <param name="runOnMainThread"></param>
        /// <param name="cancelable"></param>
        public ProgressTask(Action<IProgressPromise<TProgress, TResult>> task, bool runOnMainThread, bool cancelable = false)
        {
            if (task == null)
                throw new ArgumentNullException();

            result = new ProgressResult<TProgress, TResult>(!runOnMainThread && cancelable);
            result.Callbackable().OnProgressCallback(OnProgressChanged);

            if (runOnMainThread)
            {
                action = WrapAction(() =>
                {
                    Executors.RunOnMainThread(() => task(result), true);
                    return result.Synchronized().WaitForResult();
                });
            }
            else
            {
                action = WrapAction(() =>
                {
                    task(result);
                    return result.Synchronized().WaitForResult();
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <param name="cancelable"></param>
        public ProgressTask(Func<IProgressPromise<TProgress, TResult>, IEnumerator> task, bool cancelable = false)
        {
            if (task == null)
                throw new ArgumentNullException();

            result = new ProgressResult<TProgress, TResult>(cancelable);
            result.Callbackable().OnProgressCallback(OnProgressChanged);
            action = WrapAction(() =>
            {
                Executors.RunOnCoroutine(task(result), result);
                return result.Synchronized().WaitForResult();
            });
        }

        public virtual TResult Result => result.Result;

        object IAsyncResult.Result => result.Result;

        public virtual Exception Exception => result.Exception;

        public virtual bool IsDone => result.IsDone;

        public virtual bool IsCancelled => result.IsCancelled;

        public virtual TProgress Progress => result.Progress;

        protected virtual Action WrapAction(Func<TResult> action)
        {
            Action wrapAction = () =>
            {
                try
                {
                    try
                    {
                        if (preCallbackOnWorkerThread != null)
                            preCallbackOnWorkerThread();

                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"{e}");
                    }

                    if (result.IsCancellationRequested)
                    {
                        result.SetCancelled();
                        return;
                    }

                    TResult obj = action();
                    result.SetResult(obj);
                }
                catch (Exception e)
                {
                    result.SetException(e);
                }
                finally
                {
                    try
                    {
                        if (Exception != null)
                        {
                            if (errorCallbackOnMainThread != null)
                                Executors.RunOnMainThread(() => errorCallbackOnMainThread(Exception), true);

                            if (errorCallbackOnWorkerThread != null)
                                errorCallbackOnWorkerThread(Exception);
                        }
                        else
                        {
                            if (postCallbackOnMainThread != null)
                                Executors.RunOnMainThread(() => postCallbackOnMainThread(Result), true);

                            if (postCallbackOnWorkerThread != null)
                                postCallbackOnWorkerThread(Result);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"{e}");
                    }

                    try
                    {
                        if (finishCallbackOnMainThread != null)
                            Executors.RunOnMainThread(finishCallbackOnMainThread, true);

                        if (finishCallbackOnWorkerThread != null)
                            finishCallbackOnWorkerThread();
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(string.Format("{0}", e));
                    }

                    Interlocked.Exchange(ref running, 0);
                }
            };
            return wrapAction;
        }

        protected virtual IEnumerator DoUpdateProgressOnMainThread()
        {
            while (!result.IsDone)
            {
                try
                {
                    if (progressCallbackOnMainThread != null)
                        progressCallbackOnMainThread(result.Progress);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(string.Format("{0}", e));
                }

                yield return null;
            }
        }

        protected virtual void OnProgressChanged(TProgress progress)
        {
            try
            {
                if (result.IsDone || progressCallbackOnWorkerThread == null)
                    return;

                progressCallbackOnWorkerThread(progress);
            }
            catch (Exception e)
            {
                Debug.LogWarning(string.Format("{0}", e));
            }
        }

        public virtual bool Cancel()
        {
            return result.Cancel();
        }

        public virtual IProgressCallbackable<TProgress, TResult> Callbackable()
        {
            return result.Callbackable();
        }

        public virtual ISynchronizable<TResult> Synchronized()
        {
            return result.Synchronized();
        }

        ICallbackable IAsyncResult.Callbackable()
        {
            return (result as IAsyncResult).Callbackable();
        }

        ICallbackable<TResult> IAsyncResult<TResult>.Callbackable()
        {
            return (result as IAsyncResult<TResult>).Callbackable();
        }

        IProgressCallbackable<TProgress> IProgressResult<TProgress>.Callbackable()
        {
            return (result as IProgressResult<TProgress>).Callbackable();
        }

        ISynchronizable IAsyncResult.Synchronized()
        {
            return (result as IAsyncResult).Synchronized();
        }

        public virtual object WaitForDone()
        {
            return Executors.WaitWhile(() => !IsDone);
        }

        public IProgressTask<TProgress, TResult> OnPreExecute(Action callback, bool runOnMainThread = true)
        {
            if (runOnMainThread)
                preCallbackOnMainThread += callback;
            else
                preCallbackOnWorkerThread += callback;
            return this;
        }

        public IProgressTask<TProgress, TResult> OnPostExecute(Action<TResult> callback, bool runOnMainThread = true)
        {
            if (runOnMainThread)
                postCallbackOnMainThread += callback;
            else
                postCallbackOnWorkerThread += callback;
            return this;
        }

        public IProgressTask<TProgress, TResult> OnError(Action<Exception> callback, bool runOnMainThread = true)
        {
            if (runOnMainThread)
                errorCallbackOnMainThread += callback;
            else
                errorCallbackOnWorkerThread += callback;
            return this;
        }

        public IProgressTask<TProgress, TResult> OnProgressUpdate(Action<TProgress> callback, bool runOnMainThread = true)
        {
            if (runOnMainThread)
                progressCallbackOnMainThread += callback;
            else
                progressCallbackOnWorkerThread += callback;
            return this;
        }

        public IProgressTask<TProgress, TResult> OnFinish(Action callback, bool runOnMainThread = true)
        {
            if (runOnMainThread)
                finishCallbackOnMainThread += callback;
            else
                finishCallbackOnWorkerThread += callback;
            return this;
        }

        public IProgressTask<TProgress, TResult> Start(int delay)
        {
            if (delay <= 0)
                return Start();

            Executors.RunAsyncNoReturn(() =>
            {
#if NETFX_CORE
                Task.Delay(delay).Wait();
#else
                Thread.Sleep(delay);
#endif
                if (IsDone || running == 1)
                    return;

                Start();
            });
            return this;
        }

        public IProgressTask<TProgress, TResult> Start()
        {
            if (IsDone)
            {
                Debug.LogWarning(string.Format("The task has been done!"));

                return this;
            }

            if (Interlocked.CompareExchange(ref running, 1, 0) == 1)
            {
                Debug.LogWarning(string.Format("The task is running!"));

                return this;
            }

            try
            {
                if (preCallbackOnMainThread != null)
                    Executors.RunOnMainThread(preCallbackOnMainThread, true);

                if (progressCallbackOnMainThread != null)
                    Executors.RunOnCoroutineNoReturn(DoUpdateProgressOnMainThread());
            }
            catch (Exception e)
            {
                Debug.LogWarning(string.Format("{0}", e));
            }

            Executors.RunAsync(action);
            return this;
        }
    }
}
