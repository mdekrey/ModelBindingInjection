using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Mvc.CustomBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to register postprocess binding
    /// </summary>
    public static class PostprocessServiceCollectionExtensions
    {
        /// <summary>
        /// Adds post-process binding to the model binder. This allows for additional binding to occur after standard binding,
        /// but prior to validation.
        /// 
        /// This preserves any custom IModelBinderFactory that may already be registered, or uses the default if none is registered.
        /// </summary>
        /// <param name="services">The service collection to which </param>
        public static void AddPostprocessBinding(this IServiceCollection services)
        {
            services.TryAddSingleton<IPostprocessBinderFactory, PostprocessBinderFactory>();
            services.TryAddSingleton<IModelRebinderFactory, ModelRebinderFactory>();

            services.Decorate(DefaultFactory, originalFactory => provider =>
                ActivatorUtilities.CreateInstance<PostprocessingBinderFactory>(provider, originalFactory(provider)));
        }

        private static Func<IServiceProvider, IModelBinderFactory> DefaultFactory(IServiceCollection services)
        {
            services.AddSingleton< ModelBinderFactory>();
            return provider => provider.GetService<ModelBinderFactory>();
        }

        /// <summary>
        /// Decorates an existing registration
        /// </summary>
        /// <typeparam name="T">The type of the registration to decorate</typeparam>
        /// <param name="defaultFactory">The default factory to use, if any</param>
        /// <param name="services">The services containing the registration and where the new registration should reside</param>
        /// <param name="decorator">The replacing decorator</param>
        public static void Decorate<T>(this IServiceCollection services, Func<IServiceCollection, Func<IServiceProvider, T>> defaultFactory, Func<Func<IServiceProvider, T>, Func<IServiceProvider, T>> decorator)
            where T : class
        {
            services.AddSingleton(decorator(services.PrepareDecorateService(defaultFactory)));
        }

        private static Func<IServiceProvider, T> PrepareDecorateService<T>(this IServiceCollection services, Func<IServiceCollection, Func<IServiceProvider, T>> defaultFactory)
            where T : class
        {
            var target = services.LastOrDefault(sd => sd.ServiceType == typeof(T));
            if (target?.ImplementationFactory != null)
            {
                return (Func<IServiceProvider, T>)target.ImplementationFactory;
            }
            else if (target?.ImplementationInstance != null)
            {
                return provider => (T)target.ImplementationInstance;
            }
            else if (target?.ImplementationType != null)
            {
                var targetType = target?.ImplementationType;
                services.AddSingleton(targetType);
                return provider => (T)provider.GetService(targetType);
            }
            else if (defaultFactory != null)
            {
                return defaultFactory(services);
            }
            else
            {
                return null;
            }
        }
    }
}
