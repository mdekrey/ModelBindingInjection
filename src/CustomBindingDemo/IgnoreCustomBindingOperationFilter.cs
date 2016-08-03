using System;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;
using System.Linq;

namespace CustomBindingDemo
{
    internal class IgnoreCustomBindingOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var toRemove = operation.Parameters.Where(p => p.In == "modelbinding").ToArray();
            foreach (var removed in toRemove)
            {
                operation.Parameters.Remove(removed);
            }
        }
    }
}