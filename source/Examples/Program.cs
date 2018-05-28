class Program
{

static void Main(string[] args)
{
	var program = new Program();
	program.Example();
}



void Example()
{
	var pool = new MikValSor.Collections.ObjectPool<System.Collections.Generic.List<int>>(() => new System.Collections.Generic.List<int>());

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



}