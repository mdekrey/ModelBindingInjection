using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ModelBindingInjection
{
    /// <summary>
    /// Defines an interface to build model post-binders
    /// </summary>
    public interface IModelBindingInjectorFactory
    {
        /// <summary>
        /// Builds a model post-binder
        /// </summary>
        /// <param name="metadata">The metadata used to determine which post-binder to use</param>
        /// <returns>A model post-binder, or null if no binding should be done</returns>
        IModelBindingInjector GetModelBinder(ModelMetadata metadata);
    }
}
