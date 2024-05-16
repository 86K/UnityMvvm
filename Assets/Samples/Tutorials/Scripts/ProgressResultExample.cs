

using UnityEngine;
using System.Collections;
using System.Text;

namespace Fusion.Mvvm
{
	public class Progress
	{
		public int bytes;
		public int TotalBytes;

		public int Percentage => (bytes * 100) / TotalBytes;
	}


	public class ProgressResultExample : MonoBehaviour
	{

		protected IEnumerator Start ()
		{
			ProgressResult<Progress,string> result = new ProgressResult<Progress,string> (true);

			
			StartCoroutine (DoTask (result));

			while (!result.IsDone) {
				Debug.LogFormat ("Percentage: {0}% ", result.Progress.Percentage);
				yield return null;
			}

			Debug.LogFormat ("IsDone:{0} Result:{1}", result.IsDone, result.Result);
		}

		/// <summary>
		/// Simulate a task.
		/// </summary>
		/// <returns>The task.</returns>
		/// <param name="promise">Promise.</param>
		protected IEnumerator DoTask (IProgressPromise<Progress,string> promise)
		{
			int n = 50;
			Progress progress = new Progress ();
			progress.TotalBytes = n;
			progress.bytes = 0;
			StringBuilder buf = new  StringBuilder ();
			for (int i = 0; i < n; i++) {
				
				if (promise.IsCancellationRequested) {		
					promise.SetCancelled ();
					yield break;
				}

				progress.bytes += 1;
				buf.Append (" ").Append (i);
				promise.UpdateProgress (progress);
				yield return new WaitForSeconds (0.01f);
			}
			promise.SetResult (buf.ToString ()); 
		}

	}
}
