using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomBindingDemo.BaseModels
{
    public class Nothing
    {
    }

    public class Nothing<T> : IPostbindingFor<T>
    {

    }
}
