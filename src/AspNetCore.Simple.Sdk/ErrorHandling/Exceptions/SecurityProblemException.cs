using System.Collections.Immutable;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    public class SecurityProblemException : ProblemDetailsException
    {
        public SecurityProblemException(string title, string details, params (string key, object value)[] extensions) : base(StatusCodes.Status400BadRequest, title, details, extensions)
        {
        }

        public SecurityProblemException(string title, string details, IImmutableDictionary<string, object> extensions) : base(StatusCodes.Status400BadRequest, title, details, extensions)
        {
        }
    }
}
