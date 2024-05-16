

using System;

namespace Fusion.Mvvm
{
    public delegate void AnimationAction<T>(T view, Action startCallback, Action endCallback) where T : IUIView;

    public class GenericUIAnimation<T> : IAnimation where T : IUIView
    {
        private readonly T view;
        private readonly AnimationAction<T> animation;

        private Action _onStart;
        private Action _onEnd;

        public GenericUIAnimation(T view, AnimationAction<T> animation)
        {
            this.view = view;
            this.animation = animation;
        }

        protected virtual void OnStart()
        {
            try
            {
                if (_onStart != null)
                {
                    _onStart();
                    _onStart = null;
                }
            }
            catch (Exception) { }
        }

        protected virtual void OnEnd()
        {
            try
            {
                if (_onEnd != null)
                {
                    _onEnd();
                    _onEnd = null;
                }
            }
            catch (Exception) { }
        }

        public IAnimation OnStart(Action onStart)
        {
            _onStart += onStart;
            return this;
        }

        public IAnimation OnEnd(Action onEnd)
        {
            _onEnd += onEnd;
            return this;
        }

        public virtual IAnimation Play()
        {
            if (animation != null)
                animation(view, OnStart, OnEnd);
            return this;
        }
    }
}
