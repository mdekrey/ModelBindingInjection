using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Mvc.CustomBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CustomBindingServiceCollectionExtensions
    {
        public static void AddCustomBinding(this IServiceCollection services)
        {
            AddCustomBindingServices(services);

        }

        private static void AddCustomBindingServices(IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, CustomBindingMvcOptionsSetup>());
        }


    }
}
