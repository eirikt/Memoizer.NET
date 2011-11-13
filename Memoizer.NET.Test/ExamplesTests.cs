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

        // Example 1 [default caching policy]
        string ExpensiveFunction(long someId)
        {
            return myExpensiveFunction.CachedInvoke(someId);
        }

        // Example 2 [expiration policy: keep items cached for 30 minutes]
        string ExpensiveFunctionWithExpiration(long someId)
        {
            //return myExpensiveFunction.Memoize().KeepItemsCachedFor(30).Minutes.GetMemoizer().InvokeWith(someId);
            //// Or
            ////return myExpensiveFunction.CacheFor(30).Minutes.GetMemoizer().InvokeWith(someId);
            return myExpensiveFunction.Memoize().KeepItemsCachedFor(1).Seconds.GetMemoizer().InvokeWith(someId);
            // Or
            //return myExpensiveFunction.CacheFor(1).Seconds.GetMemoizer().InvokeWith(someId);
        }

        // Example 3 [forced expiration/clearing]
        void ExpensiveFunctionCacheClearing()
        {
            myExpensiveFunction.GetMemoizer().Clear();
        }

        Action expensiveFunctionCacheClearingAction = myExpensiveFunction.GetMemoizer().Clear;


        // Example 4 [using IMemoizer reference]
        IMemoizer<long, string> myExpensiveFunctionMemoizer = myExpensiveFunction.CacheFor(30).Minutes.InstrumentWith(Console.WriteLine).CreateMemoizer();

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
            Console.WriteLine("Example 2: first memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunctionWithExpiration(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
            Console.WriteLine("Example 2: second memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");

            Thread.Sleep(1000); // Memoized value expires

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunctionWithExpiration(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 2: third memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunctionWithExpiration(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
            Console.WriteLine("Example 2: forth memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");
        }


        [Test]
        public void RunExample3()
        {
            ExpensiveFunctionCacheClearing();

            long startTime = DateTime.Now.Ticks;
            string retVal = ExpensiveFunction(113L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 113L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 3: first memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunction(113L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 113L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
            Console.WriteLine("Example 3: second memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");

            myExpensiveFunction.GetMemoizer().Clear();

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunction(113L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 113L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 3: fifth memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunction(113L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 113L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
            Console.WriteLine("Example 3: forth memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");
        }


        static readonly Func<long, long> FIBONACCI = (arg => arg < 2 ? arg : FIBONACCI(arg - 1) + FIBONACCI(arg - 2));
        static readonly Func<long, long> MEMOIZED_FIBONACCI = (arg => arg < 2 ? arg : MEMOIZED_FIBONACCI.CachedInvoke(arg - 1) + MEMOIZED_FIBONACCI.CachedInvoke(arg - 2));

        [Test]
        public void FibonacciSequence()
        {
            Console.Write("Fibonacci(38) = ");
            long startTime = DateTime.Now.Ticks;
            Console.WriteLine(FIBONACCI(40));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Console.WriteLine("Fibonacci function took " + durationInTicks + " ticks | " + durationInMilliseconds + " ms");
            Assert.That(durationInMilliseconds, Is.GreaterThan(1000));

            Console.WriteLine();
            Console.Write("Fibonacci(38) = ");
            startTime = DateTime.Now.Ticks;
            Console.WriteLine(MEMOIZED_FIBONACCI(40));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Console.WriteLine("Fibonacci (memoized) function took " + durationInTicks + " ticks | " + durationInMilliseconds + " ms");
            Assert.That(durationInMilliseconds, Is.LessThan(50));
        }


        [Test]
        public void UnMemoize()
        {
            MemoizerFactory<long, string> myMemoizerFactory = myExpensiveFunction.CacheFor(30).Minutes.InstrumentWith(Console.WriteLine);

            // Nope...
            //MemoizerConfiguration myMemoizerConfig = new MemoizerConfiguration(myExpensiveFunction, ExpirationType.Relative, 30, TimeUnit.Minutes, Console.WriteLine);
            //MemoizerFactory<long, string> myMemoizerFactory = new MemoizerFactory<long, string>(myMemoizerConfig);

            // Maybe this standalone memoizer instance is already cached, maybe not...
            myExpensiveFunctionMemoizer.InvokeWith(1492L);

            // Maybe the memoizer^2 registry instance is already cached, maybe not...
            myMemoizerFactory.GetMemoizer().InvokeWith(1492L);

            // Cached value via the standalone memoizer
            long startTime = DateTime.Now.Ticks;
            string retVal = myExpensiveFunctionMemoizer.InvokeWith(1492L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1492L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));

            // Cached value via the memoizer^2 registry
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory.GetMemoizer().InvokeWith(1492L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1492L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));


            myExpensiveFunction.UnMemoize();
            
            // The standalone memoizer instance is still working...
            startTime = DateTime.Now.Ticks;
            retVal = myExpensiveFunctionMemoizer.InvokeWith(1492L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1492L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));

            // But the memoizer is removed from the memoizer^2 registry - so that will be re-cached...
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory.GetMemoizer().InvokeWith(1492L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1492L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            // But then the memoizer is yet again in the memoizer^2 registry
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory.GetMemoizer().InvokeWith(1492L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1492L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
        }


        [Test]
        public void Remove()
        {
            MemoizerFactory<long, string> myMemoizerFactory = myExpensiveFunction.CacheFor(30).Minutes.InstrumentWith(Console.WriteLine);

            // Nope...
            //MemoizerConfiguration myMemoizerConfig = new MemoizerConfiguration(myExpensiveFunction, ExpirationType.Relative, 30, TimeUnit.Minutes, Console.WriteLine);
            //MemoizerFactory<long, string> myMemoizerFactory = new MemoizerFactory<long, string>(myMemoizerConfig);

            // Maybe this standalone memoizer instance is already cached, maybe not...
            myExpensiveFunctionMemoizer.InvokeWith(1536L);

            // Maybe the memoizer^2 registry instance is already cached, maybe not...
            myMemoizerFactory.GetMemoizer().InvokeWith(1536L);

            // Cached value via the standalone memoizer
            long startTime = DateTime.Now.Ticks;
            string retVal = myExpensiveFunctionMemoizer.InvokeWith(1536L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1536L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));

            // Cached value via the memoizer^2 registry
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory.GetMemoizer().InvokeWith(1536L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1536L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));


            // Removal from the standalone memoizer instance
            myExpensiveFunctionMemoizer.Remove(1536L);

            // Memoizer^2 registry is not affected
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory.GetMemoizer().InvokeWith(1536L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1536L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));

            // But the value is removed from the standalone memoizer - so that will be re-cached...
            startTime = DateTime.Now.Ticks;
            retVal = myExpensiveFunctionMemoizer.InvokeWith(1536L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1536L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            startTime = DateTime.Now.Ticks;
            retVal = myExpensiveFunctionMemoizer.InvokeWith(1536L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1536L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));


            // Removal from the Memoizer^2 registry
            myMemoizerFactory.GetMemoizer().Remove(1536L);
            
            // Standalone memoizer instance is not affected
            startTime = DateTime.Now.Ticks;
            retVal = myExpensiveFunctionMemoizer.InvokeWith(1536L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1536L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));

            // But the value is removed from the memoizer^2 registry - so that will be re-cached...
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory.GetMemoizer().InvokeWith(1536L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1536L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory.GetMemoizer().InvokeWith(1536L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1536L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
        }


        [Test]
        public void RemoveFromCache()
        {
            MemoizerFactory<long, string> myMemoizerFactory = myExpensiveFunction.CacheFor(30).Minutes.InstrumentWith(Console.WriteLine);
            MemoizerFactory<long, string> myMemoizerFactory2 = myExpensiveFunction.Memoize().InstrumentWith(Console.WriteLine);
            MemoizerFactory<long, string> myMemoizerFactory3 = typicalDatabaseInvocation_DelegatedFunc3.Memoize().InstrumentWith(Console.WriteLine);

            // Maybe this standalone memoizer instance is already cached, maybe not...
            myExpensiveFunctionMemoizer.InvokeWith(1989L);

            // Maybe the memoizer^2 registry instance is already cached, maybe not...
            myMemoizerFactory.GetMemoizer().InvokeWith(1989L);
            myMemoizerFactory.GetMemoizer().InvokeWith(2089L);
            myMemoizerFactory2.GetMemoizer().InvokeWith(1989L);
            myMemoizerFactory3.GetMemoizer().InvokeWith(1989L);

            // Cached value via the standalone memoizer
            long startTime = DateTime.Now.Ticks;
            string retVal = myExpensiveFunctionMemoizer.InvokeWith(1989L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1989L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));

            // Cached value via the memoizer^2 registry
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory.GetMemoizer().InvokeWith(1989L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1989L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));


            myExpensiveFunction.RemoveFromCache(1989L);

            // Standalone memoizer instance is not affected
            startTime = DateTime.Now.Ticks;
            retVal = myExpensiveFunctionMemoizer.InvokeWith(1989L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1989L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));

            // Cached value for different parameter is not affected
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory.GetMemoizer().InvokeWith(2089L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 2089L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));

            // Cached value is removed from the memoizer^2 registry - so that will be re-cached...
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory.GetMemoizer().InvokeWith(1989L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1989L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory.GetMemoizer().InvokeWith(1989L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1989L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));

            // Second memoizer is using the same Func, so it is also cleared
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory2.GetMemoizer().InvokeWith(1989L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1989L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory2.GetMemoizer().InvokeWith(1989L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1989L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));

            // Third memoizer is a different Func, so it is not affected
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory3.GetMemoizer().InvokeWith(1989L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 1989L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
        }


        [Test]
        public void RemovalOfIndividualItemInCache___OnGoing()
        {
            MemoizerFactory<long, string> myMemoizerFactory = myExpensiveFunction.CacheFor(30).Minutes.InstrumentWith(Console.WriteLine);

            // Maybe this standalone memoizer instance is already cached, maybe not...
            long startTime = DateTime.Now.Ticks;
            string retVal = myExpensiveFunctionMemoizer.InvokeWith(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            //Assert.That(durationInMilliseconds, Is.LessThan(10));

            // Now it is...
            startTime = DateTime.Now.Ticks;
            retVal = myExpensiveFunctionMemoizer.InvokeWith(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));

            // First time invocation of the memoizer^2 registry version
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory.GetMemoizer().InvokeWith(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            // Now that's cached as well...
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory.GetMemoizer().InvokeWith(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));


            //myExpensiveFunctionMemoizer.Dispose(); // Not supported anymore
            //myExpensiveFunctionMemoizer = null;
            //myExpensiveFunctionMemoizer = myExpensiveFunction.CacheFor(30).Minutes.InstrumentWith(Console.WriteLine).GetMemoizer();

            myExpensiveFunction.UnMemoize(); // OK

            //myExpensiveFunctionMemoizer.Clear(); // OK

            //myExpensiveFunctionMemoizer.Reset(); // To be defined? (alias of Clear())

            //myExpensiveFunctionMemoizer.Remove(42L); // OK

            myExpensiveFunction.RemoveFromCache(42L); // OK


            // The IMemoizer instance is still working...
            startTime = DateTime.Now.Ticks;
            retVal = myExpensiveFunctionMemoizer.InvokeWith(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));

            // But the memoizer is removed from the memoizer^2 registry - so that will take its time...
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory.GetMemoizer().InvokeWith(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));

            // But then the memoizer is yet again in the memoizer^2 registry
            startTime = DateTime.Now.Ticks;
            retVal = myMemoizerFactory.GetMemoizer().InvokeWith(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
        }


        [Test]
        [Ignore("TODO")]
        public void IndividualItemClearing()
        {
            long startTime = DateTime.Now.Ticks;
            string retVal = ExpensiveFunctionWithExpiration(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            //Console.WriteLine("First memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunctionWithExpiration(4224L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 4224L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunctionWithExpiration2(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));

            myExpensiveFunction.Memoize().KeepItemsCachedFor(1).Seconds.GetMemoizer().Remove(42L);

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunctionWithExpiration2(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunctionWithExpiration(4224L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 4224L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
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
        [Ignore("TODO")]
        public void MultipleCacheItemPolicies_MemoizerFactoryIsMutable___OnGoing()
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
            // TODO: hmm, takes its time...
            Assert.That(durationInMilliseconds, Is.LessThan(30)); // ms

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
