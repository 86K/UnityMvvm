using System;
using System.Threading;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class InteractionTargetProxy : TargetProxyBase, IObtainable
    {
        private readonly EventHandler<InteractionEventArgs> handler;
        private readonly IInteractionAction interactionAction;
        private SendOrPostCallback postCallback;
        public InteractionTargetProxy(object target, IInteractionAction interactionAction) : base(target)
        {
            this.interactionAction = interactionAction;
            handler = OnRequest;
        }

        public override Type Type => typeof(EventHandler<InteractionEventArgs>);

        public override BindingMode DefaultMode => BindingMode.OneWayToSource;

        public object GetValue()
        {
            return handler;
        }

        public TValue GetValue<TValue>()
        {
            return (TValue)GetValue();
        }

        private void OnRequest(object sender, InteractionEventArgs args)
        {
            if (UISynchronizationContext.InThread)
            {
                var target = Target;
                if (target == null || (target is Behaviour behaviour && !behaviour.isActiveAndEnabled))
                    return;

                interactionAction.OnRequest(sender, args);
            }
            else
            {
                if (postCallback == null)
                {
                    postCallback = state =>
                    {
                        PostArgs postArgs = (PostArgs)state;
                        var target = Target;
                        if (target == null || (target is Behaviour behaviour && !behaviour.isActiveAndEnabled))
                            return;

                        interactionAction.OnRequest(postArgs.sender, postArgs.args);
                    };
                }
                UISynchronizationContext.Post(postCallback, new PostArgs(sender, args));
            }
        }

        class PostArgs
        {
            public PostArgs(object sender, InteractionEventArgs args)
            {
                this.sender = sender;
                this.args = args;
            }

            public readonly object sender;
            public readonly InteractionEventArgs args;
        }
    }
}
