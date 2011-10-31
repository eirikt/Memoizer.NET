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

#pragma warning disable 618
namespace Memoizer.NET.Test
{

    #region DictionaryMemoizer
    //[Obsolete("Just a demo - use Memoizer class - its much cooler")]
    public class DictionaryMemoizer<TResult>
    {
        static readonly object @lock = new object();

        // Not thread-safe - all kinds of write access must be serialized
        readonly IDictionary<string, object> cache = new Dictionary<string, object>();

        public void Clear() { this.cache.Clear(); }

        public TResult Invoke<TParam1, TParam2>(Func<TParam1, TParam2, TResult> memoizedMethod, TParam1 arg1, TParam2 arg2)
        {
            string key = MemoizerHelper.CreateParameterHash(arg1, arg2);
            if (this.cache.ContainsKey(key))
                return (TResult)cache[key];

            lock (@lock)
            {
                if (this.cache.ContainsKey(key))
                    return (TResult)cache[key];
                TResult retVal = memoizedMethod.Invoke(arg1, arg2);
                this.cache.Add(key, retVal);
                return retVal;
            }
        }
    }
    #endregion


    [TestFixture]
    class DictionaryMemoizerTests
    {
        /// <summary>
        /// Sequential invocations of the memoizer
        /// </summary>
        //[Ignore("This memoizer implementation is not the main focus here...")]
        [Test]
        public void SingleThreadedMemoizedInvocation_DictionaryMemoizer_WithLocks()
        {
            long startTime = DateTime.Now.Ticks;
            DictionaryMemoizer<string> memoizer = new DictionaryMemoizer<string>();
            for (int i = 0; i < MemoizerTests.NUMBER_OF_ITERATIONS; ++i)
                for (int j = 0; j < MemoizerTests.NUMBER_OF_CONCURRENT_TASKS; ++j)
                {
                    string retVal = memoizer.Invoke(MemoizerTests.ReallySlowNetworkInvocation1c, "SingleThreadedMemoizedInvocation_DictionaryMemoizer_WithLocks", 14L);
                    Assert.That(retVal, Is.EqualTo(MemoizerTests.METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedInvocation_DictionaryMemoizer_WithLocks" + 14L));
                }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(MemoizerTests.NETWORK_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine(
                "SingleThreadedMemoizedInvocation_DictionaryMemoizer_WithLocks: " + MemoizerTests.NUMBER_OF_ITERATIONS + " iterations of " + MemoizerTests.NUMBER_OF_CONCURRENT_TASKS + " sequential, identical, memoized method invocations with " + MemoizerTests.NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms latency" +
                " took " + durationInMilliseconds + " ms" +
                " (should take > " + MemoizerTests.NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms)");
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
                Action = () => this.Result = this.memoizer.Invoke(MemoizerTests.ReallySlowNetworkStaticInvocation, stringArg, longArg);
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
            for (int i = 0; i < MemoizerTests.NUMBER_OF_ITERATIONS; ++i)
            {
                // Arrange
                TwoPhaseExecutor twoPhaseExecutor = new TwoPhaseExecutor(MemoizerTests.NUMBER_OF_CONCURRENT_TASKS);
                VeryExpensiveMemoizedServiceCallTask_DictionaryMemoizer[] tasks = new VeryExpensiveMemoizedServiceCallTask_DictionaryMemoizer[MemoizerTests.NUMBER_OF_CONCURRENT_TASKS];
                for (int j = 0; j < MemoizerTests.NUMBER_OF_CONCURRENT_TASKS; ++j)
                    tasks[j] = new VeryExpensiveMemoizedServiceCallTask_DictionaryMemoizer(twoPhaseExecutor.Barrier, memoizer, "MultiThreadedMemoizedWithLocksInvocation", 15L);

                // Act
                for (int j = 0; j < MemoizerTests.NUMBER_OF_CONCURRENT_TASKS; ++j)
                    tasks[j].Start();
                twoPhaseExecutor.Start();

                // Assert
                for (int j = 0; j < MemoizerTests.NUMBER_OF_CONCURRENT_TASKS; ++j)
                    Assert.That(tasks[j].Result, Is.EqualTo(MemoizerTests.METHOD_RESPONSE_ELEMENT + "MultiThreadedMemoizedWithLocksInvocation" + 15L));
            }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / 10000;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(MemoizerTests.NETWORK_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine(
                "MultiThreadedMemoizedInvocation_DictionaryMemoizer_WithLocks: " + MemoizerTests.NUMBER_OF_CONCURRENT_TASKS + " concurrent, identical, memoized method invocations." +
                " " + MemoizerTests.NUMBER_OF_ITERATIONS + " iterations with " + MemoizerTests.NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms latency took " + durationInMilliseconds + " ms" +
                " (should take > " + MemoizerTests.NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms).");
        }
    }
}
#pragma warning restore 618
