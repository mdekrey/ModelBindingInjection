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
    public static class PostprocessServiceCollectionExtensions
    {
        public static void AddPostprocessBinding(this IServiceCollection services)
        {
            services.AddSingleton<IPostprocessBinderFactory, PostprocessBinderFactory>();

            services.Decorate<IModelBinderFactory>(originalFactory => provider =>
                new PostprocessingBinderFactory(provider.GetService<IPostprocessBinderFactory>(), originalFactory(provider)));
        }

        public static void Decorate<T>(this IServiceCollection services, Func<Func<IServiceProvider, T>, Func<IServiceProvider, T>> decorator)
            where T : class
        {
            services.AddSingleton(decorator(PrepareDecorateService<T>(services)));
        }

        private static Func<IServiceProvider, T> PrepareDecorateService<T>(IServiceCollection services)
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
            else
            {
                var targetType = target?.ImplementationType ?? typeof(ModelBinderFactory);
                services.AddSingleton(targetType);
                return provider => (T)provider.GetService(targetType);
            }
        }
    }
}
