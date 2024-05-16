

using UnityEngine;
#if NETFX_CORE || NET_STANDARD_2_0 || NET_4_6
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;
#endif

namespace Fusion.Mvvm
{
    public class AsyncAndAwaitExample : MonoBehaviour
    {
#if NETFX_CORE || NET_STANDARD_2_0 || NET_4_6
        async void Start()
        {
            await new WaitForSeconds(2f);
            Debug.Log("WaitForSeconds  End");

            await Task.Delay(1000);
            Debug.Log("Delay  End");

            UnityWebRequest www = await UnityWebRequest.Get("http://www.baidu.com").SendWebRequest();

            if (!www.isHttpError && !www.isNetworkError)
                Debug.Log(www.downloadHandler.text);

            int result = await Calculate();
            Debug.LogFormat("Calculate Result = {0} Calculate Task End,Current Thread ID:{1}", result, Thread.CurrentThread.ManagedThreadId);

            await new WaitForMainThread();
            Debug.LogFormat("Switch to the main thread,Current Thread ID:{0}", Thread.CurrentThread.ManagedThreadId);

            await new WaitForSecondsRealtime(1f);
            Debug.Log("WaitForSecondsRealtime  End");

            await DoTask(5);
            Debug.Log("DoTask End");
        }

        IAsyncResult<int> Calculate()
        {
            return Executors.RunAsync<int>(() =>
            {
                Debug.LogFormat("Calculate Task ThreadId:{0}", Thread.CurrentThread.ManagedThreadId);
                int total = 0;
                for (int i = 0; i < 20; i++)
                {
                    total += i;
                    try
                    {
                        Thread.Sleep(100);
                    }
                    catch (Exception) { }
                }
                return total;
            });
        }

        IEnumerator DoTask(int n)
        {
            yield return new WaitForSeconds(1f);

            for (int i = 0; i < n; i++)
            {
                yield return null;
            }
        }
#endif
    }
}
