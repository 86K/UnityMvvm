using System;
using System.Threading;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class InteractionTargetProxy : TargetProxyBase, IObtainable
    {
        private readonly EventHandler<InteractionEventArgs> _handler;
        private readonly IInteractionAction _interactionAction;
        private SendOrPostCallback _postCallback;

        public InteractionTargetProxy(object target, IInteractionAction interactionAction) : base(target)
        {
            _interactionAction = interactionAction;
            _handler = OnRequest;
        }

        public override Type Type => typeof(EventHandler<InteractionEventArgs>);

        public override BindingMode DefaultMode => BindingMode.OneWayToSource;

        public object GetValue()
        {
            return _handler;
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

                _interactionAction.OnRequest(sender, args);
            }
            else
            {
                if (_postCallback == null)
                {
                    _postCallback = state =>
                    {
                        PostArgs postArgs = (PostArgs)state;
                        var target = Target;
                        if (target == null || (target is Behaviour behaviour && !behaviour.isActiveAndEnabled))
                            return;

                        _interactionAction.OnRequest(postArgs.sender, postArgs.args);
                    };
                }

                UISynchronizationContext.Post(_postCallback, new PostArgs(sender, args));
            }
        }

        // NOTE：can use event system to combine 
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