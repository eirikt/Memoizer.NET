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
    class MemoizerBuilderTests
    {

        static readonly Func<long, long> FIBONACCI = (arg => arg <= 1 ? arg : FIBONACCI(arg - 1) + FIBONACCI(arg - 2));
        //readonly Func<long, long> FIBONACCI2 = (arg => arg <= 1 ? arg : FIBONACCI2(arg - 1) + FIBONACCI2(arg - 2));


        [Test]
        public void SomeTestRunsJustToSeeTheTimeSpent()
        {
            long startTime = DateTime.Now.Ticks;
            long result = (long)FIBONACCI.DynamicInvoke(40);
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Console.WriteLine("Fibonacci(40) = " + result + " [non-memoized dynamic invocation took " + durationInMilliseconds + " ms]");

            startTime = DateTime.Now.Ticks;
            result = FIBONACCI.Invoke(40);
            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Console.WriteLine("Fibonacci(40) = " + result + " [non-memoized invocation took " + durationInMilliseconds + " ms]");

            startTime = DateTime.Now.Ticks;
            result = (long)FIBONACCI.Memoize().Function.DynamicInvoke(40);
            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Console.WriteLine("Fibonacci(40) = " + result + " [first time 'on-the-spot-memoized' dynamic invocation took " + durationInMilliseconds + " ms]");

            startTime = DateTime.Now.Ticks;
            result = (long)FIBONACCI.Memoize().Function.DynamicInvoke(40);
            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Console.WriteLine("Fibonacci(40) = " + result + " [second time time 'on-the-spot-memoized' dynamic invocation took " + durationInMilliseconds + " ms]");

            startTime = DateTime.Now.Ticks;
            result = FIBONACCI.Memoize().Function.Invoke(40);
            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Console.WriteLine("Fibonacci(40) = " + result + " [first time 'on-the-spot-memoized' invocation took " + durationInMilliseconds + " ms]");

            startTime = DateTime.Now.Ticks;
            result = FIBONACCI.Memoize().Function.Invoke(40);
            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Console.WriteLine("Fibonacci(40) = " + result + " [second time time 'on-the-spot-memoized' invocation took " + durationInMilliseconds + " ms]");

            startTime = DateTime.Now.Ticks;
            Func<long, long> MEMOIZED_FIBONACCI_1 = FIBONACCI.Memoize().Function;
            long durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("FIBONACCI.Memoize().Function took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks [1 tick ~= 100 ns]");

            startTime = DateTime.Now.Ticks;
            result = (long)MEMOIZED_FIBONACCI_1.DynamicInvoke(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Fibonacci(40) = " + result + " [memoized first time dynamic invocation (of memoizer _function_) took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");

            startTime = DateTime.Now.Ticks;
            result = (long)MEMOIZED_FIBONACCI_1.DynamicInvoke(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Fibonacci(40) = " + result + " [memoized second time dynamic invocation (of memoizer _function_) took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");

            startTime = DateTime.Now.Ticks;
            result = MEMOIZED_FIBONACCI_1.Invoke(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Fibonacci(40) = " + result + " [memoized first time invocation (of memoizer _function_) took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");

            startTime = DateTime.Now.Ticks;
            result = MEMOIZED_FIBONACCI_1.Invoke(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Fibonacci(40) = " + result + " [memoized second time invocation (of memoizer _function_) took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
        }




        Func<long, long> slowSquare = (arg1 => { Thread.Sleep(500); return arg1 * arg1; });
        //Func<long, long> memSlowSquare = slowSquare.Memoize();
        //long Square(long arg)
        //{
        //    return memSlowSquare.Invoke(arg);
        //}

        [Test]
        public void ShouldBuildFullBlownMemoizerWithOneliner()
        {
            long startTime = DateTime.Now.Ticks;
            Func<long, long> memoizedSlowSquareFunc = slowSquare.Memoize().Function;
            long result = memoizedSlowSquareFunc.Invoke(123);
            long durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Memoized function construction with invocation: slowSquare.Memoize().Function.Invoke(123)= " + result + " [took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");

            startTime = DateTime.Now.Ticks;
            MemoizerBuilder<long, long> slowSquareMemoizerBuilder = slowSquare.Memoize();
            IInvocable<long, long> memoizedSlowSquare = slowSquareMemoizerBuilder.Get();
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Memoizer construction: slowSquare.Memoize().Get() took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");

            startTime = DateTime.Now.Ticks;
            result = memoizedSlowSquare.InvokeWith(123);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(123) = " + result + " [memoized first time invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");

            startTime = DateTime.Now.Ticks;
            result = memoizedSlowSquare.InvokeWith(123);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(123) = " + result + " [memoized second time invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");


            slowSquareMemoizerBuilder.InstrumentWith(Console.WriteLine);
            IInvocable<long, long> memoizedSlowSquare2 = slowSquareMemoizerBuilder.Get();

            startTime = DateTime.Now.Ticks;
            result = memoizedSlowSquare2.InvokeWith(123);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(123) = " + result + " [memoized first time (instrumented) invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");

            startTime = DateTime.Now.Ticks;
            result = memoizedSlowSquare2.InvokeWith(123);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(123) = " + result + " [memoized second time (instrumented) invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");


            //startTime = DateTime.Now.Ticks;
            //IInvocable<long, long> MEMOIZED_FIBONACCI_2 = FIBONACCI.Memoize().Get();
            ////IInvocable<long, long> MEMOIZED_FIBONACCI_2 = MEMOIZED_FIBONACCI_1.Get(); // or like this
            //durationInTicks = (DateTime.Now.Ticks - startTime) / 10000;
            //Console.WriteLine("FIBONACCI.Memoize().Get() took " + durationInTicks + " ticks");

            //IInvocable<long, long> MEMOIZED_FIBONACCI_2 = FIBONACCI.Memoize().LazyLoad().Build();

            //IInvocable<long, long> MEMOIZED_FIBONACCI_3 = FIBONACCI.Memoize().InstrumentWith(Console.WriteLine).Build();

            //IInvocable<long, long> MEMOIZED_FIBONACCI_4 = FIBONACCI.Memoize().InstrumentWith(Console.WriteLine).LazyLoad().Build();

            // "Globalize()" : put he IInvocable in a CLR-wide registry of IInvocable instances - that's eating my own dog food :-)
            //IInvocable<long, long> MEMOIZED_FIBONACCI_5 = FIBONACCI.Memoize().Build().Globalize();

            //startTime = DateTime.Now.Ticks;
            //result = MEMOIZED_FIBONACCI_2.InvokeWith(40);
            //durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            //Console.WriteLine("Fibonacci(40) = " + result + " [memoized first time dynamic invocation took " + durationInMilliseconds + " ms]");

        }
    }
}
