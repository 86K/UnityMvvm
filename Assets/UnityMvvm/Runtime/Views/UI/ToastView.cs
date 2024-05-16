

using UnityEngine.UI;

namespace Fusion.Mvvm
{
    public class ToastView : ToastViewBase
    {
        public Text text;

        protected override void OnContentChanged()
        {
            if (text != null)
                text.text = content;
        }
    }
}
