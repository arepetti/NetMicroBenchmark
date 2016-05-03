# .NET μBenchmark
This is a small tool to perform microbenchmarks (performance comparison of small code snippets) for .NET,
it is not magic then do not expect you can write algorithms you want to compare and blindless run this benchmark
to get meaningful results: you will not.

That said this tool is useful to have a first rough idea of code performance and get results in an easy to understand
graphical format. Before writing your tests and trying to analyze results you should read one good tutorial about
microbenchmarks, especially to understand their limits. Also don't forget that this tool is not intended to
facilitate output quality measurament but merely its computation performance.

##First benchmark
To write your first benchmark you have to write a _discoverable_ class:

```C#
public class MyBenchmark {
  public void Test1() { /* Code for algorithm 1 here */ }
  public void Test2() { /* Code for algorithm 2 here */ }
}
```

Now you can run that benchmark, render its output to HTML and display results simply with this:

```C#
class Program {
  static void Main() {
    Process.Start(BenchmarkEngine.ExecuteAndRenderWithDefaults());
  }
}
```

## How It Works
Logic is simple and straightforward:
* Each class is a separate benchmark and each method is a test which
will be compared to all the others within the same class. You can specify a search strategy
using `BenchmarkEngineOptions.SearchMethod`: classes and methods may be marked with
attributes or simply using a naming convention.
* Each method is then executed many times and its execution time is recordered. Using `BenchmarkOptions.Repetitions`
or `BenchmarkedMethodAttribute` you can specify how many measures you want to perform. By default each execution
is performed in a separate `AppDomain` to minimize interference between tests.
* These _measures_ are then aggregated to produce some indices. You need to specify which calculations you want
to perform, this tool ships a simple `BasicStatistics` class with a minimal statistical analysis.
* All collected data is then rendered into an output file. You can use any `OutputRenderer` you want;
default output format is HTML  implemented in `HtmlOutputRenderer` and you can customize output appearance
using an HTML (Mustache powered) template.

Full code for previous benchmark without using the `BenchmarkEngine.ExecuteAndRenderWithDefaults()` helper method is:

```C#
string templatePath = Path.Combine(Environment.CurrentDirectory, HtmlOutputRenderer.DefaultReportName);
string outputPath = Path.Combine(Path.GetTempPath(), HtmlOutputRenderer.DefaultReportName);

var options = new BenchmarkOptions { SearchMethod = BencharkSearchMethod.Convention };
var engine = new BenchmarkEngine(options, new Assembly[] { typeof(Program).Assembly });

var renderer = new HtmlOutputRenderer();
renderer.TemplatePath = templatePath;
renderer.Statistics = new BasicStatistics() { CutTails = true };
renderer.RenderTo(outputPath, engine.Execute());

Process.Start(outputPath);
```

##Benchmarks and Tests
One single code snippet to measure is called **test** and it must be a `void` public non-virtual parameterless instance method.

When searching by convention it _may_ be named starting with `Test` but if engine can't find any eligible method
within a class then it will pick any eligible method regardless its name.

```C#
public void TestAlgorithm1() { }
```

When search is performed using a declarative syntax then each  method you want to measure
must be decorated with `BenchmarkedMethodAttribute`.

```C#
[BenchmarkedMethod]
public void Algorithm1() { }
```

By default test display name is method name but you can change it using `BenchmarkedMethodAttribute.Name` property
and optionally a description may be added (where and how its displayed is left to output renderer implementation).

```C#
[BenchmarkedMethod(Name="Dixon's algorithm", Description="See en.wikipedia.org/wiki/Dixon%27s_algorithm")]
public void Dixon() { }

[BenchmarkedMethod(Name="Shor's algorithm", Description="See en.wikipedia.org/wiki/Shor%27s_algorithm")]
public void Shor() { }
```

Using `BenchmarkedMethodAttribute` you can also override defaults for `BenchmarkEngineOptions.WarmUp` and
`BenchmarkEngineOptions.Repetitions` but you probably will never need it.

All discovered methods are grouped into a **benchmark** and compared to each other. A benchmark is a public non-abstract,
non-generic class with a default constructor.

When searching by convention a class must begin or end with `Benchmark`.

```C#
public class MyBenchmark {
  public void TestAlgorithm1() { }
  public void TestAlgorithm2() { }
}
```

When search is performed using a declarative syntax then each class must be decorated with `BenchmarkedAttribute`.

```C#
[Benchmark]
public class IntegerFactorization {
  [BenchmarkedMethod]
  public void Dixon() { }

  [BenchmarkedMethod]
  public void Shor() { }
}
```

Also for `BenchmarkAttribute` you can use `Name` and `Description` to provide additional information.
Multiple benchmarks can be grouped together into a **suite** but measures are not correlated.

##Statistics
This tool provide just one simple implementation for measures analysis, I don't think we need advanced
statistical methods for simple microbenchmarks then a naive average may be enough.

TODO

##Output
TODO
