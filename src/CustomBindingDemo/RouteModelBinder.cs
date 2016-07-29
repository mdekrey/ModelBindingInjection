using Microsoft.AspNetCore.Mvc.ModelBinding;
using Mvc.CustomBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CustomBindingDemo.Controllers.ValuesController;

namespace CustomBindingDemo
{
    public class RouteModelBinder : IModelPostbinder
    {

        public Task BindModelAsync(ModelPostbindingContext bindingContext)
        {
            bindingContext.Result = ModelBindingResult.Success((bindingContext.OriginalModel as FullRequest)?.Route);
            return Microsoft.AspNetCore.Mvc.Internal.TaskCache.CompletedTask;
        }
    }
}
