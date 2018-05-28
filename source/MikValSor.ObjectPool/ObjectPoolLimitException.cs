using System;

namespace MikValSor.Collections
{
	/// <summary>
	///		Exception class used for signaling that Object Poll limit has been reached.
	/// </summary>
    public class ObjectPoolLimitException : Exception
    {
		/// <summary>
		///		Object Pool source of exception. 
		/// </summary>
		public readonly object ObjectPool;
		internal ObjectPoolLimitException(object objectPool) : base($"ObjectPool hit object limit")
		{
			ObjectPool = objectPool;
		}
	}
}
