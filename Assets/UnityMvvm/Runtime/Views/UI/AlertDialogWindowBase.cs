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

using Loxodon.Framework.ViewModels;
using UnityEngine;

namespace Loxodon.Framework.Views
{
    public abstract class AlertDialogWindowBase : Window
    {
        public GameObject Content;

        protected IUIView contentView;

        protected AlertDialogViewModel viewModel;

        public virtual IUIView ContentView
        {
            get => contentView;
            set
            {
                if (contentView == value)
                    return;

                if (contentView != null)
                    Destroy(contentView.Owner);

                contentView = value;
                if (contentView != null && contentView.Owner != null && Content != null)
                {
                    contentView.Visibility = true;
                    contentView.Transform.SetParent(Content.transform, false);
                }
            }
        }

        public virtual AlertDialogViewModel ViewModel
        {
            get => viewModel;
            set
            {
                if (viewModel == value)
                    return;

                viewModel = value;
                OnChangeViewModel();
            }
        }
        protected override void OnCreate(IBundle bundle)
        {
            WindowType = WindowType.DIALOG;
        }

        protected abstract void OnChangeViewModel();

        public abstract void Cancel();
    }
}
