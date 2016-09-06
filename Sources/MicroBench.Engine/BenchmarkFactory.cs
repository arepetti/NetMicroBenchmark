//
// The MIT License (MIT)
// Copyright (c) 2016 Adriano Repetti

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// - The above copyright notice and this permission notice shall be
//   included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
// USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace MicroBench.Engine
{
    /// <summary>
    /// Base class for factory objects to create benchmarks (<see cref="Benchmark"/>) from a given input set.
    /// </summary>
    /// <remarks>
    /// All required fields (just for example the number of repetitions
    /// for each test) are filled from all supported sources (attributes, options and defaults).
    /// </remarks>
    abstract class BenchmarkFactory
    {
        protected BenchmarkFactory(BenchmarkOptions options)
        {
            Debug.Assert(options != null);

            _options = options;
        }

        public abstract Benchmark[] Create();

        protected const string BenchmarkClassName = "Benchmark";
        protected const string BenchmarkMethodPrefix = "Test";

        protected BenchmarkOptions Options
        {
            get { return _options; }
        }

        protected IEnumerable<Benchmark> FindBenchmarks(IEnumerable<Type> types, BencharkSearchMethod searchMethod)
        {
            Debug.Assert(types != null);

            // Every benchmark class must be public and instantiable then it must have
            // a default constructor and it must not be a generic type.
            foreach (var type in types)
            {
                // Note that attribute may be used (to override name, description, etc) whatever
                // search method is. For example you may add attribute to change one specific benchmark name
                // keep searching them using by convention (using defaults for non decorated classes).
                var attribute = type.GetCustomAttribute<BenchmarkAttribute>();

                if (searchMethod == BencharkSearchMethod.Convention)
                {
                    // When discovering is performed by convention each benchmark class must start or with "Benchmark".
                    bool startsWith = type.Name.StartsWith(BenchmarkClassName, StringComparison.InvariantCultureIgnoreCase);
                    bool endsWith = type.Name.EndsWith(BenchmarkClassName, StringComparison.InvariantCultureIgnoreCase);

                    if (!startsWith && !endsWith)
                        continue;

                    yield return CreateBenchmarkForType(type, attribute);
                }
                else if (searchMethod == BencharkSearchMethod.Declarative)
                {
                    // When discovering is performed with a declarative syntax each benchmark class
                    // must be decorated with [Benchmark] attribute.
                    if (attribute == null)
                        continue;

                    yield return CreateBenchmarkForType(type, attribute);
                }

                // When discovering is BencharkSearchMethod.Everything then any eligible
                // class in assembly catalog is considered a benchmark.
                yield return CreateBenchmarkForType(type, attribute);
            }
        }

        protected static bool IsEligibleBenchmarkType(Type type)
        {
            Debug.Assert(type != null);

            bool hasDefaultConstructor = type.GetConstructor(new Type[0]) != null;
            bool isNotGeneric = type.GetGenericArguments().Length == 0;

            return !type.IsAbstract && hasDefaultConstructor && isNotGeneric;
        }

        protected Benchmark CreateBenchmarkForType(Type type, BenchmarkAttribute descriptor)
        {
            Debug.Assert(type != null);

            var benchmark = new Benchmark();
            benchmark.Type = type;

            benchmark.Group = ResolveValue(descriptor, () => descriptor.Group, "");
            benchmark.Name = ResolveValue(descriptor, () => descriptor.Name, type.Name);
            benchmark.Description = ResolveValue(descriptor, () => descriptor.Description, "");
            benchmark.SetUpMethods = GetInvokableMethods(type).Where(x => Attribute.IsDefined(x, typeof(SetUpBenchmarkAttribute))).ToArray();
            benchmark.Methods.AddRange(FindMethodsToBenchmark(type));
            benchmark.CleanUpMethods = GetInvokableMethods(type).Where(x => Attribute.IsDefined(x, typeof(CleanUpBenchmarkAttribute))).ToArray();

            return benchmark;
        }

        private readonly BenchmarkOptions _options;

        private IEnumerable<BenchmarkedMethod> FindMethodsToBenchmark(Type type)
        {
            // We do not want to make things too complicate, if required search method yelds no results
            // then we relax our rules to include eligible methods.
            var methodsToBenchmark = FindMethodsToBenchmark(type, _options.SearchMethod).ToArray();
            if (methodsToBenchmark.Length > 0)
                return methodsToBenchmark;

            // If no methods with attributes then at least let's try by convention (name). Note that this
            // won't fallback to "Everything" (which is tried only if first search is by convention).
            if (_options.SearchMethod == BencharkSearchMethod.Declarative)
                return FindMethodsToBenchmark(type, BencharkSearchMethod.Convention);

            return FindMethodsToBenchmark(type, BencharkSearchMethod.Everything);
        }

        private IEnumerable<BenchmarkedMethod> FindMethodsToBenchmark(Type type, BencharkSearchMethod searchMethod)
        {
            Debug.Assert(type != null);

            // Every eligible method must be public, must not have a return type and must not have any parameter.
            foreach (var method in GetInvokableMethods(type))
            {
                // Note that attribute may be used (to override name, description, etc) whatever
                // search method is. For example you may add attribute to change one specific benchmark name
                // keep searching them using by convention (using defaults for non decorated methods).
                var attribute = method.GetCustomAttribute<BenchmarkedMethodAttribute>();

                if (searchMethod == BencharkSearchMethod.Convention)
                {
                    // When by convention each bechmarked method must start with "Test".
                    if (!method.Name.StartsWith(BenchmarkMethodPrefix, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    yield return CreateBenchmarkForMethod(method, attribute);
                }
                else if (searchMethod == BencharkSearchMethod.Declarative)
                {
                    // When declarative each method must be decorated with [BenchmarkedMethod] attribute.
                    if (attribute == null)
                        continue;

                    yield return CreateBenchmarkForMethod(method, attribute);
                }

                // When discovering is BencharkSearchMethod.Everything then any eligible
                // method in each type is considered a benchmark.
                yield return CreateBenchmarkForMethod(method, attribute);
            }
        }

        private IEnumerable<MethodInfo> GetInvokableMethods(Type type)
        {
            return type.GetMethods().Where(IsInvokableMethod);
        }

        private static bool IsInvokableMethod(MethodInfo method)
        {
            Debug.Assert(method != null);

            bool isParameterless = method.GetParameters().Length == 0;
            bool hasNoReturnType = method.ReturnType == typeof(void);
            bool isNonVirtual = !method.IsVirtual;

            return isParameterless && hasNoReturnType && isNonVirtual;
        }

        private BenchmarkedMethod CreateBenchmarkForMethod(MethodInfo method, BenchmarkedMethodAttribute descriptor)
        {
            Debug.Assert(method != null);

            var benchmarkedMethod = new BenchmarkedMethod();
            benchmarkedMethod.Method = method;

            benchmarkedMethod.Name = ResolveValue(descriptor, () => descriptor.Name, method.Name);
            benchmarkedMethod.Description = ResolveValue(descriptor, () => descriptor.Name, method.Name);
            benchmarkedMethod.WarmUp = ResolveValue(descriptor, () => descriptor.WarmUp, null) ?? _options.WarmUp;
            benchmarkedMethod.Repetitions = ResolveValue(descriptor, () => descriptor.Repetitions, null) ?? _options.Repetitions;

            return benchmarkedMethod;
        }

        private static T ResolveValue<T>(Attribute descriptor, Func<T> valueFromAttribute, T valueFromType)
        {
            // The point is: if descriptor is provided (using declarative syntax) then it overrides any default
            // setting. Instead when there isn't a descriptor we use default values provided elsewhere.
            Debug.Assert(valueFromAttribute != null);

            if (descriptor != null)
            {
                var value = valueFromAttribute();

                // Dirty way to ignore values from descriptor which are invalid
                if (!(typeof(T) == typeof(string) && String.IsNullOrWhiteSpace(value as string)))
                    return value;
            }

            return valueFromType;
        }
    }
}
