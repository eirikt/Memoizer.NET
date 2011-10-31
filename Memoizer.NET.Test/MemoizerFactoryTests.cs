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
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace Memoizer.NET.Test
{

    [TestFixture]
    class MemoizerFactoryTests
    {

        #region Test classes
        class SomeClass
        {
            internal int IntProperty { get; set; }
            internal string StringProperty { get; set; }
            internal ICollection<SomeClass> ChildrenProperty { get; set; }
        }

        class SomeEntityClass
        {
            readonly string entityId = Guid.NewGuid().ToString();
            internal int IntProperty { get; set; }
            internal string StringProperty { get; set; }
            internal ICollection<SomeEntityClass> ChildrenProperty { get; set; }
            public override int GetHashCode()
            {
                return this.entityId.GetHashCode();
            }
            public override bool Equals(object otherObject)
            {
                if (ReferenceEquals(null, otherObject)) { return false; }
                if (ReferenceEquals(this, otherObject)) { return true; }
                if (!(otherObject is SomeEntityClass)) { return false; }
                SomeEntityClass otherSomeEntityClass = otherObject as SomeEntityClass;
                return this.entityId == otherSomeEntityClass.entityId;
            }
        }

        class SomeValueClass
        {
            internal int IntProperty { get; set; }
            internal string StringProperty { get; set; }
            internal ICollection<SomeValueClass> ChildrenProperty { get; set; }
            public override int GetHashCode()
            {
                int hash = 1;
                hash = hash * 17 + IntProperty.GetHashCode();
                hash = hash * 31 + StringProperty.GetHashCode();
                if (ChildrenProperty != null)
                    foreach (var someValueClass in ChildrenProperty)
                        hash = hash * 13 + someValueClass.GetHashCode();
                return hash;
            }
            public override bool Equals(object otherObject)
            {
                if (ReferenceEquals(null, otherObject)) { return false; }
                if (ReferenceEquals(this, otherObject)) { return true; }
                if (!(otherObject is SomeValueClass)) { return false; }
                SomeValueClass otherSomeValueClass = otherObject as SomeValueClass;
                return this.IntProperty.Equals(otherSomeValueClass.IntProperty)
                    && this.StringProperty.Equals(otherSomeValueClass.StringProperty)
                    && this.ChildrenProperty.Equals(otherSomeValueClass.ChildrenProperty);
            }
        }
        #endregion

        #region MemoizerHelper
        [Test]
        public void PrimitiveParameterHashingTests()
        {
            //Console.WriteLine(2L.GetHashCode());
            //Console.WriteLine(2M.GetHashCode());
            //Console.WriteLine(2L.GetType().GetHashCode());
            //Console.WriteLine(2M.GetType().GetHashCode());
            //Assert.That(2L.GetHashCode(), Is.Not.EqualTo(2M.GetHashCode()));

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
        public void DefaultObjectParameterHashingTests()
        {
            SomeClass someObject1A = new SomeClass { IntProperty = 1, StringProperty = "1" };
            SomeClass someObject1B = new SomeClass { IntProperty = 1, StringProperty = "1" };
            SomeClass someObject1C = new SomeClass { IntProperty = 1, StringProperty = "1" };
            SomeClass someObject2 = new SomeClass { IntProperty = 2, StringProperty = "2" };
            SomeClass someObject3 = new SomeClass { IntProperty = 3, StringProperty = "3" };
            someObject1A.ChildrenProperty = new List<SomeClass> { someObject2, someObject3 };
            someObject1B.ChildrenProperty = new List<SomeClass> { someObject2, someObject3 };
            someObject1C.ChildrenProperty = new List<SomeClass> { someObject2, someObject3, someObject2, someObject3 };

            Assert.That(MemoizerHelper.CreateParameterHash(someObject1A), Is.EqualTo(MemoizerHelper.CreateParameterHash(someObject1A)));
            // NB! Default hashing is referende equality...
            Assert.That(MemoizerHelper.CreateParameterHash(someObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someObject1B)));

            Assert.That(MemoizerHelper.CreateParameterHash(someObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someObject1C)));
            Assert.That(MemoizerHelper.CreateParameterHash(someObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someObject2)));
            Assert.That(MemoizerHelper.CreateParameterHash(someObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someObject3)));

            Assert.That(MemoizerHelper.CreateParameterHash(someObject1A, someObject2), Is.EqualTo(MemoizerHelper.CreateParameterHash(someObject1A, someObject2)));
            Assert.That(MemoizerHelper.CreateParameterHash(someObject1A, someObject2), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someObject1A, someObject1A)));
        }


        [Test]
        public void EntityObjectParameterHashingTests()
        {
            SomeEntityClass someEntityObject1A = new SomeEntityClass { IntProperty = 1, StringProperty = "1" };
            SomeEntityClass someEntityObject1B = new SomeEntityClass { IntProperty = 1, StringProperty = "1" };
            SomeEntityClass someEntityObject1C = new SomeEntityClass { IntProperty = 1, StringProperty = "1" };
            SomeEntityClass someEntityObject2 = new SomeEntityClass { IntProperty = 2, StringProperty = "2" };
            SomeEntityClass someEntityObject3 = new SomeEntityClass { IntProperty = 3, StringProperty = "3" };
            someEntityObject1A.ChildrenProperty = new List<SomeEntityClass> { someEntityObject2, someEntityObject3 };
            someEntityObject1B.ChildrenProperty = new List<SomeEntityClass> { someEntityObject2, someEntityObject3 };
            someEntityObject1C.ChildrenProperty = new List<SomeEntityClass> { someEntityObject2, someEntityObject3, someEntityObject2, someEntityObject3 };

            Assert.That(MemoizerHelper.CreateParameterHash(someEntityObject1A), Is.EqualTo(MemoizerHelper.CreateParameterHash(someEntityObject1A)));
            // NB! Different entity objects are never equal
            Assert.That(MemoizerHelper.CreateParameterHash(someEntityObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someEntityObject1B)));

            Assert.That(MemoizerHelper.CreateParameterHash(someEntityObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someEntityObject1C)));
            Assert.That(MemoizerHelper.CreateParameterHash(someEntityObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someEntityObject2)));
            Assert.That(MemoizerHelper.CreateParameterHash(someEntityObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someEntityObject3)));

            Assert.That(MemoizerHelper.CreateParameterHash(someEntityObject1A, someEntityObject2), Is.EqualTo(MemoizerHelper.CreateParameterHash(someEntityObject1A, someEntityObject2)));
            Assert.That(MemoizerHelper.CreateParameterHash(someEntityObject1A, someEntityObject2), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someEntityObject1A, someEntityObject1A)));
        }


        [Test]
        public void ValueObjectParameterHashingTests()
        {
            SomeValueClass someValueObject1A = new SomeValueClass { IntProperty = 1, StringProperty = "1" };
            SomeValueClass someValueObject1B = new SomeValueClass { IntProperty = 1, StringProperty = "1" };
            SomeValueClass someValueObject1C = new SomeValueClass { IntProperty = 1, StringProperty = "1" };
            SomeValueClass someValueObject2 = new SomeValueClass { IntProperty = 2, StringProperty = "2" };
            SomeValueClass someValueObject3 = new SomeValueClass { IntProperty = 3, StringProperty = "3" };
            someValueObject1A.ChildrenProperty = new List<SomeValueClass> { someValueObject2, someValueObject3 };
            someValueObject1B.ChildrenProperty = new List<SomeValueClass> { someValueObject2, someValueObject3 };
            someValueObject1C.ChildrenProperty = new List<SomeValueClass> { someValueObject2, someValueObject3, someValueObject2, someValueObject3 };

            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A), Is.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject1A)));
            // NB! Value equality
            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A), Is.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject1B)));

            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject1C)));
            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject2)));
            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject3)));

            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A, someValueObject1B), Is.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject1A, someValueObject1B)));
            // NB! Value equality
            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A, someValueObject1B), Is.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject1B, someValueObject1A)));
            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A, someValueObject1C), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject1C, someValueObject1A)));
            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A, someValueObject2), Is.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject1A, someValueObject2)));
            Assert.That(MemoizerHelper.CreateParameterHash(someValueObject1A, someValueObject2), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(someValueObject1A, someValueObject1A)));
        }


        static Func<long, long> FIBONACCI = (arg => arg <= 1 ? arg : FIBONACCI(arg - 1) + FIBONACCI(arg - 2));
        static Func<long, long> FIBONACCI2 = FIBONACCI;
        static Func<long, long> FIBONACCI3 = (arg => arg <= 1 ? arg : FIBONACCI(arg - 1) + FIBONACCI(arg - 2));
        static Func<long, long> FIBONACCI4 = (arg => arg <= 1 ? arg : FIBONACCI4(arg - 1) + FIBONACCI4(arg - 2));

        //Func<long, long> nonStaticFibonacci= (arg => arg <= 1 ? arg : nonStaticFibonacci(arg - 1) + nonStaticFibonacci(arg - 2)); // Does not compile


        static readonly Func<long, long> slow500Square = (arg1 =>
        {
            Thread.Sleep(500);
            return arg1 * arg1;
        });
        static long Slow500Square(long arg)
        {
            Thread.Sleep(500);
            return arg * arg;
        }


        readonly Func<long, long> slow1000PowerOfThree = (arg1 => { Thread.Sleep(1000); return arg1 * arg1 * arg1; });


        [Test]
        public void MemoizerFactoryHashingTests_ConfigValueEquality()
        {
            MemoizerFactory<long, long> slow500Square_MemoizerFactory = new Func<long, long>(slow500Square).Memoize();

            MemoizerFactory<long, long> memoizerFactory1 = FIBONACCI.Memoize().KeepItemsCachedFor(13).Minutes.InstrumentWith(Console.WriteLine);
            MemoizerFactory<long, long> memoizerFactory2 = FIBONACCI.Memoize().KeepItemsCachedFor(13).Minutes.InstrumentWith(Console.WriteLine);
            MemoizerFactory<long, long> memoizerFactory3 = FIBONACCI.Memoize().KeepItemsCachedFor(13).Minutes;
            MemoizerFactory<long, long> memoizerFactory4 = FIBONACCI.Memoize().KeepItemsCachedFor(13).Seconds.InstrumentWith(Console.WriteLine);
            MemoizerFactory<long, long> memoizerFactory5 = slow500Square_MemoizerFactory.KeepItemsCachedFor(13).Minutes.InstrumentWith(Console.WriteLine);

            //Assert.That(MemoizerHelper.CreateMemoizerHash(memoizerFactory1), Is.EqualTo(MemoizerHelper.CreateMemoizerHash(memoizerFactory1)));
            //Assert.That(MemoizerHelper.CreateMemoizerHash(memoizerFactory1), Is.EqualTo(MemoizerHelper.CreateMemoizerHash(memoizerFactory2)));
            //// Logger action property not included in MemoizerFactory equality ...yet
            //Assert.That(MemoizerHelper.CreateMemoizerHash(memoizerFactory1), Is.EqualTo(MemoizerHelper.CreateMemoizerHash(memoizerFactory3)));
            //Assert.That(MemoizerHelper.CreateMemoizerHash(memoizerFactory1), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerHash(memoizerFactory4)));
            //Assert.That(MemoizerHelper.CreateMemoizerHash(memoizerFactory1), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerHash(memoizerFactory5)));

            Assert.That(memoizerFactory1.MemoizerConfiguration, Is.EqualTo(memoizerFactory1.MemoizerConfiguration));
            Assert.That(memoizerFactory1.MemoizerConfiguration, Is.EqualTo(memoizerFactory2.MemoizerConfiguration));
            // Logger action property not included in MemoizerFactory equality ...yet
            Assert.That(memoizerFactory1.MemoizerConfiguration, Is.EqualTo(memoizerFactory3.MemoizerConfiguration));
            Assert.That(memoizerFactory1.MemoizerConfiguration, Is.Not.EqualTo(memoizerFactory4.MemoizerConfiguration));
            Assert.That(memoizerFactory1.MemoizerConfiguration, Is.Not.EqualTo(memoizerFactory5.MemoizerConfiguration));
        }


        static readonly MemoizerFactory<long, long> FIBONACCI_MEMOIZER_FACTORY = new Func<long, long>(FIBONACCI).Memoize();
        static readonly MemoizerFactory<long, long> FIBONACCI2_MEMOIZER_FACTORY = new Func<long, long>(FIBONACCI2).Memoize();
        static readonly MemoizerFactory<long, long> FIBONACCI3_MEMOIZER_FACTORY = new Func<long, long>(FIBONACCI3).Memoize();
        static readonly MemoizerFactory<long, long> FIBONACCI4_MEMOIZER_FACTORY = new Func<long, long>(FIBONACCI4).Memoize();

        //MemoizerFactory<long, long> slow500Square_MemoizerFactory = new Func<long, long>(slow500Square).Memoize(); // Does not compile
        //MemoizerFactory<long, long> Slow500Square_MemoizerFactory = new Func<long, long>(Slow500Square).Memoize(); // Does not compile


        [Test]
        public void MemoizerFactoryHashingTests_FuncReferenceEquality()
        {
            MemoizerFactory<long, long> slow500Square_MemoizerFactory = new Func<long, long>(Slow500Square).Memoize();
            MemoizerFactory<long, long> slow1000PowerOfThree_MemoizerFactory = slow1000PowerOfThree.Memoize();

            //Assert.That(MemoizerHelper.CreateMemoizerHash(FIBONACCI_MEMOIZER_FACTORY), Is.EqualTo(MemoizerHelper.CreateMemoizerHash(FIBONACCI_MEMOIZER_FACTORY)));
            //// NB! Separate MemoizerFactory instances are not equal due to Func reference equality demand
            //Assert.That(MemoizerHelper.CreateMemoizerHash(FIBONACCI_MEMOIZER_FACTORY), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerHash(FIBONACCI2_MEMOIZER_FACTORY)));
            //Assert.That(MemoizerHelper.CreateMemoizerHash(FIBONACCI_MEMOIZER_FACTORY), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerHash(FIBONACCI3_MEMOIZER_FACTORY)));
            //Assert.That(MemoizerHelper.CreateMemoizerHash(FIBONACCI_MEMOIZER_FACTORY), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerHash(FIBONACCI4_MEMOIZER_FACTORY)));
            //Assert.That(MemoizerHelper.CreateMemoizerHash(FIBONACCI_MEMOIZER_FACTORY), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerHash(slow500Square_MemoizerFactory)));
            //Assert.That(MemoizerHelper.CreateMemoizerHash(FIBONACCI_MEMOIZER_FACTORY), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerHash(slow1000PowerOfThree_MemoizerFactory)));

            //Assert.That(MemoizerHelper.CreateMemoizerHash(slow500Square_MemoizerFactory), Is.EqualTo(MemoizerHelper.CreateMemoizerHash(slow500Square_MemoizerFactory)));
            //Assert.That(MemoizerHelper.CreateMemoizerHash(slow500Square_MemoizerFactory), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerHash(slow1000PowerOfThree_MemoizerFactory)));
            //Assert.That(MemoizerHelper.CreateMemoizerHash(slow1000PowerOfThree_MemoizerFactory), Is.EqualTo(MemoizerHelper.CreateMemoizerHash(slow1000PowerOfThree_MemoizerFactory)));
            //Assert.That(MemoizerHelper.CreateMemoizerHash(slow1000PowerOfThree_MemoizerFactory), Is.Not.EqualTo(MemoizerHelper.CreateMemoizerHash(slow500Square_MemoizerFactory)));

            Assert.That(FIBONACCI_MEMOIZER_FACTORY.MemoizerConfiguration, Is.EqualTo(FIBONACCI_MEMOIZER_FACTORY.MemoizerConfiguration));
            // NB! Separate MemoizerFactory instances are not equal due to Func reference equality demand
            Assert.That(FIBONACCI_MEMOIZER_FACTORY.MemoizerConfiguration, Is.Not.EqualTo(FIBONACCI2_MEMOIZER_FACTORY.MemoizerConfiguration));
            Assert.That(FIBONACCI_MEMOIZER_FACTORY.MemoizerConfiguration, Is.Not.EqualTo(FIBONACCI3_MEMOIZER_FACTORY.MemoizerConfiguration));
            Assert.That(FIBONACCI_MEMOIZER_FACTORY.MemoizerConfiguration, Is.Not.EqualTo(FIBONACCI4_MEMOIZER_FACTORY.MemoizerConfiguration));
            Assert.That(FIBONACCI_MEMOIZER_FACTORY.MemoizerConfiguration, Is.Not.EqualTo(slow500Square_MemoizerFactory.MemoizerConfiguration));
            Assert.That(FIBONACCI_MEMOIZER_FACTORY.MemoizerConfiguration, Is.Not.EqualTo(slow1000PowerOfThree_MemoizerFactory.MemoizerConfiguration));

            Assert.That(slow500Square_MemoizerFactory.MemoizerConfiguration, Is.EqualTo(slow500Square_MemoizerFactory.MemoizerConfiguration));
            Assert.That(slow500Square_MemoizerFactory.MemoizerConfiguration, Is.Not.EqualTo(slow1000PowerOfThree_MemoizerFactory.MemoizerConfiguration));
            Assert.That(slow1000PowerOfThree_MemoizerFactory.MemoizerConfiguration, Is.EqualTo(slow1000PowerOfThree_MemoizerFactory.MemoizerConfiguration));
            Assert.That(slow1000PowerOfThree_MemoizerFactory.MemoizerConfiguration, Is.Not.EqualTo(slow500Square_MemoizerFactory.MemoizerConfiguration));
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
            Console.WriteLine("Fibonacci(40) = " + result + " [first time 'on-the-spot-memoized' memoizerFactory invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");

            startTime = DateTime.Now.Ticks;
            result = FIBONACCI.GetMemoizer().InvokeWith(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Fibonacci(40) = " + result + " [second time time 'on-the-spot-memoized' memoizerFactory invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");

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
            result = slow500Square.Memoize().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(40) = " + result + " [first time 'on-the-spot-memoized', instrumented invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.GreaterThanOrEqualTo(500)); // ms (not memoized invocation)

            startTime = DateTime.Now.Ticks;
            result = slow500Square.Memoize().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(40) = " + result + " [second time time 'on-the-spot-memoized' instrumented invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.LessThan(20)); // ms (memoized invocation)

            startTime = DateTime.Now.Ticks;
            result = slow500Square.Memoize().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(50);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(50) = " + result + " [first time 'on-the-spot-memoized' instrumented invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.GreaterThanOrEqualTo(500)); // ms (not memoized invocation)

            startTime = DateTime.Now.Ticks;
            result = slow500Square.Memoize().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(60);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(60) = " + result + " [first time 'on-the-spot-memoized' instrumented invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.GreaterThanOrEqualTo(500)); // ms (not memoized invocation)

            startTime = DateTime.Now.Ticks;
            result = slow500Square.Memoize().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(50);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(50) = " + result + " [second time time 'on-the-spot-memoized' instrumented invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.LessThan(20)); // ms (memoized invocation)

            startTime = DateTime.Now.Ticks;
            result = slow500Square.Memoize().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(60);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Square(60) = " + result + " [second time time 'on-the-spot-memoized' instrumented invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.LessThan(20)); // ms (memoized invocation)


            // TODO: fails!!
            startTime = DateTime.Now.Ticks;
            IMemoizer<long, long> slow1000PowerOfThreeMemoizer1 = slow1000PowerOfThree.Memoize().InstrumentWith(Console.WriteLine).GetMemoizer();
            result = slow1000PowerOfThree.Memoize().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(40);
            durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("PowerOfThree(40) = " + result + " [first time 'on-the-spot-memoized' instrumented invocation took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.GreaterThanOrEqualTo(1000)); // ms (not memoized invocation)

            startTime = DateTime.Now.Ticks;
            IMemoizer<long, long> slow1000PowerOfThreeMemoizer2 = slow1000PowerOfThree.Memoize().InstrumentWith(Console.WriteLine).GetMemoizer();
            result = slow1000PowerOfThree.Memoize().InstrumentWith(Console.WriteLine).GetMemoizer().InvokeWith(40);
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
            MemoizerFactory<long, long> slowSquareMemoizerFactory = slow500Square.Memoize();
            IMemoizer<long, long> memoizedSlowSquare = slowSquareMemoizerFactory.GetMemoizer();
            long durationInTicks = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Memoizer construction: square.Memoize().Get() took " + durationInTicks / 10000 + " ms | " + durationInTicks + " ticks]");
            Assert.That(durationInTicks / 10000, Is.LessThan(20)); // ms (MemoizerFactory and Memoizer creation)

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


            slowSquareMemoizerFactory.InstrumentWith(Console.WriteLine);
            IMemoizer<long, long> memoizedSlowSquare2 = slowSquareMemoizerFactory.GetMemoizer();

            startTime = DateTime.Now.Ticks;
            result = slowSquareMemoizerFactory.GetMemoizer().InvokeWith(123);
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
        public void MemoizerFactoryIsMutable()
        {
            MemoizerFactory<long, long> memoizerFactory1 =
                slow1000PowerOfThree.Memoize().InstrumentWith(Console.WriteLine);

            MemoizerFactory<long, long> memoizerFactory2 =
                memoizerFactory1.KeepItemsCachedFor(0).Milliseconds
                                .KeepItemsCachedFor(12).Milliseconds
                                .KeepItemsCachedFor(120).Milliseconds;

            Assert.That(memoizerFactory1, Is.EqualTo(memoizerFactory2));
            Assert.That(memoizerFactory1, Is.EqualTo(memoizerFactory2));
            Assert.That(memoizerFactory1.GetMemoizer(), Is.SameAs(memoizerFactory2.GetMemoizer()));
        }
    }
}
