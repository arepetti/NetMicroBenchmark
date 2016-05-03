using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyTitle("MicroBench")]
[assembly: AssemblyDescription("Attributes to decorate your code for microbenchmark.")]
[assembly: AssemblyCompany("Adriano Repetti")]
[assembly: AssemblyProduct("MicroBench")]
[assembly: AssemblyCopyright("Copyright © Adriano Repetti 2016. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: Guid("dfde7e38-8820-4c5b-bd0b-81cf2b09e48b")]

[assembly: AssemblyVersion("1.0.0.0")]
