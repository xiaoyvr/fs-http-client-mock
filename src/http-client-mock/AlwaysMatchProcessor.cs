namespace HttpClientMock
{
    internal class AlwaysMatchProcessor : RequestProcessorBase
    {
        protected override RequestCapture DoProcess(HttpRequestMessageWrapper httpRequestMessageWrapper)
        {
            return httpRequestMessageWrapper.GetRequestCapture();
        }
    }
}