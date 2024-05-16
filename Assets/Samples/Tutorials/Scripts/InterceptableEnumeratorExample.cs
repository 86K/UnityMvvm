

using UnityEngine;
using System.Collections;

namespace Fusion.Mvvm
{
    public class InterceptableEnumeratorExample : MonoBehaviour
    {
        IEnumerator Start()
        {
            yield return TestInterceptException();

            //yield return this.TestInterceptMoveNextMethod();
        }

        protected IEnumerator TestInterceptException()
        {
            ProgressResult<float, bool> result = new ProgressResult<float, bool>(true);

            
            result.Callbackable().OnProgressCallback(p => Debug.LogFormat("Progress:{0}%", p * 100));

            result.Callbackable().OnCallback((r) =>
            {
                Debug.LogFormat("The task is finished. IsCancelled:{0} Result:{1} Exception:{2}", r.IsCancelled, r.Result, r.Exception);
            });

            InterceptableEnumerator routine = InterceptableEnumerator.Create(DoTask(result));

            routine.RegisterCatchBlock((e) =>
            {
                Debug.LogError(e);
            });

            routine.RegisterFinallyBlock(() =>
            {
                Debug.Log("this is a finally block.");
            });

            
            StartCoroutine(routine);
            yield break;
        }

        protected IEnumerator TestInterceptMoveNextMethod()
        {
            ProgressResult<float, bool> result = new ProgressResult<float, bool>(true);

            
            result.Callbackable().OnProgressCallback(p => Debug.LogFormat("Progress:{0}%", p * 100));

            result.Callbackable().OnCallback((r) =>
            {
                Debug.LogFormat("The task is finished. IsCancelled:{0} Result:{1} Exception:{2}", r.IsCancelled, r.Result, r.Exception);
            });

            InterceptableEnumerator routine = InterceptableEnumerator.Create(DoTask(result));

            
            routine.RegisterConditionBlock(() => !(result.IsCancellationRequested));

            routine.RegisterFinallyBlock(() =>
            {
                Debug.Log("this is a finally block.");
            });

            
            StartCoroutine(routine);

            yield return new WaitForSeconds(0.5f);
            result.Cancel();
        }


        /// <summary>
        /// Simulate a task.
        /// </summary>
        /// <returns>The task.</returns>
        /// <param name="promise">Promise.</param>
        protected IEnumerator DoTask(IProgressPromise<float, bool> promise)
        {
            int n = 50;
            for (int i = 0; i < n; i++)
            {
                promise.UpdateProgress((float)i / n);
                yield return new WaitForSeconds(0.1f);
                if (i == 20)
                    throw new System.Exception("This is a test, not a bug.");
            }
            promise.SetResult(true);
        }
    }
}
