using System.Collections.Generic;

namespace com.bbbirder.unity{
    [InheritRetrieve]
    public interface IInjection{
        /// <summary>
        /// set this property to populate injections
        /// </summary>
        /// <value></value>
        public abstract IEnumerable<InjectionInfo> ProvideInjections();
    }
}