using System.Collections.Generic;

namespace HttpClientMock
{
    internal interface IRequestProcessor
    {
        bool Process(HttpRequestMessageWrapper httpRequestMessageWrapper);
        List<RequestCapture> Captures { get; set; }
    }
}