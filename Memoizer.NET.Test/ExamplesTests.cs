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

        Func<long, string> TypicalDatabaseInvocation1a =
           new Func<long, string>(delegate(long longArg)
           {
               Console.WriteLine("TypicalDatabaseInvocation invoked...");
               Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
               Console.WriteLine("TypicalDatabaseInvocation returns...");
               return METHOD_RESPONSE_ELEMENT + longArg;
               // Or:
               //return TypicalDatabaseStaticInvocation(stringArg, longArg);
           });

        Func<long, string> TypicalDatabaseInvocation1b =
          (delegate(long longArg)
          {
              Console.WriteLine("TypicalDatabaseInvocation invoked...");
              Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
              Console.WriteLine("TypicalDatabaseInvocation returns...");
              return METHOD_RESPONSE_ELEMENT + longArg;
              // Or:
              //return TypicalDatabaseStaticInvocation(stringArg, longArg);
          });

        Func<long, string> TypicalDatabaseInvocation1c = TypicalDatabaseStaticInvocation;

        Func<long, string> TypicalDatabaseInvocation2(long longArg)
        {
            TypicalDatabaseStaticInvocation(longArg);
            return null;
        }

        string TypicalDatabaseInvocation3(long longArg)
        {
            return TypicalDatabaseStaticInvocation(longArg);
        }


        // Example 1 [default caching policy]
        readonly Func<long, string> MyExpensiveFunction1 = TypicalDatabaseStaticInvocation;

        string ExpensiveFunction1(long someId)
        {
            return MyExpensiveFunction1.MemoizedInvoke(someId);
        }


        // Example 2a [expiration policy: keep items alive for 30 minutes]
        readonly Func<long, string> MyExpensiveFunction2a = TypicalDatabaseStaticInvocation;

        //readonly CacheItemPolicy cacheItemEvictionPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(30)) };
        readonly CacheItemPolicy cacheItemEvictionPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMilliseconds(1000L) };

        string ExpensiveFunction2a(long someId)
        {
            return MyExpensiveFunction2a.Memoize().CachePolicy(cacheItemEvictionPolicy).Get().InvokeWith(someId);
        }


        // Example 2b [expiration policy: keep items alive for 30 minutes]
        readonly Func<long, string> MyExpensiveFunction2b = TypicalDatabaseStaticInvocation;

        string ExpensiveFunction2b(long someId)
        {
            return MyExpensiveFunction2b.Memoize().KeepItemsAliveFor(30).Minutes.Get().InvokeWith(someId);
        }


        [Test]
        public void RunExample1()
        {
            long startTime = DateTime.Now.Ticks;
            string retVal = ExpensiveFunction1(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / 10000;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 1: first memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunction1(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / 10000;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
            Console.WriteLine("Example 1: second memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");
        }


        [Test]
        public void RunExample2a()
        {
            long startTime = DateTime.Now.Ticks;
            string retVal = ExpensiveFunction2a(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / 10000;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 2a: first memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunction2a(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / 10000;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
            Console.WriteLine("Example 2a: second memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");

            Thread.Sleep(1000);

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunction2a(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / 10000;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 2a: third memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");
        }


        [Test]
        public void RunExample2b()
        {
            long startTime = DateTime.Now.Ticks;
            string retVal = ExpensiveFunction2b(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            long durationInTicks = DateTime.Now.Ticks - startTime;
            long durationInMilliseconds = durationInTicks / 10000;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 2b: first memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunction2b(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / 10000;
            Assert.That(durationInMilliseconds, Is.LessThan(10));
            Console.WriteLine("Example 2b: second memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");

            Thread.Sleep(1000);

            startTime = DateTime.Now.Ticks;
            retVal = ExpensiveFunction2b(42L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + 42L));
            durationInTicks = DateTime.Now.Ticks - startTime;
            durationInMilliseconds = durationInTicks / 10000;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine("Example 2b: third memoized method invocation with latency " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");
        }
    }
}
