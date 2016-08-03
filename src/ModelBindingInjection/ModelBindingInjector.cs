using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModelBindingInjection
{
    class ModelBindingInjector : IModelBinder
    {
        private readonly IModelBindingInjector postbinder;

        public ModelBindingInjector(IModelBindingInjector postbinder)
        {
            this.postbinder = postbinder;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var target = bindingContext.Result.Model;

            if (target is IPreModelBindingInjection)
            {
                if (!await (target as IPreModelBindingInjection).CanInjectBoundModel(bindingContext))
                {
                    return;
                }
            }

            var postbindingContext = ModelBindingInjectorContext.CreateBindingContext(bindingContext.ActionContext, bindingContext.ValueProvider, bindingContext.ModelMetadata, new BindingInfo(), bindingContext.ModelName, target);
            await postbinder.BindModelAsync(postbindingContext);
        }
    }
}
