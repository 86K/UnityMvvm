namespace Fusion.Mvvm
{
    /// <summary>
    /// ApplicationContext
    /// </summary>
    public class ApplicationContext : Context
    {
        private readonly IMainLoopExecutor mainLoopExecutor;

        public ApplicationContext() : this(null, null)
        {
        }

        public ApplicationContext(IServiceContainer container, IMainLoopExecutor mainLoopExecutor) : base(container, null)
        {
            this.mainLoopExecutor = mainLoopExecutor;
            if (this.mainLoopExecutor == null)
                this.mainLoopExecutor = new MainLoopExecutor();
        }

        /// <summary>
        /// Retrieve a executor on the main thread.
        /// </summary>
        /// <returns></returns>
        public virtual IMainLoopExecutor GetMainLoopExcutor()
        {
            return mainLoopExecutor;
        }
    }
}
