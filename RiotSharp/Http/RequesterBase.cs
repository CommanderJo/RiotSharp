﻿using RiotSharp.Http.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RiotSharp.Http
{
    abstract class RequesterBase
    {
        protected string rootDomain;
        private readonly HttpClient httpClient;

        public string ApiKey { get; set; }

        protected RequesterBase(string apiKey = "")
        {
            ApiKey = apiKey;
            httpClient = new HttpClient();
        }

        #region Protected Methods

        protected HttpResponseMessage Get(HttpRequestMessage request)
        {
            var response = httpClient.GetAsync(request.RequestUri).Result;
            if (!response.IsSuccessStatusCode)
            {
                HandleRequestFailure(response.StatusCode);
            }
            return response;
        }

        protected async Task<HttpResponseMessage> GetAsync(HttpRequestMessage request)
        {
            var response = await httpClient.GetAsync(request.RequestUri);
            if (!response.IsSuccessStatusCode)
            {
                HandleRequestFailure(response.StatusCode);
            }
            return response;
        }

        protected HttpResponseMessage Put(HttpRequestMessage request)
        {
            var response = httpClient.PutAsync(request.RequestUri, request.Content).Result;
            if (!response.IsSuccessStatusCode)
            {
                HandleRequestFailure(response.StatusCode);
            }
            return response;
        }

        protected async Task<HttpResponseMessage> PutAsync(HttpRequestMessage request)
        {
            var response = await httpClient.PutAsync(request.RequestUri, request.Content);
            if (!response.IsSuccessStatusCode)
            {
                HandleRequestFailure(response.StatusCode);
            }
            return response;
        }

        protected HttpResponseMessage Post(HttpRequestMessage request)
        {
            var response = httpClient.PostAsync(request.RequestUri, request.Content).Result;
            if (!response.IsSuccessStatusCode)
            {
                HandleRequestFailure(response.StatusCode);
            }
            return response;
        }

        protected async Task<HttpResponseMessage> PostAsync(HttpRequestMessage request)
        {
            var response = await httpClient.PostAsync(request.RequestUri, request.Content);
            if (!response.IsSuccessStatusCode)
            {
                HandleRequestFailure(response.StatusCode);
            }
            return response;
        }

        protected HttpRequestMessage PrepareRequest(string relativeUrl, List<string> addedArguments,
            bool useHttps, HttpMethod httpMethod)
        {
            var scheme = useHttps ? "https" : "http";
            var url = addedArguments == null ?
                string.Format("{0}://{1}{2}?api_key={3}", scheme, rootDomain, relativeUrl, ApiKey) :
                string.Format("{0}://{1}{2}?{3}api_key={4}",
                    scheme, rootDomain, relativeUrl, BuildArgumentsString(addedArguments), ApiKey);

            return new HttpRequestMessage(httpMethod, url);
        }

        protected string BuildArgumentsString(List<string> arguments)
        {
            return arguments
                .Where(arg => arg != string.Empty)
                .Aggregate(string.Empty, (current, arg) => current + (arg + "&"));
        }

        protected void HandleRequestFailure(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.ServiceUnavailable:
                    throw new RiotSharpException("503, Service unavailable", statusCode);
                case HttpStatusCode.InternalServerError:
                    throw new RiotSharpException("500, Internal server error", statusCode);
                case HttpStatusCode.Unauthorized:
                    throw new RiotSharpException("401, Unauthorized", statusCode);
                case HttpStatusCode.BadRequest:
                    throw new RiotSharpException("400, Bad request", statusCode);
                case HttpStatusCode.NotFound:
                    throw new RiotSharpException("404, Resource not found", statusCode);
                case HttpStatusCode.Forbidden:
                    throw new RiotSharpException("403, Forbidden", statusCode);
                default:
                    throw new RiotSharpException("Unexpeced failure", statusCode);
            }
        }

        protected string GetResponseContent(HttpResponseMessage response)
        {
            var result = string.Empty;

            using (var content = response.Content)
            { 
                result = content.ReadAsStringAsync().Result;
            }
            return result;
        }

        protected async Task<string> GetResponseContentAsync(HttpResponseMessage response)
        {
            Task<string> result = null;
            using (response)
            {
                using (var content = response.Content)
                {
                    result = content.ReadAsStringAsync();
                }
            }
            return await result;
        }

        #endregion
    }
}
