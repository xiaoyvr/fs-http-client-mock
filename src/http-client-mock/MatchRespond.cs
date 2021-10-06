using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace HttpClientMock
{
    internal static class MatchRespond
    {
        public static HttpResponseMessage Respond(HttpRequestMessage request, 
            Func<HttpRequestMessage, HttpContent> createContent, HttpStatusCode statusCode, 
            IDictionary<string, string> headers, Uri location)
        {
            var httpResponseMessage = new HttpResponseMessage(statusCode)
            {
                Content = createContent(request)
            };
            if (headers != null && headers.Count > 0)
                headers.ToList().ForEach(header => 
                    httpResponseMessage.Headers.Add(header.Key, header.Value));
            httpResponseMessage.Headers.Location = location;
            return httpResponseMessage;
        }

        public static bool Match<TR>(HttpRequestMessage request, Func<string, bool> urlMatcher, HttpMethod method, 
            Func<RequestCapture, TR, bool> matchFunc, List<RequestCapture> captures)
        {
            if (method != request.Method) 
                return false;
            
            var pathAndQuery = request.RequestUri.PathAndQuery;
            if (!urlMatcher(pathAndQuery) && !urlMatcher(Uri.UnescapeDataString(pathAndQuery)))
                return false;
        
            var capture = new RequestCapture(request.RequestUri, request.Method, request.Content ?? new EmptyContent());
            var matched = matchFunc(capture, capture.Model<TR>());
            if (matched)
            {
                captures.Add(capture);
            }
            return matched;
        }
        
    }
}