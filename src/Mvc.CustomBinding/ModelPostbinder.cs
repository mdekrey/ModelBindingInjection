using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mvc.CustomBinding
{
    class ModelPostbinder : IModelBinder
    {
        private readonly IModelPostbinder postbinder;

        public ModelPostbinder(IModelPostbinder postbinder)
        {
            this.postbinder = postbinder;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var target = bindingContext.Result.Model;

            if (target is ICanPostbind)
            {
                if (!await (target as ICanPostbind).CanPostbind(bindingContext))
                {
                    return;
                }
            }

            var postbindingContext = ModelPostbindingContext.CreateBindingContext(bindingContext.ActionContext, bindingContext.ValueProvider, bindingContext.ModelMetadata, new BindingInfo(), bindingContext.ModelName, target);
            await postbinder.BindModelAsync(postbindingContext);
        }
    }
}
