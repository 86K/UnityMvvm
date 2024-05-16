namespace Fusion.Mvvm
{
    public class BindingServiceBundle : AbstractServiceBundle
    {
        public BindingServiceBundle(IServiceContainer container) : base(container)
        {
        }

        protected override void OnStart(IServiceContainer container)
        {
            PathParser pathParser = new PathParser();
            ExpressionPathFinder expressionPathFinder = new ExpressionPathFinder();
            ConverterRegistry converterRegistry = new ConverterRegistry();

            ObjectSourceProxyFactory objectSourceProxyFactory = new ObjectSourceProxyFactory();
            objectSourceProxyFactory.Register(new UniversalNodeProxyFactory(), 0);

            SourceProxyFactory sourceFactory = new SourceProxyFactory();
            sourceFactory.Register(new LiteralSourceProxyFactory(), 0);
            sourceFactory.Register(new ExpressionSourceProxyFactory(sourceFactory, expressionPathFinder), 1);
            sourceFactory.Register(objectSourceProxyFactory, 2);

            TargetProxyFactory targetFactory = new TargetProxyFactory();
            targetFactory.Register(new UniversalTargetProxyFactory(pathParser), 0);
            targetFactory.Register(new UnityTargetProxyFactory(), 10);
#if UNITY_2019_1_OR_NEWER
            targetFactory.Register(new VisualElementProxyFactory(), 30);
#endif
            
            Binder binder = new Binder(sourceFactory, targetFactory);
            container.Register<IBinder>(binder);
            
            container.Register<IConverterRegistry>(converterRegistry);
            container.Register<IExpressionPathFinder>(expressionPathFinder);
            container.Register<IPathParser>(pathParser);

            container.Register<INodeProxyFactory>(objectSourceProxyFactory);
            container.Register<INodeProxyFactoryRegister>(objectSourceProxyFactory);

            container.Register<ISourceProxyFactory>(sourceFactory);
            container.Register<ISourceProxyFactoryRegistry>(sourceFactory);

            container.Register<ITargetProxyFactory>(targetFactory);
            container.Register<ITargetProxyFactoryRegister>(targetFactory);
        }

        protected override void OnStop(IServiceContainer container)
        {
            container.Unregister<IConverterRegistry>();
            container.Unregister<IExpressionPathFinder>();
            container.Unregister<IPathParser>();

            container.Unregister<INodeProxyFactory>();
            container.Unregister<INodeProxyFactoryRegister>();

            container.Unregister<ISourceProxyFactory>();
            container.Unregister<ISourceProxyFactoryRegistry>();

            container.Unregister<ITargetProxyFactory>();
            container.Unregister<ITargetProxyFactoryRegister>();
        }
    }
}
