using System;
using System.Collections.Concurrent;

namespace MikValSor.Collections
{
	public class ObjectPool<T, Tshim> where Tshim : IDisposable
	{
		private readonly ObjectPool<T> inner;
		private readonly Func<T, Tshim> ShimGenerator;
		public int Count => inner.Count;
		public int Limit => inner.Limit;

		public ObjectPool(Func<T> objectGenerator, Func<T, Tshim> shimGenerator, int limit = 500)
		{
			inner = new ObjectPool<T>(objectGenerator, limit);
			ShimGenerator = shimGenerator ?? throw new ArgumentNullException(nameof(shimGenerator));
		}

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
	}

	public class ObjectPool<T>
	{
		private readonly Func<T> ObjectGenerator;
		private readonly ConcurrentBag<T> Objects = new ConcurrentBag<T>();
		private object CountLock = new object();
		public int Count { get; private set; } = 0;
		public int Limit { get; private set; }

		public ObjectPool(Func<T> objectGenerator, int limit = 500)
		{
			ObjectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
		}

		private T Take()
		{
			if (Objects.TryTake(out var item)) return item;

			lock (CountLock)
			{
				if (Count >= Limit) throw new ObjectPoolLimitException(this);
				Count++;
			}
			return ObjectGenerator();
		}

		public void Use(Action<T> action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			var t = Take();
			try
			{
				action(t);
			}
			finally
			{
				Put(t);
			}
		}

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
			Objects.Add(item);
		}
	}
}
