namespace Fusion.Mvvm
{
    public enum BindingMode
    {
        Default = 0,
        
        /// <summary>
        /// 双向绑定。
        /// v->vm  vm->v
        /// </summary>
        TwoWay,
        
        /// <summary>
        /// 单向绑定。
        /// vm->v
        /// </summary>
        OneWay,
        
        /// <summary>
        /// 单次绑定。
        /// 一般是静态的数据绑定一次。比如：语言本地化。
        /// 但是用的很少.
        /// </summary>
        OneTime,
        
        /// <summary>
        /// 单向绑定到来源。
        /// v->vm
        /// </summary>
        OneWayToSource
    }
}