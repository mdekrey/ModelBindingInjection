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
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, PostprocessMvcOptionsSetup>());
            services.AddSingleton<IModelBinderFactory, PostprocessingBinderFactory>(sp => new PostprocessingBinderFactory(sp.GetService<ModelBinderFactory>()));
            services.AddSingleton<ModelBinderFactory>();
        }


    }
}
