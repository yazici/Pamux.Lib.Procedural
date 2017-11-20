using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Pamux.Lib.Procedural.Models
{
    public struct ThreadInfo
    {
        public readonly Action<object> callback;
        public readonly object parameter;

        public ThreadInfo(Action<object> callback, object parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
