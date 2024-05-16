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

using Loxodon.Framework.Asynchronous;
using Loxodon.Framework.Binding;
using Loxodon.Framework.ViewModels;
using System.Threading.Tasks;
using UnityEngine;

namespace Loxodon.Framework.Views
{
    public class Tips : UIBase
    {
        public static Tips Create(UIView view, IUIViewGroup viewGroup = null)
        {
            if (viewGroup == null)
                viewGroup = GetCurrentViewGroup();

            view.Visibility = false;
            return new Tips(view, viewGroup);
        }
        public static Tips Create(string viewName, IUIViewGroup viewGroup = null)
        {
            IUIViewLocator locator = GetUIViewLocator();
            UIView view = locator.LoadView<UIView>(viewName);
            if (view == null)
                throw new NotFoundException("Not found the \"UIView\".");

            if (viewGroup == null)
                viewGroup = GetCurrentViewGroup();

            view.Visibility = false;
            return new Tips(view, viewGroup);
        }

        public static async Task<Tips> CreateAsync(string viewName, IUIViewGroup viewGroup = null)
        {
            IUIViewLocator locator = GetUIViewLocator();
            UIView view = await locator.LoadViewAsync<UIView>(viewName);
            if (view == null)
                throw new NotFoundException("Not found the \"UIView\".");

            if (viewGroup == null)
                viewGroup = GetCurrentViewGroup();

            view.Visibility = false;
            return new Tips(view, viewGroup);
        }

        private readonly IUIViewGroup viewGroup;
        private readonly UIView view;

        protected Tips(UIView view, IUIViewGroup viewGroup)
        {
            this.view = view;
            this.viewGroup = viewGroup;
        }

        public UIView View => view;

        public void Show(IViewModel viewModel, UILayout layout = null)
        {
            viewGroup.AddView(view, layout);
            view.SetDataContext(viewModel);
            view.Visibility = true;

            if (view.EnterAnimation != null)
                view.EnterAnimation.Play();
        }

        public void Hide()
        {
            if (view == null || view.Owner == null)
                return;

            if (!view.Visibility)
                return;

            if (view.ExitAnimation != null)
            {
                view.ExitAnimation.OnEnd(() =>
                {
                    view.Visibility = false;
                }).Play();
            }
            else
            {
                view.Visibility = false;
            }
        }

        public void Dismiss()
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
                    Object.Destroy(view.Owner);
                }).Play();
            }
            else
            {
                view.Visibility = false;
                Object.Destroy(view.Owner);
            }
        }
    }
}
