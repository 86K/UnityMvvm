

using System.Threading;

namespace Fusion.Mvvm
{
    /// <summary>
    /// 
    /// </summary>
    public class CountDownEvent
    {
        private readonly int max = 1;
        private int count = 0;
        private readonly AutoResetEvent reset = new AutoResetEvent(false);

        public CountDownEvent(int max)
        {
            this.max = max;
            count = 0;
        }

        public bool Set()
        {
            Interlocked.Decrement(ref count);
            return reset.Set();
        }

        public bool Wait()
        {
            if (Interlocked.Increment(ref count) >= max)
                return reset.WaitOne();
            return false;
        }
    }
}

