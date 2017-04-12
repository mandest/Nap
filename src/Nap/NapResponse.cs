﻿using Nap.Exceptions.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Nap
{
    /// <summary>
    /// A basic parsed response from a server.
    /// Contains the most basic information necessary from a server - StatusCode, Headers (Including Cookies
    /// </summary>
    public class NapResponse
    {
        /// <summary>
        /// Gets the request that generated this response.
        /// </summary>
        public NapRequest Request { get; set; }

        /// <summary>
        /// Gets the URI that the request was made against.
        /// </summary>
        public Uri Url { get; }

        /// <summary>
        /// The status code for the response.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// The key/value pair collection of headers present in the response.
        /// Also contains all <see cref="ContentHeaders"/> and <see cref="NonContentHeaders"/> in a more raw form.
        /// </summary>
        public IReadOnlyCollection<KeyValuePair<string, string>> Headers { get; }

        /// <summary>
        /// Get content headers for more in-depth use.
        /// All of these headers are also contained within <see cref="Headers"/>.
        /// </summary>
        public HttpContentHeaders ContentHeaders { get; }

        /// <summary>
        /// Get non-content headers for more in-depth use.
        /// All of these headers are also contained within <see cref="Headers"/>.
        /// </summary>
        public HttpResponseHeaders NonContentHeaders { get; }

        /// <summary>
        /// Gets the set of headers that are of key 'Set-Cookie' and casts them to cookies.
        /// </summary>
        public IReadOnlyCollection<NapCookie> Cookies { get; }

        /// <summary>
        /// Gets the body of the response.
        /// </summary>
        public string Body { get; }

        /// <summary>
        /// Create a new instance of the <see cref="NapResponse"/> object, populated with response information.
        /// </summary>
        /// <param name="request">The request that generated this response.</param>
        /// <param name="response">The response that will generate the current nap response.</param>
        /// <param name="body">The body of the response.</param>
        internal NapResponse(NapRequest request, HttpResponseMessage response, string body)
        {
            Request = request;
            Url = new Uri(request.Url);
            StatusCode = response.StatusCode;
            Headers = new ReadOnlyCollection<KeyValuePair<string, string>>(response.Headers
                                                                                   .Union(response.Content.Headers)
                                                                                   .SelectMany(h => h.Value.Select(v => new KeyValuePair<string, string>(h.Key, v)))
                                                                                   .ToList());
            Cookies = new ReadOnlyCollection<NapCookie>(Headers.Where(h => h.Key.Equals("set-cookie", StringComparison.OrdinalIgnoreCase))
                                                               .Select(h => new NapCookie(new Uri(request.Url), h.Value))
                                                               .ToList());
            Body = body;
            ContentHeaders = response.Content.Headers;
            NonContentHeaders = response.Headers;
        }

        /// <summary>
        /// Get the value of a header for this response.
        /// </summary>
        /// <param name="headerName">The name of the header value to return.</param>
        /// <returns>Returns the value of the given header.</returns>
        public string GetHeader(string headerName)
        {
            return Headers.First(h => h.Key.Equals(headerName, StringComparison.OrdinalIgnoreCase)).Value;
        }

        /// <summary>
        /// Get the value of all headers matching a header name for this response.
        /// </summary>
        /// <param name="headerName">The name of the header value to return.</param>
        /// <returns>Returns the value of the given header.</returns>
        public IEnumerable<string> GetHeaders(string headerName)
        {
            return Headers.Where(h => h.Key.Equals(headerName, StringComparison.OrdinalIgnoreCase)).Select(h => h.Value);
        }

        /// <summary>
        /// Get the cookie matching a specific name.
        /// </summary>
        /// <param name="cookieName">The name of the cookie to find.</param>
        /// <returns>The cookie that matches <paramref name="cookieName"/>.</returns>
        public NapCookie GetCookie(string cookieName)
        {
            return Cookies.First(c => c.Name.Equals(cookieName));
        }
    }
}
