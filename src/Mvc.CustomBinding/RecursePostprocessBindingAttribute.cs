using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mvc.CustomBinding
{
    /// <summary>
    /// Tells the postprocess binding framework that the class which the attribute decorates should be
    /// post-process bound. Without this, only top-level classes will be checked for binding.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RecursePostprocessBindingAttribute : Attribute
    {
    }
}
