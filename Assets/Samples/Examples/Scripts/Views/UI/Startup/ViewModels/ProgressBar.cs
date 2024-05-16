

namespace Fusion.Mvvm
{
    public class ProgressBar : ViewModelBase
    {

        private float progress;
        private string tip;
        private bool enable;

        public bool Enable
        {
            get => enable;
            set => Set(ref enable, value);
        }

        public float Progress
        {
            get => progress;
            set => Set(ref progress, value);
        }

        public string Tip
        {
            get => tip;
            set => Set(ref tip, value);
        }
    }
}