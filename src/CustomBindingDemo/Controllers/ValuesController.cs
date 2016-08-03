using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ModelBindingInjection;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using CustomBindingDemo.ModelBinders;
using CustomBindingDemo.BaseModels;

namespace CustomBindingDemo.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        [RecurseModelBindingInjection]
        public class Shallow : IValidatableObject
        {
            [Required]
            [ModelBindingInjector(typeof(CustomModelBinder))]
            public object Value { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if ((Value as string) != typeof(Shallow).FullName)
                {
                    yield return new ValidationResult("The Value must match the FullName");
                }
            }
        }

        [RecurseModelBindingInjection]
        public class Deep
        {
            public Shallow[] Shallow { get; set; }

            [ModelBindingInjector(typeof(CustomModelBinder))]
            public object Value { get; set; }
        }

        public class ShallowBody
        {
        }

        public class DeepBody
        {
            public ShallowBody[] Shallow { get; set; }
        }

        public class DeepBodyBind : IPostbindingFor<DeepBody>
        {
            [ModelBindingInjector(typeof(BodyModelBinder))]
            public DeepBody Body { get; set; }

            [ModelBindingInjector(typeof(CustomModelBinder))]
            public object Value { get; set; }

            [OverrideInjector("Constant!")]
            public string Overridden { get; set; }
        }

        public class RequestRoute
        {
            [MinLength(2)]
            public string Id { get; set; }
        }

        [RecurseModelBindingInjection]
        public class FullRequest
        {
            [FromRoute(Name = "Route")]
            public RequestRoute Route { get; set; }

            [FromBody]
            public Deep Body { get; set; }

            [ModelBindingInjector(typeof(CustomModelBinder))]
            public object Value { get; set; }
        }

        [HttpPost("simple")]
        public Tuple<Deep, string> Post([FromBody] Deep value, [ModelBindingInjector(typeof(CustomModelBinder))] string other)
        {
            return Tuple.Create(value, other);
        }

        // PUT api/values/simple/51
        [HttpPut("simple/{route.id}")]
        public FullRequest Put(FullRequest rq)
        {
            if (!this.ModelState.IsValid)
            {
                throw new ValidationException();
            }
            return rq;
        }


        // PUT api/values/complex/51
        [HttpPut("complex/{route.id}")]
        public object PutComplex(BodiedRequest<RequestRoute, Nothing<RequestRoute>, DeepBody, DeepBodyBind> rq)
        {
            if (!this.ModelState.IsValid)
            {
                throw new ValidationException();
            }
            return rq;
        }
    }
}
