using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

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

            //if (context.Metadata.ModelType.GetTypeInfo().GetCustomAttribute<DeepDataAttribute>() != null)
            if (context.BindingInfo.BindingSource == DeepDataAttribute.Source)
            {
                return new CustomModelBinder(context.Metadata);
            }
            //else if (context.Metadata.ModelType.GetTypeInfo().GetCustomAttribute<DeepDataRecurseAttribute>() != null)
            //{
            //    return new Microsoft.AspNetCore.Mvc.ModelBinding.Binders.ComplexTypeModelBinderProvider().GetBinder(context);
            //}
            //if (typeof(object) == context.Metadata.ModelType) //(context.BindingInfo.BindingSource == BindingSource)
            //{
            //}

            return null;
        }
    }
}
