using System;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Mvvm
{
    public abstract class ViewModelBase : ObservableObject, IViewModel
    {
        ~ViewModelBase()
        {
            Dispose(false);
        }
        
        protected virtual void Dispose(bool disposing)
        {
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}