using System.Threading.Tasks;
using UnityEngine;

namespace Fusion.Mvvm
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
