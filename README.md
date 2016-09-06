# .NET μBenchmark
This is a small tool to perform microbenchmarks (performance comparison of small code snippets) for .NET,
it is not magic then do not expect you can write algorithms you want to compare and blindless run this benchmark
to get meaningful results: you will not.

Default output report looks like this but you can customize it in the way you prefer (or export gathered data
to be analyzed in Microsoft Excel):

![Performance report example](https://raw.githubusercontent.com/arepetti/NetMicroBenchmark/master/ReportScreenshot.png)

That said this tool is useful to have a first rough idea of code performance and get results in an easy to understand
graphical format. Before writing your tests and trying to analyze results you should read one good tutorial about
microbenchmarks, especially to understand their limits. Also don't forget that this tool is not intended to
facilitate output quality measurement but merely its computation performance.

Note that the same infrastructure may be used also to perform other performance tests which involves, for example, I/O
which are usually excluded from microbenchmarks. There are more complete and sophisticated tools to perform such
tests (because you probably also need to interpolate measures with system performance and resources) but you may
use this tool also to perform a first indicative analysis.

##First benchmark
To write your first benchmark you have to write a _discoverable_ class:

```C#
public class MyBenchmark {
  public void Test1() { /* Code for algorithm 1 here */ }
  public void Test2() { /* Code for algorithm 2 here */ }
}
```

Now you can run that benchmark, render its output to HTML and display results simply with this (don't forget to add a reference to `MicroBench.Engine.dll`):

```C#
class Program {
  static void Main() {
    Process.Start(BenchmarkEngine.ExecuteAndRenderWithDefaults());
  }
}
```

If you need a quick benchmark you may simply edit `Benchmark.cs` in `Tests/ManualTest.exe`, it is a console application
with the sole purpose to show you how to use this tool.

Do you want to write your own fancy user interface to quickly write tests? Don't forget to also take a look to
`BenchmarkEngine.SuiteProgressChanged` and `BenchmarkEngine.BenchmarkProgressChanged` events, tests may take a long
time to run and users should be informed about what is going on.

##Unit Testing Integration

Sometimes you need to measure performance in your unit testing, you have some simplified methods for this:

```C#
[TestClass]
public sealed class BackendTests {
    [TestMethod]
    public void EnsureMinimumPerformance() {
      Assert.IsTrue(BenchmarkEngine.ExecuteSingle(typeof(Benchmark)) <= TimeSpan.FromMilliseconds(10));
    }
    
    sealed class Benchmark {
      public void Test() {
        // Write here code you want to test
      }
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
is performed in a separate `AppDomain` to minimize interference between tests. To use attributes you need a reference
to `MicroBench.dll`.
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

If you need to initialize some data for your tests (and eventually perform some clean-up) you can freely mix one or more of these:
* To setup: declare a default constructor.
* To setup: declare one or more public `void` and parameterless instance methods (can be virtual) and mark it with `SetUpBenchmarkAttribute`. Execution order is not granted.
* To cleanup: implement `IDisposable`.
* To cleanup: declare one or more public `void` and parameterless instance methods (can be virtual) and mark it with `CleanUpBenchmarkAttribute`. Execution order is not granted.

Setup (and symmetric cleanup) code will be executed for each run of each method, don't forget it if you have to perform some very expensive initialization code. Code is executed each time in separate a new `AppDomain` then you can't share resources between instances. To workaround this (unless you want - but you shouldn't - set `BenchmarkEngineOptions.RunInIsolation` to `false`) you have two options:

* Prepare your data before you run the test (this is viable only if data can stay on disk and then simply loaded during test initialization).
* Save values into the _main_ `AppDomain`. To do it you need to write your own `MarshalByRefObject` and expose it to other domains through `AppDomain.SetData()`, like this:

```C#
AppDomain.CurrentDomain.SetData("__BenchmarkCrossDomainData",
    new MyCrossDomainServiceRepository());
```

Note that cross-domain communication is slow then it must not be done only inside setup/cleanup methods, also don't
forget that data you exchange must be derive from `MarshalByReRefObject` or marked as `[Serializable]`.

##Statistics
This tool provide just one simple implementation for measures analysis, I don't think we need advanced
statistical methods for simple microbenchmarks then a naive average may be enough. Proposed `BasicStatistics`
implementation also calculates few dispersion indices, they may be useful to determine benchmark _quality_
but also to have a better view of what happen in a complex scenario like this one:

```C#
public class Benchmark {
    public void SerialDownload() {
        foreach (var fileToDownload in GetFilesToDownload()) {
            DownloadFileWithHttp(fileToDownload.LocalPath, fileToDownload.ServerUrl);
        }
    }

    public void ParallelDownload() {
        Parallel.ForEach(GetFilesToDownload(), fileToDownload =>  {
            DownloadFileWithHttp(fileToDownload.LocalPath, fileToDownload.ServerUrl);
        });
    }

    public void LimitedParallelDownload() {
        var options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
        Parallel.ForEach(GetFilesToDownload(), options, fileToDownload =>  {
            DownloadFileWithHttp(fileToDownload.LocalPath, fileToDownload.ServerUrl);
        });
    }

    // ...
}
```

Note that statistical analysis is not mandatory and you _may_ have an `OutputRenderer` which directly
work with raw measures produced by `BenchmarkEngine`. See for example #4.

To increase measure quality `BasicStatistics` implementation provides a `CutTails` property which
enable a _trimmed average_ where best and worst results (one for each tail) are ignored. If you wish
to visualize also JIT compilation overhead (setting `BenchmarkEngineOptions.WarmUp` to `false`) then
you probably also want to keep this property to `false`.

##Output
_Default_ `OutputRenderer` implementation is for an HTML document. Performance report is
generated from a template file (preprocessed with mustache-sharp and it uses [Bootstrap](http://getbootstrap.com) and [Highcharts](http://www.highcharts.com)). You can write your own templates
to customize report content and appearance or directly modify `DefaultTemplate.html`. Plain text
reports are supported through `TextOutputRenderer` base class (which is also the base implementation
for `HtmlOutputRenderer`.

`HtmlOutputRenderer` has only two public properties: `Statistics` to set the set of analysis to
perform and `TemplatePath` to set the full path of the HTML template to use to generate output report.

If you want to analyze raw data you can use `ExcelOutputRenderer` (which ignores `Statistics` property and always
exports all measures).

