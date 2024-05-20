namespace Fusion.Mvvm
{
    public abstract class ExecutorBase
    {
        static ExecutorBase()
        {
            Executors.Create();
        }
    }
}