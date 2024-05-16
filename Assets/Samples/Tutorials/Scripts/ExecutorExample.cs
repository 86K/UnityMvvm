

using System.Collections;
using UnityEngine;
#if NETFX_CORE
using System.Threading.Tasks;
#endif
namespace Fusion.Mvvm
{
    public class ExecutorExample : MonoBehaviour
    {

        IEnumerator Start()
        {
            Executors.RunAsync(() =>
            {
                Debug.LogFormat("RunAsync ");
            });


            Executors.RunAsync(() =>
            {
#if NETFX_CORE
            Task.Delay(1000).Wait();
#endif
            Executors.RunOnMainThread(() =>
                {
                    Debug.LogFormat("RunOnMainThread Time:{0} frame:{1}", Time.time, Time.frameCount);
                }, true);
            });

            Executors.RunOnMainThread(() =>
            {
                Debug.LogFormat("RunOnMainThread 2 Time:{0} frame:{1}", Time.time, Time.frameCount);
            }, false);

            IAsyncResult result = Executors.RunOnCoroutine(DoRun());

            yield return result.WaitForDone();

            Debug.LogFormat("============finished=============");

        }

        IEnumerator DoRun()
        {
            for (int i = 0; i < 10; i++)
            {
                Debug.LogFormat("i = {0}", i);
                yield return null;
            }
        }
    }
}
