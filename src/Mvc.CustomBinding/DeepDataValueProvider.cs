using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mvc.CustomBinding
{
    public class DeepDataValueProvider : IValueProvider
    {
        //private ModelMetadata modelMetadata;
        //private string modelName;

        //public DeepDataValueProvider(string modelName, ModelMetadata modelMetadata)
        //{
        //    this.modelName = modelName;
        //    this.modelMetadata = modelMetadata;
        //}

        public bool ContainsPrefix(string prefix)
        {
            return prefix == "other";
        }

        public ValueProviderResult GetValue(string key)
        {
            return new ValueProviderResult(new Microsoft.Extensions.Primitives.StringValues("here"));
        }
    }
}
