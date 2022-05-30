using System.Collections.Immutable;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    public class SecurityProblemException : ProblemDetailsException
    {
        public SecurityProblemException(string title, string details, params (string key, string value)[] extensions) : base(StatusCodes.Status400BadRequest, title, details, extensions)
        {
        }

        public SecurityProblemException(string title, string details, IImmutableDictionary<string, string> extensions) : base(StatusCodes.Status400BadRequest, title, details, extensions)
        {
        }
    }
}
