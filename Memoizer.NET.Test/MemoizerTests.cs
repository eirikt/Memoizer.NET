#region Licence
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
#endregion

#region Using
using System;
using System.Runtime.Caching;
using System.Threading;
using NUnit.Framework;
#endregion

#region Namespace
#pragma warning disable 618
namespace Memoizer.NET.Test
{

    #region MemoizerTests
    [TestFixture]
    class MemoizerTests
    {

        #region Test constants
        const int NUMBER_OF_ITERATIONS = 10;
        const int NUMBER_OF_CONCURRENT_TASKS = 600;
        const int DATABASE_RESPONSE_LATENCY_IN_MILLIS = 30;
        const int NETWORK_RESPONSE_LATENCY_IN_MILLIS = 1000;
        const string METHOD_RESPONSE_ELEMENT = "VeryExpensiveMethodResponseFor";
        #endregion

        #region Test helper methods
        static string Concatinate(string arg1, string arg2, string arg3) { return arg1 + arg2 + arg3; }

        static string ReallySlowNetworkStaticInvocation(string stringArg, long longArg)
        {
            //Console.WriteLine("TypicalNetworkInvocation invoked...");
            Thread.Sleep(NETWORK_RESPONSE_LATENCY_IN_MILLIS);
            //Console.WriteLine("TypicalNetworkInvocation returns...");
            return Concatinate(METHOD_RESPONSE_ELEMENT, stringArg, Convert.ToString(longArg));
        }
        string ReallySlowNetworkInvocation(string stringArg, long longArg) { return ReallySlowNetworkStaticInvocation(stringArg, longArg); }

        static string TypicalDatabaseStaticInvocation(string stringArg, long longArg)
        {
            Console.WriteLine("TypicalDatabaseInvocation invoked...");
            Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
            Console.WriteLine("TypicalDatabaseInvocation returns...");
            return Concatinate(METHOD_RESPONSE_ELEMENT, stringArg, Convert.ToString(longArg));
        }
        string TypicalDatabaseInvocation(string stringArg, long longArg) { return TypicalDatabaseStaticInvocation(stringArg, longArg); }
        #endregion

        #region MemoizerHelper
        // TODO: test hashing complex objects!
        [Ignore("TODO")]
        [Test]
        public void ShouldCreateAHashForClassNameAndMethodName()
        {
            // ...
        }
        #endregion

        #region Direct invocation
        [Ignore("Takes its time...")]
        [Test]
        public void SingleThreadedDirectInvocation()
        {
            long startTime = DateTime.Now.Ticks;
            for (int i = 0; i < NUMBER_OF_ITERATIONS; ++i)
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                {
                    var retVal = ReallySlowNetworkInvocation("SingleThreadedDirectInvocation", 13L);
                    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedDirectInvocation" + 13L));
                }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(NUMBER_OF_ITERATIONS * NETWORK_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine(
                "SingleThreadedDirectInvocation: " + NUMBER_OF_ITERATIONS + " iterations of " + NUMBER_OF_CONCURRENT_TASKS + " sequential, identical, non-memoized method invocations with " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms latency" +
                " took " + durationInMilliseconds + " ms" +
                " (should take > " + NUMBER_OF_ITERATIONS * NUMBER_OF_CONCURRENT_TASKS * NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms)");
        }


        class VeryExpensiveDirectServiceCallTask : AbstractTwoPhaseExecutorThread
        {
            static int TASK_COUNTER;
            public string Result { get; private set; }

            public VeryExpensiveDirectServiceCallTask(Barrier barrier, string stringArg, long longArg)
                : base(barrier)
            {
                TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
                ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);
                Action = () => this.Result = ReallySlowNetworkStaticInvocation(stringArg, longArg);

                if (Instrumentation)
                    Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]");
            }
        }

        //[Ignore("Temporary disabled...")]
        [Test]
        public void MultiThreadedDirectInvocation()
        {
            long startTime = DateTime.Now.Ticks;
            for (int i = 0; i < NUMBER_OF_ITERATIONS; ++i)
            {
                // Arrange
                TwoPhaseExecutor twoPhaseExecutor = new TwoPhaseExecutor(NUMBER_OF_CONCURRENT_TASKS);
                VeryExpensiveDirectServiceCallTask[] tasks = new VeryExpensiveDirectServiceCallTask[NUMBER_OF_CONCURRENT_TASKS];
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                    tasks[j] = new VeryExpensiveDirectServiceCallTask(twoPhaseExecutor.Barrier, "MultiThreadedDirectInvocation", 15L);

                // Act
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                    tasks[j].Start();
                twoPhaseExecutor.Start();

                // Assert
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                    Assert.That(tasks[j].Result, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "MultiThreadedDirectInvocation" + 15L));
            }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(NUMBER_OF_ITERATIONS * NETWORK_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine(
                "MultiThreadedDirectInvocation: " + NUMBER_OF_CONCURRENT_TASKS + " concurrent, identical, non-memoized method invocations." +
                " " + NUMBER_OF_ITERATIONS + " iterations with " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms latency took " + durationInMilliseconds + " ms" +
                " (should take > " + NUMBER_OF_ITERATIONS * NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms).");
        }
        #endregion

        #region DictionaryMemoizer
        /// <summary>
        /// Sequential invocations of the memoizer
        /// </summary>
        //[Ignore("This memoizer implementation is not the main focus here...")]
        [Test]
        public void SingleThreadedMemoizedInvocation_DictionaryMemoizer_WithLocks()
        {
            long startTime = DateTime.Now.Ticks;
            DictionaryMemoizer<string> memoizer = new DictionaryMemoizer<string>();
            for (int i = 0; i < NUMBER_OF_ITERATIONS; ++i)
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                {
                    string retVal = memoizer.Invoke(ReallySlowNetworkInvocation, "SingleThreadedMemoizedInvocation_DictionaryMemoizer_WithLocks", 14L);
                    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedInvocation_DictionaryMemoizer_WithLocks" + 14L));
                }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(NETWORK_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine(
                "SingleThreadedMemoizedInvocation_DictionaryMemoizer_WithLocks: " + NUMBER_OF_ITERATIONS + " iterations of " + NUMBER_OF_CONCURRENT_TASKS + " sequential, identical, memoized method invocations with " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms latency" +
                " took " + durationInMilliseconds + " ms" +
                " (should take > " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms)");
        }


        class VeryExpensiveMemoizedServiceCallTask_DictionaryMemoizer : AbstractTwoPhaseExecutorThread
        {
            static int TASK_COUNTER;
            public string Result { get; private set; }
            readonly DictionaryMemoizer<string> memoizer;

            public VeryExpensiveMemoizedServiceCallTask_DictionaryMemoizer(Barrier barrier, DictionaryMemoizer<string> memoizer, string stringArg, long longArg)
                : base(barrier)
            {
                TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
                ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);
                this.memoizer = memoizer;
                Action = () => this.Result = this.memoizer.Invoke(ReallySlowNetworkStaticInvocation, stringArg, longArg);
                if (Instrumentation)
                    Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]");
            }
        }

        /// <summary>
        /// Concurrent invocations of the memoizer
        /// </summary>
        //[Ignore("This memoizer implementation is not the main focus here...")]
        [Test]
        public void MultiThreadedMemoizedInvocation_DictionaryMemoizer_WithLocks()
        {
            long startTime = DateTime.Now.Ticks;
            DictionaryMemoizer<string> memoizer = new DictionaryMemoizer<string>();
            for (int i = 0; i < NUMBER_OF_ITERATIONS; ++i)
            {
                // Arrange
                TwoPhaseExecutor twoPhaseExecutor = new TwoPhaseExecutor(NUMBER_OF_CONCURRENT_TASKS);
                VeryExpensiveMemoizedServiceCallTask_DictionaryMemoizer[] tasks = new VeryExpensiveMemoizedServiceCallTask_DictionaryMemoizer[NUMBER_OF_CONCURRENT_TASKS];
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                    tasks[j] = new VeryExpensiveMemoizedServiceCallTask_DictionaryMemoizer(twoPhaseExecutor.Barrier, memoizer, "MultiThreadedMemoizedWithLocksInvocation", 15L);

                // Act
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                    tasks[j].Start();
                twoPhaseExecutor.Start();

                // Assert
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                    Assert.That(tasks[j].Result, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "MultiThreadedMemoizedWithLocksInvocation" + 15L));
            }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(NETWORK_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine(
                "MultiThreadedMemoizedInvocation_DictionaryMemoizer_WithLocks: " + NUMBER_OF_CONCURRENT_TASKS + " concurrent, identical, memoized method invocations." +
                " " + NUMBER_OF_ITERATIONS + " iterations with " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms latency took " + durationInMilliseconds + " ms" +
                " (should take > " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms).");
        }
        #endregion

        #region ConcurrentDictionaryMemoizer
        //[Ignore("This memoizer implementation is not the main focus here...")]
        [Test]
        public void SingleThreadedMemoizedInvocation_ConcurrentDictionaryMemoizer()
        {
            long startTime = DateTime.Now.Ticks;
            ConcurrentDictionaryMemoizer<string> memoizer = new ConcurrentDictionaryMemoizer<string>();
            for (int i = 0; i < NUMBER_OF_ITERATIONS; ++i)
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                {
                    string retVal = memoizer.Invoke(ReallySlowNetworkInvocation, "SingleThreadedMemoizedDirectInvocation", 14L);
                    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocation" + 14L));
                }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(NETWORK_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine(
                "SingleThreadedMemoizedInvocation_ConcurrentDictionaryMemoizer: " + NUMBER_OF_ITERATIONS + " iterations of " + NUMBER_OF_CONCURRENT_TASKS + " sequential, identical, memoized method invocations with " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms latency" +
                " took " + durationInMilliseconds + " ms" +
                " (should take > " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms)");
        }


        class VeryExpensiveMemoizedServiceCallTask_ConcurrentDictionaryMemoizer : AbstractTwoPhaseExecutorThread
        {
            static int TASK_COUNTER;
            public string Result { get; private set; }
            readonly ConcurrentDictionaryMemoizer<string> memoizer;

            public VeryExpensiveMemoizedServiceCallTask_ConcurrentDictionaryMemoizer(Barrier barrier, ConcurrentDictionaryMemoizer<string> memoizer, string stringArg, long longArg)
                : base(barrier)
            {
                TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
                ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);
                this.memoizer = memoizer;
                Action = () => this.Result = this.memoizer.Invoke(ReallySlowNetworkStaticInvocation, stringArg, longArg);
                if (Instrumentation)
                    Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]");
            }
        }

        //[Ignore("This memoizer implementation is not the main focus here...")]
        [Test]
        public void MultiThreadedMemoizedInvocation_ConcurrentDictionaryMemoizer()
        {
            long startTime = DateTime.Now.Ticks;
            ConcurrentDictionaryMemoizer<string> memoizer = new ConcurrentDictionaryMemoizer<string>();
            for (int i = 0; i < NUMBER_OF_ITERATIONS; ++i)
            {
                // Arrange
                TwoPhaseExecutor twoPhaseExecutor = new TwoPhaseExecutor(NUMBER_OF_CONCURRENT_TASKS);
                VeryExpensiveMemoizedServiceCallTask_ConcurrentDictionaryMemoizer[] tasks = new VeryExpensiveMemoizedServiceCallTask_ConcurrentDictionaryMemoizer[NUMBER_OF_CONCURRENT_TASKS];
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                    tasks[j] = new VeryExpensiveMemoizedServiceCallTask_ConcurrentDictionaryMemoizer(twoPhaseExecutor.Barrier, memoizer, "MultiThreadedMemoizedWithLocksInvocation", 15L);

                // Act
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                    tasks[j].Start();
                twoPhaseExecutor.Start();

                // Assert
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                    Assert.That(tasks[j].Result, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "MultiThreadedMemoizedWithLocksInvocation" + 15L));
            }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(NETWORK_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine(
                "MultiThreadedMemoizedInvocation_ConcurrentDictionaryMemoizer: " + NUMBER_OF_CONCURRENT_TASKS + " concurrent, identical, memoized method invocations." +
                " " + NUMBER_OF_ITERATIONS + " iterations with " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms latency took " + durationInMilliseconds + " ms" +
                " (should take > " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms).");
        }
        #endregion

        #region MemoryCacheMemoizer
        //[Ignore("This memoizer implementation is not the main focus here...")]
        [Test]
        public void SingleThreadedMemoizedInvocation_MemoryCacheMemoizer()
        {
            long startTime = DateTime.Now.Ticks;
            MemoryCacheMemoizer<string> memoizer = new MemoryCacheMemoizer<string>(this.GetType(), "SingleThreadedMemoizedInvocation_MemoryCacheMemoizer");
            for (int i = 0; i < NUMBER_OF_ITERATIONS; ++i)
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                {
                    var retVal = memoizer.Invoke(ReallySlowNetworkInvocation, "SingleThreadedMemoizedDirectInvocation", 14L);
                    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocation" + 14L));
                }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(NETWORK_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine(
                "SingleThreadedMemoizedInvocation_MemoryCacheMemoizer: " + NUMBER_OF_ITERATIONS + " iterations of " + NUMBER_OF_CONCURRENT_TASKS + " sequential, identical, memoized method invocations with " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms latency" +
                " took " + durationInMilliseconds + " ms" +
                " (should take > " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms)");
        }


        class VeryExpensiveMemoizedServiceCallTask_MemoryCacheMemoizer : AbstractTwoPhaseExecutorThread
        {
            static int TASK_COUNTER;
            public string Result { get; private set; }
            readonly MemoryCacheMemoizer<string> memoizer;

            public VeryExpensiveMemoizedServiceCallTask_MemoryCacheMemoizer(Barrier barrier, MemoryCacheMemoizer<string> memoizer, string stringArg, long longArg)
                : base(barrier)
            {
                TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
                ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);
                this.memoizer = memoizer;
                Action = () => this.Result = this.memoizer.Invoke(ReallySlowNetworkStaticInvocation, stringArg, longArg);
                if (Instrumentation)
                    Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]");
            }
        }

        //[Ignore("This memoizer implementation is not the main focus here...")]
        [Test]
        public void MultiThreadedMemoizedInvocation_MemoryCacheMemoizer()
        {
            long startTime = DateTime.Now.Ticks;
            MemoryCacheMemoizer<string> memoizer = new MemoryCacheMemoizer<string>(this.GetType(), "MultiThreadedMemoizedInvocation_MemoryCacheMemoizer");

            for (int i = 0; i < NUMBER_OF_ITERATIONS; ++i)
            {
                // Arrange
                TwoPhaseExecutor twoPhaseExecutor = new TwoPhaseExecutor(NUMBER_OF_CONCURRENT_TASKS);
                VeryExpensiveMemoizedServiceCallTask_MemoryCacheMemoizer[] tasks = new VeryExpensiveMemoizedServiceCallTask_MemoryCacheMemoizer[NUMBER_OF_CONCURRENT_TASKS];
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                    tasks[j] = new VeryExpensiveMemoizedServiceCallTask_MemoryCacheMemoizer(twoPhaseExecutor.Barrier, memoizer, "MultiThreadedMemoizedWithLocksInvocation", 15L);

                // Act
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                    tasks[j].Start();
                twoPhaseExecutor.Start();

                // Assert
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                    Assert.That(tasks[j].Result, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "MultiThreadedMemoizedWithLocksInvocation" + 15L));
            }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(NETWORK_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine(
                "MultiThreadedMemoizedInvocation_MemoryCacheMemoizer: " + NUMBER_OF_CONCURRENT_TASKS + " concurrent, identical, memoized method invocations." +
                " " + NUMBER_OF_ITERATIONS + " iterations with " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms latency took " + durationInMilliseconds + " ms" +
                " (should take > " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms).");
        }
        #endregion

        #region GoetzMemoryCacheMemoizer, a.k.a. THE Memoizer
        string VeryExpensiveNullInvocation(string arg)
        {
            //Console.WriteLine("Sleeping for 1000ms... [" + DateTime.Now + "]");
            Thread.Sleep(NETWORK_RESPONSE_LATENCY_IN_MILLIS);
            //Console.WriteLine("Awake again...[" + DateTime.Now + "]");
            return null;
        }

        //[Ignore("Temporary disabled...")]
        [Test]
        public void ShouldCacheNullValues()
        {
            long startTime = DateTime.Now.Ticks;

            string result1 = VeryExpensiveNullInvocation("whatever");
            Assert.That(result1, Is.Null);

            string result2 = VeryExpensiveNullInvocation("whatever");
            Assert.That(result2, Is.Null);

            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Console.WriteLine("2 invocations of non-memoized null-return-functions took " + durationInMilliseconds + " milliseconds (should take > " + 2 * NETWORK_RESPONSE_LATENCY_IN_MILLIS + ")");
            Assert.That(durationInMilliseconds, Is.InRange(1990L, 2100L)); // < 2000L due to CLR/platform magic

            startTime = DateTime.Now.Ticks;
            Memoizer<string, string> memoizer = new Memoizer<string, string>(
                this.GetType(),
                "ShouldCacheNullValues",
                //arg => { Thread.Sleep(1000); return null; });
                VeryExpensiveNullInvocation);
            string result3 = memoizer.InvokeWith("whatever");
            Assert.That(result3, Is.Null);

            string result4 = memoizer.InvokeWith("whatever");
            Assert.That(result4, Is.Null);

            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Console.WriteLine("2 invocations of memoized null-return-functions took " + durationInMilliseconds + " milliseconds (should take > " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + ")");
            Assert.That(durationInMilliseconds, Is.InRange(990L, 1200L)); // < 1000L due to CLR/platform magic
        }


        /// <summary>
        /// Sequential invocations of the memoizer
        /// </summary>
        //[Ignore("Temporary disabled...")]
        [Test]
        public void SingleThreadedMemoizedDirectInvocation_Memoizer()
        {
            long startTime = DateTime.Now.Ticks;
            Memoizer<string, string, long> memoizer =
                new Memoizer<string, string, long>(this.GetType(),
                                                   "SingleThreadedMemoizedDirectInvocation_Memoizer",
                                                   ReallySlowNetworkInvocation);
            for (int i = 0; i < NUMBER_OF_ITERATIONS; ++i)
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                {
                    var retVal = memoizer.InvokeWith("SingleThreadedMemoizedDirectInvocation_Memoizer", 14L);
                    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocation_Memoizer" + 14L));
                }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Assert.That(durationInMilliseconds, Is.InRange(1000L, 1050L));
            Console.WriteLine(
                "SingleThreadedMemoizedDirectInvocation_Memoizer: " + NUMBER_OF_ITERATIONS + " iterations of " + NUMBER_OF_CONCURRENT_TASKS + " sequential, identical, memoized method invocations with " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms latency" +
                " took " + durationInMilliseconds + " ms" +
                " (should take > " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms)");
        }


        /// <summary>
        /// Sequential invocation of the memoizer
        /// </summary>
        //[Ignore("Temporary disabled...")]
        [Test]
        public void SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer()
        {
            long startTime = DateTime.Now.Ticks;
            CacheItemPolicy cacheItemEvictionPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMilliseconds(NETWORK_RESPONSE_LATENCY_IN_MILLIS * 3) };
            Memoizer<string, string, long> memoizer =
                new Memoizer<string, string, long>(this.GetType(),
                                                   "SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer",
                                                   ReallySlowNetworkInvocation,
                                                   cacheItemEvictionPolicy);
            // New function value, not yet cached
            var retVal = memoizer.InvokeWith("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer", 15L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer" + 15L));
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Assert.That(durationInMilliseconds, Is.InRange(1000L, 1200L));
            Console.WriteLine("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer: first non-memoized method invocation with latency " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take >= " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + ")");

            // Memoized function within time span, should use cached value
            startTime = DateTime.Now.Ticks;
            retVal = memoizer.InvokeWith("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer", 15L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer" + 15L));
            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Assert.That(durationInMilliseconds, Is.LessThan(10L));
            Console.WriteLine("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer: second memoized method invocation with latency " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");

            // New function value, not yet cached
            startTime = DateTime.Now.Ticks;
            retVal = memoizer.InvokeWith("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer", 16L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer" + 16L));
            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Assert.That(durationInMilliseconds, Is.InRange(1000L, 1200L));
            Console.WriteLine("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer: another first non-memoized method invocation with latency " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take >= " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + ")");

            Console.WriteLine("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer: waiting for memoizer cache item evictions ...");
            Thread.Sleep(NETWORK_RESPONSE_LATENCY_IN_MILLIS * 3);

            // Memoized function evicted due to exceeded time span, should take its time (for re-memoization)
            startTime = DateTime.Now.Ticks;
            retVal = memoizer.InvokeWith("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer", 15L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer" + 15L));
            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Assert.That(durationInMilliseconds, Is.InRange(1000L, 1200L));
            Console.WriteLine("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer: third memoized (but evicted) method invocation with latency " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take >= " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + ")");
        }


        class VeryExpensiveMemoizedServiceCallTask_Memoizer : AbstractTwoPhaseExecutorThread
        {
            static int TASK_COUNTER;
            public string Result { get; private set; }
            readonly Memoizer<string, string, long> memoizer;

            public VeryExpensiveMemoizedServiceCallTask_Memoizer(Barrier barrier, Memoizer<string, string, long> memoizer, string stringArg, long longArg)
                : base(barrier)
            {
                TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
                ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);
                this.memoizer = memoizer;
                Action = () => this.Result = this.memoizer.InvokeWith(stringArg, longArg);

                if (Instrumentation)
                    Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]");
            }
        }

        /// <summary>
        /// Concurrent invocations of the memoizer
        /// </summary>
        //[Ignore("Temporary disabled...")]
        [Test]
        public void MultiThreadedMemoizedInvocation_Memoizer()
        {
            long startTime = DateTime.Now.Ticks;
            Memoizer<string, string, long> memoizer =
                new Memoizer<string, string, long>(this.GetType(),
                                                   "MultiThreadedMemoizedInvocation_Memoizer",
                                                   ReallySlowNetworkInvocation);
            for (int i = 0; i < NUMBER_OF_ITERATIONS; ++i)
            {
                // Arrange
                TwoPhaseExecutor twoPhaseExecutor = new TwoPhaseExecutor(NUMBER_OF_CONCURRENT_TASKS);
                VeryExpensiveMemoizedServiceCallTask_Memoizer[] tasks = new VeryExpensiveMemoizedServiceCallTask_Memoizer[NUMBER_OF_CONCURRENT_TASKS];
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                    tasks[j] = new VeryExpensiveMemoizedServiceCallTask_Memoizer(twoPhaseExecutor.Barrier, memoizer, "MultiThreadedMemoizedInvocation_Memoizer", 15L);

                // Act
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                    tasks[j].Start();
                twoPhaseExecutor.Start();

                // Assert
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                    Assert.That(tasks[j].Result, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "MultiThreadedMemoizedInvocation_Memoizer" + 15L));
            }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Assert.That(durationInMilliseconds, Is.InRange(1000L, 2600L));
            Console.WriteLine(
                "MultiThreadedMemoizedInvocation_Memoizer: " + NUMBER_OF_CONCURRENT_TASKS + " concurrent, identical, memoized method invocations." +
                " " + NUMBER_OF_ITERATIONS + " iterations with " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms latency took " + durationInMilliseconds + " ms" +
                " (should take > " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms).");
        }


        class ClearCacheTask_Memoizer : AbstractTwoPhaseExecutorThread
        {
            static int TASK_COUNTER;
            readonly Memoizer<string, string, long> memoizer;

            public ClearCacheTask_Memoizer(Barrier barrier, Memoizer<string, string, long> memoizer)
                : base(barrier)
            {
                TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
                ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);
                this.memoizer = memoizer;
                Action = () =>
                             {
                                 ////while (barrier.ParticipantsRemaining > 5)
                                 ////{
                                 //if (Instrumentation)
                                 //    Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " yielding...");
                                 //Thread.Yield();
                                 //Thread.Sleep(10);
                                 ////}

                                 //Thread.SpinWait(1000);

                                 //Thread.Yield();

                                 if (Instrumentation)
                                     Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " clearing cache...");
                                 this.memoizer.Clear();
                             };

                if (Instrumentation)
                    Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]");
            }
        }


        /// <summary>
        /// Concurrent invocation of the memoizer
        /// </summary>
        [Ignore("Temporary disabled... TODO")]
        [Test]
        public void MultiThreadedMemoizedInvocationWithClearing_Memoizer()
        {
            const int NUMBER_OF_CONCURRENT_TASKS = 400;
            long startTime = DateTime.Now.Ticks;
            Memoizer<string, string, long> memoizer =
                new Memoizer<string, string, long>(this.GetType(),
                                                   "MultiThreadedMemoizedInvocationWithClearing_Memoizer",
                                                   TypicalDatabaseInvocation);
            //for (int i = 0; i < NUMBER_OF_ITERATIONS; ++i)
            //{
            // Arrange
            TwoPhaseExecutor twoPhaseExecutor = new TwoPhaseExecutor(NUMBER_OF_CONCURRENT_TASKS + 3);
            //TwoPhaseExecutor twoPhaseExecutor = new TwoPhaseExecutor(NUMBER_OF_CONCURRENT_TASKS);
            twoPhaseExecutor.Instrumentation = false;
            VeryExpensiveMemoizedServiceCallTask_Memoizer[] tasks = new VeryExpensiveMemoizedServiceCallTask_Memoizer[NUMBER_OF_CONCURRENT_TASKS];

            ClearCacheTask_Memoizer clearingTask1 = new ClearCacheTask_Memoizer(twoPhaseExecutor.Barrier, memoizer);
            clearingTask1.Instrumentation = false;

            for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
            {
                tasks[j] = new VeryExpensiveMemoizedServiceCallTask_Memoizer(twoPhaseExecutor.Barrier, memoizer, "MultiThreadedMemoizedInvocationWithClearing_Memoizer", 15L);
                tasks[j].Instrumentation = false;
            }

            ClearCacheTask_Memoizer clearingTask2 = new ClearCacheTask_Memoizer(twoPhaseExecutor.Barrier, memoizer);
            clearingTask2.Instrumentation = false;
            ClearCacheTask_Memoizer clearingTask3 = new ClearCacheTask_Memoizer(twoPhaseExecutor.Barrier, memoizer);
            clearingTask3.Instrumentation = false;

            // Act
            clearingTask1.Start();

            for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                tasks[j].Start();

            clearingTask2.Start();
            clearingTask3.Start();
            twoPhaseExecutor.Start();

            // Assert
            for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                Assert.That(tasks[j].Result, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "MultiThreadedMemoizedInvocationWithClearing_Memoizer" + 15L));
            //}
            //long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;

            //Assert.That(durationInMilliseconds, Is.GreaterThan(METHOD_RESPONSE_LATENCY_IN_MILLIS * NUMBER_OF_ITERATIONS));// - 2000L)); // -2000 due to possible CLR optomizations

            //Assert.That(durationInMilliseconds, Is.LessThan(METHOD_RESPONSE_LATENCY_IN_MILLIS * NUMBER_OF_ITERATIONS + METHOD_RESPONSE_LATENCY_IN_MILLIS));

            //Console.WriteLine("MultiThreadedMemoizedInvocationWithClearing_Memoizer: " + NUMBER_OF_ITERATIONS + " * " + NUMBER_OF_CONCURRENT_TASKS + " concurrent, identical method invocations (latency: " + METHOD_RESPONSE_LATENCY_IN_MILLIS + " ms) took " + durationInMilliseconds + " ms");

            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            //Assert.That(durationInMilliseconds, Is.InRange(DATABASE_RESPONSE_LATENCY_IN_MILLIS, DATABASE_RESPONSE_LATENCY_IN_MILLIS+20L));
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine(
                "MultiThreadedMemoizedInvocationWithClearing_Memoizer: " + NUMBER_OF_CONCURRENT_TASKS + " concurrent, identical, memoized method invocations." +
                " " + NUMBER_OF_ITERATIONS + " iterations with " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms latency took " + durationInMilliseconds + " ms" +
                " (should take > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms).");

            //long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            //Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(DATABASE_RESPONSE_LATENCY_IN_MILLIS));
            //Console.WriteLine("MultiThreadedMemoizedInvocationWithClearing_Memoizer: " + NUMBER_OF_ITERATIONS + " * " + NUMBER_OF_CONCURRENT_TASKS + " concurrent, identical, memoized method invocations" +
            //    "(latency: " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms) took " + durationInMilliseconds + " ms " +
            //    "(should be > " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms)");
        }


        // TODO: test when caching with complex objects as parameters

        #endregion
    }
    #endregion
}
#pragma warning restore 618
#endregion
