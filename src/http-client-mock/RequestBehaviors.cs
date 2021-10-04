﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace HttpClientMock
{
    internal class RequestBehaviors
    {
        private readonly RequestBehavior[] behaviors;

        public RequestBehaviors(IEnumerable<RequestBehavior> behaviors)
        {
            this.behaviors = behaviors.ToArray();
        }

        public HttpResponseMessage CreateResponse(HttpRequestMessage request)
        {
            var httpRequestMessageWrapper = new HttpRequestMessageWrapper(request);
            var requestBehavior = behaviors.Reverse().FirstOrDefault(behavior => behavior.Process(httpRequestMessageWrapper));

            if (requestBehavior != null)
                return requestBehavior.CreateResponseMessage(request);
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}