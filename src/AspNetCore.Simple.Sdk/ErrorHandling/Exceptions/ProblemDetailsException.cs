using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    public class ProblemDetailsException : Exception
    {
        public ProblemDetailsException(string title,
                                       string details,
                                       params (string key, object value)[] extensions) : this(StatusCodes.Status500InternalServerError, title, details, extensions)
        {
        }

        // i know this is evil with the conversion to immutable dictionary but a fast fix for now.
        public ProblemDetailsException(int statusCode,
                                       string title,
                                       string details,
                                       params (string key, object value)[] extensions) : this(statusCode, title, details, extensions.ToImmutableDictionary(item => item.key, item => item.value))
        {
        }

        public ProblemDetailsException(int statusCode,
                                       string title,
                                       string details,
                                       IImmutableDictionary<string, object> errorDetails) : base(title)
        {
            ProblemDetails = new PulseProblemDetails(title, details, statusCode, errorDetails);
        }

        public PulseProblemDetails ProblemDetails { get; }
    }

    public record PulseProblemDetails(string Title,
                                      string Details,
                                      int StatusCode,
                                      IImmutableDictionary<string, object> ErrorDetails);
}
