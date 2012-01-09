#region Licence
/*
 * Copyright 2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

#region Using
using System;
using System.Threading;
using NUnit.Framework;
#endregion

#region Namespace
//#pragma warning disable 618
namespace Memoizer.NET.Test
{

    #region MemoizerTests
    [TestFixture]
    class MemoizerTests
    {

        #region Test constants
        internal const int NUMBER_OF_ITERATIONS = 3;
        internal const int NUMBER_OF_CONCURRENT_TASKS = 600;

        internal const int DATABASE_RESPONSE_LATENCY_IN_MILLIS = 50;
        internal const int NETWORK_RESPONSE_LATENCY_IN_MILLIS = 500;

        internal const string METHOD_RESPONSE_ELEMENT = "VeryExpensiveMethodResponseFor";
        #endregion

        #region Test helper methods
        static string Concatinate(string arg1, string arg2, string arg3) { return arg1 + arg2 + arg3; }


        static internal string ReallySlowNetworkStaticInvocation(string stringArg, long longArg)
        {
            //Console.WriteLine("TypicalNetworkInvocation invoked...");
            Thread.Sleep(NETWORK_RESPONSE_LATENCY_IN_MILLIS);
            //Console.WriteLine("TypicalNetworkInvocation returns...");
            return Concatinate(METHOD_RESPONSE_ELEMENT, stringArg, Convert.ToString(longArg));
        }

        internal Func<string, long, string> reallySlowNetworkInvocation1a =
           new Func<string, long, string>(delegate(string stringArg, long longArg)
           {
               //return ReallySlowNetworkStaticInvocation(stringArg, longArg);

               Thread.Sleep(NETWORK_RESPONSE_LATENCY_IN_MILLIS);
               return Concatinate(METHOD_RESPONSE_ELEMENT, stringArg, Convert.ToString(longArg));
           });

        internal Func<string, long, string> reallySlowNetworkInvocation1b =
           (delegate(string stringArg, long longArg)
           {
               return ReallySlowNetworkStaticInvocation(stringArg, longArg);
           });

        internal Func<string, long, string> reallySlowNetworkInvocation1c =
            (stringArg, longArg) =>
            {
                return ReallySlowNetworkStaticInvocation(stringArg, longArg);
            };

        internal Func<string, long, string> reallySlowNetworkInvocation1d = ReallySlowNetworkStaticInvocation;


        internal Func<string, long, string> reallySlowNetworkInvocation2(string stringArg, long longArg)
        {
            ReallySlowNetworkStaticInvocation(stringArg, longArg);
            return null;
        }


        internal string ReallySlowNetworkInvocation3(string stringArg, long longArg)
        {
            return ReallySlowNetworkStaticInvocation(stringArg, longArg);
        }


        internal static string TypicalDatabaseStaticInvocation(string stringArg, long longArg)
        {
            //Console.WriteLine("TypicalDatabaseInvocation invoked...");
            Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
            //Console.WriteLine("TypicalDatabaseInvocation returns...");
            return Concatinate(METHOD_RESPONSE_ELEMENT, stringArg, Convert.ToString(longArg));
        }

        internal Func<string, long, string> typicalDatabaseInvocation1a =
            new Func<string, long, string>(delegate(string stringArg, long longArg)
            {
                //Console.WriteLine("TypicalDatabaseInvocation invoked...");
                Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
                //Console.WriteLine("TypicalDatabaseInvocation returns...");
                return Concatinate(METHOD_RESPONSE_ELEMENT, stringArg, Convert.ToString(longArg));
                // Or:
                //return TypicalDatabaseStaticInvocation(stringArg, longArg);
            });

        internal Func<string, long, string> typicalDatabaseInvocation1b =
           (delegate(string stringArg, long longArg)
           {
               //Console.WriteLine("TypicalDatabaseInvocation invoked...");
               Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
               //Console.WriteLine("TypicalDatabaseInvocation returns...");
               return Concatinate(METHOD_RESPONSE_ELEMENT, stringArg, Convert.ToString(longArg));
               // Or:
               //return TypicalDatabaseStaticInvocation(stringArg, longArg);
           });

        internal Func<string, long, string> typicalDatabaseInvocation1c =
           (stringArg, longArg) =>
           {
               Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
               return Concatinate(METHOD_RESPONSE_ELEMENT, stringArg, Convert.ToString(longArg));
           };

        internal Func<string, long, string> typicalDatabaseInvocation1d = TypicalDatabaseStaticInvocation;


        internal Func<string, long, string> typicalDatabaseInvocation2(string stringArg, long longArg)
        {
            TypicalDatabaseStaticInvocation(stringArg, longArg);
            return null;
        }


        internal string TypicalDatabaseInvocation3(string stringArg, long longArg)
        {
            return TypicalDatabaseStaticInvocation(stringArg, longArg);
        }
        #endregion

        #region MemoizerHelper
        Func<long, long> slow500Square = (arg1 => { Thread.Sleep(500); return arg1 * arg1; });
        Func<long, long> slow500PowerOfThree = (arg1 => { Thread.Sleep(500); return arg1 * arg1 * arg1; });

        Func<long, long> slow1000Square = (arg1 => { Thread.Sleep(1000); return arg1 * arg1; });
        Func<long, long> slow1000PowerOfThree = (arg1 => { Thread.Sleep(1000); return arg1 * arg1 * arg1; });


        //[Ignore("Temporary disabled...")]
        [Test]
        public void StringsArePrimitivesArentThey()
        {
            Assert.That(typeof(long).IsPrimitive, Is.True);
            Assert.That(typeof(Int64).IsPrimitive, Is.True);
            Assert.That(typeof(ulong).IsPrimitive, Is.True);
            Assert.That(typeof(UInt64).IsPrimitive, Is.True);
            Assert.That(typeof(String).IsPrimitive, Is.False); // Nope, they're not
        }


        //[Ignore("Temporary disabled...")]
        [Test]
        public void ShouldCreateAHashForDelegates()
        {
            Assert.That(MemoizerHelper.CreateParameterHash(40L), Is.EqualTo(MemoizerHelper.CreateParameterHash(40L)));
            //Assert.That(MemoizerHelper.CreateParameterHash(slow500Square), Is.EqualTo(MemoizerHelper.CreateParameterHash(slow500Square)));
            //Assert.That(MemoizerHelper.CreateParameterHash(slow1000Square), Is.EqualTo(MemoizerHelper.CreateParameterHash(slow1000Square)));
            //Assert.That(MemoizerHelper.CreateParameterHash(slow500Square), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(slow1000Square)));
            //Assert.That(MemoizerHelper.CreateParameterHash(slow1000Square), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(slow500Square))); 
            Assert.That(MemoizerHelper.CreateFunctionHash(slow500Square), Is.EqualTo(MemoizerHelper.CreateFunctionHash(slow500Square)));
            Assert.That(MemoizerHelper.CreateFunctionHash(slow1000Square), Is.EqualTo(MemoizerHelper.CreateFunctionHash(slow1000Square)));
            Assert.That(MemoizerHelper.CreateFunctionHash(slow500Square), Is.Not.EqualTo(MemoizerHelper.CreateFunctionHash(slow1000Square)));
            Assert.That(MemoizerHelper.CreateFunctionHash(slow1000Square), Is.Not.EqualTo(MemoizerHelper.CreateFunctionHash(slow500Square)));
        }

        // TODO: test hashing complex objects!

        #endregion

        #region Direct invocation
        //[Ignore("Takes its time...")]
        [Test]
        public void SingleThreadedDirectInvocation()
        {
            // Overrides
            //int NUMBER_OF_ITERATIONS = 3;
            int NUMBER_OF_CONCURRENT_TASKS = 3;

            long startTime = DateTime.Now.Ticks;
            for (int i = 0; i < NUMBER_OF_ITERATIONS; ++i)
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                {
                    var retVal = ReallySlowNetworkInvocation3("SingleThreadedDirectInvocation", 13L);
                    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedDirectInvocation" + 13L));
                }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(NUMBER_OF_ITERATIONS * NETWORK_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine(
                "SingleThreadedDirectInvocation: " + NUMBER_OF_ITERATIONS + " iterations of " + NUMBER_OF_CONCURRENT_TASKS + " sequential, non-memoized method invocations with " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms latency" +
                " took " + durationInMilliseconds + " ms" +
                " (should take > " + NUMBER_OF_ITERATIONS * NUMBER_OF_CONCURRENT_TASKS * NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms)");
        }


        //[Ignore("Temporary disabled...")]
        [Test]
        public void MultiThreadedDirectInvocation()
        {
            this.reallySlowNetworkInvocation1a.CreateExecutionContext(numberOfConcurrentThreadsWitinhEachIteration: NUMBER_OF_CONCURRENT_TASKS,
                                                                      numberOfIterations: NUMBER_OF_ITERATIONS
                //concurrent: true,
                //memoize: false
                //memoizerClearingTask: false,
                //functionLatency: default(long),
                //instrumentation: true
                                                                      )
                //.TestUsingArguments("MultiThreadedDirectInvocation", 15L);
            .Test();
        }
        #endregion

        #region GoetzMemoryCacheMemoizer, a.k.a. THE Memoizer
        Func<string, string> veryExpensiveNullInvocationFunc3 = s =>
        {
            Thread.Sleep(NETWORK_RESPONSE_LATENCY_IN_MILLIS);
            return null;
        };


        //[Ignore("Temporary disabled...")]
        [Test]
        public void ShouldCacheNullValues()
        {
            long startTime = DateTime.Now.Ticks;

            string result1 = veryExpensiveNullInvocationFunc3("whatever");
            Assert.That(result1, Is.Null);

            string result2 = veryExpensiveNullInvocationFunc3("whatever");
            Assert.That(result2, Is.Null);

            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Console.WriteLine("2 invocations of non-memoized function returning null - took " + durationInMilliseconds + " milliseconds (should take > " + 2 * NETWORK_RESPONSE_LATENCY_IN_MILLIS + ")");
            Assert.That(durationInMilliseconds, Is.InRange(2 * NETWORK_RESPONSE_LATENCY_IN_MILLIS - 20L, 2 * NETWORK_RESPONSE_LATENCY_IN_MILLIS + 100L)); // -20 ms due to CLR/platform magic

            startTime = DateTime.Now.Ticks;
            string result3 = veryExpensiveNullInvocationFunc3.CachedInvoke("whatever");
            Assert.That(result3, Is.Null);

            string result4 = veryExpensiveNullInvocationFunc3.CachedInvoke("whatever");
            Assert.That(result4, Is.Null);

            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Console.WriteLine("2 invocations of memoized function returning null - took " + durationInMilliseconds + " milliseconds (should take > " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + ")");
            Assert.That(durationInMilliseconds, Is.InRange(NETWORK_RESPONSE_LATENCY_IN_MILLIS - 20L, 2 * NETWORK_RESPONSE_LATENCY_IN_MILLIS + 100L)); // -20 ms due to CLR/platform magic
        }


        /// <summary>
        /// Sequential invocations of the memoizer
        /// </summary>
        //[Ignore("Temporary disabled...")]
        [Test]
        public void SingleThreadedMemoizedDirectInvocation_Memoizer()
        {
            long startTime = DateTime.Now.Ticks;
            for (int i = 0; i < NUMBER_OF_ITERATIONS; ++i)
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                {
                    var retVal = reallySlowNetworkInvocation1d.CachedInvoke("SingleThreadedMemoizedDirectInvocation_Memoizer", 14L);
                    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocation_Memoizer" + 14L));
                }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.InRange(NETWORK_RESPONSE_LATENCY_IN_MILLIS, NETWORK_RESPONSE_LATENCY_IN_MILLIS + 100));
            Console.WriteLine(
                "SingleThreadedMemoizedDirectInvocation_Memoizer: " + NUMBER_OF_ITERATIONS + " iterations of " + NUMBER_OF_CONCURRENT_TASKS + " sequential, memoized, identical method invocations with " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms latency" +
                " took " + durationInMilliseconds + " ms" +
                " (should take [" + NETWORK_RESPONSE_LATENCY_IN_MILLIS + ", " + (NETWORK_RESPONSE_LATENCY_IN_MILLIS + 100) + "] ms)");
        }


        /// <summary>
        /// Sequential invocation of the memoizer
        /// </summary>
        //[Ignore("Temporary disabled...")]
        [Test]
        public void SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer()
        {
            long startTime = DateTime.Now.Ticks;
            IMemoizer<string, long, string> memoizer = reallySlowNetworkInvocation1b.CacheFor(NETWORK_RESPONSE_LATENCY_IN_MILLIS * 3).Milliseconds.GetMemoizer();

            // New function value, not yet cached
            var retVal = memoizer.InvokeWith("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer", 15L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer" + 15L));
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.InRange(NETWORK_RESPONSE_LATENCY_IN_MILLIS, NETWORK_RESPONSE_LATENCY_IN_MILLIS + 50L));
            Console.WriteLine("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer: first non-memoized method invocation with latency " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take >= " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + ")");

            // Memoized function within time span, should use cached value
            startTime = DateTime.Now.Ticks;
            retVal = memoizer.InvokeWith("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer", 15L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer" + 15L));
            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10L));
            Console.WriteLine("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer: second memoized method invocation with latency " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");

            // New function value, not yet cached
            startTime = DateTime.Now.Ticks;
            retVal = memoizer.InvokeWith("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer", 16L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer" + 16L));
            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.InRange(NETWORK_RESPONSE_LATENCY_IN_MILLIS, NETWORK_RESPONSE_LATENCY_IN_MILLIS + 50L));
            Console.WriteLine("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer: another first non-memoized method invocation with latency " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take >= " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + ")");

            Console.WriteLine("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer: waiting for memoizer cache item evictions ...");
            Thread.Sleep(NETWORK_RESPONSE_LATENCY_IN_MILLIS * 3);

            // Memoized function evicted due to exceeded time span, should take its time (for re-memoization)
            startTime = DateTime.Now.Ticks;
            retVal = memoizer.InvokeWith("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer", 15L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer" + 15L));
            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.InRange(NETWORK_RESPONSE_LATENCY_IN_MILLIS, NETWORK_RESPONSE_LATENCY_IN_MILLIS + 50L));
            Console.WriteLine("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer: third memoized (but evicted) method invocation with latency " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take >= " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + ")");
        }


        //[Ignore("Temporary disabled...")]
        [Test]
        public void MultiThreadedMemoizedInvocation_Memoizer(
            [Values(1, 2, 4, 10, 30, 60, 100, 200, 400, 800, 1000, 1200)] int numberOfConcurrentWorkerThreads,
            [Values(1, 3)] int numberOfIterations)
        {
            int NUMBER_OF_ITERATIONS = numberOfIterations;

            // Not memoized func
            this.reallySlowNetworkInvocation1b
                .CreateExecutionContext(numberOfConcurrentWorkerThreads, NUMBER_OF_ITERATIONS, instrumentation: false)
                .TestUsingArguments("MultiThreadedMemoizedInvocation_NonMemoized", 191L);

            // Memoized func
            this.reallySlowNetworkInvocation1c
                .CreateExecutionContext(numberOfConcurrentWorkerThreads, NUMBER_OF_ITERATIONS, memoize: true, instrumentation: false)
                .TestUsingArguments("MultiThreadedMemoizedInvocation_Memoized", 192L);


            // TODO: seems not to be necessary - but where does the function get cleared from memoizer registry?
            // It doesn't - parameterized NUnit tests using the Value attribute are resetting the (lazy-loaded static) memoizer factory object
            // But I don't know how or why... It is executed as it was a regular standalone NUnit test execution
            // The other explanations is that is a bug/weakness in the MemoizerRegistry...

            // Hmm, the next day... it wasn't so anymore - this was weird...

            // Clean-up: must remove memoized function from registry when doing several test method iterations
            this.reallySlowNetworkInvocation1c.UnMemoize();
        }


        //[Ignore("Temporary disabled...")]
        [Test]
        public void MultiThreadedMemoizedInvocationWithClearing_Memoizer(
            [Values(1, 2, 4, 10, 30, 60, 100, 200, 400, 800, 1000, 1200)] int numberOfConcurrentWorkerThreads
            //[Values(1)] int numberOfConcurrentWorkerThreads,
            //[Values(1, 3)] int numberOfIterations
            )
        {
            //int NUMBER_OF_ITERATIONS = numberOfIterations;

            TwoPhaseExecutionContext<string, long, string> twoPhaseExecutionContext1 =
                this.reallySlowNetworkInvocation1a.CreateExecutionContext(numberOfConcurrentThreadsWitinhEachIteration: numberOfConcurrentWorkerThreads,
                                                                          numberOfIterations: NUMBER_OF_ITERATIONS,
                                                                          memoize: true,
                                                                          instrumentation: false);
            TwoPhaseExecutionContext<string, long, string> twoPhaseExecutionContext2 =
                this.reallySlowNetworkInvocation1d.CreateExecutionContext(
                numberOfConcurrentThreadsWitinhEachIteration: 1,
                numberOfIterations: 100, // N/A as it is merged into another TwoPhaseExecutionContext
                concurrent: true,
                memoize: true,
                memoizerClearingTask: false,
                functionLatency: default(long), // N/A as it is merged into another TwoPhaseExecutionContext
                instrumentation: false);

            twoPhaseExecutionContext1.And(twoPhaseExecutionContext2).Test();

            //twoPhaseExecutionContext1.Test();

            // Clean-up: must remove memoized function from registry when doing several test method iterations
            this.reallySlowNetworkInvocation1a.UnMemoize();
            this.reallySlowNetworkInvocation1d.UnMemoize();

            //Console.WriteLine("---");
        }


        static int FIBONACCI_INVOCATIONS;

        static readonly Func<int, long> fibonacci = (arg =>
            {
                ++FIBONACCI_INVOCATIONS;
                if (arg <= 1) { return arg; }
                return fibonacci(arg - 1) + fibonacci(arg - 2);
            });

        static readonly Func<int, long> fibonacci_memoized = (arg =>
            {
                ++FIBONACCI_INVOCATIONS;
                if (arg <= 1) { return arg; }
                return fibonacci_memoized.CachedInvoke(arg - 1) + fibonacci_memoized.CachedInvoke(arg - 2);
            });


        [Ignore("Temporary disabled...")]
        [Test]
        public void FibonacciFunctionGetsCached()
        {
            //Assert.That(MemoizerHelper.CreateMemoizerHash(fibonacci.Memoize()), Is.EqualTo(MemoizerHelper.CreateMemoizerHash(fibonacci.Memoize())));
            //Assert.That(MemoizerHelper.CreateMemoizerHash(memoizedFibonacci.Memoize()), Is.EqualTo(MemoizerHelper.CreateMemoizerHash(memoizedFibonacci.Memoize())));
            //Assert.That(MemoizerHelper.CreateMemoizerHash(fibonacci.Memoize()), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerHash(memoizedFibonacci.Memoize())));
            //Assert.That(MemoizerHelper.CreateMemoizerHash(memoizedFibonacci.Memoize()), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerHash(fibonacci.Memoize())));

            // TODO: use reflection
            //Assert.That(fibonacci.Memoize().MemoizerConfiguration, Is.EqualTo(fibonacci.Memoize().MemoizerConfiguration));
            //Assert.That(memoizedFibonacci.Memoize().MemoizerConfiguration, Is.EqualTo(memoizedFibonacci.Memoize().MemoizerConfiguration));
            //Assert.That(fibonacci.Memoize().MemoizerConfiguration, Is.Not.EqualTo(memoizedFibonacci.Memoize().MemoizerConfiguration));
            //Assert.That(memoizedFibonacci.Memoize().MemoizerConfiguration, Is.Not.EqualTo(fibonacci.Memoize().MemoizerConfiguration));
        }


        //[Ignore("Temporary disabled...")]
        [Test]
        public void FibonacciNumbers(
            [Values(40)] int numberOfFibonacciArguments)
        {
            FIBONACCI_INVOCATIONS = 0;
            Console.Write("Fibonacci(" + numberOfFibonacciArguments + ") = ");
            StopWatch stopWatch = new StopWatch();
            Console.Write(fibonacci(numberOfFibonacciArguments));
            stopWatch.Stop();
            Assert.That(FIBONACCI_INVOCATIONS, Is.EqualTo(331160281));
            Assert.That(stopWatch.DurationInMilliseconds, Is.GreaterThan(1500));
            Console.WriteLine("Fibonacci function invoked " + FIBONACCI_INVOCATIONS + " times. Took " + stopWatch.DurationInTicks + " ticks | " + stopWatch.DurationInMilliseconds + " ms");

            Console.WriteLine();
            FIBONACCI_INVOCATIONS = 0;
            Console.Write("Fibonacci(" + numberOfFibonacciArguments + ") = ");
            stopWatch.Start();
            Console.Write(fibonacci_memoized(numberOfFibonacciArguments));
            stopWatch.Stop();
            Assert.That(FIBONACCI_INVOCATIONS, Is.EqualTo(41));
            Assert.That(stopWatch.DurationInMilliseconds, Is.LessThan(/*30*/200)); // Hmm, why the immense variation in duration...
            Console.WriteLine();
            Console.WriteLine("Fibonacci function invoked " + FIBONACCI_INVOCATIONS + " times. Took " + stopWatch.DurationInTicks + " ticks | " + stopWatch.DurationInMilliseconds + " ms");
        }


        [Test]
        [Ignore("Demo - activate on demand...")]
        public void FibonacciSequence_NonMemoized(
        [Values(-1, 0, 1, 2, 3, 4, 8, 12, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 39, 40, 41, 42)] int numberOfFibonacciArguments)
        {
            FIBONACCI_INVOCATIONS = 0;
            Console.WriteLine("Fibonacci(" + numberOfFibonacciArguments + ") = ");
            StopWatch stopWatch = new StopWatch();
            for (int i = 0; i <= numberOfFibonacciArguments; ++i)
                Console.Write(fibonacci(i) + " ");
            stopWatch.Stop();
            Console.WriteLine();
            Console.WriteLine("Fibonacci function invoked " + String.Format("{0:0,0}", FIBONACCI_INVOCATIONS) + " times. Took " + String.Format("{0:0,0}", stopWatch.DurationInTicks) + " ticks | " + stopWatch.DurationInMilliseconds + " ms");
            if (numberOfFibonacciArguments > 30)
                Assert.That(stopWatch.DurationInMilliseconds, Is.GreaterThan(50));
        }


        [Test]
        [Ignore("Demo - activate on demand...")]
        public void FibonacciSequence_StillNotMemoized(
        [Values(-1, 0, 1, 2, 3, 4, 8, 12, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 39, 40, 41, 42)] int numberOfFibonacciArguments)
        {
            FIBONACCI_INVOCATIONS = 0;
            Console.WriteLine("Fibonacci(" + numberOfFibonacciArguments + ") = ");
            StopWatch stopWatch = new StopWatch();
            for (int i = 0; i <= numberOfFibonacciArguments; ++i)
                Console.Write(fibonacci.CachedInvoke(i) + " ");
            stopWatch.Stop();
            Console.WriteLine();
            Console.WriteLine("Fibonacci function invoked " + String.Format("{0:0,0}", FIBONACCI_INVOCATIONS) + " times. Took " + String.Format("{0:0,0}", stopWatch.DurationInTicks) + " ticks | " + stopWatch.DurationInMilliseconds + " ms");
            if (numberOfFibonacciArguments > 30)
                Assert.That(stopWatch.DurationInMilliseconds, Is.GreaterThan(50));
        }


        [Test]
        [Ignore("Demo - activate on demand...")]
        public void FibonacciSequence_Memoized(
        [Values(-1, 0, 1, 2, 3, 4, 8, 12, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 39, 40, 41, 42)] int numberOfFibonacciArguments)
        {
            FIBONACCI_INVOCATIONS = 0;
            Console.WriteLine("Fibonacci(" + numberOfFibonacciArguments + ") = ");
            StopWatch stopWatch = new StopWatch();
            for (int i = 0; i <= numberOfFibonacciArguments; ++i)
                Console.Write(fibonacci_memoized(i) + " ");
            stopWatch.Stop();
            Console.WriteLine();
            Console.WriteLine("Fibonacci function invoked " + String.Format("{0:0,0}", FIBONACCI_INVOCATIONS) + " times. Took " + stopWatch.DurationInTicks + " ticks | " + stopWatch.DurationInMilliseconds + " ms");
            if (numberOfFibonacciArguments > 30)
                Assert.That(stopWatch.DurationInMilliseconds, Is.LessThan(10));
        }
        #endregion


        // TODO: test when caching with complex objects as parameters

        // TODO: check the behaviour of the MemoryCache name parameter (Gets or sets the name of a particular cache configuration) via multiple memoizer instances at the same time

    }
    #endregion
}
//#pragma warning restore 618
#endregion
