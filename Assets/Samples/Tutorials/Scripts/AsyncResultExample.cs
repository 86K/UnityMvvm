

using UnityEngine;
using System.Collections;

namespace Fusion.Mvvm
{

	public class AsyncResultExample : MonoBehaviour
	{

		protected IEnumerator Start ()
		{
			AsyncResult result = new AsyncResult (true);

			
			StartCoroutine (DoTask (result));

			
			StartCoroutine (DoCancel (result));
	
			
			yield return result.WaitForDone ();

            //		while (!result.IsDone)
            //			yield return null;

            Debug.LogFormat ("IsDone:{0} IsCanceled:{1} Exception:{2}", result.IsDone, result.IsCancelled, result.Exception);
		}

		/// <summary>
		/// Simulate a task.
		/// </summary>
		/// <returns>The task.</returns>
		/// <param name="promise">Promise.</param>
		protected IEnumerator DoTask (IPromise promise)
		{
			for (int i = 0; i < 20; i++) {
				
				if (promise.IsCancellationRequested) {		
					promise.SetCancelled ();
					yield break;
				}

				Debug.LogFormat ("i = {0}", i);
				yield return new WaitForSeconds (0.5f);
			}

			promise.SetResult ();
		}

		/// <summary>
		/// Cancel a task.
		/// </summary>
		/// <returns>The cancel.</returns>
		/// <param name="result">Result.</param>
		protected IEnumerator DoCancel (IAsyncResult result)
		{
			yield return new WaitForSeconds (3f);
			result.Cancel ();
		}
	}

}