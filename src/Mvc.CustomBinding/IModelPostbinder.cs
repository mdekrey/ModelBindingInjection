using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mvc.CustomBinding
{
    public interface IModelPostbinder
    {
        Task BindModelAsync(ModelPostbindingContext bindingContext);
    }
}
