using System.Collections.Generic;

namespace com.bbbirder.injection
{
    public interface IInjection : IDirectRetrieve
    {
        /// <summary>
        /// set this property to populate injections
        /// </summary>
        /// <value></value>
        public IEnumerable<InjectionInfo> ProvideInjections();
    }
}