using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AspNetCore.Simple.Sdk.Extensions
{
    internal static class ActionDescriptorExtensions
    {
        internal static IEnumerable<ParameterDescriptor> GetQueryParameters(this ActionDescriptor actionDescriptor)
        {
            return actionDescriptor.Parameters.Where(p => p.BindingInfo.BindingSource.EqualsTo(BindingSource.Query));
        }
    }
}