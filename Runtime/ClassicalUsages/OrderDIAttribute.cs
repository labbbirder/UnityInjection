using System;
using DG.DemiLib.Attributes;

namespace com.bbbirder.injection
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
    public class OrderDIAttribute : Attribute
    {
        internal int order;
        /// <summary>
        /// The default assign order of this type
        /// </summary>
        /// <param name="order"> pick from litter </param>
        public OrderDIAttribute(int order)
        {
            this.order = order;
        }
    }
}