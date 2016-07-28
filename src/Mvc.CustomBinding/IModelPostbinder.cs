using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mvc.CustomBinding
{
    /// <summary>
    /// Post-binds a property of an object
    /// </summary>
    public interface IModelPostbinder
    {
        /// <summary>
        /// Attempts to bind a model.
        /// </summary>
        /// <param name="bindingContext">The <see cref="ModelPostbindingContext"/>.</param>
        /// <returns>
        /// <para>
        /// A <see cref="Task"/> which will complete when the model binding process completes.
        /// </para>
        /// <para>
        /// If model binding was successful, the <see cref="ModelBindingContext.Result"/> should have
        /// <see cref="ModelBindingResult.IsModelSet"/> set to <c>true</c>.
        /// </para>
        /// <para>
        /// A model binder that completes successfully should set <see cref="ModelBindingContext.Result"/> to
        /// a value returned from <see cref="ModelBindingResult.Success"/>. 
        /// </para>
        /// </returns>
        Task BindModelAsync(ModelPostbindingContext bindingContext);
    }
}
