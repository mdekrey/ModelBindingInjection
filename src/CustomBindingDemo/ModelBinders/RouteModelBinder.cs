using CustomBindingDemo.BaseModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Mvc.CustomBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomBindingDemo.ModelBinders
{
    public class RouteModelBinder : IModelPostbinder
    {
        public Task BindModelAsync(ModelPostbindingContext bindingContext)
        {
            bindingContext.Result = ModelBindingResult.Success((bindingContext.OriginalModel as IRoutedRequest<object>)?.Route);
            return Microsoft.AspNetCore.Mvc.Internal.TaskCache.CompletedTask;
        }
    }
}
