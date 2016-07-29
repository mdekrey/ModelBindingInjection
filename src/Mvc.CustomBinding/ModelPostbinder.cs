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
            var postbindingContext = ModelPostbindingContext.CreateBindingContext(bindingContext.ActionContext, bindingContext.ValueProvider, bindingContext.ModelMetadata, new BindingInfo(), bindingContext.ModelName, bindingContext.Result.Model);
            await postbinder.BindModelAsync(postbindingContext);
        }
    }
}
