﻿using System;
 using System.IO;
 using System.Net.Http;
 using System.Net.Http.Json;
 using System.Text.Json;

 namespace HttpClientMock
{
    public class RequestCapture
    {
        public string Method { get; set; }
        public HttpContent Content { private get; set; }
        public Uri RequestUri { get; set; }

        public T Body<T>(T schema = default)
        {
            return Read<T>();
        }
        
        private T Read<T>()
        {
            if (Content is EmptyContent)
            {
                return default;
            }
            
            if (typeof(T) == typeof(string))
            {
                return (T) (object) Content.ReadAsStringAsync().Result;
            }
            if (typeof(T) == typeof(Stream))
            {
                return (T) (object) Content.ReadAsStreamAsync().Result;
            }
            if (typeof(T) == typeof(byte[]))
            {
                return (T) (object) Content.ReadAsByteArrayAsync().Result;
            }
            return Content.ReadFromJsonAsync<T>().Result;
        }
    }
}