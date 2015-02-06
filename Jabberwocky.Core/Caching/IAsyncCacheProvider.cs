﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jabberwocky.Core.Caching
{
	public interface IAsyncCacheProvider
	{
		///// <summary>
		///// Retrieves the desired object from the cache. If the object is null, executes the callback
		///// method to set it up and store it in the cache.
		///// </summary>
		///// <typeparam name="T">A reference type</typeparam>
		///// <param name="key">The cache key, must be unique for each object</param>
		///// <param name="callback">A callback method to retrieve the object if it is not in cache.</param>
		///// <param name="token">Cancellation token</param>
		///// <returns></returns>
		//Task<T> GetFromCacheAsync<T>(string key, Func<T> callback, CancellationToken token = default(CancellationToken)) where T : class;

		///// <summary>
		///// Retrieves the desired object from the cache. If the object is null, executes the callback
		///// method to set it up and store it in the cache.
		///// </summary>
		///// <typeparam name="T">A reference type</typeparam>
		///// <param name="key">The cache key, must be unique for each object</param>
		///// <param name="absoluteExpiration">A TimeSpan after which the item will expire from the cache.</param>
		///// <param name="callback">A callback method to retrieve the object if it is not in cache.</param>
		///// <returns></returns>
		//Task<T> GetFromCacheAsync<T>(string key, TimeSpan absoluteExpiration, Func<T> callback, CancellationToken token = default(CancellationToken)) where T : class;

		///// <summary>
		///// Retrieves the desired object from the cache. If the object is null, executes the callback
		///// method to set it up and store it in the cache.
		///// </summary>
		///// <typeparam name="T">A reference type</typeparam>
		///// <param name="key">The cache key, must be unique for each object</param>
		///// <param name="callback">A callback method to retrieve the object if it is not in cache.</param>
		///// <param name="token">Cancellation token</param>
		///// <returns></returns>
		//Task<T> GetFromCacheAsync<T>(string key, Func<CancellationToken, Task<T>> callback, CancellationToken token = default(CancellationToken)) where T : class;

		///// <summary>
		///// Retrieves the desired object from the cache. If the object is null, executes the callback
		///// method to set it up and store it in the cache.
		///// </summary>
		///// <typeparam name="T">A reference type</typeparam>
		///// <param name="key">The cache key, must be unique for each object</param>
		///// <param name="absoluteExpiration">A TimeSpan after which the item will expire from the cache.</param>
		///// <param name="callback">A callback method to retrieve the object if it is not in cache.</param>
		///// <returns></returns>
		//Task<T> GetFromCacheAsync<T>(string key, TimeSpan absoluteExpiration, Func<CancellationToken, Task<T>> callback, CancellationToken token = default(CancellationToken)) where T : class;

		///// <summary>
		///// Adds an object to the cache; if it already exists, it will overwrite the existing item.
		///// </summary>
		///// <typeparam name="T">A reference type</typeparam>
		///// <param name="key">The cache key, must be unique for each object</param>
		///// <param name="value">The object to store in the cache</param>
		//Task AddToCacheAsync<T>(string key, T value, CancellationToken token = default(CancellationToken)) where T : class;

		///// <summary>
		///// Retrieves the desired object from the cache.  If it doesn't exist, returns null
		///// </summary>
		///// <typeparam name="T">A reference type</typeparam>
		///// <param name="key">The cache key, must be unique for each object</param>
		///// <returns>The requested object by key, or null if not found</returns>
		//Task<T> GetFromCacheAsync<T>(string key, CancellationToken token = default(CancellationToken)) where T : class;
	}
}
