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

        Func<long, string> typicalDatabaseInvocation_InlinedFunc1 =
           new Func<long, string>(delegate(long longArg)
               {
                   Console.WriteLine("TypicalDatabaseInvocation invoked...");
                   Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
                   Console.WriteLine("TypicalDatabaseInvocation returns...");
                   return METHOD_RESPONSE_ELEMENT + longArg;
               });

        Func<long, string> typicalDatabaseInvocation_InlinedFunc2 =
            delegate(long longArg)
            {
                Console.WriteLine("TypicalDatabaseInvocation invoked...");
                Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
                Console.WriteLine("TypicalDatabaseInvocation returns...");
                return METHOD_RESPONSE_ELEMENT + longArg;
            };


        Func<long, string> typicalDatabaseInvocation_DelegatedFunc1 =
            new Func<long, string>(delegate(long longArg)
                {
                    return TypicalDatabaseStaticInvocation(longArg);
                });

        Func<long, string> typicalDatabaseInvocation_DelegatedFunc2 =
            delegate(long longArg)
            {
                return TypicalDatabaseStaticInvocation(longArg);
            };

        Func<long, string> typicalDatabaseInvocation_DelegatedFunc3 = TypicalDatabaseStaticInvocation;

        Func<long, string> typicalDatabaseInvocation_DelegatedFunc4(long longArg)
        {
            TypicalDatabaseStaticInvocation(longArg);
            return null;
        }

        #region README
        static readonly Func<long, string> myExpensiveFunction = TypicalDatabaseStaticInvocation;

        //Example 1 [default caching policy]
        string ExpensiveFunction(long someId)
        {
            return myExpensiveFunction.CachedInvoke(someId);
        }

        //Example 2 [expiration policy: keep items cached for 30 minutes]
        string ExpensiveFunctionWithExpiration(long someId)
        {
            //return myExpensiveFunction.Memoize().KeepItemsCachedFor(30).Minutes.GetMemoizer().InvokeWith(someId);
            //// Or
            ////return myExpensiveFunction.CacheFor(30).Minutes.GetMemoizer().InvokeWith(someId);
            return myExpensiveFunction.Memoize().KeepItemsCachedFor(1).Seconds.GetMemoizer().InvokeWith(someId);
            // Or
            //return myExpensiveFunction.CacheFor(1).Seconds.GetMemoizer().InvokeWith(someId);
        }

        //Example 3 [expiration policy: parameterized number of minutes]
        string ExpensiveFunctionWithParameterizedExpiration(long someId, int expirationInMinutes = 30)
        {
            return myExpensiveFunction.CacheFor(expirationInMinutes).Minutes.GetMemoizer().InvokeWith(someId);
        }


        //Example 4 [forced expiration/clearing]
        void ExpensiveFunctionCacheClearing()
        {
            myExpensiveFunction.GetMemoizer().Clear();
        }

        Action expensiveFunctionCacheClearingAction = myExpensiveFunction.GetMemoizer().Clear;


        //Example 5 [using IMemoizer reference]
        IMemoizer<long, string> myExpensiveFunctionMemoizer = myExpensiveFunction.CacheFor(30).Minutes.InstrumentWith(Console.WriteLine).GetMemoizer();

        string ExpensiveFunctionWithExpiration2(long someId) { return myExpensiveFunctionMemoizer.InvokeWith(someId); }
        void ExpensiveFunctionCacheClearing2() { myExpensiveFunctionMemoizer.Clear(); }
        #endregion

        [Test]
        public void RunExample1()
        {
            long startTime = DateTime.Now.Ticks;
            string retVal = ExpensiveFunction(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 1: first memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunction(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
            Console.WriteLine("Example 1: second memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");
        }

        
        [Test]
        public void RunExample2()
        {
            long startTime = DateTime.Now.Ticks;
            string retVal = ExpensiveFunctionWithExpiration(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 2b: first memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunctionWithExpiration(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
            Console.WriteLine("Example 2b: second memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");

            Thread.Sleep(1000); // Memoized value expires

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunctionWithExpiration(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 2b: third memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunctionWithExpiration(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
            Console.WriteLine("Example 2b: forth memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");

            //ClearMemoizer();
            //memoizerClearingAction.Invoke();
            ExpensiveFunctionCacheClearing2();

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunctionWithExpiration2(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 2b: fifth memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");
        
            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunctionWithExpiration2(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
            Console.WriteLine("Example 2b: forth memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");
        }
        

        // TODO: hmm, putting the casted expression inside a method does not help...
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

            Console.WriteLine("FuncCastedStaticMethodMemoizerDoesNotGetMemoized: second on-the-spot-memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms ...)");
        }

        
        // Strange timing issues with this approach... don't know exactly why
        long LatencyInstrumentedRun(MemoizerFactory<long, string> memoizerFactory, long latencyInMilliseconds = 0, string heading = "", string ending = "")
        {
            long startTime = DateTime.Now.Ticks;
            string retVal = memoizerFactory.GetMemoizer().InvokeWith(4224L);
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 4224L));
            Console.WriteLine(heading + " invocation with latency " + latencyInMilliseconds + " ms took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks " + ending);
            return durationInMilliseconds;
        }

        // TODO: MemoizerFactory is mutable - not good enough as it is key in the memoizer^2...
        [Test]
        public void MultipleCacheItemPolicies_MemoizerFactoryIsMutable___()
        {
            MemoizerFactory<long, string> myMessyMemoizerFactory =
                typicalDatabaseInvocation_InlinedFunc2.CacheFor(12).Hours
                                                      .InstrumentWith(Console.WriteLine);

            IMemoizer<long, string> myMemoizedFunction = myMessyMemoizerFactory.GetMemoizer();

            //Assert.That(LatencyInstrumentedRun(myMessyMemoizerFactory, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "First method invocation", "(should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)"),
            //    Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            long startTime = DateTime.Now.Ticks;
            string retVal = myMemoizedFunction.InvokeWith(4224L);
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 4224L));
            Console.WriteLine("(4224) invocation with latency " + durationInMilliseconds + " ms took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks " + "(should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            //Assert.That(LatencyInstrumentedRun(myMessyMemoizerFactory, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "Second method invocation", "(cached, should take < 5 ms)"),
            //    Is.LessThan(20)); // ms (20 ms just for LatencyInstrumentedRun method to finish...)
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizedFunction.InvokeWith(4224L);
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 4224L));
            Console.WriteLine("(4224) invocation with latency " + durationInMilliseconds + " ms took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks " + "(cached, should take < 5 ms)");
            //Assert.That(durationInTicks, Is.LessThan(50500)); // ticks
            // TODO: hmm, takes its time...
            Assert.That(durationInMilliseconds, Is.LessThan(25)); // ms

            myMessyMemoizerFactory = myMessyMemoizerFactory.KeepItemsCachedFor(0).Milliseconds
                                                           .InstrumentWith(Console.WriteLine)
                                                           .KeepItemsCachedFor(120).Milliseconds;

            IMemoizer<long, string> myMemoizedFunction2 = myMessyMemoizerFactory.GetMemoizer();

            //Assert.That(LatencyInstrumentedRun(myMessyMemoizerFactory, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "Second method invocation", "(cached, should take < 5 ms)"),
            //    Is.LessThan(20)); // ms (20 ms just for LatencyInstrumentedRun method to finish...)
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizedFunction2.InvokeWith(123456L);
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 123456L));
            Console.WriteLine("(123456) invocation with latency " + durationInMilliseconds + " ms took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks " + "(should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            startTime = DateTime.Now.Ticks;
            retVal = myMemoizedFunction2.InvokeWith(123456);
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 123456));
            Console.WriteLine("(123456) invocation with latency " + durationInMilliseconds + " ms took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks " + "(cached, should take < 5 ms)");
            Assert.That(durationInMilliseconds, Is.LessThan(5)); // ms

            Thread.Sleep(300);

            //Assert.That(LatencyInstrumentedRun(myMessyMemoizerFactory, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "Third method invocation", "(cached, should take < 5 ms)"),
            //    Is.LessThan(20)); // ms (20 ms just for LatencyInstrumentedRun method to finish...)
            // Previous memoizer still intact
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizedFunction.InvokeWith(4224L);
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 4224L));
            Console.WriteLine("(4224) invocation with latency " + durationInMilliseconds + " ms took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks " + "(cached, should take < 5 ms)");
            Assert.That(durationInMilliseconds, Is.LessThan(5)); // ms

            startTime = DateTime.Now.Ticks;
            retVal = myMemoizedFunction2.InvokeWith(123456L);
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 123456L));
            Console.WriteLine("(123456L) invocation with latency " + durationInMilliseconds + " ms took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks " + "(should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
        }


        // TODO: concurrent use of different and equal memoizer factories with the same func

    }
}
