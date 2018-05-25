using System;
using System.Collections.Concurrent;
using System.Net.Http;

namespace MikValSor.Collections
{
	public static class HttpClientPool
	{
		private static Lazy<ConcurrentDictionary<string, ObjectPool<HttpClient>>> LazyDomainPools = new Lazy<ConcurrentDictionary<string, ObjectPool<HttpClient>>>();
		private static ConcurrentDictionary<string, ObjectPool<HttpClient>> DomainPools => LazyDomainPools.Value;

		public static (HttpResponseMessage Message, string Content) GetMessageAndContent(Uri baseAddress, TimeSpan timeout, Func<HttpClient, HttpResponseMessage> workFunction)
		{
			if (baseAddress == null) throw new ArgumentNullException(nameof(baseAddress));

			var objectPoolName = $"{timeout.TotalSeconds}|{baseAddress.ToString().ToLower()}";
			var clientPool = DomainPools.GetOrAdd(objectPoolName, (s) => CreateObjectPool(baseAddress, timeout));
			return clientPool.Use((client) =>
			{
				var response = workFunction.Invoke(client);
				var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

				return (response, content);
			});
		}

		private static ObjectPool<HttpClient> CreateObjectPool(Uri baseAddress, TimeSpan timeout)
		{
			return new ObjectPool<HttpClient>(() => CreateHttpClient(baseAddress, timeout));
		}

		private static HttpClient CreateHttpClient(Uri baseAddress, TimeSpan timeout)
		{
			var client = new HttpClient();
			client.BaseAddress = baseAddress;
			client.Timeout = timeout;
			return client;
		}
	}
}
