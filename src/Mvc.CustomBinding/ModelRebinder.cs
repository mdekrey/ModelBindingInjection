using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Mvc.CustomBinding
{
    internal class ModelRebinder : IModelBinder
    {
        private readonly IModelBinder binder;
        private readonly IModelBinder postbinder;

        public ModelRebinder(IModelBinder binder, IModelBinder postbinder)
        {
            this.binder = binder;
            this.postbinder = postbinder;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            await binder.BindModelAsync(bindingContext);
            var rebindingContext = ModelPostbindingContext.CreateBindingContext(bindingContext.ActionContext, bindingContext.ValueProvider, bindingContext.ModelMetadata, new BindingInfo(), bindingContext.ModelName, bindingContext.Result.Model);
            await postbinder.BindModelAsync(rebindingContext);
        }
    }
}