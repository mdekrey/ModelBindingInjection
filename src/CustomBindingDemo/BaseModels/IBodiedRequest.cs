using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomBindingDemo.BaseModels
{
    public interface IBodiedRequest<out TBody>
    {
        TBody Body { get; }
    }
}
