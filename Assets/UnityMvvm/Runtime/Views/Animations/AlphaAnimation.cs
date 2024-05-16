

using UnityEngine;
using System.Collections;

namespace Fusion.Mvvm
{
    public class AlphaAnimation : UIAnimation
    {
        [Range(0f, 1f)]
        public float from = 1f;
        [Range(0f, 1f)]
        public float to = 1f;

        public float duration = 2f;

        private IUIView view;

        void OnEnable()
        {
            view = GetComponent<IUIView>();
            switch (AnimationType)
            {
                case AnimationType.EnterAnimation:
                    view.EnterAnimation = this;
                    break;
                case AnimationType.ExitAnimation:
                    view.ExitAnimation = this;
                    break;
                case AnimationType.ActivationAnimation:
                    if (view is IWindowView)
                        (view as IWindowView).ActivationAnimation = this;
                    break;
                case AnimationType.PassivationAnimation:
                    if (view is IWindowView)
                        (view as IWindowView).PassivationAnimation = this;
                    break;
            }

            if (AnimationType == AnimationType.ActivationAnimation || AnimationType == AnimationType.EnterAnimation)
            {
                view.CanvasGroup.alpha = from;
            }
        }

        public override IAnimation Play()
        {
            ////use the DoTween
            //this.view.CanvasGroup.DOFade (this.to, this.duration).OnStart (this.OnStart).OnComplete (this.OnEnd).Play ();		

            StartCoroutine(DoPlay());
            return this;
        }

        IEnumerator DoPlay()
        {
            OnStart();

            var delta = (to - from) / duration;
            var alpha = from;
            view.Alpha = alpha;
            if (delta > 0f)
            {
                while (alpha < to)
                {
                    alpha += delta * Time.deltaTime;
                    if (alpha > to)
                    {
                        alpha = to;
                    }
                    view.Alpha = alpha;
                    yield return null;
                }
            }
            else
            {
                while (alpha > to)
                {
                    alpha += delta * Time.deltaTime;
                    if (alpha < to)
                    {
                        alpha = to;
                    }
                    view.Alpha = alpha;
                    yield return null;
                }
            }

            OnEnd();
        }

    }
}
