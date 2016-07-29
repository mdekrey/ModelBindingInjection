using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mvc.CustomBinding
{
    /// <summary>
    /// Allows a model to handle some processing before post-binding is completed
    /// </summary>
    public interface ICanPostbind
    {
        /// <summary>
        /// Determines if the model is in a state to allow post-binding
        /// </summary>
        /// <param name="bindingContext">The binding context in use for binding</param>
        /// <returns>True if the model should have post-binding, otherwise false.</returns>
        Task<bool> CanPostbind(ModelBindingContext bindingContext);
    }
}
