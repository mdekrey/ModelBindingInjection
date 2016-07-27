using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mvc.CustomBinding
{
    public class CustomModelBinder : IModelBinder
    {
        private readonly object result;

        public CustomModelBinder(ModelMetadata metadata)
        {
            this.result = (metadata.ModelType == typeof(string) || metadata.ModelType == typeof(object))
                ? (metadata.ContainerType?.FullName ?? "root")
                : Activator.CreateInstance(metadata.ModelType);
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            bindingContext.Result = ModelBindingResult.Success(result);
            return Microsoft.AspNetCore.Mvc.Internal.TaskCache.CompletedTask;
        }
    }
}
