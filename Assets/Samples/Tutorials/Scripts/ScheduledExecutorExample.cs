

using UnityEngine;

namespace Fusion.Mvvm
{
#pragma warning disable
    public class ScheduledExecutorExample : MonoBehaviour
    {
        private IScheduledExecutor scheduled;

        void Start()
        {
            //		this.scheduled = new CoroutineScheduledExecutor (); 
            scheduled = new ThreadScheduledExecutor(); 
            scheduled.Start();


            IAsyncResult result = scheduled.ScheduleAtFixedRate(() =>
            {
                Debug.Log("This is a test.");
            }, 1000, 2000);
        }

        void OnDestroy()
        {
            if (scheduled != null)
            {
                scheduled.Stop();
                scheduled = null;
            }
        }

    }
}