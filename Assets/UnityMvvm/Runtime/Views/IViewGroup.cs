

using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Mvvm
{
    public delegate void Layout(Transform transform);

    /// <summary>
    /// View group
    /// </summary>
    public interface IViewGroup : IView
    {
        List<IView> Views { get; }

        IView GetView(string name);

        void AddView(IView view, bool worldPositionStays = false);

        void AddView(IView view, Layout layout);

        void RemoveView(IView view, bool worldPositionStays = false);
    }
}
