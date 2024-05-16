﻿/*
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

using Loxodon.Framework.Binding;
using Loxodon.Framework.Views;
using UnityEngine.UI;

namespace Loxodon.Framework.Tutorials
{
    public class ListItemEditView : UIView
    {
        public Text title;
        public Text price;
        public Slider priceSlider;
        public Button changeIcon;
        public Image image;
        public Button submit;
        public Button cancel;

        public ListItemEditViewModel ViewModel
        {
            get => (ListItemEditViewModel)this.GetDataContext();
            set => this.SetDataContext(value);
        }

        protected override void Start()
        {
            var bindingSet = this.CreateBindingSet<ListItemEditView, ListItemEditViewModel>();
            bindingSet.Bind(title).For(v => v.text).To(vm => vm.Title);
            bindingSet.Bind(price).For(v => v.text).ToExpression(vm => $"${vm.Price:0.00}").OneWay();
            bindingSet.Bind(priceSlider).For(v => v.value, v => v.onValueChanged).To(vm => vm.Price).TwoWay();
            bindingSet.Bind(image).For(v => v.sprite).To(vm => vm.Icon).WithConversion("spriteConverter").OneWay();
            bindingSet.Bind(changeIcon).For(v => v.onClick).To(vm => vm.OnChangeIcon);
            bindingSet.Build();

            cancel.onClick.AddListener(Cancel);
            submit.onClick.AddListener(Submit);
        }

        private void Cancel()
        {            
            ViewModel.Cancelled = true;
            gameObject.SetActive(false);
            //this.Visibility = false;
            this.SetDataContext(null);
        }

        private void Submit()
        {            
            ViewModel.Cancelled = false;
            gameObject.SetActive(false);
            //this.Visibility = false;           
            this.SetDataContext(null);
        }
    }

}