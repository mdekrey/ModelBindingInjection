using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Mvc.CustomBinding
{
    internal class CustomModelRebinder : IModelBinder
    {
        private readonly IModelBinder binder;
        private readonly IModelBinder rebinder;

        public CustomModelRebinder(IModelBinder binder, IModelBinder rebinder)
        {
            this.binder = binder;
            this.rebinder = rebinder;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            await binder.BindModelAsync(bindingContext);
            var rebindingContext = DefaultModelBindingContext.CreateBindingContext(bindingContext.ActionContext, bindingContext.ValueProvider, bindingContext.ModelMetadata, new BindingInfo(), bindingContext.ModelName);
            rebindingContext.Model = bindingContext.Result.Model;
            await rebinder.BindModelAsync(rebindingContext);
        }
    }
}