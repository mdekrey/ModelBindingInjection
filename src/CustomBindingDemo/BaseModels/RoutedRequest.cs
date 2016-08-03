using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using ModelBindingInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomBindingDemo.BaseModels
{
    [RecurseModelBindingInjection]
    public class RoutedRequest<TRoute, TRoutePostbind> : IRoutedRequest<TRoute>, IPreModelBindingInjection
        where TRoutePostbind : IPostbindingFor<TRoute>
    {
        [FromRoute]
        public TRoute Route { get; set; }

        [RecurseModelBindingInjection]
        public TRoutePostbind RouteData { get; set; }


        public virtual Task<bool> CanInjectBoundModel(ModelBindingContext bindingContext)
        {
            var intermediate = GetCurrentValidations(bindingContext);
            var invalid = intermediate
                .FindKeysWithPrefix(nameof(Route))
                .Any(entries => entries.Value.ValidationState == ModelValidationState.Invalid);
            return Task.FromResult(!invalid);
        }

        protected static ModelStateDictionary GetCurrentValidations(ModelBindingContext bindingContext)
        {
            var services = bindingContext.HttpContext.RequestServices;
            var modelMetadataProvider = services.GetRequiredService<IModelMetadataProvider>();
            var validatorProviders = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<MvcOptions>>().Value.ModelValidatorProviders;
            var validatorProvider = new Microsoft.AspNetCore.Mvc.ModelBinding.Validation.CompositeModelValidatorProvider(validatorProviders);
            var validationVisitor = new Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidationVisitor(bindingContext.ActionContext,
                validatorProvider,
                new Microsoft.AspNetCore.Mvc.Internal.ValidatorCache(),
                modelMetadataProvider,
                null);

            validationVisitor.Validate(bindingContext.ModelMetadata, string.Empty, bindingContext.Result.Model ?? bindingContext.Model);
            var intermediate = new ModelStateDictionary(bindingContext.ActionContext.ModelState);
            bindingContext.ActionContext.ModelState.Clear();
            return intermediate;
        }
    }
}
