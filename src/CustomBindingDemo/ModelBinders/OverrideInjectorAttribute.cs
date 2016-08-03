using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModelBindingInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomBindingDemo.ModelBinders
{
    public class OverrideInjectorAttribute : ModelBindingInjectorAttribute
    {
        public OverrideInjectorAttribute(string value)
            : base(typeof(OverrideModelBinder))
        {
            Value = value;
        }

        public string Value { get; }

        public class OverrideModelBinder : IModelBindingInjector
        {
            private readonly string result;

            public OverrideModelBinder(OverrideInjectorAttribute attr)
            {
                this.result = attr.Value;
            }

            public Task BindModelAsync(ModelBindingInjectorContext bindingContext)
            {
                bindingContext.Result = ModelBindingResult.Success(result);
                return Microsoft.AspNetCore.Mvc.Internal.TaskCache.CompletedTask;
            }
        }
    }
}
