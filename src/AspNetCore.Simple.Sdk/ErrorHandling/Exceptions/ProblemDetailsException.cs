using System;
using System.Collections.Immutable;
using System.Net;
using Extensions.Pack;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    public class ProblemDetailsException : Exception
    {
        public ProblemDetailsException(string title,
                                       string details,
                                       params (string key, string value)[] extensions) : this(HttpStatusCode.InternalServerError, title, details, extensions)
        {
        }

        public ProblemDetailsException(HttpStatusCode statusCode,
                                       string title,
                                       string details,
                                       params (string key, string value)[] extensions) : this(statusCode.ToInt(), title, details, extensions.ToImmutableDictionary(item => item.key, item => item.value.ToString()))
        {
        }

        public ProblemDetailsException(int statusCode,
                                       string title,
                                       string details,
                                       params (string key, string value)[] extensions) : this(statusCode.ToInt(), title, details, extensions.ToImmutableDictionary(item => item.key, item => item.value.ToString()))
        {
        }

        // i know this is evil with the conversion to immutable dictionary but a fast fix for now.

        public ProblemDetailsException(int statusCode,
                                       string title,
                                       string details,
                                       IImmutableDictionary<string, string> errorDetails) : base(title)
        {
            ProblemDetails = new ProblemDetails(title, details, statusCode, errorDetails);
        }

        public ProblemDetails ProblemDetails { get; }
    }

    public record ProblemDetails(string Title,
                                 string Details,
                                 int StatusCode,
                                 IImmutableDictionary<string, string> ErrorDetails);
}
