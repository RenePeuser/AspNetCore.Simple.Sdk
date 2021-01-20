using System.Collections.Generic;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public class ErrorLogInfo
    {
        public ErrorLogInfo(string message,
                            string title,
                            IEnumerable<string> stackTrace,
                            Dictionary<string, object> requestInfos)
        {
            Title = title;
            Message = message;
            StackTrace = stackTrace;
            RequestInfos = requestInfos;
            ErrorType = "PulseError";
        }

        public string Title { get; }

        public string Message { get; }

        // Gets our unique identifier to find all our errors in cloud watch via search query
        public string ErrorType { get; }

        public Dictionary<string, object> RequestInfos { get; }

        public IEnumerable<string> StackTrace { get; }
    }
}
