using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Fusion.Mvvm
{
    public static class CoroutineAwaiterExtensions
    {
        private static CoroutineAwaiter RunOnCoroutine(IEnumerator routine)
        {
            CoroutineAwaiter awaiter = new CoroutineAwaiter();
            InterceptableEnumerator enumerator = routine is InterceptableEnumerator ? (InterceptableEnumerator)routine : InterceptableEnumerator.Create(routine);
            enumerator.RegisterCatchBlock(e =>
            {
                awaiter.SetResult(e);
            });
            enumerator.RegisterFinallyBlock(() =>
            {
                if (!awaiter.IsCompleted)
                    awaiter.SetResult(null);
            });
            Executors.RunOnCoroutineNoReturn(enumerator);
            return awaiter;
        }

        //private static CoroutineAwaiter<TResult> RunOnCoroutine<TResult>(IEnumerator routine, Func<TResult> getter)
        //{
        //    CoroutineAwaiter<TResult> awaiter = new CoroutineAwaiter<TResult>();
        //    InterceptableEnumerator enumerator = routine is InterceptableEnumerator ? (InterceptableEnumerator)routine : InterceptableEnumerator.Create(routine);
        //    enumerator.RegisterCatchBlock(e =>
        //    {
        //        awaiter.SetResult(default(TResult), e);
        //    });
        //    enumerator.RegisterFinallyBlock(() =>
        //    {
        //        if (!awaiter.IsCompleted)
        //            awaiter.SetResult(getter(), null);
        //    });
        //    Executors.RunOnCoroutineNoReturn(enumerator);
        //    return awaiter;
        //}

        private static IEnumerator DoYieldInstruction(YieldInstruction instruction, CoroutineAwaiter awaiter)
        {
            yield return instruction;
            awaiter.SetResult(null);
        }

        private static IEnumerator DoYieldInstruction(CustomYieldInstruction instruction, CoroutineAwaiter<CustomYieldInstruction> awaiter)
        {
            yield return instruction;
            awaiter.SetResult(instruction, null);
        }

        public static IAwaiter GetAwaiter(this IEnumerator coroutine)
        {
            return RunOnCoroutine(coroutine);
        }

        public static IAwaiter GetAwaiter(this YieldInstruction instruction)
        {
            CoroutineAwaiter awaiter = new CoroutineAwaiter();
            Executors.RunOnCoroutineNoReturn(DoYieldInstruction(instruction, awaiter));
            return awaiter;
        }

        public static IAwaiter GetAwaiter(this WaitForMainThread instruction)
        {
            CoroutineAwaiter awaiter = new CoroutineAwaiter();
            Executors.RunOnMainThread(() =>
            {
                awaiter.SetResult(null);
            });
            return awaiter;
        }

        public static IAwaiter GetAwaiter(this WaitForBackgroundThread instruction)
        {
            CoroutineAwaiter awaiter = new CoroutineAwaiter();
            Executors.RunAsyncNoReturn(() =>
            {
                awaiter.SetResult(null);
            });
            return awaiter;
        }

        public static IAwaiter<CustomYieldInstruction> GetAwaiter(this CustomYieldInstruction instruction)
        {
            CoroutineAwaiter<CustomYieldInstruction> awaiter = new CoroutineAwaiter<CustomYieldInstruction>();
            Executors.RunOnCoroutineNoReturn(DoYieldInstruction(instruction, awaiter));
            return awaiter;
        }

        public static IAwaiter GetAwaiter(this AsyncOperation target)
        {
            return new AsyncOperationAwaiter(target);
        }

        public static IAwaiter<Object> GetAwaiter(this ResourceRequest target)
        {
            return new AsyncOperationAwaiter<ResourceRequest, Object>(target, (request) => request.asset);
        }

        public static IAwaiter<Object> GetAwaiter(this AssetBundleRequest target)
        {
            return new AsyncOperationAwaiter<AssetBundleRequest, Object>(target, (request) => request.asset);
        }

        public static IAwaiter<AssetBundle> GetAwaiter(this AssetBundleCreateRequest target)
        {
            return new AsyncOperationAwaiter<AssetBundleCreateRequest, AssetBundle>(target, (request) => request.assetBundle);
        }

        public static IAwaiter<UnityWebRequest> GetAwaiter(this UnityWebRequestAsyncOperation target)
        {
            return new AsyncOperationAwaiter<UnityWebRequestAsyncOperation, UnityWebRequest>(target, (request) => request.webRequest);
        }

        public static IAwaiter<object> GetAwaiter(this IAsyncResult target)
        {
            return new AsyncResultAwaiter<IAsyncResult>(target);
        }

        public static IAwaiter<TResult> GetAwaiter<TResult>(this IAsyncResult<TResult> target)
        {
            return new AsyncResultAwaiter<IAsyncResult<TResult>, TResult>(target);
        }

        public static IAwaiter<object> GetAwaiter(this AsyncResult target)
        {
            return new AsyncResultAwaiter<IAsyncResult>(target);
        }

        public static IAwaiter<TResult> GetAwaiter<TResult>(this AsyncResult<TResult> target)
        {
            return new AsyncResultAwaiter<IAsyncResult<TResult>, TResult>(target);
        }
    }

    public class WaitForMainThread : CustomYieldInstruction
    {
        public static readonly WaitForMainThread Default = new WaitForMainThread();

        public override bool keepWaiting => false;
    }

    public class WaitForBackgroundThread
    {
        public static readonly WaitForBackgroundThread Default = new WaitForBackgroundThread();
    }
}