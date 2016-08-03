using System;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;
using System.Linq;

namespace CustomBindingDemo
{
    internal class FixPathOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            foreach (var pathVariable in operation.Parameters.Where(p => p.In == "path"))
            {
                var index = context.ApiDescription.RelativePath.IndexOf("{" + pathVariable.Name + "}", StringComparison.OrdinalIgnoreCase);
                if (index == -1)
                {
                    continue;
                }
                pathVariable.Name = context.ApiDescription.RelativePath.Substring(index + 1, pathVariable.Name.Length);
            }
        }
    }
}