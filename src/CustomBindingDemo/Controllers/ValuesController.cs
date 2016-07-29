using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mvc.CustomBinding;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace CustomBindingDemo.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        [RecursePostprocessBinding]
        public class Shallow : IValidatableObject
        {
            [Required]
            [PostprocessBinding(typeof(CustomModelBinder))]
            public object Value { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if ((Value as string) != typeof(Shallow).FullName)
                {
                    yield return new ValidationResult("The Value must match the FullName");
                }
            }
        }

        [RecursePostprocessBinding]
        public class Deep
        {
            public Shallow[] Shallow { get; set; }

            [PostprocessBinding(typeof(CustomModelBinder))]
            public object Value { get; set; }

            [PostprocessBinding(typeof(RouteModelBinder))]
            public object Route { get; set; }
        }

        public class RequestRoute
        {
            [MinLength(2)]
            public string Id { get; set; }
        }

        [RecursePostprocessBinding]
        public class FullRequest : ICanPostbind
        {
            [FromRoute(Name = "Route")]
            public RequestRoute Route { get; set; }

            [FromBody]
            public Deep Body { get; set; }

            [PostprocessBinding(typeof(CustomModelBinder))]
            public object Value { get; set; }

            public Task<bool> CanPostbind(ModelBindingContext bindingContext)
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
                var invalid = intermediate.FindKeysWithPrefix("Route").Any(entries => entries.Value.ValidationState == ModelValidationState.Invalid);
                if (invalid)
                {
                    throw new InvalidOperationException();
                }
                return Task.FromResult(!invalid);
            }
        }

        [HttpPost("blob")]
        public Tuple<Deep, string> Post([FromBody] Deep value, [PostprocessBinding(typeof(CustomModelBinder))] string other)
        {
            return Tuple.Create(value, other);
        }

        // PUT api/values/5
        [HttpPut("{route.id}")]
        public FullRequest Put(FullRequest rq)
        {
            if (!this.ModelState.IsValid)
            {
                throw new ValidationException();
            }
            return rq;
        }
    }
}
