using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModelBindingInjection
{
    /// <summary>
    /// Tells the postprocess binding framework that the class which the attribute decorates should be
    /// post-process bound. Without this, only top-level classes will be checked for binding.
    /// 
    /// Also may be applied to properties to force the children to be post-process bound.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class RecurseModelBindingInjectionAttribute : Attribute
    {
    }
}
