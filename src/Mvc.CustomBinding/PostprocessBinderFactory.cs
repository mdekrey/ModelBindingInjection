using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Mvc.CustomBinding
{
    /// <summary>
    /// Builds a model post-binder
    /// </summary>
    public class PostprocessBinderFactory : IPostprocessBinderFactory
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Builds a model postbinder factory
        /// </summary>
        /// <param name="serviceProvider">The services with which to construct the model postbinders</param>
        public PostprocessBinderFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Builds a specific model postbinder
        /// </summary>
        /// <param name="metadata">The metadata of the property to be bound. Determines the type of the model postbinder and is passed
        /// via dependency injection to the model postbinder</param>
        /// <returns>A model postbinder, or null if none is applicable</returns>
        public IModelPostbinder GetModelBinder(ModelMetadata metadata)
        {
            var attributes = (metadata as DefaultModelMetadata).Attributes.Attributes.OfType<PostprocessBindingAttribute>().ToArray();
            if (attributes.Length != 1)
            {
                throw new InvalidOperationException("Exactly 1 attribute must be provided of type " + nameof(PostprocessBindingAttribute));
            }
            var target = attributes[0];
            return ActivatorUtilities.CreateInstance(this.serviceProvider, target.PostprocessModelBinder, metadata) as IModelPostbinder;
        }
    }
}
