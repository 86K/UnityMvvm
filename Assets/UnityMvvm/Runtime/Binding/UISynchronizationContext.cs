using System.Threading;
using UnityEngine;

namespace Fusion.Mvvm
{
    /// <summary>
    /// UI同步上下文。
    /// 用于在unity的主线程中抛出或者发出事件。
    /// </summary>
    public static class UISynchronizationContext
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnInitialize()
        {
            _context = SynchronizationContext.Current;
            _threadId = Thread.CurrentThread.ManagedThreadId;
        }

        private static int _threadId;
        private static SynchronizationContext _context;

        public static bool InThread => _threadId == Thread.CurrentThread.ManagedThreadId;

        public static void Post(SendOrPostCallback callback, object state)
        {
            if (_threadId == Thread.CurrentThread.ManagedThreadId)
                callback(state);
            else
                _context.Post(callback, state);
        }

        public static void Send(SendOrPostCallback callback, object state)
        {
            if (_threadId == Thread.CurrentThread.ManagedThreadId)
                callback(state);
            else
                _context.Send(callback, state);
        }
    }
}