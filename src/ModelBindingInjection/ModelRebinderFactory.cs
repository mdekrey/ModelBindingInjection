using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ModelBindingInjection
{
    class ModelRebinderFactory : IModelRebinderFactory
    {
        public IModelBinder CreateRebinder(IModelBinder first, IModelBinder second)
        {
            return new ModelRebinder(first, second);
        }
    }
}
