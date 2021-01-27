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
        }

        public string Title { get; }

        public string Message { get; }

        public Dictionary<string, object> RequestInfos { get; }

        public IEnumerable<string> StackTrace { get; }
    }
}
