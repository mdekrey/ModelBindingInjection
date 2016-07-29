using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Mvc.CustomBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomBindingDemo.BaseModels
{
    public class BodiedRequest<TRoute, TRoutePostbind, TBody, TBodyPostbind> : RoutedRequest<TRoute, TRoutePostbind>, IBodiedRequest<TBody>
        where TRoutePostbind : IPostbindingFor<TRoute>
        where TBodyPostbind : IPostbindingFor<TBody>
    {
        [FromBody]
        public TBody Body { get; set; }

        [RecursePostprocessBinding]
        public TBodyPostbind BodyData { get; set; }

        public override Task<bool> CanPostbind(ModelBindingContext bindingContext)
        {
            var intermediate = GetCurrentValidations(bindingContext);
            var invalid = intermediate
                .FindKeysWithPrefix(nameof(Route))
                .Concat(intermediate
                    .FindKeysWithPrefix(nameof(Body)))
                .Any(entries => entries.Value.ValidationState == ModelValidationState.Invalid);
            return Task.FromResult(!invalid);
        }
    }
}
