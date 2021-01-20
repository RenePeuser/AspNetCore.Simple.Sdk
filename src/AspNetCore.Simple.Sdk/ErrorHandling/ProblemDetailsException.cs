using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    public class ProblemDetailsException : Exception
    {
        public ProblemDetailsException(int statusCode,
                                       string title,
                                       string details,
                                       params (string key, object value)[] extensions) : base(title)
        {
            ProblemDetails = new ProblemDetails
            {
                Title = title,
                Detail = details,
                Status = statusCode
            };

            foreach (var extension in extensions.Select(item => new KeyValuePair<string, object>(item.key, item.value)))
            {
                ProblemDetails.Extensions.Add(extension);
            }
        }

        internal ProblemDetails ProblemDetails { get; }
    }
}
