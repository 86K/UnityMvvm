

using UnityEngine;
#if NETFX_CORE || NET_STANDARD_2_0 || NET_4_6
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Fusion.Mvvm
{
    public class TaskToCoroutineExample : MonoBehaviour
    {
#if NETFX_CORE || NET_STANDARD_2_0 || NET_4_6
        IEnumerator Start()
        {
            Task task = Task.Run(() =>
            {
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        Thread.Sleep(200);
                    }
                    catch (Exception) { }

                    Debug.LogFormat("Task ThreadId:{0}", Thread.CurrentThread.ManagedThreadId);
                }
            });

            yield return task.AsCoroutine();
            Debug.LogFormat("Task End,Current Thread ID:{0}", Thread.CurrentThread.ManagedThreadId);

            yield return Task.Delay(1000).AsCoroutine();
            Debug.LogFormat("Delay End");

        }
#endif
    }
}
