using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mvc.CustomBinding
{
    public class CustomModelBinder : IModelBinder
    {
        private ModelMetadata metadata;

        public CustomModelBinder(ModelMetadata metadata)
        {
            this.metadata = metadata;
        }
        
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var container = (bindingContext as RebindingModelBindingContext)?.ContainerModelMetadata;

            bindingContext.Result = ModelBindingResult.Success((bindingContext.ModelType == typeof(string) || bindingContext.ModelType == typeof(object)) ? (container?.ModelType.FullName ?? "root") : Activator.CreateInstance(bindingContext.ModelType));
            return Microsoft.AspNetCore.Mvc.Internal.TaskCache.CompletedTask;
        }
    }
}
