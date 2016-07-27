using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Mvc.CustomBinding
{
    public interface IPostprocessBinderFactory
    {
        IModelBinder GetModelBinder(ModelMetadata metadata);
    }
}
