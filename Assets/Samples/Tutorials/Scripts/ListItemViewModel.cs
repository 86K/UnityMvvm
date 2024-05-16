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

using Loxodon.Framework.Commands;
using Loxodon.Framework.ViewModels;

namespace Loxodon.Framework.Tutorials
{
    public class ListItemViewModel : ViewModelBase
    {
        private string title;
        private string icon;
        private float price;
        private bool selected;
        private readonly ICommand clickCommand;
        private readonly ICommand selectCommand;

        public ListItemViewModel(ICommand selectCommand, ICommand clickCommand)
        {
            this.selectCommand = selectCommand;
            this.clickCommand = clickCommand;
        }

        public ICommand ClickCommand => clickCommand;

        public ICommand SelectCommand => selectCommand;

        public string Title
        {
            get => title;
            set => Set(ref title, value);
        }
        public string Icon
        {
            get => icon;
            set => Set(ref icon, value);
        }

        public float Price
        {
            get => price;
            set => Set(ref price, value);
        }

        public bool IsSelected
        {
            get => selected;
            set => Set(ref selected, value);
        }
    }
}
