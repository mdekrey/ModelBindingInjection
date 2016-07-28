using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Mvc.CustomBinding
{
    public class PostprocessBinderFactory : IPostprocessBinderFactory
    {
        private readonly IServiceProvider serviceProvider;

        public PostprocessBinderFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IModelPostbinder GetModelBinder(ModelMetadata metadata)
        {
            var attributes = (metadata as DefaultModelMetadata).Attributes.Attributes.OfType<PostprocessBindingAttribute>().ToArray();
            if (attributes.Length != 1)
            {
                throw new InvalidOperationException("Exactly 1 attribute must be provided of type " + nameof(PostprocessBindingAttribute));
            }
            var target = attributes[0];
            return ActivatorUtilities.CreateInstance(this.serviceProvider, target.PostprocessModelBinder, metadata) as IModelPostbinder;
        }
    }
}
