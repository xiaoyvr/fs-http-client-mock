using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using JetBrains.Annotations;

namespace HttpClientMock
{
    public class RequestBehaviorBuilder
    {
        private readonly Func<string, bool> urlMatcher;
        private readonly HttpMethod method;

        private HttpStatusCode statusCode;
        private IDictionary<string, string> headers;
        private Uri location;
        private Func<HttpRequestMessage, HttpContent> createContent;
        private readonly List<RequestCapture> captures = new List<RequestCapture>();
        private Func<HttpRequestMessage, bool> matcher;

        public RequestBehaviorBuilder(Func<string, bool> urlMatcher, HttpMethod method)
        {
            this.method = method;
            this.urlMatcher = urlMatcher;
            createContent = r => JsonContent.Create(string.Empty);
            matcher = r => MatchRespond.Match<object>(r, urlMatcher, method, (capture, model) => true, captures);
        }

        [PublicAPI]
        public RequestBehaviorBuilder MatchRequest<TR>(TR schema = default, Func<RequestCapture, TR, bool> matchFunc = null)
        {
            return MatchRequest(matchFunc);
        }
        
        [PublicAPI]
        public RequestBehaviorBuilder MatchRequest(Func<RequestCapture, object, bool> matchFunc = null)
        {
            return MatchRequest<object>(matchFunc);
        }
        
        [PublicAPI]
        public RequestBehaviorBuilder MatchRequest<TR>(Func<RequestCapture, TR, bool> matchFunc = null)
        {
            matcher = r => MatchRespond.Match(r, urlMatcher, method, matchFunc?? ((capture, model) =>true) , captures);
            return this;
        }
        
        [PublicAPI]
        public Func<RequestCapture> Capture()
        {
            return () => captures.LastOrDefault();
        }
        
        [PublicAPI]
        public Func<(RequestCapture requestCapture, TR model)> Capture<TR>(TR schema = default)
        {
            return () =>
            {
                var requestCapture = captures.LastOrDefault();
                TR model = default;
                if (requestCapture != null)
                {
                    model = requestCapture.Model<TR>();
                }
                return (requestCapture, model);
            };
        }

        [PublicAPI]
        public Func<IEnumerable<RequestCapture>> CaptureAll()
        {
            return () => captures.ToArray();
        }
        
        [PublicAPI]
        public Func<IEnumerable<(RequestCapture requestCapture, TR model)>> CaptureAll<TR>(TR schema = default)
        {
            return () => captures.Select(c => (c, c.Model<TR>())).ToArray();
        }
        
        [PublicAPI]
        public RequestBehaviorBuilder Respond(HttpStatusCode httpStatusCode)
        {
            statusCode = httpStatusCode;
            return this;
        }
        [PublicAPI]
        public RequestBehaviorBuilder Respond(HttpStatusCode httpStatusCode, object content, Uri location = null)
        {
            statusCode = httpStatusCode;
            createContent = r => JsonContent.Create(content);
            this.location = location;
            return this;
        }
        [PublicAPI]
        public RequestBehaviorBuilder RespondContent(HttpStatusCode httpStatusCode, Func<HttpRequestMessage,HttpContent> contentFn, Uri location = null)
        {
            statusCode = httpStatusCode;
            createContent = contentFn;
            this.location = location;
            return this;
        }
        [PublicAPI]
        public RequestBehaviorBuilder RespondHeaders(dynamic headers)
        {
            this.headers = Dyn2Dict((object)headers);
            return this;
        }
        
        internal (Func<HttpRequestMessage, bool> Matcher, Func<HttpRequestMessage, HttpResponseMessage> Responder) Build()
        {
            return (
                Matcher: r => matcher(r),
                Responder: r => MatchRespond.Respond(r, createContent, statusCode, headers, location)
            );
        }

        private static Dictionary<string, string> Dyn2Dict(object dynObj)
        {
            return dynObj.GetType().GetProperties().ToDictionary(prop => prop.Name, 
                prop => prop.GetValue(dynObj, null).ToString());
        }
    }
}