

using UnityEngine;
using System.Collections;

namespace Fusion.Mvvm
{
    public class AsyncTaskExample2 : MonoBehaviour
    {
        protected IEnumerator Start()
        {
            AsyncTask<int> task = new AsyncTask<int>(new System.Func<IPromise<int>, IEnumerator>(DoTask), true);

            
            task.OnPreExecute(() =>
            {
                Debug.Log("The task has started.");
            }).OnPostExecute((result) =>
            {
                Debug.LogFormat("The task has completed.result={0}", result);
            }).OnError((e) =>
            {
                Debug.LogFormat("An error occurred:{0}", e);
            }).OnFinish(() =>
            {
                Debug.Log("The task has been finished.");
            }).Start();

            //		
            StartCoroutine(DoCancel(task));

            
            yield return task.WaitForDone();

            Debug.LogFormat("IsDone:{0} IsCanceled:{1} Exception:{2} Result:{3}", task.IsDone, task.IsCancelled, task.Exception, task.Result);
        }

        /// <summary>
        /// Simulate a task.
        /// </summary>
        /// <returns>The task.</returns>
        /// <param name="promise">Promise.</param>
        protected IEnumerator DoTask(IPromise<int> promise)
        {
            int n = 10;
            for (int i = 0; i < n; i++)
            {
                
                if (promise.IsCancellationRequested)
                {
                    promise.SetCancelled();
                    yield break;
                }

                Debug.LogFormat("i = {0}", i);
                yield return new WaitForSeconds(0.5f);
            }

            promise.SetResult(n);
        }

        /// <summary>
        /// Cancel a task.
        /// </summary>
        /// <returns>The cancel.</returns>
        /// <param name="result">Result.</param>
        protected IEnumerator DoCancel(IAsyncResult result)
        {
            yield return new WaitForSeconds(3f);
            result.Cancel();
        }

    }
}