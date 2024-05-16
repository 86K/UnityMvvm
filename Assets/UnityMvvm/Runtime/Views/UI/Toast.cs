/*
 * MIT License
 *
 * Copyright (c) 2018 Clark Yang
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
 * of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 */

using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loxodon.Framework.Views
{
    public class Toast : UIBase
    {
        private const string DEFAULT_VIEW_NAME = "UI/Toast";

        public static new IUIViewGroup GetCurrentViewGroup()
        {
            return UIBase.GetCurrentViewGroup();
        }

        private static string viewName;
        public static string ViewName
        {
            get => string.IsNullOrEmpty(viewName) ? DEFAULT_VIEW_NAME : viewName;
            set => viewName = value;
        }

        public static Toast Show(string text, float duration = 3f)
        {
            return Show(ViewName, null, text, duration, null, null);
        }

        public static Toast Show(string text, float duration, UILayout layout)
        {
            return Show(ViewName, null, text, duration, layout, null);
        }

        public static Toast Show(string text, float duration, UILayout layout, Action callback)
        {
            return Show(ViewName, null, text, duration, layout, callback);
        }

        public static Toast Show(IUIViewGroup viewGroup, string text, float duration = 3f)
        {
            return Show(ViewName, viewGroup, text, duration, null, null);
        }

        public static Toast Show(IUIViewGroup viewGroup, string text, float duration, UILayout layout)
        {
            return Show(ViewName, viewGroup, text, duration, layout, null);
        }

        public static Toast Show(IUIViewGroup viewGroup, string text, float duration, UILayout layout, Action callback)
        {
            return Show(ViewName, viewGroup, text, duration, layout, callback);
        }

        public static Toast Show(string viewName, IUIViewGroup viewGroup, string text, float duration, UILayout layout, Action callback)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ViewName;

            IUIViewLocator locator = GetUIViewLocator();
            ToastViewBase view = locator.LoadView<ToastViewBase>(viewName);
            if (view == null)
                throw new NotFoundException("Not found the \"ToastView\".");

            if (viewGroup == null)
                viewGroup = GetCurrentViewGroup();

            Toast toast = new Toast(view, viewGroup, text, duration, layout, callback);
            toast.Show();
            return toast;
        }

        private readonly IUIViewGroup viewGroup;
        private readonly float duration;
        private readonly string text;
        private readonly ToastViewBase view;
        private readonly UILayout layout;
        private readonly Action callback;

        protected Toast(ToastViewBase view, IUIViewGroup viewGroup, string text, float duration) : this(view, viewGroup, text, duration, null, null)
        {
        }

        protected Toast(ToastViewBase view, IUIViewGroup viewGroup, string text, float duration, UILayout layout) : this(view, viewGroup, text, duration, layout, null)
        {
        }

        protected Toast(ToastViewBase view, IUIViewGroup viewGroup, string text, float duration, UILayout layout, Action callback)
        {
            this.view = view;
            this.viewGroup = viewGroup;
            this.text = text;
            this.duration = duration;
            this.layout = layout;
            this.callback = callback;
        }

        public float Duration => duration;

        public string Text => text;

        public ToastViewBase View => view;

        public void Cancel()
        {
            if (view == null || view.Owner == null)
                return;

            if (!view.Visibility)
            {
                Object.Destroy(view.Owner);
                return;
            }

            if (view.ExitAnimation != null)
            {
                view.ExitAnimation.OnEnd(() =>
                {
                    view.Visibility = false;
                    viewGroup.RemoveView(view);
                    Object.Destroy(view.Owner);
                    DoCallback();
                }).Play();
            }
            else
            {
                view.Visibility = false;
                viewGroup.RemoveView(view);
                Object.Destroy(view.Owner);
                DoCallback();
            }
        }

        public void Show()
        {
            if (view.Visibility)
                return;

            viewGroup.AddView(view, layout);
            view.Visibility = true;
            view.Content = text;

            if (view.EnterAnimation != null)
                view.EnterAnimation.Play();

            view.StartCoroutine(DelayDismiss(duration));
        }

        protected IEnumerator DelayDismiss(float duration)
        {
            yield return new WaitForSeconds(duration);
            Cancel();
        }

        protected void DoCallback()
        {
            try
            {
                if (callback != null)
                    callback();
            }
            catch (Exception)
            {
            }
        }
    }
}
