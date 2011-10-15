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

        string TypicalDatabaseInvocation(long longArg)
        {
            Console.WriteLine("TypicalDatabaseInvocation invoked...");
            Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
            Console.WriteLine("TypicalDatabaseInvocation returns...");
            return METHOD_RESPONSE_ELEMENT + longArg;
        }

        Func<long, string> typicalDatabaseInvocation_InlinedFunc1 = new Func<long, string>(delegate(long longArg)
           {
               Console.WriteLine("TypicalDatabaseInvocation invoked...");
               Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
               Console.WriteLine("TypicalDatabaseInvocation returns...");
               return METHOD_RESPONSE_ELEMENT + longArg;
           });

        Func<long, string> typicalDatabaseInvocation_InlinedFunc2 = delegate(long longArg)
          {
              Console.WriteLine("TypicalDatabaseInvocation invoked...");
              Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
              Console.WriteLine("TypicalDatabaseInvocation returns...");
              return METHOD_RESPONSE_ELEMENT + longArg;
          };


        Func<long, string> typicalDatabaseInvocation_DelegatedFunc1 = new Func<long, string>(delegate(long longArg)
           {
               return TypicalDatabaseStaticInvocation(longArg);
           });

        Func<long, string> typicalDatabaseInvocation_DelegatedFunc2 = delegate(long longArg)
          {
              return TypicalDatabaseStaticInvocation(longArg);
          };

        Func<long, string> typicalDatabaseInvocation_DelegatedFunc3 = TypicalDatabaseStaticInvocation;

        Func<long, string> typicalDatabaseInvocation_DelegatedFunc4(long longArg)
        {
            TypicalDatabaseStaticInvocation(longArg);
            return null;
        }


        // Example 1 [default caching policy]
        readonly Func<long, string> myExpensiveFunction = TypicalDatabaseStaticInvocation;

        string ExpensiveFunction1(long someId)
        {
            return myExpensiveFunction.CachedInvoke(someId);
        }


        // TODO: explicit CacheItemPolicy setting not supported - should we?
        //// Example 2a [expiration policy: keep items alive for 30 minutes]
        //readonly Func<long, string> MyExpensiveFunction2a = TypicalDatabaseStaticInvocation;

        ////readonly CacheItemPolicy cacheItemEvictionPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(30)) };
        //readonly CacheItemPolicy cacheItemEvictionPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMilliseconds(1000L) };

        //string ExpensiveFunction2a(long someId)
        //{
        //    return MyExpensiveFunction2a.Memoize().CachePolicy(cacheItemEvictionPolicy).Get().InvokeWith(someId);
        //}


        // Example 2b [expiration policy: keep items alive for 1 secons]
        //readonly Func<long, string> myExpensiveFunction2b = TypicalDatabaseStaticInvocation;

        string ExpensiveFunction2b(long someId)
        {
            return myExpensiveFunction.CacheFor(1).Seconds.GetMemoizer().InvokeWith(someId);
            // Or
            //return MyExpensiveFunction2b.Memoize().KeepElementsCachedFor(1).Seconds.GetMemoizer().InvokeWith(someId);
        }


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

            Thread.Sleep(1000); // Memoized value expires

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunction2b(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 2b: third memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");
        }


        //#region Not so concurrent environments

        //readonly Func<long, string> MyExpensiveFunction1_NotSoThreadSafeButCompact = TypicalDatabaseStaticInvocation;

        //// [default caching policy]
        //[Test]
        //public void NotSoThreadSafe_ButCompact()
        //{
        //    long startTime = DateTime.Now.Ticks;

        //    string retVal = MyExpensiveFunction1_NotSoThreadSafeButCompact.Memoize().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(42L); // Fails
        //    //string retVal = .CachedInvoke(42L); // OK
        //    //string retVal = ExpensiveFunction1(42L); // OK

        //    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
        //    long durationInTicks = DateTime.Now.Ticks - startTime;
        //    long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
        //    Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
        //    Console.WriteLine("Example 1: first on-the-spot-memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

        //    startTime = DateTime.Now.Ticks;

        //    retVal = MyExpensiveFunction1_NotSoThreadSafeButCompact.Memoize().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(42L); // Fails
        //    //retVal = MyExpensiveFunction1_NotSoThreadSafeButCompact.CachedInvoke(42L); // OK
        //    //retVal = ExpensiveFunction1(42L); // OK

        //    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
        //    durationInTicks = DateTime.Now.Ticks - startTime;
        //    durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
        //    Assert.That(durationInMilliseconds, Is.LessThan(10));
        //    Console.WriteLine("Example 1: second on-the-spot-memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");
        //}


        // TODO: putting the casted expression inside a method does not help...
        [Test]
        public void FuncCastedStaticMethodMemoizerDoesNotGetCached()
        {
            long startTime = DateTime.Now.Ticks;

            string retVal = ((Func<long, string>)TypicalDatabaseStaticInvocation).Memoize().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(42L);

            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("FuncCastedStaticMethodMemoizerDoesNotGetMemoized: first on-the-spot-memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;

            retVal = ((Func<long, string>)TypicalDatabaseStaticInvocation).Memoize().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(42L); // Fails

            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            // NB!
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            Console.WriteLine("FuncCastedStaticMethodMemoizerDoesNotGetMemoized: secondfirst on-the-spot-memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");
            Console.WriteLine("Example 1: second on-the-spot-memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");
        }
        //#endregion

        //#region Concurrent environments

        //readonly IMemoizer<long, string> MyExpensiveMemoizedFunction = ((Func<long, string>)TypicalDatabaseStaticInvocation).CreateMemoizer();

        //public void ThreadSafe()
        //{
        //    long startTime = DateTime.Now.Ticks;
        //    string retVal = MyExpensiveMemoizedFunction.InvokeWith(42L);
        //    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
        //    long durationInTicks = DateTime.Now.Ticks - startTime;
        //    long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
        //    Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
        //    Console.WriteLine("Example 1: first on-the-spot-memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

        //    startTime = DateTime.Now.Ticks;
        //    retVal = MyExpensiveMemoizedFunction.InvokeWith(42L);
        //    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
        //    durationInTicks = DateTime.Now.Ticks - startTime;
        //    durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
        //    Assert.That(durationInMilliseconds, Is.LessThan(10));
        //    Console.WriteLine("Example 1: second on-the-spot-memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");
        //}
        //#endregion


        // Strange timing issues with this approach... don't know exactly why
        //long LatencyInstrumentedRun(MemoizerBuilder<long, string> memoizerBuilder, long latencyInMilliseconds = 0, string heading = "", string ending = "")
        //{
        //    long startTime = DateTime.Now.Ticks;
        //    string retVal = memoizerBuilder.GetMemoizer().InvokeWith(4224L);
        //    long durationInTicks = DateTime.Now.Ticks - startTime;
        //    long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
        //    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 4224L));
        //    Console.WriteLine(heading + " invocation with latency " + latencyInMilliseconds + " ms took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks " + ending);
        //    return durationInMilliseconds;
        //}


        // TODO: rename...
        [Test]
        public void MultipleCacheItemPolicies_LatestAddedOverridesPrevouslyAddedOnes___()
        {
            MemoizerBuilder<long, string> myMessyMemoizerBuilder =
                typicalDatabaseInvocation_InlinedFunc2.CacheFor(12).Hours
                                                      .InstrumentWith(Console.WriteLine);

            IMemoizer<long, string> myMemoizedFunction = myMessyMemoizerBuilder.GetMemoizer();

            //Assert.That(LatencyInstrumentedRun(myMessyMemoizerBuilder, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "First method invocation", "(should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)"),
            //    Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            long startTime = DateTime.Now.Ticks;
            string retVal = myMemoizedFunction.InvokeWith(4224L);
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 4224L));
            Console.WriteLine("4224 invocation with latency " + durationInMilliseconds + " ms took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks " + "(should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            //Assert.That(LatencyInstrumentedRun(myMessyMemoizerBuilder, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "Second method invocation", "(cached, should take < 5 ms)"),
            //    Is.LessThan(20)); // ms (20 ms just for LatencyInstrumentedRun method to finish...)
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizedFunction.InvokeWith(4224L);
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 4224L));
            Console.WriteLine("4224 invocation with latency " + durationInMilliseconds + " ms took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks " + "(cached, should take < 5 ms)");
            Assert.That(durationInTicks, Is.LessThan(50000)); // ticks
            Assert.That(durationInMilliseconds, Is.LessThan(5)); // ms

            myMessyMemoizerBuilder = myMessyMemoizerBuilder.KeepItemsCachedFor(0).Milliseconds
                                                           .InstrumentWith(Console.WriteLine)
                                                           .KeepItemsCachedFor(120).Milliseconds;

            IMemoizer<long, string> myMemoizedFunction2 = myMessyMemoizerBuilder.GetMemoizer();

            //Assert.That(LatencyInstrumentedRun(myMessyMemoizerBuilder, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "Second method invocation", "(cached, should take < 5 ms)"),
            //    Is.LessThan(20)); // ms (20 ms just for LatencyInstrumentedRun method to finish...)
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizedFunction2.InvokeWith(123456L);
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 123456L));
            Console.WriteLine("123456 invocation with latency " + durationInMilliseconds + " ms took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks " + "(should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            startTime = DateTime.Now.Ticks;
            retVal = myMemoizedFunction2.InvokeWith(123456);
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 123456));
            Console.WriteLine("123456 invocation with latency " + durationInMilliseconds + " ms took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks " + "(cached, should take < 5 ms)");
            Assert.That(durationInMilliseconds, Is.LessThan(5)); // ms

            Thread.Sleep(300);

            //Assert.That(LatencyInstrumentedRun(myMessyMemoizerBuilder, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "Third method invocation", "(cached, should take < 5 ms)"),
            //    Is.LessThan(20)); // ms (20 ms just for LatencyInstrumentedRun method to finish...)
            // Previous memoizer still intact
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizedFunction.InvokeWith(4224L);
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 4224L));
            Console.WriteLine("4224 invocation with latency " + durationInMilliseconds + " ms took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks " + "(cached, should take < 5 ms)");
            Assert.That(durationInMilliseconds, Is.LessThan(5)); // ms

            startTime = DateTime.Now.Ticks;
            retVal = myMemoizedFunction2.InvokeWith(123456L);
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 123456L));
            Console.WriteLine("4224 invocation with latency " + durationInMilliseconds + " ms took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks " + "(should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
        }


        // TODO: concurrent use of different and equal memoizer builders with the same func



    }
}
