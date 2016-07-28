﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mvc.CustomBinding;
using System.ComponentModel.DataAnnotations;

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
        }

        public class RequestRoute
        {
            public string Id { get; set; }
        }

        [RecursePostprocessBinding]
        public class FullRequest
        {
            [FromRoute(Name = "")]
            public RequestRoute Route { get; set; }

            [FromBody]
            public Deep Body { get; set; }

            [PostprocessBinding(typeof(CustomModelBinder))]
            public object Value { get; set; }
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        [HttpPost("blob")]
        public Tuple<Deep, string> Post([FromBody] Deep value, [PostprocessBinding(typeof(CustomModelBinder))] string other)
        {
            return Tuple.Create(value, other);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public FullRequest Put(FullRequest rq)
        {
            if (!this.ModelState.IsValid)
            {
                throw new ValidationException();
            }
            return rq;
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
