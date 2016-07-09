using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Mvc.CustomBinding
{
    public class CustomModelBinderProvider : IModelBinderProvider
    {
        public static readonly BindingSource BindingSource = new BindingSource(
            "Custom",
            "Custom",
            isGreedy: true,
            isFromRequest: false);

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (typeof(object) == context.Metadata.ModelType) //(context.BindingInfo.BindingSource == BindingSource)
            {
                return new CustomModelBinder(context.Metadata);
            }

            return null;
        }
    }
}
