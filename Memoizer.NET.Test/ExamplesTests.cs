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
using NUnit.Framework.Constraints;


namespace Memoizer.NET.Test
{

    [TestFixture]
    class ExamplesTests
    {

        const int DATABASE_RESPONSE_LATENCY_IN_MILLIS = 50;
        const string METHOD_RESPONSE_ELEMENT = "VeryExpensiveMethodResponseFor";


        //static string TypicalReferenceDataStaticInvocation()
        //{
        //    Console.WriteLine("TypicalReferenceDataStaticInvocation invoked...");
        //    Thread.Sleep(1345);
        //    Console.WriteLine("TypicalReferenceDataStaticInvocation returns...");
        //    return "Some one-time-invocation data stuff";
        //}

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


        static void AssertThat<TParam1, TResult>(Func<TParam1, TResult> invocation, TParam1 arg, ComparisonConstraint comparisonConstraint)
        {
            long startTime = DateTime.Now.Ticks;
            TResult retVal = invocation.Invoke(arg);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + arg));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, comparisonConstraint);
        }

        [Test]
        public void UnMemoize()
        {
            IMemoizer<long, string> standaloneMemoizer = myExpensiveFunction.CacheFor(30).Minutes.InstrumentWith(Console.WriteLine).CreateMemoizer();

            MemoizerFactory<long, string> sharedMemoizerConfig1 = myExpensiveFunction.CacheFor(30).Minutes.InstrumentWith(Console.WriteLine);
            MemoizerFactory<long, string> sharedMemoizerConfig2 = myExpensiveFunction.Memoize().InstrumentWith(Console.WriteLine);
            MemoizerFactory<long, string> sharedMemoizerConfig3 = typicalDatabaseInvocation_DelegatedFunc3.Memoize().InstrumentWith(Console.WriteLine);

            // Nope - this does not fly - cannot close mutable structures in a Func ...#help why?
            //Func<long, string> standaloneMemoizerInvocation = standaloneMemoizer.InvokeWith;
            //Func<long, string> sharedMemoizer1Invocation = sharedMemoizerConfig1.GetMemoizer().InvokeWith;
            //Func<long, string> sharedMemoizer2Invocation = sharedMemoizerConfig2.GetMemoizer().InvokeWith;
            //Func<long, string> sharedMemoizer3Invocation = sharedMemoizerConfig3.GetMemoizer().InvokeWith;

            // The standalone memoizer instance is not cached yet
            AssertThat(standaloneMemoizer.InvokeWith, 1989L, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            // ...now it is
            AssertThat(standaloneMemoizer.InvokeWith, 1989L, Is.LessThan(10));

            // The memoizer^2 registry instances are either cached, or not...
            sharedMemoizerConfig1.GetMemoizer().InvokeWith(1989L);
            sharedMemoizerConfig1.GetMemoizer().InvokeWith(2089L);
            sharedMemoizerConfig2.GetMemoizer().InvokeWith(1989L);
            sharedMemoizerConfig3.GetMemoizer().InvokeWith(1989L);

            // ...now they are
            AssertThat(sharedMemoizerConfig1.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));
            AssertThat(sharedMemoizerConfig1.GetMemoizer().InvokeWith, 2089L, Is.LessThan(10));
            AssertThat(sharedMemoizerConfig2.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));
            AssertThat(sharedMemoizerConfig3.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));

            // Remove all shared memoizers having function 'myExpensiveFunction'
            myExpensiveFunction.UnMemoize();

            // Standalone memoizer instance is not affected
            AssertThat(standaloneMemoizer.InvokeWith, 1989L, Is.LessThan(10));

            AssertThat(sharedMemoizerConfig1.GetMemoizer().InvokeWith, 1989L, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            AssertThat(sharedMemoizerConfig1.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));

            // ...for all arguments
            AssertThat(sharedMemoizerConfig1.GetMemoizer().InvokeWith, 2089L, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            AssertThat(sharedMemoizerConfig1.GetMemoizer().InvokeWith, 2089L, Is.LessThan(10));

            // Second registry memoizer is using the same Func, so it is also cleared
            AssertThat(sharedMemoizerConfig2.GetMemoizer().InvokeWith, 1989L, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            AssertThat(sharedMemoizerConfig2.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));

            // Third registry memoizer is a different Func, so it is not affected
            AssertThat(sharedMemoizerConfig3.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));
        }


        [Test]
        public void Remove()
        {
            IMemoizer<long, string> standaloneMemoizer = myExpensiveFunction.CacheFor(30).Minutes.InstrumentWith(Console.WriteLine).CreateMemoizer();

            MemoizerFactory<long, string> sharedMemoizer1 = myExpensiveFunction.CacheFor(30).Minutes.InstrumentWith(Console.WriteLine);
            MemoizerFactory<long, string> sharedMemoizer2 = myExpensiveFunction.Memoize().InstrumentWith(Console.WriteLine);
            MemoizerFactory<long, string> sharedMemoizer3 = typicalDatabaseInvocation_DelegatedFunc3.Memoize().InstrumentWith(Console.WriteLine);

            // The standalone memoizer instance is not cached yet
            AssertThat(standaloneMemoizer.InvokeWith, 1989L, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            // ...now it is
            AssertThat(standaloneMemoizer.InvokeWith, 1989L, Is.LessThan(10));

            // The memoizer^2 registry instances are either cached, or not...
            sharedMemoizer1.GetMemoizer().InvokeWith(1989L);
            sharedMemoizer1.GetMemoizer().InvokeWith(2089L);
            sharedMemoizer2.GetMemoizer().InvokeWith(1989L);
            sharedMemoizer3.GetMemoizer().InvokeWith(1989L);

            // ...now they are
            AssertThat(sharedMemoizer1.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));
            AssertThat(sharedMemoizer1.GetMemoizer().InvokeWith, 2089L, Is.LessThan(10));
            AssertThat(sharedMemoizer2.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));
            AssertThat(sharedMemoizer3.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));

            // Removal from the standalone memoizer instance
            standaloneMemoizer.Remove(1989L);

            // Standalone memoizer must be re-cached
            AssertThat(standaloneMemoizer.InvokeWith, 1989L, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            AssertThat(standaloneMemoizer.InvokeWith, 1989L, Is.LessThan(10));

            // Neither of the shared memoizer instances are affected
            AssertThat(sharedMemoizer1.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));
            AssertThat(sharedMemoizer2.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));
            AssertThat(sharedMemoizer3.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));
        }


        [Test]
        public void RemoveFromCache()
        {
            IMemoizer<long, string> standaloneMemoizer = myExpensiveFunction.CacheFor(30).Minutes.InstrumentWith(Console.WriteLine).CreateMemoizer();

            MemoizerFactory<long, string> sharedMemoizer1 = myExpensiveFunction.CacheFor(30).Minutes.InstrumentWith(Console.WriteLine);
            MemoizerFactory<long, string> sharedMemoizer2 = myExpensiveFunction.Memoize().InstrumentWith(Console.WriteLine);
            MemoizerFactory<long, string> sharedMemoizer3 = typicalDatabaseInvocation_DelegatedFunc3.Memoize().InstrumentWith(Console.WriteLine);

            // The standalone memoizer instance is not cached yet
            AssertThat(standaloneMemoizer.InvokeWith, 1989L, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));

            // ...now it is
            AssertThat(standaloneMemoizer.InvokeWith, 1989L, Is.LessThan(10));

            // The memoizer^2 registry instances are either cached, or not...
            sharedMemoizer1.GetMemoizer().InvokeWith(1989L);
            sharedMemoizer1.GetMemoizer().InvokeWith(2089L);
            sharedMemoizer2.GetMemoizer().InvokeWith(1989L);
            sharedMemoizer3.GetMemoizer().InvokeWith(1989L);

            // ...now they are
            AssertThat(sharedMemoizer1.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));
            AssertThat(sharedMemoizer1.GetMemoizer().InvokeWith, 2089L, Is.LessThan(10));
            AssertThat(sharedMemoizer2.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));
            AssertThat(sharedMemoizer3.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));

            // Remove all cached values originating from 'myExpensiveFunction(1989L)'
            myExpensiveFunction.RemoveFromCache(1989L);

            // Standalone memoizer instance is not affected
            AssertThat(standaloneMemoizer.InvokeWith, 1989L, Is.LessThan(10));

            // Cached value for different parameter is not affected
            AssertThat(sharedMemoizer1.GetMemoizer().InvokeWith, 2089L, Is.LessThan(10));

            // Cached value is removed from the memoizer^2 registry - so that will be re-cached...
            AssertThat(sharedMemoizer1.GetMemoizer().InvokeWith, 1989L, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            AssertThat(sharedMemoizer1.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));

            // Second registry memoizer is using the same Func, so it is also cleared
            AssertThat(sharedMemoizer2.GetMemoizer().InvokeWith, 1989L, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            AssertThat(sharedMemoizer2.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));

            // Third registry memoizer is a different Func, so it is not affected
            AssertThat(sharedMemoizer3.GetMemoizer().InvokeWith, 1989L, Is.LessThan(10));
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


        [Test]
        public void NoArgumentsMemoizer()
        {
            Func<string> expensiveNoArgsInvocationFunc =
                delegate
                {
                    Console.WriteLine("TypicalReferenceDataStaticInvocation invoked...");
                    Thread.Sleep(1345);
                    Console.WriteLine("TypicalReferenceDataStaticInvocation returns...");
                    return "Some no-arguments-one-time-only-invocation data stuff";
                };

            IMemoizer<string> memoizer = expensiveNoArgsInvocationFunc.CacheFor(1).Seconds.GetMemoizer();


            // Cached-for-1-second memoizer
            long startTime = DateTime.Now.Ticks;
            string retVal = memoizer.Invoke();
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo("Some no-arguments-one-time-only-invocation data stuff"));
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(1345)); // ms
            Console.WriteLine("One-time-invocation took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks");

            startTime = DateTime.Now.Ticks;
            retVal = memoizer.Invoke();
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo("Some no-arguments-one-time-only-invocation data stuff"));
            Assert.That(durationInMilliseconds, Is.LessThan(5)); // ms
            Console.WriteLine("One-time-invocation took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks");


            // Cached-for-1-second-on-the-fly function - should be cached already
            startTime = DateTime.Now.Ticks;
            retVal = expensiveNoArgsInvocationFunc.CacheFor(1).Seconds.GetMemoizer().Invoke();
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo("Some no-arguments-one-time-only-invocation data stuff"));
            Assert.That(durationInMilliseconds, Is.LessThan(5)); // ms
            Console.WriteLine("One-time-invocation took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks");


            // Function only - not yet cached
            startTime = DateTime.Now.Ticks;
            retVal = expensiveNoArgsInvocationFunc.CachedInvoke();
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo("Some no-arguments-one-time-only-invocation data stuff"));
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(1345)); // ms
            Console.WriteLine("One-time-invocation took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks");

            startTime = DateTime.Now.Ticks;
            retVal = expensiveNoArgsInvocationFunc.CachedInvoke();
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo("Some no-arguments-one-time-only-invocation data stuff"));
            Assert.That(durationInMilliseconds, Is.LessThan(5)); // ms
            Console.WriteLine("One-time-invocation took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks");


            expensiveNoArgsInvocationFunc.RemoveFromCache();

            startTime = DateTime.Now.Ticks;
            retVal = expensiveNoArgsInvocationFunc.CachedInvoke();
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo("Some no-arguments-one-time-only-invocation data stuff"));
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(1345)); // ms
            Console.WriteLine("One-time-invocation took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks");

            startTime = DateTime.Now.Ticks;
            retVal = expensiveNoArgsInvocationFunc.CachedInvoke();
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo("Some no-arguments-one-time-only-invocation data stuff"));
            Assert.That(durationInMilliseconds, Is.LessThan(5)); // ms
            Console.WriteLine("One-time-invocation took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks");


            expensiveNoArgsInvocationFunc.UnMemoize();

            startTime = DateTime.Now.Ticks;
            retVal = expensiveNoArgsInvocationFunc.CacheFor(1).Seconds.GetMemoizer().Invoke();
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo("Some no-arguments-one-time-only-invocation data stuff"));
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(1345)); // ms
            Console.WriteLine("One-time-invocation took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks");

            startTime = DateTime.Now.Ticks;
            retVal = expensiveNoArgsInvocationFunc.CacheFor(1).Seconds.GetMemoizer().Invoke();
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo("Some no-arguments-one-time-only-invocation data stuff"));
            Assert.That(durationInMilliseconds, Is.LessThan(5)); // ms
            Console.WriteLine("One-time-invocation took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks");


            Thread.Sleep(1000);

            startTime = DateTime.Now.Ticks;
            retVal = expensiveNoArgsInvocationFunc.CacheFor(55).Seconds.GetMemoizer().Invoke();
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo("Some no-arguments-one-time-only-invocation data stuff"));
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(1345)); // ms
            Console.WriteLine("One-time-invocation took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks");

            startTime = DateTime.Now.Ticks;
            retVal = expensiveNoArgsInvocationFunc.CacheFor(55).Seconds.GetMemoizer().Invoke();
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / TimeSpan.TicksPerMillisecond;
            Assert.That(retVal, Is.EqualTo("Some no-arguments-one-time-only-invocation data stuff"));
            Assert.That(durationInMilliseconds, Is.LessThan(5)); // ms
            Console.WriteLine("One-time-invocation took " + durationInMilliseconds + " ms | " + durationInTicks + " ticks");
        }


        [Test, ExpectedException(typeof(NullReferenceException), ExpectedMessage = "Whoops!", MatchType = MessageMatch.Exact)]
        public void ExceptionsShouldBubbleAllTheWay()
        {
            Func<string> exceptionInvocationFunc = delegate { throw new NullReferenceException("Whoops!"); };
            exceptionInvocationFunc.CachedInvoke();
        }


        [Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = "Aiii!", MatchType = MessageMatch.Exact)]
        public void ExceptionsShouldBubbleAllTheWay2()
        {
            Func<string> exceptionInvocationFunc = delegate { throw new ArgumentException("Aiii!"); };
            IMemoizer<string> memoizer = exceptionInvocationFunc.CacheFor(1).Seconds.GetMemoizer();
            memoizer.Invoke();
        }


        // TODO: concurrent use of different and equal memoizer factories with the same func


    }
}
