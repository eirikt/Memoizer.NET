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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Caching;
using System.Threading;
using NUnit.Framework;

namespace Memoizer.NET.Test
{

    [TestFixture]
    class MemoizerBuilderTests
    {

        #region Test classes
        class SomeDomainClass
        {
            internal int IntProperty { get; set; }
            internal string StringProperty { get; set; }
            internal ICollection<SomeDomainClass> ChildrenProperty { get; set; }
        }
        class SomeValueClass
        {
            internal int IntProperty { get; set; }
            internal string StringProperty { get; set; }
            internal ICollection<SomeValueClass> ChildrenProperty { get; set; }
            public override int GetHashCode()
            {
                int intPropertyHashCode = IntProperty.GetHashCode();
                int stringPropertyHashCode = StringProperty.GetHashCode();
                int childrenPropertyHashCode = -1;
                if (ChildrenProperty != null)
                    foreach (var someValueClass in ChildrenProperty)
                        childrenPropertyHashCode = childrenPropertyHashCode + someValueClass.GetHashCode();
                //Console.WriteLine("IntProperty.GetHashCode()=" + intPropertyHashCode);
                //Console.WriteLine("StringProperty.GetHashCode()=" + stringPropertyHashCode);
                //Console.WriteLine("Children.GetHashCode()=" + childrenPropertyHashCode);
                return intPropertyHashCode + stringPropertyHashCode + childrenPropertyHashCode;
            }

            public override bool Equals(object otherObject)
            {
                if (otherObject == null) { return false; }
                if (otherObject == this) { return true; }
                if (!(otherObject is SomeValueClass)) { return false; }
                SomeValueClass otherSomeValueClass = otherObject as SomeValueClass;
                bool equal = this.IntProperty.Equals(otherSomeValueClass.IntProperty)
                             && this.StringProperty.Equals(otherSomeValueClass.StringProperty)
                             && this.ChildrenProperty.Equals(otherSomeValueClass.ChildrenProperty);
                return equal;
            }
        }
        #endregion

        #region MemoizerHelper
        [Test]
        public void PrimitiveParameterHashingTests()
        {
            Assert.That(MemoizerHelper.CreateParameterHash(2L), Is.EqualTo(MemoizerHelper.CreateParameterHash(2L)));
            Assert.That(MemoizerHelper.CreateParameterHash(2L), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(2M)));
            Assert.That(MemoizerHelper.CreateParameterHash(2L, 2M), Is.EqualTo(MemoizerHelper.CreateParameterHash(2L, 2M)));
            Assert.That(MemoizerHelper.CreateParameterHash(2L, 2M), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(2M, 2L)));

            Assert.That(MemoizerHelper.CreateParameterHash("2L"), Is.EqualTo(MemoizerHelper.CreateParameterHash("2L")));
            Assert.That(MemoizerHelper.CreateParameterHash("2L"), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(2L)));
            Assert.That(MemoizerHelper.CreateParameterHash("2L", 2L), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(2L, "2L")));
            Assert.That(MemoizerHelper.CreateParameterHash("2L", 2L), Is.EqualTo(MemoizerHelper.CreateParameterHash("2L", 2L)));
        }


        [Test]
        public void ComplexParameterHashingTests()
        {
            SomeDomainClass someDomainObject1A = new SomeDomainClass { IntProperty = 1, StringProperty = "1" };
            SomeDomainClass someDomainObject1B = new SomeDomainClass { IntProperty = 1, StringProperty = "1" };
            SomeDomainClass someDomainObject1C = new SomeDomainClass { IntProperty = 1, StringProperty = "1" };
            SomeDomainClass someDomainObject2 = new SomeDomainClass { IntProperty = 2, StringProperty = "2" };
            SomeDomainClass someDomainObject3 = new SomeDomainClass { IntProperty = 3, StringProperty = "3" };
            someDomainObject1A.ChildrenProperty = new List<SomeDomainClass> { someDomainObject2, someDomainObject3 };
            someDomainObject1B.ChildrenProperty = new List<SomeDomainClass> { someDomainObject2, someDomainObject3 };
            someDomainObject1C.ChildrenProperty = new List<SomeDomainClass> { someDomainObject2, someDomainObject3, someDomainObject2, someDomainObject3 };

            Assert.That(MemoizerHelper.CreateParameterHash(someDomainObject1A), Is.EqualTo(MemoizerHelper.CreateParameterHash(someDomainObject1A)));
            // NB!
            Assert.That(MemoizerHelper.CreateParameterHash(someDomainObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someDomainObject1B)));

            Assert.That(MemoizerHelper.CreateParameterHash(someDomainObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someDomainObject1C)));
            Assert.That(MemoizerHelper.CreateParameterHash(someDomainObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someDomainObject2)));
            Assert.That(MemoizerHelper.CreateParameterHash(someDomainObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someDomainObject3)));

            Assert.That(MemoizerHelper.CreateParameterHash(someDomainObject1A, someDomainObject2), Is.EqualTo(MemoizerHelper.CreateParameterHash(someDomainObject1A, someDomainObject2)));
            Assert.That(MemoizerHelper.CreateParameterHash(someDomainObject1A, someDomainObject2), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someDomainObject1A, someDomainObject1A)));

            SomeValueClass someValueObject1A = new SomeValueClass { IntProperty = 1, StringProperty = "1" };
            SomeValueClass someValueObject1B = new SomeValueClass { IntProperty = 1, StringProperty = "1" };
            SomeValueClass someValueObject1C = new SomeValueClass { IntProperty = 1, StringProperty = "1" };
            SomeValueClass someValueObject2 = new SomeValueClass { IntProperty = 2, StringProperty = "2" };
            SomeValueClass someValueObject3 = new SomeValueClass { IntProperty = 3, StringProperty = "3" };
            someValueObject1A.ChildrenProperty = new List<SomeValueClass> { someValueObject2, someValueObject3 };
            someValueObject1B.ChildrenProperty = new List<SomeValueClass> { someValueObject2, someValueObject3 };
            someValueObject1C.ChildrenProperty = new List<SomeValueClass> { someValueObject2, someValueObject3, someValueObject2, someValueObject3 };

            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A), Is.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject1A)));
            // NB!
            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A), Is.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject1B)));
            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A, someValueObject1B), Is.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject1A, someValueObject1B)));
            // NB!
            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A, someValueObject1B), Is.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject1B, someValueObject1A)));
            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A, someValueObject1C), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject1C, someValueObject1A)));

            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject1C)));
            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject2)));
            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject3)));

            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A, someValueObject2), Is.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject1A, someValueObject2)));
            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A, someValueObject2), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject1A, someValueObject1A)));
        }


        static Func<long, long> FIBONACCI = (arg => arg <= 1 ? arg : FIBONACCI(arg - 1) + FIBONACCI(arg - 2));
        static Func<long, long> FIBONACCI2 = FIBONACCI;
        static Func<long, long> FIBONACCI3 = (arg => arg <= 1 ? arg : FIBONACCI(arg - 1) + FIBONACCI(arg - 2));
        static Func<long, long> FIBONACCI4 = (arg => arg <= 1 ? arg : FIBONACCI4(arg - 1) + FIBONACCI4(arg - 2));

        // Func<long, long> nonStaticFibonacci= (arg => arg <= 1 ? arg : nonStaticFibonacci(arg - 1) + nonStaticFibonacci(arg - 2)); // Does not compile


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


        static readonly MemoizerBuilder<long, long> FIBONACCI_MEMOIZER_BUILDER = new Func<long, long>(FIBONACCI).Cache();
        static readonly MemoizerBuilder<long, long> FIBONACCI2_MEMOIZER_BUILDER = new Func<long, long>(FIBONACCI2).Cache();
        static readonly MemoizerBuilder<long, long> FIBONACCI3_MEMOIZER_BUILDER = new Func<long, long>(FIBONACCI3).Cache();
        static readonly MemoizerBuilder<long, long> FIBONACCI4_MEMOIZER_BUILDER = new Func<long, long>(FIBONACCI4).Cache();

        //MemoizerBuilder<long, long> slow500Square_MemoizerBuilder = new Func<long, long>(slow500Square).Cache(); // Does not compile
        //MemoizerBuilder<long, long> Slow500Square_MemoizerBuilder = new Func<long, long>(Slow500Square).Cache(); // Does not compile


        [Test]
        public void MemoizerBuilderHashingTests()
        {
            MemoizerBuilder<long, long> slow500Square_MemoizerBuilder = new Func<long, long>(slow500Square).Cache();
            MemoizerBuilder<long, long> slow1000PowerOfThree_MemoizerBuilder = new Func<long, long>(slow1000PowerOfThree).Cache();

            Assert.That(MemoizerHelper.CreateMemoizerBuilderHash(FIBONACCI_MEMOIZER_BUILDER), Is.EqualTo(MemoizerHelper.CreateMemoizerBuilderHash(FIBONACCI_MEMOIZER_BUILDER)));
            // NB!
            Assert.That(MemoizerHelper.CreateMemoizerBuilderHash(FIBONACCI_MEMOIZER_BUILDER), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerBuilderHash(FIBONACCI2_MEMOIZER_BUILDER)));
            Assert.That(MemoizerHelper.CreateMemoizerBuilderHash(FIBONACCI_MEMOIZER_BUILDER), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerBuilderHash(FIBONACCI3_MEMOIZER_BUILDER)));
            Assert.That(MemoizerHelper.CreateMemoizerBuilderHash(FIBONACCI_MEMOIZER_BUILDER), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerBuilderHash(FIBONACCI4_MEMOIZER_BUILDER)));
            Assert.That(MemoizerHelper.CreateMemoizerBuilderHash(FIBONACCI_MEMOIZER_BUILDER), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerBuilderHash(slow500Square_MemoizerBuilder)));

            Console.WriteLine(MemoizerHelper.CreateMemoizerBuilderHash(slow500Square_MemoizerBuilder));
            Console.WriteLine(MemoizerHelper.CreateMemoizerBuilderHash(slow1000PowerOfThree_MemoizerBuilder));
            Assert.That(MemoizerHelper.CreateMemoizerBuilderHash(slow500Square_MemoizerBuilder), Is.EqualTo(MemoizerHelper.CreateMemoizerBuilderHash(slow500Square_MemoizerBuilder)));
            Assert.That(MemoizerHelper.CreateMemoizerBuilderHash(slow500Square_MemoizerBuilder), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerBuilderHash(slow1000PowerOfThree_MemoizerBuilder)));
            Assert.That(MemoizerHelper.CreateMemoizerBuilderHash(slow1000PowerOfThree_MemoizerBuilder), Is.EqualTo(MemoizerHelper.CreateMemoizerBuilderHash(slow1000PowerOfThree_MemoizerBuilder)));
            Assert.That(MemoizerHelper.CreateMemoizerBuilderHash(slow1000PowerOfThree_MemoizerBuilder), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerBuilderHash(slow500Square_MemoizerBuilder)));

            MemoizerBuilder<long, long> memoizerBuilder1 = FIBONACCI_MEMOIZER_BUILDER.KeepElementsCachedFor(13).Minutes.InstrumentWith(Console.WriteLine);
            MemoizerBuilder<long, long> memoizerBuilder2 = FIBONACCI_MEMOIZER_BUILDER.KeepElementsCachedFor(23).Minutes.InstrumentWith(Console.WriteLine);
            MemoizerBuilder<long, long> memoizerBuilder3 = FIBONACCI_MEMOIZER_BUILDER.KeepElementsCachedFor(23).Seconds.InstrumentWith(Console.WriteLine);
            MemoizerBuilder<long, long> memoizerBuilder4 = slow500Square_MemoizerBuilder.KeepElementsCachedFor(13).Minutes.InstrumentWith(Console.WriteLine);
            MemoizerBuilder<long, long> memoizerBuilder5 = FIBONACCI_MEMOIZER_BUILDER.KeepElementsCachedFor(13).Minutes;
            MemoizerBuilder<long, long> memoizerBuilder6 = FIBONACCI_MEMOIZER_BUILDER.KeepElementsCachedFor(13).Minutes.InstrumentWith(Console.WriteLine);

            //Console.WriteLine(MemoizerHelper.CreateParameterHash(memoizerBuilder1));
            //Console.WriteLine(MemoizerHelper.CreateParameterHash(memoizerBuilder2));
            //Console.WriteLine(MemoizerHelper.CreateParameterHash(memoizerBuilder3));
            //Console.WriteLine(MemoizerHelper.CreateParameterHash(memoizerBuilder4));
            //Console.WriteLine(MemoizerHelper.CreateParameterHash(memoizerBuilder5));
            //Console.WriteLine(MemoizerHelper.CreateParameterHash(memoizerBuilder6));
            Assert.That(MemoizerHelper.CreateMemoizerBuilderHash(memoizerBuilder1), Is.EqualTo(MemoizerHelper.CreateMemoizerBuilderHash(memoizerBuilder1)));
            Assert.That(MemoizerHelper.CreateMemoizerBuilderHash(memoizerBuilder1), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerBuilderHash(memoizerBuilder2)));
            Assert.That(MemoizerHelper.CreateMemoizerBuilderHash(memoizerBuilder1), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerBuilderHash(memoizerBuilder3)));
            Assert.That(MemoizerHelper.CreateMemoizerBuilderHash(memoizerBuilder1), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerBuilderHash(memoizerBuilder4)));
            Assert.That(MemoizerHelper.CreateMemoizerBuilderHash(memoizerBuilder1), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerBuilderHash(memoizerBuilder5)));
            Assert.That(MemoizerHelper.CreateMemoizerBuilderHash(memoizerBuilder1), Is.EqualTo(MemoizerHelper.CreateMemoizerBuilderHash(memoizerBuilder6)));



            CacheItemPolicy cacheItemPolicy1 = new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 2, 0, 2) };
            CacheItemPolicy cacheItemPolicy2 = new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 2, 0, 2) };
            CacheItemPolicy cacheItemPolicy3 = new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 2, 0, 3) };

            Console.WriteLine(MemoizerHelper.CreateParameterHash(cacheItemPolicy1));
            Console.WriteLine(MemoizerHelper.CreateParameterHash(cacheItemPolicy2));
            Console.WriteLine(MemoizerHelper.CreateParameterHash(cacheItemPolicy3));
            Assert.That(MemoizerHelper.CreateParameterHash(cacheItemPolicy1), Is.EqualTo(MemoizerHelper.CreateParameterHash(cacheItemPolicy1)));
            Assert.That(MemoizerHelper.CreateParameterHash(cacheItemPolicy1), Is.EqualTo(MemoizerHelper.CreateParameterHash(cacheItemPolicy2)));
            Assert.That(MemoizerHelper.CreateParameterHash(cacheItemPolicy1), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(cacheItemPolicy3)));


            Action<String> loggingAction = Console.WriteLine;

            Console.WriteLine(MemoizerHelper.CreateParameterHash(loggingAction));
            Assert.That(MemoizerHelper.CreateParameterHash(loggingAction), Is.EqualTo(MemoizerHelper.CreateParameterHash(loggingAction)));

        }
        #endregion


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
