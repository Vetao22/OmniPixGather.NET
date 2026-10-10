using System;
using System.Net.Http;

namespace OmniPixGather.NET.Providers
{
    /// <summary>
    /// Base abstract provider sharing a secure, optimized HttpClient instance.
    /// </summary>
    public abstract class BaseHttpProvider
    {
        protected static readonly HttpClient SharedHttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        static BaseHttpProvider()
        {
            SharedHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("OmniPixGather.NET/1.0 (Compatible; C# Pure Engine)");
        }
    }
}