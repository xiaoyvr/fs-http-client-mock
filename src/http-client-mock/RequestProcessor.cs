using System;

 namespace HttpClientMock
{
    internal class RequestProcessor<TBody> : RequestProcessorBase
    {
        private readonly Func<RequestCapture, TBody, bool> matchFunc;

        public RequestProcessor(Func<RequestCapture, TBody, bool> matchFunc)
        {
            this.matchFunc = matchFunc;
        }
        
        protected override RequestCapture DoProcess(HttpRequestMessageWrapper httpRequestMessageWrapper)
        {
            var requestCapture = httpRequestMessageWrapper.GetRequestCapture();
            var matched = matchFunc(requestCapture, requestCapture.Body<TBody>());
            if (!matched) 
                return null;
            return requestCapture;
        }
    }
}