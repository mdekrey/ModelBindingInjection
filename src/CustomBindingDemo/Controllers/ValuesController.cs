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
        [DeepDataRecurse]
        public class Shallow
        {
            [Required]
            [DeepData]
            public object Value { get; set; }
        }

        [DeepDataRecurse]
        public class Deep
        {
            public Shallow[] Shallow { get; set; }

            [DeepData]
            public object Value { get; set; }
        }

        public class RequestRoute
        {
            [FromRoute]
            public string Id { get; set; }
        }

        [DeepDataRecurse]
        public class FullRequest : RequestRoute
        {
            [FromBody]
            public Deep Body { get; set; }

            [DeepData]
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
        public Deep Post([FromBody] Deep value, [DeepData] string other)
        {
            return value;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public FullRequest Put(FullRequest rq)
        {
            return rq;
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
