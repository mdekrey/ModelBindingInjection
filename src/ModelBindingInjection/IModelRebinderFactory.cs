using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModelBindingInjection
{
    /// <summary>
    /// Creates a model rebinder
    /// </summary>
    public interface IModelRebinderFactory
    {
        /// <summary>
        /// Combines two model binders to bind a model twice
        /// </summary>
        /// <param name="first">Original model binder</param>
        /// <param name="second">Supplemental model binder</param>
        /// <returns>A single combined model binder</returns>
        IModelBinder CreateRebinder(IModelBinder first, IModelBinder second);
    }
}
