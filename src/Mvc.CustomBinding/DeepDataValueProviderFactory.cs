using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Mvc.CustomBinding
{
    public class DeepDataValueProviderFactory : IValueProviderFactory
    {
        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            //var valueProvider = new DeepDataValueProvider(context);

            //context.ValueProviders.Add(valueProvider);

            return TaskCache.CompletedTask;
        }
    }
}
