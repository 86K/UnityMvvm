using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace Fusion.Mvvm
{
    public class StartupWindow : Window
    {
        public Text progressBarText;
        public Slider progressBarSlider;
        public Text tipText;
        public Button button;

        private StartupViewModel viewModel;
        private IUIViewLocator viewLocator;
        private AsyncWindowInteractionAction loginWindowInteractionAction;
        private AsynSceneInteractionAction sceneInteractionAction;

        protected override void OnCreate(IBundle bundle)
        {
            viewLocator = Context.GetApplicationContext().GetService<IUIViewLocator>();
            loginWindowInteractionAction = new AsyncWindowInteractionAction("UI/Logins/Login", viewLocator, WindowManager);
            sceneInteractionAction = new AsynSceneInteractionAction("Prefabs/Cube");
            viewModel = new StartupViewModel();


            BindingSet<StartupWindow, StartupViewModel> bindingSet = this.CreateBindingSet(viewModel);
            //bindingSet.Bind().For(v => v.OnOpenLoginWindow).To(vm => vm.LoginRequest);
            bindingSet.Bind().For(v => v.loginWindowInteractionAction).To(vm => vm.LoginRequest);
            bindingSet.Bind().For(v => v.OnDismissRequest).To(vm => vm.DismissRequest);
            bindingSet.Bind().For(v => v.sceneInteractionAction).To(vm => vm.LoadSceneRequest);

            // bindingSet.Bind(progressBarSlider).For("value", "onValueChanged").To("ProgressBar.Progress").TwoWay();// 不推荐的写法
            bindingSet.Bind(progressBarSlider).For(v => v.value, v => v.onValueChanged).To(vm => vm.ProgressBar.Progress).TwoWay();


            bindingSet.Bind(progressBarSlider.gameObject).For(v => v.activeSelf).To(vm => vm.ProgressBar.Enable).OneWay();
            bindingSet.Bind(progressBarText).For(v => v.text).ToExpression(vm => $"{Mathf.FloorToInt(vm.ProgressBar.Progress * 100f)}%").OneWay();
            bindingSet.Bind(tipText).For(v => v.text).To(vm => vm.ProgressBar.Tip).OneWay();

            //bindingSet.Bind(this.button).For(v => v.onClick).To(vm=>vm.OnClick()).OneWay(); //Method binding,only bound to the onClick event.
            bindingSet.Bind(button).For(v => v.onClick).To(vm => vm.Click)
                .OneWay(); //Command binding,bound to the onClick event and interactable property.
            bindingSet.Build();

            viewModel.Unzip();
        }

        protected void OnDismissRequest(object sender, InteractionEventArgs args)
        {
            Dismiss();
        }

        //// Use AsyncWindowInteractionAction instead of this method.
        //protected void OnOpenLoginWindow(object sender, InteractionEventArgs args)
        //{
        //    try
        //    {
        //        LoginWindow loginWindow = viewLocator.LoadWindow<LoginWindow>(this.WindowManager, "UI/Logins/Login");
        //        var callback = args.Callback;
        //        var loginViewModel = args.Context;

        //        if (callback != null)
        //        {
        //            EventHandler handler = null;
        //            handler = (window, e) =>
        //            {
        //                loginWindow.OnDismissed -= handler;
        //                if (callback != null)
        //                    callback();
        //            };
        //            loginWindow.OnDismissed += handler;
        //        }

        //        loginWindow.SetDataContext(loginViewModel);
        //        loginWindow.Create();
        //        loginWindow.Show();
        //    }
        //    catch (Exception e)
        //    {
        //        if (log.IsWarnEnabled)
        //            log.Warn(e);
        //    }
        //}

        //Load game objects in the scene using AsynSceneInteractionAction
        class AsynSceneInteractionAction : AsyncInteractionActionBase<ProgressBar>
        {
            private readonly string path;

            public AsynSceneInteractionAction(string path)
            {
                this.path = path;
            }

            public override async Task Action(ProgressBar progressBar)
            {
                progressBar.Enable = true;
                // progressBar.Tip = R.startup_progressbar_tip_loading;
                try
                {
                    var request = Resources.LoadAsync<GameObject>(path);
                    while (!request.isDone)
                    {
                        progressBar.Progress = request.progress;
                        await new WaitForSecondsRealtime(0.02f);
                    }

                    GameObject sceneTemplate = (GameObject)request.asset;
                    Instantiate(sceneTemplate);
                }
                finally
                {
                    progressBar.Tip = "";
                    progressBar.Enable = false;
                }
            }
        }
    }
}