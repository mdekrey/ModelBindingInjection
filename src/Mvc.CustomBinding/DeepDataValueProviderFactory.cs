using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Mvc.CustomBinding
{
    public class DeepDataValueProviderFactory : IValueProviderFactory
    {
        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            var modelMetadataProvider = context.ActionContext.HttpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();

            var valueProvider = new DeepDataValueProvider(); // context);

            context.ValueProviders.Add(valueProvider);

            var parameterInfo = from param in context.ActionContext.ActionDescriptor.Parameters
                                select new
                                {
                                    BindingInfo = param.BindingInfo,
                                    Metadata = modelMetadataProvider.GetMetadataForType(param.ParameterType)
                                };


            return TaskCache.CompletedTask;
        }
    }
}
