.NET Libaray for Object pools. Object pools can improve application performance in situations where you require multiple instances of a class and the class is expensive to create or destroy. When a client program requests a new object, the object pool first attempts to provide one that has already been created and returned to the pool. If none is available, only then is a new object created.Hashing

Nuget package: [https://www.nuget.org/packages/MikValSor.ObjectPool](https://www.nuget.org/packages/MikValSor.ObjectPool)

## Example:
```cs
void Example()
{
	var pool = new MikValSor.Collections.ObjectPool<System.Collections.Generic.List<int>>(
		() => new System.Collections.Generic.List<int>()
	);

	// Create a high demand for objects.
	System.Threading.Tasks.Parallel.For(0, 1000000, (i, loopState) =>
	{
		pool.Use(list => list.Add(i));
	});

	System.Console.WriteLine($"Pool Count: {pool.Count}");

	System.Console.ReadLine();
}

/**
	Output:
	Pool Count: 8
**/
```