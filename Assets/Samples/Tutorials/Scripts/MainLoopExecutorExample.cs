using System.Threading;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class MainLoopExecutorExample : MonoBehaviour
	{
		private IMainLoopExecutor executor;

		void Start ()
		{
			executor = new MainLoopExecutor (); 

			Debug.LogFormat ("ThreadID:{0}", Thread.CurrentThread.ManagedThreadId);

			Executors.RunAsync(() => {
			
				executor.RunOnMainThread (Task1, true);

				executor.RunOnMainThread<string> (Task2);

				Debug.LogFormat ("run on the backgound thread. ThreadID:{0}", Thread.CurrentThread.ManagedThreadId);
			});
		}

		void Task1 ()
		{		
			Debug.LogFormat ("This is a task1,run on the main thread. ThreadID:{0}", Thread.CurrentThread.ManagedThreadId);
		}

		string Task2 ()
		{
			Debug.LogFormat ("This is a task2,run on the main thread. ThreadID:{0}", Thread.CurrentThread.ManagedThreadId);
			return name;
		}
	}
}