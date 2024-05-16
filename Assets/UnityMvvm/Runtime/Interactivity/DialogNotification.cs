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

namespace Loxodon.Framework.Interactivity
{
    public class DialogNotification : Notification
    {
        private readonly string confirmButtonText;
        private readonly string neutralButtonText;
        private readonly string cancelButtonText;
        private readonly bool canceledOnTouchOutside;

        private int dialogResult;

        public DialogNotification(string title, string message, string confirmButtonText, bool canceledOnTouchOutside = true) : this(title, message, confirmButtonText, null, null, canceledOnTouchOutside)
        {
        }

        public DialogNotification(string title, string message, string confirmButtonText, string cancelButtonText, bool canceledOnTouchOutside = true) : this(title, message, confirmButtonText, null, cancelButtonText, canceledOnTouchOutside)
        {
        }

        public DialogNotification(string title, string message, string confirmButtonText, string neutralButtonText, string cancelButtonText, bool canceledOnTouchOutside = true) : base(title, message)
        {
            this.confirmButtonText = confirmButtonText;
            this.neutralButtonText = neutralButtonText;
            this.cancelButtonText = cancelButtonText;
            this.canceledOnTouchOutside = canceledOnTouchOutside;
        }

        public string ConfirmButtonText => confirmButtonText;

        public string NeutralButtonText => neutralButtonText;

        public string CancelButtonText => cancelButtonText;

        public bool CanceledOnTouchOutside => canceledOnTouchOutside;

        public int DialogResult
        {
            get => dialogResult;
            set => dialogResult = value;
        }
    }
}
