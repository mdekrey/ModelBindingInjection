using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ModelBindingInjection
{
    /// <summary>
    /// Specifies that a property should be updated post-binding
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
    public class ModelBindingInjectorAttribute : Attribute, IBindingSourceMetadata
    {
        /// <summary>
        /// Special data source to skip the normal binding process
        /// </summary>
        public static readonly BindingSource Source = new BindingSource("PostprocessBinding", "PostprocessBinding", true, false);

        /// <summary>
        /// Specifies that a property should be post-bound
        /// </summary>
        /// <param name="postprocessModelBinder">The model post-binder that should be used. Must implement IModelPostbinder.</param>
        public ModelBindingInjectorAttribute(Type postprocessModelBinder)
        {
            this.PostprocessModelBinder = postprocessModelBinder;
            if (!postprocessModelBinder.GetTypeInfo().GetInterfaces().Contains(typeof(IModelBindingInjector)))
            {
                throw new ArgumentException("Provided type must implement " + nameof(IModelBindingInjector));
            }
        }

        /// <summary>
        /// Specifies the binding source as postprocess binding
        /// </summary>
        public BindingSource BindingSource => Source;

        /// <summary>
        /// Gets the postprocess model binder type
        /// </summary>
        public Type PostprocessModelBinder { get; }
    }
}
