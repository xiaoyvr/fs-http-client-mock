using System.Collections.Generic;

namespace HttpClientMock
{
    internal abstract class RequestProcessorBase : IRequestProcessor
    {
        public bool Process(HttpRequestMessageWrapper httpRequestMessageWrapper)
        {
            var capture = DoProcess(httpRequestMessageWrapper);
            if (capture != null)
            {
                Captures.Add(capture);
            }
            return capture != null;
        }
        protected abstract RequestCapture DoProcess(HttpRequestMessageWrapper httpRequestMessageWrapper);
        public List<RequestCapture> Captures { get; set; } = new List<RequestCapture>();
    }
}