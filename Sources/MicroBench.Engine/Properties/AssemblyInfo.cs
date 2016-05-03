using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyTitle("MicroBench.Engine")]
[assembly: AssemblyDescription("Engine to perform microbenchmarks to evaluate code performance.")]
[assembly: AssemblyCompany("Adriano Repetti")]
[assembly: AssemblyProduct("MicroBench.Engine")]
[assembly: AssemblyCopyright("Copyright © Adriano Repetti 2016. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: Guid("ef2798b1-b092-4eb7-90c8-e1256a0a2f5a")]

[assembly: AssemblyVersion("1.0.0.0")]
