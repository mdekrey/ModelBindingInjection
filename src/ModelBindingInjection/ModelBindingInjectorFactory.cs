using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace ModelBindingInjection
{
    /// <summary>
    /// Builds a model post-binder
    /// </summary>
    public class ModelBindingInjectorFactory : IModelBindingInjectorFactory
    {
        private delegate IModelBindingInjector PostbinderFactory(ModelMetadata metadata, ModelBindingInjectorAttribute attribute);

        private readonly IServiceProvider serviceProvider;
        private readonly ConcurrentDictionary<Type, PostbinderFactory> postbinderFactories
            = new ConcurrentDictionary<Type, PostbinderFactory>();
        private static readonly Type[] injectedMetadata = new[] { typeof(ModelMetadata) };

        /// <summary>
        /// Builds a model postbinder factory
        /// </summary>
        /// <param name="serviceProvider">The services with which to construct the model postbinders</param>
        public ModelBindingInjectorFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Builds a specific model postbinder
        /// </summary>
        /// <param name="metadata">The metadata of the property to be bound. Determines the type of the model postbinder and is passed
        /// via dependency injection to the model postbinder</param>
        /// <returns>A model postbinder, or null if none is applicable</returns>
        public IModelBindingInjector GetModelBinder(ModelMetadata metadata)
        {
            var attributes = (metadata as DefaultModelMetadata).Attributes.Attributes.OfType<ModelBindingInjectorAttribute>().ToArray();
            if (attributes.Length != 1)
            {
                throw new InvalidOperationException("Exactly 1 attribute must be provided of type " + nameof(ModelBindingInjectorAttribute));
            }
            var target = attributes[0];
            return postbinderFactories.GetOrAdd(target.PostprocessModelBinder, BuildPostbinderFactory(target.GetType()))(metadata, target);
        }

        private Func<Type, PostbinderFactory> BuildPostbinderFactory(Type attributeType)
        {
            return (Type targetType) =>
            {
                try
                {
                    try
                    {
                        var factory = ActivatorUtilities.CreateFactory(targetType, new[] { typeof(ModelMetadata), attributeType });
                        return (metadata, attribute) => factory(serviceProvider, new object[] { metadata, attribute }) as IModelBindingInjector;
                    }
                    catch
                    {
                        var factory = ActivatorUtilities.CreateFactory(targetType, new[] { attributeType });
                        return (metadata, attribute) => factory(serviceProvider, new object[] { attribute }) as IModelBindingInjector;
                    }
                }
                catch
                {
                    try
                    {
                        var factory = ActivatorUtilities.CreateFactory(targetType, injectedMetadata);
                        return (metadata, attribute) => factory(serviceProvider, new object[] { metadata }) as IModelBindingInjector;
                    }
                    catch
                    {
                        var factory = ActivatorUtilities.CreateFactory(targetType, Array.Empty<Type>());
                        return (metadata, attribute) => factory(serviceProvider, Array.Empty<object>()) as IModelBindingInjector;
                    }
                }
            };
        }
    }
}
