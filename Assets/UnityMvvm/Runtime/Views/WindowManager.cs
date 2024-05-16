

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Mvvm
{
    [DisallowMultipleComponent]
    public class WindowManager : MonoBehaviour, IWindowManager
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(WindowManager));
        private static BlockingCoroutineTransitionExecutor blockingExecutor;

        //For compatibility with the "Configurable Enter Play Mode" feature
#if UNITY_2019_3_OR_NEWER //&& UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void OnInitialize()
        {
            if (blockingExecutor != null)
                blockingExecutor = null;
        }
#endif
        private static BlockingCoroutineTransitionExecutor GetTransitionExecutor()
        {
            if (blockingExecutor == null)
                blockingExecutor = new BlockingCoroutineTransitionExecutor();
            return blockingExecutor;
        }

        private bool lastActivated = true;
        private bool activated = true;
        private readonly List<IWindow> windows = new List<IWindow>();

        public virtual bool Activated
        {
            get => activated;
            set
            {
                if (activated == value)
                    return;

                activated = value;
            }
        }

        public int Count => windows.Count;

        public int VisibleCount { get { return windows.FindAll(w => w.Visibility).Count; } }

        public virtual IWindow Current
        {
            get
            {
                if (windows == null || windows.Count <= 0)
                    return null;

                IWindow window = windows[0];
                return window != null && window.Visibility ? window : null;
            }
        }

        public virtual IWindow GetVisibleWindow(int index)
        {
            if (windows == null || windows.Count <= 1)
                return null;

            int currIndex = -1;
            var ie = Visibles();
            while (ie.MoveNext())
            {
                currIndex++;
                if (currIndex > index)
                    return null;

                if (currIndex == index)
                    return ie.Current;
            }
            return null;
        }

        protected virtual void OnEnable()
        {
            Activated = lastActivated;
        }

        protected virtual void OnDisable()
        {
            lastActivated = Activated;
            Activated = false;
        }

        protected virtual void OnDestroy()
        {
            if (windows.Count > 0)
            {
                Clear();
            }
        }

        protected virtual void OnApplicationQuit()
        {
            if (blockingExecutor != null)
            {
                blockingExecutor.Shutdown();
                blockingExecutor = null;
            }
        }

        public virtual void Clear()
        {
            for (int i = 0; i < windows.Count; i++)
            {
                try
                {
                    windows[i].Dismiss(true);
                }
                catch (Exception) { }
            }
            windows.Clear();
        }

        public virtual bool Contains(IWindow window)
        {
            return windows.Contains(window);
        }

        public virtual int IndexOf(IWindow window)
        {
            return windows.IndexOf(window);
        }

        public virtual IWindow Get(int index)
        {
            if (index < 0 || index > windows.Count - 1)
                throw new IndexOutOfRangeException();

            return windows[index];
        }

        public virtual void Add(IWindow window)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            if (windows.Contains(window))
                return;

            windows.Add(window);
            AddChild(GetTransform(window));
        }

        public virtual bool Remove(IWindow window)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            RemoveChild(GetTransform(window));
            return windows.Remove(window);
        }

        public virtual IWindow RemoveAt(int index)
        {
            if (index < 0 || index > windows.Count - 1)
                throw new IndexOutOfRangeException();

            var window = windows[index];
            RemoveChild(GetTransform(window));
            windows.RemoveAt(index);
            return window;
        }

        protected virtual void MoveToLast(IWindow window)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            try
            {
                int index = IndexOf(window);
                if (index < 0 || index == Count - 1)
                    return;

                windows.RemoveAt(index);
                windows.Add(window);
            }
            finally
            {
                Transform transform = GetTransform(window);
                if (transform != null)
                    transform.SetAsFirstSibling();
            }
        }

        protected virtual void MoveToFirst(IWindow window)
        {
            MoveToIndex(window, 0);
        }

        protected virtual void MoveToIndex(IWindow window, int index)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            int oldIndex = IndexOf(window);
            try
            {
                if (oldIndex < 0 || oldIndex == index)
                    return;

                windows.RemoveAt(oldIndex);
                windows.Insert(index, window);
            }
            finally
            {
                Transform transform = GetTransform(window);
                if (transform != null)
                {
                    if (index == 0)
                    {
                        transform.SetAsLastSibling();
                    }
                    else
                    {
                        IWindow preWindow = windows[index - 1];
                        int preWindowPosition = GetChildIndex(GetTransform(preWindow));
                        int currWindowPosition = oldIndex >= index ? preWindowPosition - 1 : preWindowPosition;
                        transform.SetSiblingIndex(currWindowPosition);
                    }
                }
            }
        }

        public virtual IEnumerator<IWindow> Visibles()
        {
            return new InternalVisibleEnumerator(windows);
        }

        public virtual List<IWindow> Find(bool visible)
        {
            return windows.FindAll(w => w.Visibility == visible);
        }

        public virtual IWindow Find(Type windowType)
        {
            if (windowType == null)
                return null;

            return windows.Find(w => windowType.IsAssignableFrom(w.GetType()));
        }

        public virtual T Find<T>() where T : IWindow
        {
            return (T)windows.Find(w => w is T);
        }

        public virtual IWindow Find(string name, Type windowType)
        {
            if (name == null || windowType == null)
                return null;

            return windows.Find(w => windowType.IsAssignableFrom(w.GetType()) && w.Name.Equals(name));
        }

        public virtual T Find<T>(string name) where T : IWindow
        {
            return (T)windows.Find(w => w is T && w.Name.Equals(name));
        }

        public virtual List<IWindow> FindAll(Type windowType)
        {
            List<IWindow> list = new List<IWindow>();
            foreach (IWindow window in windows)
            {
                if (windowType.IsAssignableFrom(window.GetType()))
                    list.Add(window);
            }
            return list;
        }

        public virtual List<T> FindAll<T>() where T : IWindow
        {
            List<T> list = new List<T>();
            foreach (IWindow window in windows)
            {
                if (window is T)
                    list.Add((T)window);
            }
            return list;
        }

        protected virtual Transform GetTransform(IWindow window)
        {
            try
            {
                if (window == null)
                    return null;

                if (window is UIView)
                    return (window as UIView).Transform;

                var propertyInfo = window.GetType().GetProperty("Transform");
                if (propertyInfo != null)
                    return (Transform)propertyInfo.GetGetMethod().Invoke(window, null);

                if (window is Component)
                    return (window as Component).transform;
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected virtual int GetChildIndex(Transform child)
        {
            Transform transform = this.transform;
            int count = transform.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).Equals(child))
                    return i;
            }
            return -1;
        }

        protected virtual void AddChild(Transform child, bool worldPositionStays = false)
        {
            if (child == null || transform.Equals(child.parent))
                return;

            child.gameObject.layer = gameObject.layer;
            child.SetParent(transform, worldPositionStays);
            child.SetAsFirstSibling();
        }

        protected virtual void RemoveChild(Transform child, bool worldPositionStays = false)
        {
            if (child == null || !transform.Equals(child.parent))
                return;

            child.SetParent(null, worldPositionStays);
        }

        public ITransition Show(IWindow window)
        {
            ShowTransition transition = new ShowTransition(this, (IManageable)window);
            GetTransitionExecutor().Execute(transition);
            return transition.OnStateChanged((w, state) =>
                {
                    
                    if (state == WindowState.VISIBLE)
                        MoveToIndex(w, transition.Layer);

                    //if (state == WindowState.INVISIBLE)
                    //    this.MoveToLast(w);
                });
        }

        public ITransition Hide(IWindow window)
        {
            HideTransition transition = new HideTransition(this, (IManageable)window, false);
            GetTransitionExecutor().Execute(transition);
            return transition.OnStateChanged((w, state) =>
                {
                    
                    if (state == WindowState.INVISIBLE)
                        MoveToLast(w);
                });
        }

        public ITransition Dismiss(IWindow window)
        {
            HideTransition transition = new HideTransition(this, (IManageable)window, true);
            GetTransitionExecutor().Execute(transition);
            return transition.OnStateChanged((w, state) =>
                {
                    
                    if (state == WindowState.INVISIBLE)
                        MoveToLast(w);
                });
        }

        class InternalVisibleEnumerator : IEnumerator<IWindow>
        {
            private readonly List<IWindow> windows;
            private int index = -1;
            public InternalVisibleEnumerator(List<IWindow> list)
            {
                windows = list;
            }

            public IWindow Current => index < 0 || index >= windows.Count ? null : windows[index];

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                index = -1;
                windows.Clear();
            }

            public bool MoveNext()
            {
                if (index >= windows.Count - 1)
                    return false;

                index++;
                for (; index < windows.Count; index++)
                {
                    IWindow window = windows[index];
                    if (window != null && window.Visibility)
                        return true;
                }

                return false;
            }

            public void Reset()
            {
                index = -1;
            }
        }

        class ShowTransition : Transition
        {
            private readonly WindowManager manager;
            public ShowTransition(WindowManager manager, IManageable window) : base(window)
            {
                this.manager = manager;
            }

            protected virtual ActionType Overlay(IWindow previous, IWindow current)
            {
                if (previous == null || previous.WindowType == WindowType.FULL)
                    return ActionType.None;

                if (previous.WindowType == WindowType.POPUP)
                    return ActionType.Dismiss;

                return ActionType.None;
            }

            protected override IEnumerator DoTransition()
            {
                IManageable current = Window;
                int layer = (Layer < 0 || current.WindowType == WindowType.DIALOG || current.WindowType == WindowType.PROGRESS) ? 0 : Layer;
                if (layer > 0)
                {
                    int visibleCount = manager.VisibleCount;
                    if (layer > visibleCount)
                        layer = visibleCount;
                }
                Layer = layer;

                IManageable previous = (IManageable)manager.GetVisibleWindow(layer);
                if (previous != null)
                {
                    //Passivate the previous window
                    if (previous.Activated)
                    {
                        IAsyncResult passivate = previous.Passivate(AnimationDisabled);
                        yield return passivate.WaitForDone();
                    }

                    Func<IWindow, IWindow, ActionType> policy = OverlayPolicy;
                    if (policy == null)
                        policy = Overlay;
                    ActionType actionType = policy(previous, current);
                    switch (actionType)
                    {
                        case ActionType.Hide:
                            previous.DoHide(AnimationDisabled);
                            break;
                        case ActionType.Dismiss:
                            previous.DoHide(AnimationDisabled).Callbackable().OnCallback((r) =>
                            {
                                previous.DoDismiss();
                            });
                            break;
                        default:
                            break;
                    }
                }

                if (!current.Visibility)
                {
                    IAsyncResult show = current.DoShow(AnimationDisabled);
                    yield return show.WaitForDone();
                }

                if (manager.Activated && current.Equals(manager.Current))
                {
                    IAsyncResult activate = current.Activate(AnimationDisabled);
                    yield return activate.WaitForDone();
                }
            }
        }

        class HideTransition : Transition
        {
            private readonly WindowManager manager;
            private readonly bool dismiss;
            public HideTransition(WindowManager manager, IManageable window, bool dismiss) : base(window)
            {
                this.dismiss = dismiss;
                this.manager = manager;
            }

            protected override IEnumerator DoTransition()
            {
                IManageable current = Window;
                if (manager.IndexOf(current) == 0 && current.Activated)
                {
                    IAsyncResult passivate = current.Passivate(AnimationDisabled);
                    yield return passivate.WaitForDone();
                }

                if (current.Visibility)
                {
                    IAsyncResult hide = current.DoHide(AnimationDisabled);
                    yield return hide.WaitForDone();
                }

                if (dismiss)
                {
                    current.DoDismiss();
                }
            }
        }

        class BlockingCoroutineTransitionExecutor
        {
            private IAsyncResult taskResult;
            private bool running = false;
            private readonly List<Transition> transitions = new List<Transition>();
            public bool IsRunning => running;

            public int Count => transitions.Count;

            public BlockingCoroutineTransitionExecutor()
            {
            }

            public void Execute(Transition transition)
            {
                try
                {
                    if (transition is ShowTransition && transition.Window.WindowType == WindowType.QUEUED_POPUP)
                    {
                        int index = transitions.FindLastIndex((t) => (t is ShowTransition)
                        && t.Window.WindowType == WindowType.QUEUED_POPUP
                        && t.Window.WindowManager == transition.Window.WindowManager
                        && t.Window.WindowPriority >= transition.Window.WindowPriority);
                        if (index >= 0)
                        {
                            transitions.Insert(index + 1, transition);
                            return;
                        }

                        index = transitions.FindIndex((t) => (t is ShowTransition)
                        && t.Window.WindowType == WindowType.QUEUED_POPUP
                        && t.Window.WindowManager == transition.Window.WindowManager
                        && t.Window.WindowPriority < transition.Window.WindowPriority);
                        if (index >= 0)
                        {
                            transitions.Insert(index, transition);
                            return;
                        }
                    }

                    transitions.Add(transition);
                }
                finally
                {
                    if (!running)
                        taskResult = Executors.RunOnCoroutine(DoTask());
                }
            }

            public void Shutdown()
            {
                if (taskResult != null)
                {
                    taskResult.Cancel();
                    running = false;
                    taskResult = null;
                }
                transitions.Clear();
            }

            private bool Check(Transition transition)
            {
                if (!(transition is ShowTransition))
                    return true;

                IManageable window = transition.Window;
                IWindowManager manager = window.WindowManager;
                var current = manager.Current;
                if (current == null)
                    return true;

                if (current.WindowType == WindowType.DIALOG || current.WindowType == WindowType.PROGRESS)
                    return false;

                if (current.WindowType == WindowType.QUEUED_POPUP && !(window.WindowType == WindowType.DIALOG || window.WindowType == WindowType.PROGRESS))
                    return false;
                return true;
            }

            protected virtual IEnumerator DoTask()
            {
                try
                {
                    running = true;
                    yield return null;//wait one frame
                    while (transitions.Count > 0)
                    {
                        Transition transition = transitions.Find(e => Check(e));
                        if (transition != null)
                        {
                            transitions.Remove(transition);
                            var result = Executors.RunOnCoroutine(transition.TransitionTask());
                            yield return result.WaitForDone();

                            IWindowManager manager = transition.Window.WindowManager;
                            var current = manager.Current;
                            if (manager.Activated && current != null && !current.Activated && !transitions.Exists((e) => e.Window.WindowManager.Equals(manager)))
                            {
                                IAsyncResult activate = (current as IManageable).Activate(transition.AnimationDisabled);
                                yield return activate.WaitForDone();
                            }
                        }
                        else
                        {
                            yield return null;
                        }
                    }
                }
                finally
                {
                    running = false;
                    taskResult = null;
                }
            }
        }
    }
}
