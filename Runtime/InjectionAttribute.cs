using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Assertions;

namespace com.bbbirder.injection
{
    public abstract class InjectionAttribute : DirectRetrieveAttribute
    {
        /// <summary>
        /// set this property to populate injections
        /// </summary>
        /// <value></value>
        public abstract IEnumerable<InjectionInfo> ProvideInjections();
    }
}