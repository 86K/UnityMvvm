

namespace Fusion.Mvvm
{
    public class GlobalSetting
    {
        /// <summary>
        /// Whether to use the CanvasGroup.blocksRaycasts instead of the CanvasGroup.interactable to control the interactivity of the view  
        /// </summary>
        public static bool useBlocksRaycastsInsteadOfInteractable = false;

        /// <summary>
        /// Whether to enable the window state broadcast feature.
        /// </summary>
        public static bool enableWindowStateBroadcast = true;
    }
}