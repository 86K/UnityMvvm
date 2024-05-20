

using UnityEngine;
using System.Globalization;
using System.Collections;

namespace Fusion.Mvvm
{
    public class Launcher : MonoBehaviour
    {
        private Context context;
        ISubscription<WindowStateEventArgs> subscription;
        void Awake()
        {
            GlobalWindowManagerBase windowManager = FindObjectOfType<GlobalWindowManagerBase>();
            if (windowManager == null)
                throw new NotFoundException("Not found the GlobalWindowManager.");

            context = Context.GetGlobalContext();

            IServiceContainer container = context.GetContainer();

            
            BindingServiceBundle bundle = new BindingServiceBundle(context.GetContainer());
            bundle.Start();

            
            container.Register<IUIViewLocator>(new ResourcesViewLocator());

            
            IAccountRepository accountRepository = new AccountRepository();
            container.Register<IAccountService>(new AccountService(accountRepository));

            
            GlobalSetting.enableWindowStateBroadcast = true;
            
            GlobalSetting.useBlocksRaycastsInsteadOfInteractable = true;

            
            subscription = Window.Messenger.Subscribe<WindowStateEventArgs>(e =>
            {
                Debug.LogFormat("The window[{0}] state changed from {1} to {2}", e.Window.Name, e.OldState, e.State);
            });
        }

        IEnumerator Start()
        {
            
            WindowContainer winContainer = WindowContainer.Create("MAIN");

            yield return null;

            IUIViewLocator locator = context.GetService<IUIViewLocator>();
            StartupWindow window = locator.LoadWindow<StartupWindow>(winContainer, "UI/Startup/Startup");
            window.Create();
            ITransition transition = window.Show().OnStateChanged((w, state) =>
            {
                //log.DebugFormat("Window:{0} State{1}",w.Name,state);
            });

            yield return transition.WaitForDone();
        }

    }
}