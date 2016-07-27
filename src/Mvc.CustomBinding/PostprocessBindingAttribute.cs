using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mvc.CustomBinding
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
    public class PostprocessBindingAttribute : Attribute, IBindingSourceMetadata
    {
        public static readonly BindingSource Source = new BindingSource("DeepData", "DeepData", true, false);

        public BindingSource BindingSource
        {
            get
            {
                return Source;
            }
        }

        public Type PostprocessModelBinder => typeof(CustomModelBinder);
    }
}
