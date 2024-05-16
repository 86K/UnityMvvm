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
using UnityEngine;
using UnityEngine.UI;

namespace Loxodon.Framework.Views
{
    public class AlertDialogWindow : AlertDialogWindowBase
    {
        public Text Title;

        public Text Message;

        public Button ConfirmButton;

        public Button NeutralButton;

        public Button CancelButton;

        public Button OutsideButton;

        public bool CanceledOnTouchOutside { get; set; }

        public override IUIView ContentView
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
                    if (Message != null)
                        Message.gameObject.SetActive(false);
                }
            }
        }

        protected virtual void Button_OnClick(int which)
        {
            try
            {
                viewModel.OnClick(which);
            }
            catch (Exception) { }
            finally
            {
                Dismiss();
            }
        }

        public override void Cancel()
        {
            Button_OnClick(AlertDialog.BUTTON_NEGATIVE);
        }

        protected override void OnCreate(IBundle bundle)
        {
            WindowType = WindowType.DIALOG;
        }

        protected override void OnChangeViewModel()
        {
            if (Message != null)
            {
                if (!string.IsNullOrEmpty(viewModel.Message))
                {
                    Message.gameObject.SetActive(true);
                    Message.text = viewModel.Message;
                    if (contentView != null && contentView.Visibility)
                        contentView.Visibility = false;
                }
                else
                    Message.gameObject.SetActive(false);
            }

            if (Title != null)
            {
                if (!string.IsNullOrEmpty(viewModel.Title))
                {
                    Title.gameObject.SetActive(true);
                    Title.text = viewModel.Title;
                }
                else
                    Title.gameObject.SetActive(false);
            }

            if (ConfirmButton != null)
            {
                if (!string.IsNullOrEmpty(viewModel.ConfirmButtonText))
                {
                    ConfirmButton.gameObject.SetActive(true);
                    ConfirmButton.onClick.AddListener(() => { Button_OnClick(AlertDialog.BUTTON_POSITIVE); });
                    Text text = ConfirmButton.GetComponentInChildren<Text>();
                    if (text != null)
                        text.text = viewModel.ConfirmButtonText;
                }
                else
                {
                    ConfirmButton.gameObject.SetActive(false);
                }
            }

            if (CancelButton != null)
            {
                if (!string.IsNullOrEmpty(viewModel.CancelButtonText))
                {
                    CancelButton.gameObject.SetActive(true);
                    CancelButton.onClick.AddListener(() => { Button_OnClick(AlertDialog.BUTTON_NEGATIVE); });
                    Text text = CancelButton.GetComponentInChildren<Text>();
                    if (text != null)
                        text.text = viewModel.CancelButtonText;
                }
                else
                {
                    CancelButton.gameObject.SetActive(false);
                }
            }

            if (NeutralButton != null)
            {
                if (!string.IsNullOrEmpty(viewModel.NeutralButtonText))
                {
                    NeutralButton.gameObject.SetActive(true);
                    NeutralButton.onClick.AddListener(() => { Button_OnClick(AlertDialog.BUTTON_NEUTRAL); });
                    Text text = NeutralButton.GetComponentInChildren<Text>();
                    if (text != null)
                        text.text = viewModel.NeutralButtonText;
                }
                else
                {
                    NeutralButton.gameObject.SetActive(false);
                }
            }

            CanceledOnTouchOutside = viewModel.CanceledOnTouchOutside;
            if (OutsideButton != null && CanceledOnTouchOutside)
            {
                OutsideButton.gameObject.SetActive(true);
                OutsideButton.interactable = true;
                OutsideButton.onClick.AddListener(() => { Button_OnClick(AlertDialog.BUTTON_NEGATIVE); });
            }
        }
    }
}
