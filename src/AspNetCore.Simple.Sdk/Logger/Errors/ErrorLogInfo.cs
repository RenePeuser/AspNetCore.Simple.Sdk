using System.Collections.Generic;
using System.Collections.Immutable;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public class ErrorLogInfo
    {
        public ErrorLogInfo(string message,
                            string title,
                            IEnumerable<string> stackTrace,
                            IImmutableDictionary<string, string> requestInfos)
        {
            Title = title;
            Message = message;
            StackTrace = stackTrace;
            RequestInfos = requestInfos;
        }

        public string Title { get; }

        public string Message { get; }

        public IImmutableDictionary<string, string> RequestInfos { get; }

        public IEnumerable<string> StackTrace { get; }
    }
}
