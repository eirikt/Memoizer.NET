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
using System;
using System.Runtime.Caching;
using System.Threading;
using NUnit.Framework;

namespace Memoizer.NET.Test
{

    [TestFixture]
    class ExamplesTests
    {
        const int DATABASE_RESPONSE_LATENCY_IN_MILLIS = 50;
        const string METHOD_RESPONSE_ELEMENT = "VeryExpensiveMethodResponseFor";


        static string TypicalDatabaseStaticInvocation(long longArg)
        {
            Console.WriteLine("TypicalDatabaseInvocation invoked...");
            Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
            Console.WriteLine("TypicalDatabaseInvocation returns...");
            return METHOD_RESPONSE_ELEMENT + longArg;
        }

        string TypicalDatabaseInvocation1(long longArg)
        {
            return TypicalDatabaseStaticInvocation(longArg);
        }

        Func<long, string> TypicalDatabaseInvocation2a =
           new Func<long, string>(delegate(long longArg)
           {
               Console.WriteLine("TypicalDatabaseInvocation invoked...");
               Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
               Console.WriteLine("TypicalDatabaseInvocation returns...");
               return METHOD_RESPONSE_ELEMENT + longArg;
               // Or via _static_ method declaration:
               //return TypicalDatabaseStaticInvocation(longArg);
           });

        Func<long, string> TypicalDatabaseInvocation2b =
          delegate(long longArg)
          {
              Console.WriteLine("TypicalDatabaseInvocation invoked...");
              Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
              Console.WriteLine("TypicalDatabaseInvocation returns...");
              return METHOD_RESPONSE_ELEMENT + longArg;
              // Or via _static_ method declaration:
              //return TypicalDatabaseStaticInvocation(longArg);
          };

        Func<long, string> TypicalDatabaseInvocation2c = TypicalDatabaseStaticInvocation;

        Func<long, string> TypicalDatabaseInvocation3(long longArg)
        {
            TypicalDatabaseStaticInvocation(longArg);
            return null;
        }


        // Example 1 [default caching policy]
        readonly Func<long, string> MyExpensiveFunction1 = TypicalDatabaseStaticInvocation;

        string ExpensiveFunction1(long someId)
        {
            return MyExpensiveFunction1.CachedInvoke(someId);
        }


        //// Example 2a [expiration policy: keep items alive for 30 minutes]
        //readonly Func<long, string> MyExpensiveFunction2a = TypicalDatabaseStaticInvocation;

        ////readonly CacheItemPolicy cacheItemEvictionPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(30)) };
        //readonly CacheItemPolicy cacheItemEvictionPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMilliseconds(1000L) };

        //string ExpensiveFunction2a(long someId)
        //{
        //    return MyExpensiveFunction2a.Memoize().CachePolicy(cacheItemEvictionPolicy).Get().InvokeWith(someId);
        //}


        // Example 2b [expiration policy: keep items alive for 30 minutes]
        readonly Func<long, string> MyExpensiveFunction2b = TypicalDatabaseStaticInvocation;

        string ExpensiveFunction2b(long someId)
        {
            return MyExpensiveFunction2b.CacheFor(1).Seconds.GetMemoizer().InvokeWith(someId);
        }



        #region Not so concurrent environments

        readonly Func<long, string> MyExpensiveFunction1_NotSoThreadSafeButCompact = TypicalDatabaseStaticInvocation;

        // [default caching policy]
        [Test]
        public void NotSoThreadSafe_ButCompact()
        {
            long startTime = DateTime.Now.Ticks;

            //string retVal = ((Func<long, string>)TypicalDatabaseStaticInvocation).Cache().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(42L); // Fails
            string retVal = MyExpensiveFunction1_NotSoThreadSafeButCompact.CachedInvoke(42L); // OK
            //string retVal = ExpensiveFunction1(42L); // OK

            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 1: first on-the-spot-memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;

            //retVal = ((Func<long, string>)TypicalDatabaseStaticInvocation).Cache().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(42L); // Fails
            retVal = MyExpensiveFunction1_NotSoThreadSafeButCompact.CachedInvoke(42L); // OK
            //retVal = ExpensiveFunction1(42L); // OK

            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
            Console.WriteLine("Example 1: second on-the-spot-memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");
        }


        // TODO: putting the casted expression inside a method does not help either...
        [Test]
        public void FuncCastedStaticMethodMemoizerDoesNotGetMemoized()
        {
            long startTime = DateTime.Now.Ticks;

            string retVal = ((Func<long, string>)TypicalDatabaseStaticInvocation).Cache().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(42L);

            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("FuncCastedStaticMethodMemoizerDoesNotGetMemoized: first on-the-spot-memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;

            retVal = ((Func<long, string>)TypicalDatabaseStaticInvocation).Cache().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(42L); // Fails

            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            // NB!
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            Console.WriteLine("FuncCastedStaticMethodMemoizerDoesNotGetMemoized: secondfirst on-the-spot-memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");
            Console.WriteLine("Example 1: second on-the-spot-memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");
        }
        #endregion

        #region Concurrent environments

        readonly IMemoizer<long, string> MyExpensiveMemoizedFunction = ((Func<long, string>)TypicalDatabaseStaticInvocation).CreateMemoizer();

        public void ThreadSafe()
        {
            long startTime = DateTime.Now.Ticks;
            string retVal = MyExpensiveMemoizedFunction.InvokeWith(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 1: first on-the-spot-memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;
            retVal = MyExpensiveMemoizedFunction.InvokeWith(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
            Console.WriteLine("Example 1: second on-the-spot-memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");
        }
        #endregion

        [Test]
        public void RunExample1()
        {
            long startTime = DateTime.Now.Ticks;
            string retVal = ExpensiveFunction1(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 1: first memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunction1(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
            Console.WriteLine("Example 1: second memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");
        }


        //[Test]
        //public void RunExample2a()
        //{
        //    long startTime = DateTime.Now.Ticks;
        //    string retVal = ExpensiveFunction2a(42L);
        //    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
        //    long durationInTicks = DateTime.Now.Ticks - startTime;
        //    long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
        //    Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
        //    Console.WriteLine("Example 2a: first memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

        //    startTime = DateTime.Now.Ticks;
        //    retVal = ExpensiveFunction2a(42L);
        //    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
        //    durationInTicks = DateTime.Now.Ticks - startTime;
        //    durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
        //    Assert.That(durationInMilliseconds, Is.LessThan(10));
        //    Console.WriteLine("Example 2a: second memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");

        //    Thread.Sleep(1000);

        //    startTime = DateTime.Now.Ticks;
        //    retVal = ExpensiveFunction2a(42L);
        //    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
        //    durationInTicks = DateTime.Now.Ticks - startTime;
        //    durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
        //    Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
        //    Console.WriteLine("Example 2a: third memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");
        //}


        [Test]
        public void RunExample2b()
        {
            long startTime = DateTime.Now.Ticks;
            string retVal = ExpensiveFunction2b(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 2b: first memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunction2b(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
            Console.WriteLine("Example 2b: second memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");

            Thread.Sleep(1000);

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunction2b(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 2b: third memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");
        }

        #region API
        //[Test]
        //public void MultipleMemoizedFunc()
        //{
        //    Func<long, string> MyMemoizerAbusedFunction = MyExpensiveFunction1.MemoizedFunc().MemoizedFunc().MemoizedFunc();

        //    //Assert.That(LatencyInstrumentedRun(MyMemoizerAbusedFunction, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "First memoized^3 method"), Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS * TimeSpan.TicksPerMillisecond));
        //    long startTime = DateTime.Now.Ticks;
        //    string retVal = MyMemoizerAbusedFunction(42L);
        //    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
        //    long durationInTicks = DateTime.Now.Ticks - startTime;
        //    long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
        //    Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
        //    Console.WriteLine("First memoized^3 method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

        //    //Assert.That(LatencyInstrumentedRun(MyMemoizerAbusedFunction, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "Second memoized^3 method"), Is.LessThan(10*TimeSpan.TicksPerMillisecond));
        //    startTime = DateTime.Now.Ticks;
        //    retVal = MyMemoizerAbusedFunction(42L);
        //    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
        //    durationInTicks = DateTime.Now.Ticks - startTime;
        //    durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
        //    Assert.That(durationInMilliseconds, Is.LessThan(10));
        //    Console.WriteLine("Second memoized^3 method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");
        //}


        [Test]
        public void MultipleCacheItemPolicies_LatestAddedOverridesPrevouslyAddedOnes_NotThreadSafeVersion()
        {
            MemoizerBuilder<long, string> myMessyMemoizerBuilder =
                //((Func<long, string>)TypicalDatabaseStaticInvocation).Cache()
                TypicalDatabaseInvocation2c.Cache()
                                           .CachePolicy(default(CacheItemPolicy))
                                           .InstrumentWith(Console.WriteLine);
            //IInvocable<long, string> MyMemoizerAbusedInvocable = MyMemoizerAbusedFunction.GetInvocable();
            myMessyMemoizerBuilder = myMessyMemoizerBuilder.CachePolicy(default(CacheItemPolicy))
                                                           .KeepElementsCachedFor(0).Milliseconds
                                                           .KeepElementsCachedFor(12).Milliseconds
                                                           .KeepElementsCachedFor(120).Milliseconds;
            Assert.That(LatencyInstrumentedRun(myMessyMemoizerBuilder, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "First method invocation", "(should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)"),
                Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS * TimeSpan.TicksPerMillisecond));
            // TODO: ajaj - this is not thread-safe!!
            Assert.That(LatencyInstrumentedRun(myMessyMemoizerBuilder, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "Second method invocation", "(cached, should take < 5 ms)"),
                Is.LessThan(5)); // ms
            Thread.Sleep(30);
            Assert.That(LatencyInstrumentedRun(myMessyMemoizerBuilder, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "Third method invocation", "(cached, should take < 5 ms)"),
                Is.LessThan(5)); // ms
        }

        long LatencyInstrumentedRun(MemoizerBuilder<long, string> memoizerBuilder, long latencyInMilliseconds, string heading, string ending)
        {
            long startTime = DateTime.Now.Ticks;
            string retVal = memoizerBuilder.GetMemoizer().InvokeWith(4224L);
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 4224L));
            //Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine(heading + " invocation with latency " + latencyInMilliseconds + " ms took " + durationInMilliseconds + " ms" + " " + ending);
            return durationInTicks;
        }

        //long LatencyInstrumentedRun(Func<long, string> func, long latencyInMilliseconds, string heading)
        //{
        //    long startTime = DateTime.Now.Ticks;
        //    string retVal = func(4224L);
        //    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 4224L));
        //    long durationInTicks = DateTime.Now.Ticks - startTime;
        //    long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
        //    //Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
        //    Console.WriteLine(heading + " invocation with latency " + latencyInMilliseconds + " ms took " + durationInMilliseconds + " ms");// (should take > " + latencyInMilliseconds + " ms)");
        //    return durationInTicks;
        //}


        [Test]
        public void MultipleCacheItemPolicies_LatestAddedOverridesPrevouslyAddedOnes_ThreadSafeVersion()
        {
            IMemoizer<long, string> myMessyMemoizerBuilder1 =
                ((Func<long, string>)TypicalDatabaseStaticInvocation).Cache()
                                    .CachePolicy(default(CacheItemPolicy))
                                    .InstrumentWith(Console.WriteLine)
                                    .CreateMemoizer(); // unique memoizer
            IMemoizer<long, string> myMessyMemoizerBuilder2 = ((Func<long, string>)TypicalDatabaseStaticInvocation).Cache()
                                                                                  .CachePolicy(default(CacheItemPolicy))
                                                                                  .KeepElementsCachedFor(0).Milliseconds
                                                                                  .KeepElementsCachedFor(12).Milliseconds
                                                                                  .KeepElementsCachedFor(120).Milliseconds
                                                                                  .GetMemoizer(); // cached and shared memoizer
            // TODO: ...
            //Assert.That(LatencyInstrumentedRun(myMessyMemoizerBuilder, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "First method invocation", "(should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)"),
            //    Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS * TimeSpan.TicksPerMillisecond));
            //Assert.That(LatencyInstrumentedRun(myMessyMemoizerBuilder, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "Second method invocation", "(cached, should take < 5 ms)"),
            //    Is.LessThan(5)); // ms
            //Thread.Sleep(30);
            //Assert.That(LatencyInstrumentedRun(myMessyMemoizerBuilder, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "Third method invocation", "(cached, should take < 5 ms)"),
            //    Is.LessThan(5)); // ms
        }

        long LatencyInstrumentedRun(IMemoizer<long, string> memoizerBuilder, long latencyInMilliseconds, string heading, string ending)
        {
            long startTime = DateTime.Now.Ticks;
            string retVal = memoizerBuilder.InvokeWith(4224L);
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 4224L));
            //Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine(heading + " invocation with latency " + latencyInMilliseconds + " ms took " + durationInMilliseconds + " ms" + " " + ending);
            return durationInTicks;
        }

        //long LatencyInstrumentedRun(Func<long, string> func, long latencyInMilliseconds, string heading)
        //{
        //    long startTime = DateTime.Now.Ticks;
        //    string retVal = func(4224L);
        //    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 4224L));
        //    long durationInTicks = DateTime.Now.Ticks - startTime;
        //    long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
        //    //Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
        //    Console.WriteLine(heading + " invocation with latency " + latencyInMilliseconds + " ms took " + durationInMilliseconds + " ms");// (should take > " + latencyInMilliseconds + " ms)");
        //    return durationInTicks;
        //}
        #endregion
    }
}
