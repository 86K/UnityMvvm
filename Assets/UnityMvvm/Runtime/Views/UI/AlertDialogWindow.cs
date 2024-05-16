

using System;
using UnityEngine.UI;

namespace Fusion.Mvvm
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
