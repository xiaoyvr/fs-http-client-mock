using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using HttpClientMock.Responses;

namespace HttpClientMock
{
    public class RequestBehaviorBuilder
    {
        private readonly Func<string, bool> urlMatcher;

        private readonly HttpMethod method;

        private HttpStatusCode statusCode;

        private IRequestProcessor processor;

        private IDictionary<string, string> headers;

        public RequestBehaviorBuilder(Func<string, bool> urlMatcher, HttpMethod method)
        {
            this.method = method;
            this.urlMatcher = urlMatcher;
        }

        public RequestBehaviorBuilder MatchRequest<TModel>(TModel schema = default, Func<RequestCapture, TModel, bool> matchFunc = null)
        {
            processor = new RequestProcessor<TModel>(matchFunc ?? ((r,t) => true));
            return this;
        }
        
        public RequestBehaviorBuilder MatchRequest(Func<RequestCapture, object, bool> matchFunc = null)
        {
            return MatchRequest<object>(matchFunc);
        }

        public RequestBehaviorBuilder MatchRequest<TModel>(Func<RequestCapture, TModel, bool> matchFunc = null)
        {
            processor = new RequestProcessor<TModel>(matchFunc ?? ((r,t) => true));
            return this;
        }
        
        public Func<RequestCapture> Capture()
        {
            processor ??= new AlwaysMatchProcessor();
            return () => processor.Captures.LastOrDefault();
        }
        
        public Func<(RequestCapture requestCapture, TBody body)> Capture<TBody>(TBody schema = default)
        {
            processor ??= new AlwaysMatchProcessor();
            return () =>
            {
                var requestCapture = processor.Captures.LastOrDefault();
                TBody body = default;
                if (requestCapture != null)
                {
                    body = requestCapture.Body<TBody>();
                }
                return (requestCapture, body);
            };
        }

        public Func<IEnumerable<RequestCapture>> CaptureAll()
        {
            processor ??= new AlwaysMatchProcessor();
            return () => processor.Captures.ToArray();
        }
        
        public Func<IEnumerable<(RequestCapture requestCapture, T body)>> CaptureAll<T>(T schema = default)
        {
            processor ??= new AlwaysMatchProcessor();
            return () => processor.Captures.Select(c => (c, c.Body<T>())).ToArray();
        }

        public RequestBehaviorBuilder Respond(HttpStatusCode httpStatusCode)
        {
            statusCode = httpStatusCode;
            return this;
        }

        public RequestBehaviorBuilder Respond(HttpStatusCode httpStatusCode, object response, Uri location = null)
        {
            statusCode = httpStatusCode;
            this.response = new ObjectResponseCreator(response);
            this.location = location;
            return this;
        }

        public RequestBehaviorBuilder RespondContent(HttpStatusCode httpStatusCode, Func<HttpRequestMessage,HttpContent> contentFn, Uri location = null)
        {
            statusCode = httpStatusCode;
            response = new HttpContentResponseCreator(contentFn);
            this.location = location;
            return this;
        }

        public RequestBehaviorBuilder RespondHeaders(dynamic headers)
        {
            this.headers = Dyn2Dict((object)headers);
            return this;
        }

        private IResponseCreator response = new ObjectResponseCreator(string.Empty);

        private Uri location;

        internal RequestBehavior Build()
        {
            return new RequestBehavior(statusCode, urlMatcher, method, processor ?? new AlwaysMatchProcessor(), response, location, headers);
        }

        private Dictionary<string, string> Dyn2Dict(object dynObj)
        {
            return dynObj.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(dynObj, null).ToString());
        }
    }
}