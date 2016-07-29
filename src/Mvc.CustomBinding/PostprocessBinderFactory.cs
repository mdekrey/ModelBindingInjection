using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Mvc.CustomBinding
{
    /// <summary>
    /// Builds a model post-binder
    /// </summary>
    public class PostprocessBinderFactory : IPostprocessBinderFactory
    {
        private delegate IModelPostbinder PostbinderFactory(ModelMetadata metadata);

        private readonly IServiceProvider serviceProvider;
        private readonly ConcurrentDictionary<Type, PostbinderFactory> postbinderFactories
            = new ConcurrentDictionary<Type, PostbinderFactory>();
        private static readonly Type[] injectedMetadata = new[] { typeof(ModelMetadata) };

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
            return postbinderFactories.GetOrAdd(target.PostprocessModelBinder, BuildPostbinderFactory)(metadata);
        }

        private PostbinderFactory BuildPostbinderFactory(Type targetType)
        {
            try
            {
                var factory = ActivatorUtilities.CreateFactory(targetType, injectedMetadata);
                return metadata => factory(serviceProvider, new object[] { metadata }) as IModelPostbinder;
            }
            catch
            {
                var factory = ActivatorUtilities.CreateFactory(targetType, Array.Empty<Type>());
                return metadata => factory(serviceProvider, Array.Empty<object>()) as IModelPostbinder;
            }
        }
    }
}
