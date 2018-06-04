using System;

namespace MikValSor.Collections
{
	/// <summary>
	///		Event class used to signal when new object are created in object pool.
	/// </summary>
	/// <typeparam name="T">Type of contained objects in pool.</typeparam>
	public sealed class CreatedObjectEventArgs<T> : EventArgs
	{
		/// <summary>
		///		The object pool that object was created in.
		/// </summary>
		public readonly ObjectPool<T> ObjectPool;

		internal CreatedObjectEventArgs(ObjectPool<T> objectPool)
		{
			ObjectPool = objectPool;
		}
	}

	/// <summary>
	///		Event class used to signal when new object are created in object pool.
	/// </summary>
	/// <typeparam name="T">Type of contained objects in pool.</typeparam>
	/// <typeparam name="Tshim">Type of the shimming object.</typeparam>
	public sealed class CreatedObjectEventArgs<T, Tshim> : EventArgs where Tshim : IDisposable
	{
		/// <summary>
		///		The object pool that object was created in.
		/// </summary>
		public readonly ObjectPool<T, Tshim> ObjectPool;

		internal CreatedObjectEventArgs(ObjectPool<T, Tshim> objectPool)
		{
			ObjectPool = objectPool;
		}
	}
}
