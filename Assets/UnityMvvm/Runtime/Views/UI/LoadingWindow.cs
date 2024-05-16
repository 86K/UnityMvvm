

namespace Fusion.Mvvm
{
    public class LoadingWindow : Window
    {
        protected override void OnCreate(IBundle bundle)
        {
            WindowType = WindowType.PROGRESS;
        }
    }
}
