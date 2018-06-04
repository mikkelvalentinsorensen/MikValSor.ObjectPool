using System;
using System.Collections.Concurrent;

namespace MikValSor.Collections
{
	/// <summary>
	///		Object pools can improve application performance in situations where you require multiple instances of a class and the class is expensive to create or destroy. When a client program requests a new object, the object pool first attempts to provide one that has already been created and returned to the pool. If none is available, only then is a new object created. This class also provides disposable shimming object capability to ensure compleate isolation of inner object.
	/// </summary>
	/// <typeparam name="T">Type of contained objects in pool.</typeparam>
	/// <typeparam name="Tshim">Type of the shimming object.</typeparam>
	public sealed class ObjectPool<T, Tshim> where Tshim : IDisposable
	{
		private readonly ObjectPool<T> inner;
		private readonly Func<T, Tshim> ShimGenerator;

		/// <summary>
		///		Count of how many objects are in the pool.
		/// </summary>
		public int Count => inner.Count;

		/// <summary>
		///		The maximum amount of objects allowed.
		/// </summary>
		public int Limit => inner.Limit;

		/// <summary>
		///		Constructs a new instance of ObjectPool
		/// </summary>
		/// <param name="objectGenerator">
		///		Funtions for constructing new instance of T
		/// </param>
		/// <param name="shimGenerator">
		///		Function that greater shinning object for the object of T
		/// </param>
		/// <param name="limit">
		///		Sets the limit of how many objects will be in the pool.
		/// </param>
		public ObjectPool(Func<T> objectGenerator, Func<T, Tshim> shimGenerator, int limit = 500)
		{
			inner = new ObjectPool<T>(objectGenerator, limit);
			inner.OnObjectCreated += (sender, args)=> OnObjectCreated?.Invoke(this, new CreatedObjectEventArgs<T, Tshim>(this));
			ShimGenerator = shimGenerator ?? throw new ArgumentNullException(nameof(shimGenerator));
		}

		/// <summary>
		///		Function for using a shimmed instance of object in pool.
		/// </summary>
		/// <param name="action">
		///		Action that will be invoked with shimmed item.
		/// </param>
		public void Use(Action<Tshim> action)
		{
			inner.Use((t) =>
			{
				Tshim tshim = default(Tshim);
				try
				{
					tshim = ShimGenerator(t);
					action(tshim);
				}
				finally
				{
					if (tshim != null) tshim.Dispose();
				}
			});
		}

		/// <summary>
		///		Function for using a shimmed instance of object in pool.
		/// </summary>
		/// <param name="func">
		///		Function that will be invoked with shimmed item.
		/// </param>
		/// <returns>
		///		Returns returned value of provided function.
		/// </returns>
		public Tout Use<Tout>(Func<Tshim, Tout> func)
		{
			return inner.Use((t) =>
			{
				Tshim tshim = default(Tshim);
				try
				{
					tshim = ShimGenerator(t);
					return func(tshim);
				}
				finally
				{
					if (tshim != null) tshim.Dispose();
				}
			});
		}

		/// <summary>
		///		Event invoked when new Object is created in pool.
		/// </summary>
		public event EventHandler<CreatedObjectEventArgs<T, Tshim>> OnObjectCreated;
	}

	/// <summary>
	///		Object pools can improve application performance in situations where you require multiple instances of a class and the class is expensive to create or destroy. When a client program requests a new object, the object pool first attempts to provide one that has already been created and returned to the pool. If none is available, only then is a new object created.
	/// </summary>
	/// <typeparam name="T">Type of contained objects in pool.</typeparam>
	public sealed class ObjectPool<T> : IDisposable
	{
		private readonly Func<T> ObjectGenerator;
		private readonly ConcurrentBag<T> FreeObjects = new ConcurrentBag<T>();
		private readonly ConcurrentBag<T> AllObjects = new ConcurrentBag<T>();

		/// <summary>
		///		Count of how many objects are in the pool.
		/// </summary>
		public int Count => AllObjects.Count;

		/// <summary>
		///		The maximum amount of objects allowed.
		/// </summary>
		public int Limit { get; private set; }

		/// <summary>
		///		Constructs a new instance of ObjectPool
		/// </summary>
		/// <param name="objectGenerator">
		///		Funtions for constructing new instance of T
		/// </param>
		/// <param name="limit">
		///		Sets the limit of how many objects will be in the pool.
		/// </param>
		public ObjectPool(Func<T> objectGenerator, int limit = int.MaxValue)
		{
			ObjectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
		}

		private T Take()
		{
			if (FreeObjects.TryTake(out var item)) return item;

			var newObject = ObjectGenerator();
			AllObjects.Add(newObject);
			OnObjectCreated?.Invoke(this, new CreatedObjectEventArgs<T>(this));

			return newObject;
		}

		/// <summary>
		///		Function for using a instance of object in pool.
		/// </summary>
		/// <param name="action">
		///		Action that will be invoked with item.
		/// </param>
		public void Use(Action<T> action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			var t = Take();
			try
			{
				lock (t)
				{
					action(t);
				}
			}
			finally
			{
				Put(t);
			}
		}

		/// <summary>
		///		Function for using a instance of object in pool.
		/// </summary>
		/// <param name="func">
		///		Function that will be invoked with item.
		/// </param>
		/// <returns>
		///		Returns returned value of provided function.
		/// </returns>
		public Tout Use<Tout>(Func<T, Tout> func)
		{
			if (func == null) throw new ArgumentNullException(nameof(func));

			var t = Take();
			try
			{
				return func(t);
			}
			finally
			{
				Put(t);
			}
		}

		private void Put(T item)
		{
			FreeObjects.Add(item);
		}

		/// <summary>
		///		Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			while (AllObjects.TryTake(out var t))
			{
				if (t is IDisposable d)
				{
					d.Dispose();
				}
			}
		}

		/// <summary>
		///		Event invoked when new Object is created in pool.
		/// </summary>
		public event EventHandler<CreatedObjectEventArgs<T>> OnObjectCreated;
	}
}
