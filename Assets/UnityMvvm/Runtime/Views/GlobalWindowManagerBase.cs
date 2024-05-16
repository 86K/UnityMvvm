

using UnityEngine;

namespace Fusion.Mvvm
{
    [DisallowMultipleComponent]
    public abstract class GlobalWindowManagerBase : WindowManager
    {
        public static GlobalWindowManagerBase Root;

        protected virtual void Start()
        {
            Root = this;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Root = null;
        }
    }
}