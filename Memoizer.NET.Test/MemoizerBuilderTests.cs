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
    class MemoizerBuilderTests
    {

        static readonly Func<long, long> FIBONACCI = (arg => arg <= 1 ? arg : FIBONACCI(arg - 1) + FIBONACCI(arg - 2));
        //readonly Func<long, long> NON_STATIC_FIBONACCI = (arg => arg <= 1 ? arg : NON_STATIC_FIBONACCI(arg - 1) + NON_STATIC_FIBONACCI(arg - 2)); // Does not compile


        [Test]
        public void SomeTestRunsJustToSeeTheTimeSpent()
        {
            long startTime = DateTime.Now.Ticks;
            long result = (long)FIBONACCI.DynamicInvoke(40);
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Console.WriteLine("Fibonacci(40) = " + result + " [non-memoized func dynamic invocation took " + durationInMilliseconds + " ms]");

            startTime = DateTime.Now.Ticks;
            result = FIBONACCI.Invoke(40);
            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Console.WriteLine("Fibonacci(40) = " + result + " [non-memoized func invocation took " + durationInMilliseconds + " ms]");

            //startTime = DateTime.Now.Ticks;
            //result = (long)FIBONACCI.MemoizedFunc().DynamicInvoke(40);
            //durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            //Console.WriteLine("Fibonacci(40) = " + result + " [first time 'on-the-spot-memoized' func dynamic invocation took " + durationInMilliseconds + " ms]");

            //startTime = DateTime.Now.Ticks;
            //result = (long)FIBONACCI.MemoizedFunc().DynamicInvoke(40);
            //durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            //Console.WriteLine("Fibonacci(40) = " + result + " [second time time 'on-the-spot-memoized' func dynamic invocation took " + durationInMilliseconds + " ms]");

            //startTime = DateTime.Now.Ticks;
            //result = FIBONACCI.MemoizedFunc().Invoke(40);
            //durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            //Console.WriteLine("Fibonacci(40) = " + result + " [first time 'on-the-spot-memoized' func invocation took " + durationInMilliseconds + " ms]");

            //startTime = DateTime.Now.Ticks;
            //result = FIBONACCI.MemoizedFunc().Invoke(40);
            //durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            //Console.WriteLine("Fibonacci(40) = " + result + " [second time time 'on-the-spot-memoized' func invocation took " + durationInMilliseconds + " ms]");

            //startTime = DateTime.Now.Ticks;
            //Func<long, long> MEMOIZED_FIBONACCI = FIBONACCI.MemoizedFunc();
            //long durationInTicks = DateTime.Now.Ticks - startTime;
            //Console.WriteLine("FIBONACCI.Memoize().Function took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks [1 tick ~= 100 ns]");

            //startTime = DateTime.Now.Ticks;
            //result = (long)MEMOIZED_FIBONACCI.DynamicInvoke(40);
            //durationInTicks = DateTime.Now.Ticks - startTime;
            //Console.WriteLine("Fibonacci(40) = " + result + " [memoized first time dynamic invocation (of memoized func) took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");

            //startTime = DateTime.Now.Ticks;
            //result = (long)MEMOIZED_FIBONACCI.DynamicInvoke(40);
            //durationInTicks = DateTime.Now.Ticks - startTime;
            //Console.WriteLine("Fibonacci(40) = " + result + " [memoized second time dynamic invocation (of memoized func) took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");

            //startTime = DateTime.Now.Ticks;
            //result = MEMOIZED_FIBONACCI.Invoke(40);
            //durationInTicks = DateTime.Now.Ticks - startTime;
            //Console.WriteLine("Fibonacci(40) = " + result + " [memoized first time invocation (of memoized func) took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");

            //startTime = DateTime.Now.Ticks;
            //result = MEMOIZED_FIBONACCI.Invoke(40);
            //durationInTicks = DateTime.Now.Ticks - startTime;
            //Console.WriteLine("Fibonacci(40) = " + result + " [memoized second time invocation (of memoized func) took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");

            startTime = DateTime.Now.Ticks;
            result = FIBONACCI.GetMemoizer().InvokeWith(40);
            long durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Fibonacci(40) = " + result + " [first time 'on-the-spot-memoized' memoizerbuilder invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");

            startTime = DateTime.Now.Ticks;
            result = FIBONACCI.GetMemoizer().InvokeWith(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Fibonacci(40) = " + result + " [second time time 'on-the-spot-memoized' memoizerbuilder invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");

            startTime = DateTime.Now.Ticks;
            result = FIBONACCI.CachedInvoke<long, long>(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Fibonacci(40) = " + result + " [first time 'on-the-spot-memoized' invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");

            startTime = DateTime.Now.Ticks;
            result = FIBONACCI.CachedInvoke<long, long>(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Fibonacci(40) = " + result + " [second time time 'on-the-spot-memoized' invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
        }


        Func<long, long> slow500Square = (arg1 => { Thread.Sleep(500); return arg1 * arg1; });
        long Slow500Square(long arg)
        {
            Thread.Sleep(500);
            return arg * arg;
        }

        Func<long, long> slow1000PowerOfThree = (arg1 => { Thread.Sleep(1000); return arg1 * arg1 * arg1; });

        //Func<long, long> memSlowSquare = slowSquare.Memoize();
        //long Square(long arg)
        //{
        //    return memSlowSquare.Invoke(arg);
        //}

        [Test]
        public void ShouldBuildFullBlownMemoizedFuncsWithOnelineAndStillGetMemoization()
        {
            long startTime = DateTime.Now.Ticks;
            long result = Slow500Square(40);
            long durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(40) = " + result + " [ordinary method invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.GreaterThanOrEqualTo(500)); // ms

            startTime = DateTime.Now.Ticks;
            result = (long)slow500Square.DynamicInvoke(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(40) = " + result + " [non-memoized dynamic func invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.GreaterThanOrEqualTo(500)); // ms

            startTime = DateTime.Now.Ticks;
            result = slow500Square.Invoke(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(40) = " + result + " [non-memoized func invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.GreaterThanOrEqualTo(500)); // ms


            startTime = DateTime.Now.Ticks;
            result = slow500Square.Cache().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(40) = " + result + " [first time 'on-the-spot-memoized', instrumented invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.GreaterThanOrEqualTo(500)); // ms (not memoized invocation)

            startTime = DateTime.Now.Ticks;
            result = slow500Square.Cache().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(40) = " + result + " [second time time 'on-the-spot-memoized' instrumented invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.LessThan(20)); // ms (memoized invocation)

            startTime = DateTime.Now.Ticks;
            result = slow500Square.Cache().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(50);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(50) = " + result + " [first time 'on-the-spot-memoized' instrumented invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.GreaterThanOrEqualTo(500)); // ms (not memoized invocation)

            startTime = DateTime.Now.Ticks;
            result = slow500Square.Cache().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(60);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(60) = " + result + " [first time 'on-the-spot-memoized' instrumented invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.GreaterThanOrEqualTo(500)); // ms (not memoized invocation)

            startTime = DateTime.Now.Ticks;
            result = slow500Square.Cache().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(50);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(50) = " + result + " [second time time 'on-the-spot-memoized' instrumented invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.LessThan(20)); // ms (memoized invocation)

            startTime = DateTime.Now.Ticks;
            result = slow500Square.Cache().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(60);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(60) = " + result + " [second time time 'on-the-spot-memoized' instrumented invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.LessThan(20)); // ms (memoized invocation)


            // TODO: fails!!
            startTime = DateTime.Now.Ticks;
            IMemoizer<long, long> slow1000PowerOfThreeMemoizer1 = slow1000PowerOfThree.Cache().InstrumentWith(Console.WriteLine).GetMemoizer();
            result = slow1000PowerOfThree.Cache().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("PowerOfThree(40) = " + result + " [first time 'on-the-spot-memoized' instrumented invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.GreaterThanOrEqualTo(1000)); // ms (not memoized invocation)

            startTime = DateTime.Now.Ticks;
            IMemoizer<long, long> slow1000PowerOfThreeMemoizer2 = slow1000PowerOfThree.Cache().InstrumentWith(Console.WriteLine).GetMemoizer();
            result = slow1000PowerOfThree.Cache().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("PowerOfThree(40) = " + result + " [second time time 'on-the-spot-memoized' instrumented invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(slow1000PowerOfThreeMemoizer1, Is.SameAs(slow1000PowerOfThreeMemoizer2));
            Assert.That(durationInTicks / 10000, Is.LessThan(10)); // ms (memoized invocation)

            startTime = DateTime.Now.Ticks;
            result = slow1000PowerOfThree.CachedInvoke<long, long>(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("PowerOfThree(40) = " + result + " [third time time 'on-the-spot-memoized' not-instrumented invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.LessThan(10)); // ms (memoized invocation)
        }


        [Test]
        public void ShouldBuildFullBlownMemoizerWithOnelineAndStillGetMemoization()
        {
            long startTime = DateTime.Now.Ticks;
            MemoizerBuilder<long, long> slowSquareMemoizerBuilder = slow500Square.Cache();
            IMemoizer<long, long> memoizedSlowSquare = slowSquareMemoizerBuilder.GetMemoizer();
            long durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Memoizer construction: square.Memoize().Get() took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.LessThan(20)); // ms (MemoizerBuilder and Memoizer creation)

            startTime = DateTime.Now.Ticks;
            long result = memoizedSlowSquare.InvokeWith(123);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(123) = " + result + " [memoized first time invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.GreaterThanOrEqualTo(500)); // ms (not memoized invocation)

            startTime = DateTime.Now.Ticks;
            result = memoizedSlowSquare.InvokeWith(123);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(123) = " + result + " [memoized second time invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.LessThan(10)); // ms (memoized invocation)


            slowSquareMemoizerBuilder.InstrumentWith(Console.WriteLine);
            IMemoizer<long, long> memoizedSlowSquare2 = slowSquareMemoizerBuilder.GetMemoizer();

            startTime = DateTime.Now.Ticks;
            result = slowSquareMemoizerBuilder.GetMemoizer().InvokeWith(123);
            //result = memoizedSlowSquare2.InvokeWith(123);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(123) = " + result + " [memoized first time (instrumented) invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.LessThan(10)); // ms (memoized invocation)

            startTime = DateTime.Now.Ticks;
            result = memoizedSlowSquare2.InvokeWith(123);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(123) = " + result + " [memoized second time (instrumented) invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.LessThan(10)); // ms (memoized invocation)
        }


        [Test]
        //[Ignore]
        public void MultipleCacheItemPolicies_LatestAddedOverridesPrevouslyAddedOnes()
        {
            MemoizerBuilder<long, long> memoizerBuilder1 =
                slow1000PowerOfThree.Cache()
                                    .CachePolicy(default(CacheItemPolicy))
                                    .InstrumentWith(Console.WriteLine);
            MemoizerBuilder<long, long> memoizerBuilder2 =
                 memoizerBuilder1.CachePolicy(default(CacheItemPolicy))
                                 .KeepElementsCachedFor(0).Milliseconds
                                 .KeepElementsCachedFor(12).Milliseconds
                                 .KeepElementsCachedFor(120).Milliseconds;

            Assert.That(memoizerBuilder1, Is.EqualTo(memoizerBuilder2));
            Assert.That(memoizerBuilder1, Is.EqualTo(memoizerBuilder2));
            Assert.That(memoizerBuilder1.GetMemoizer(), Is.SameAs(memoizerBuilder2.GetMemoizer()));

            //// TODO: ajaj - this test is not thread-safe!!
            //Assert.That(LatencyInstrumentedRun(myMessyMemoizerBuilder, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "First method invocation", "(should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)"),
            //    Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS * TimeSpan.TicksPerMillisecond));
            //Assert.That(LatencyInstrumentedRun(myMessyMemoizerBuilder, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "Second method invocation", "(cached, should take < 5 ms)"),
            //    Is.LessThan(5)); // ms
            //Thread.Sleep(30);
            //Assert.That(LatencyInstrumentedRun(myMessyMemoizerBuilder, DATABASE_RESPONSE_LATENCY_IN_MILLIS, "Third method invocation", "(cached, should take < 5 ms)"),
            //    Is.LessThan(5)); // ms
        }


    }
}
