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
        public static readonly BindingSource Source = new BindingSource("PostprocessBinding", "PostprocessBinding", true, false);

        public PostprocessBindingAttribute(Type postprocessModelBinder)
        {
            this.PostprocessModelBinder = postprocessModelBinder;
        }

        public BindingSource BindingSource => Source;

        public Type PostprocessModelBinder { get; }
    }
}
