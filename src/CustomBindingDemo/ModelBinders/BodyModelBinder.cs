using CustomBindingDemo.BaseModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Mvc.CustomBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomBindingDemo.ModelBinders
{
    public class BodyModelBinder : IModelPostbinder
    {
        public Task BindModelAsync(ModelPostbindingContext bindingContext)
        {
            bindingContext.Result = ModelBindingResult.Success((bindingContext.OriginalModel as IBodiedRequest<object>)?.Body);
            return Microsoft.AspNetCore.Mvc.Internal.TaskCache.CompletedTask;
        }
    }
}
