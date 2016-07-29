using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Mvc.CustomBinding
{
    internal class ModelRebinder : IModelBinder
    {
        private readonly IModelBinder first;
        private readonly IModelBinder second;

        public ModelRebinder(IModelBinder first, IModelBinder second)
        {
            this.first = first;
            this.second = second;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            await first.BindModelAsync(bindingContext);
            
            await second.BindModelAsync(bindingContext);
        }
    }
}