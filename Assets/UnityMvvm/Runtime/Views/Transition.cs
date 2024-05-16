

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Fusion.Mvvm
{
    public abstract class Transition : ITransition
    {
        private IManageable window;
        private bool done;
        private bool animationDisabled;
        private int layer;
        private Func<IWindow, IWindow, ActionType> overlayPolicy;

        private bool running;

        //bind the StateChange event.
        private bool bound;
        private Action onStart;
        private Action<IWindow, WindowState> onStateChanged;
        private Action onFinish;

        public Transition(IManageable window)
        {
            this.window = window;
        }

        ~Transition()
        {
            Unbind();
        }

        protected virtual void Bind()
        {
            if (bound)
                return;

            bound = true;
            if (window != null)
                window.StateChanged += StateChanged;
        }

        protected virtual void Unbind()
        {
            if (!bound)
                return;

            bound = false;

            if (window != null)
                window.StateChanged -= StateChanged;
        }

        public virtual IManageable Window
        {
            get => window;
            set => window = value;
        }

        public virtual bool IsDone
        {
            get => done;
            protected set => done = value;
        }

        public virtual object WaitForDone()
        {
            return Executors.WaitWhile(() => !IsDone);
        }

        public virtual bool AnimationDisabled
        {
            get => animationDisabled;
            protected set => animationDisabled = value;
        }

        public virtual int Layer
        {
            get => layer;
            protected set => layer = value;
        }

        public virtual Func<IWindow, IWindow, ActionType> OverlayPolicy
        {
            get => overlayPolicy;
            protected set => overlayPolicy = value;
        }

        protected void StateChanged(object sender, WindowStateEventArgs e)
        {
            RaiseStateChanged((IWindow)sender, e.State);
        }

        protected virtual void RaiseStart()
        {
            try
            {
                if (onStart != null)
                    onStart();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        protected virtual void RaiseStateChanged(IWindow window, WindowState state)
        {
            try
            {
                if (onStateChanged != null)
                    onStateChanged(window, state);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        protected virtual void RaiseFinished()
        {
            try
            {
                if (onFinish != null)
                    onFinish();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        protected virtual void OnStart()
        {
            Bind();
            RaiseStart();
        }

        protected virtual void OnEnd()
        {
            done = true;
            RaiseFinished();
            Unbind();
        }

        public IAwaiter GetAwaiter()
        {
            return new TransitionAwaiter(this);
        }

        public ITransition DisableAnimation(bool disabled)
        {
            if (running)
            {
                Debug.Log("The transition is running.DisableAnimation failed.");

                return this;
            }

            if (done)
            {
                Debug.Log("The transition is done.DisableAnimation failed.");

                return this;
            }

            animationDisabled = disabled;
            return this;
        }

        public ITransition AtLayer(int layer)
        {
            if (running)
            {
                Debug.Log("The transition is running.sets the layer failed.");

                return this;
            }

            if (done)
            {
                Debug.Log("The transition is done.sets the layer failed.");

                return this;
            }

            this.layer = layer;
            return this;
        }

        public ITransition Overlay(Func<IWindow, IWindow, ActionType> policy)
        {
            if (running)
            {
                Debug.Log("The transition is running.sets the policy failed.");

                return this;
            }

            if (done)
            {
                Debug.Log("The transition is done.sets the policy failed.");

                return this;
            }

            OverlayPolicy = policy;
            return this;
        }

        public ITransition OnStart(Action callback)
        {
            if (running)
            {
                Debug.Log("The transition is running.OnStart failed.");

                return this;
            }

            if (done)
            {
                callback();
                return this;
            }

            onStart += callback;
            return this;
        }

        public ITransition OnStateChanged(Action<IWindow, WindowState> callback)
        {
            if (running)
            {
                Debug.Log("The transition is running.OnStateChanged failed.");

                return this;
            }

            if (done)
                return this;

            onStateChanged += callback;
            return this;
        }

        public ITransition OnFinish(Action callback)
        {
            if (done)
            {
                callback();
                return this;
            }

            onFinish += callback;
            return this;
        }

        public virtual IEnumerator TransitionTask()
        {
            running = true;
            OnStart();
#if UNITY_5_3_OR_NEWER
            yield return DoTransition();
#else
            var transitionAction = this.DoTransition();
            while (transitionAction.MoveNext())
                yield return transitionAction.Current;
#endif
            OnEnd();
        }

        protected abstract IEnumerator DoTransition();
    }

    public class CompletedTransition : Transition
    {
        public CompletedTransition(IManageable window) : base(window)
        {
            IsDone = true;
        }

        protected override IEnumerator DoTransition()
        {
            yield break;
        }
    }

    public struct TransitionAwaiter : IAwaiter, ICriticalNotifyCompletion
    {
        private readonly Transition transition;

        public TransitionAwaiter(Transition transition)
        {
            this.transition = transition ?? throw new ArgumentNullException("transition");
        }

        public bool IsCompleted => transition.IsDone;

        public void GetResult()
        {
            if (!IsCompleted)
                throw new Exception("The task is not finished yet");
        }

        public void OnCompleted(Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException("continuation");
            transition.OnFinish(continuation);
        }
    }
}
