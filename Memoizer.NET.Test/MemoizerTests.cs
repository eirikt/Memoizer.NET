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
        internal const int NUMBER_OF_ITERATIONS = 3;
        internal const int NUMBER_OF_CONCURRENT_TASKS = 600;

        internal const int DATABASE_RESPONSE_LATENCY_IN_MILLIS = 30;
        internal const int NETWORK_RESPONSE_LATENCY_IN_MILLIS = 1000;

        internal const string METHOD_RESPONSE_ELEMENT = "VeryExpensiveMethodResponseFor";
        #endregion

        #region Test helper methods
        static string Concatinate(string arg1, string arg2, string arg3) { return arg1 + arg2 + arg3; }


        internal static string ReallySlowNetworkStaticInvocation(string stringArg, long longArg)
        {
            //Console.WriteLine("TypicalNetworkInvocation invoked...");
            Thread.Sleep(NETWORK_RESPONSE_LATENCY_IN_MILLIS);
            //Console.WriteLine("TypicalNetworkInvocation returns...");
            return Concatinate(METHOD_RESPONSE_ELEMENT, stringArg, Convert.ToString(longArg));
        }

        internal Func<string, long, string> ReallySlowNetworkInvocation1a =
           new Func<string, long, string>(delegate(string stringArg, long longArg)
           {
               return ReallySlowNetworkStaticInvocation(stringArg, longArg);
           });

        internal Func<string, long, string> ReallySlowNetworkInvocation1b =
           (delegate(string stringArg, long longArg)
           {
               return ReallySlowNetworkStaticInvocation(stringArg, longArg);
           });

        internal static Func<string, long, string> ReallySlowNetworkInvocation1c = ReallySlowNetworkStaticInvocation;

        internal Func<string, long, string> ReallySlowNetworkInvocation2(string stringArg, long longArg)
        {
            ReallySlowNetworkStaticInvocation(stringArg, longArg);
            return null;
        }

        internal string ReallySlowNetworkInvocation3(string stringArg, long longArg)
        {
            return ReallySlowNetworkStaticInvocation(stringArg, longArg);
        }


        internal static string TypicalDatabaseStaticInvocation(string stringArg, long longArg)
        {
            Console.WriteLine("TypicalDatabaseInvocation invoked...");
            Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
            Console.WriteLine("TypicalDatabaseInvocation returns...");
            return Concatinate(METHOD_RESPONSE_ELEMENT, stringArg, Convert.ToString(longArg));
        }

        internal Func<string, long, string> TypicalDatabaseInvocation1a =
            new Func<string, long, string>(delegate(string stringArg, long longArg)
            {
                Console.WriteLine("TypicalDatabaseInvocation invoked...");
                Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
                Console.WriteLine("TypicalDatabaseInvocation returns...");
                return Concatinate(METHOD_RESPONSE_ELEMENT, stringArg, Convert.ToString(longArg));
                // Or:
                //return TypicalDatabaseStaticInvocation(stringArg, longArg);
            });

        internal Func<string, long, string> TypicalDatabaseInvocation1b =
           (delegate(string stringArg, long longArg)
           {
               Console.WriteLine("TypicalDatabaseInvocation invoked...");
               Thread.Sleep(DATABASE_RESPONSE_LATENCY_IN_MILLIS);
               Console.WriteLine("TypicalDatabaseInvocation returns...");
               return Concatinate(METHOD_RESPONSE_ELEMENT, stringArg, Convert.ToString(longArg));
               // Or:
               //return TypicalDatabaseStaticInvocation(stringArg, longArg);
           });

        internal Func<string, long, string> TypicalDatabaseInvocation1c = TypicalDatabaseStaticInvocation;

        internal Func<string, long, string> TypicalDatabaseInvocation2(string stringArg, long longArg)
        {
            TypicalDatabaseStaticInvocation(stringArg, longArg);
            return null;
        }

        internal string TypicalDatabaseInvocation3(string stringArg, long longArg)
        {
            return TypicalDatabaseStaticInvocation(stringArg, longArg);
        }
        #endregion

        #region MemoizerHelper
        Func<long, long> slow500Square = (arg1 => { Thread.Sleep(500); return arg1 * arg1; });
        Func<long, long> slow1000Square = (arg1 => { Thread.Sleep(1000); return arg1 * arg1; });
        Func<long, long> slow500PowerOfThree = (arg1 => { Thread.Sleep(500); return arg1 * arg1 * arg1; });
        Func<long, long> slow1000PowerOfThree = (arg1 => { Thread.Sleep(1000); return arg1 * arg1 * arg1; });

        [Test]
        public void StringsArePrimitivesArentThey()
        {
            Assert.That(typeof(long).IsPrimitive, Is.True);
            Assert.That(typeof(Int64).IsPrimitive, Is.True);
            Assert.That(typeof(ulong).IsPrimitive, Is.True);
            Assert.That(typeof(UInt64).IsPrimitive, Is.True);
            Assert.That(typeof(String).IsPrimitive, Is.False); // Nope
        }

        [Test]
        public void ShouldCreateAHashForDelegates()
        {
            Assert.That(MemoizerHelper.CreateParameterHash(40L), Is.EqualTo(MemoizerHelper.CreateParameterHash(40L)));
            Assert.That(MemoizerHelper.CreateParameterHash(slow500Square), Is.EqualTo(MemoizerHelper.CreateParameterHash(slow500Square)));
            Assert.That(MemoizerHelper.CreateParameterHash(slow1000Square), Is.EqualTo(MemoizerHelper.CreateParameterHash(slow1000Square)));
            Assert.That(MemoizerHelper.CreateParameterHash(slow500Square), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(slow1000Square)));
        }

        // TODO: test hashing complex objects!

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
                    var retVal = ReallySlowNetworkInvocation3("SingleThreadedDirectInvocation", 13L);
                    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedDirectInvocation" + 13L));
                }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
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
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.GreaterThanOrEqualTo(NUMBER_OF_ITERATIONS * NETWORK_RESPONSE_LATENCY_IN_MILLIS));
            Console.WriteLine(
                "MultiThreadedDirectInvocation: " + NUMBER_OF_CONCURRENT_TASKS + " concurrent, identical, non-memoized method invocations." +
                " " + NUMBER_OF_ITERATIONS + " iterations with " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms latency took " + durationInMilliseconds + " ms" +
                " (should take > " + NUMBER_OF_ITERATIONS * NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms).");
        }
        #endregion

        #region GoetzMemoryCacheMemoizer, a.k.a. THE Memoizer
        //Func<string, string> VeryExpensiveNullInvocationFunc =
        //    new Func<string, string>(delegate(string s)
        //    {
        //        Thread.Sleep(NETWORK_RESPONSE_LATENCY_IN_MILLIS);
        //        return null;
        //    });
        Func<string, string> VeryExpensiveNullInvocationFunc = (delegate
        {
            Thread.Sleep(NETWORK_RESPONSE_LATENCY_IN_MILLIS);
            return null;
        });
        //Func<string, string> VeryExpensiveNullInvocation() 
        //{
        //    Thread.Sleep(NETWORK_RESPONSE_LATENCY_IN_MILLIS);
        //    return null;
        //}
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

            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Console.WriteLine("2 invocations of non-memoized null-return-functions took " + durationInMilliseconds + " milliseconds (should take > " + 2 * NETWORK_RESPONSE_LATENCY_IN_MILLIS + ")");
            Assert.That(durationInMilliseconds, Is.InRange(1990L, 2100L)); // < 2000L due to CLR/platform magic

            startTime = DateTime.Now.Ticks;
            string result3 = VeryExpensiveNullInvocationFunc.CachedInvoke("whatever");
            Assert.That(result3, Is.Null);

            string result4 = VeryExpensiveNullInvocationFunc.CachedInvoke("whatever");
            Assert.That(result4, Is.Null);

            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
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
            //IMemoizer<string, long, string> memoizer = ReallySlowNetworkInvocation1c.Memoize().GetMemoizer();
            for (int i = 0; i < NUMBER_OF_ITERATIONS; ++i)
                for (int j = 0; j < NUMBER_OF_CONCURRENT_TASKS; ++j)
                {
                    var retVal = ReallySlowNetworkInvocation1c.CachedInvoke("SingleThreadedMemoizedDirectInvocation_Memoizer", 14L);
                    Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocation_Memoizer" + 14L));
                }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
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
            IMemoizer<string, long, string> memoizer = ReallySlowNetworkInvocation1b.CacheFor(NETWORK_RESPONSE_LATENCY_IN_MILLIS * 3).Milliseconds.GetMemoizer();

            // New function value, not yet cached
            var retVal = memoizer.InvokeWith("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer", 15L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer" + 15L));
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.InRange(1000L, 1200L));
            Console.WriteLine("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer: first non-memoized method invocation with latency " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take >= " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + ")");

            // Memoized function within time span, should use cached value
            startTime = DateTime.Now.Ticks;
            retVal = memoizer.InvokeWith("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer", 15L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer" + 15L));
            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.LessThan(10L));
            Console.WriteLine("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer: second memoized method invocation with latency " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take < 10 ms)");

            // New function value, not yet cached
            startTime = DateTime.Now.Ticks;
            retVal = memoizer.InvokeWith("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer", 16L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer" + 16L));
            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.InRange(1000L, 1200L));
            Console.WriteLine("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer: another first non-memoized method invocation with latency " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take >= " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + ")");

            Console.WriteLine("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer: waiting for memoizer cache item evictions ...");
            Thread.Sleep(NETWORK_RESPONSE_LATENCY_IN_MILLIS * 3);

            // Memoized function evicted due to exceeded time span, should take its time (for re-memoization)
            startTime = DateTime.Now.Ticks;
            retVal = memoizer.InvokeWith("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer", 15L);
            Assert.That(retVal, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer" + 15L));
            durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.InRange(1000L, 1200L));
            Console.WriteLine("SingleThreadedMemoizedDirectInvocationWithPolicy_Memoizer: third memoized (but evicted) method invocation with latency " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms took " + durationInMilliseconds + " ms (should take >= " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + ")");
        }


        class VeryExpensiveMemoizedServiceCallTask_Memoizer : AbstractTwoPhaseExecutorThread
        {
            static int TASK_COUNTER;
            public string Result { get; private set; }
            readonly IMemoizer<string, long, string> memoizer;


            internal VeryExpensiveMemoizedServiceCallTask_Memoizer(Barrier barrier, IMemoizer<string, long, string> memoizer, string stringArg, long longArg)
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

        //static void LoggingMethod(string loggingMessage)
        //{
        //    Console.WriteLine(loggingMessage);
        //}

        /// <summary>
        /// Concurrent invocations of the memoizer
        /// </summary>
        //[Ignore("Temporary disabled...")]
        [Test]
        void MultiThreadedMemoizedInvocation_Memoizer(
           [Values(1, 2, 10, 30, 60, 100, 200, 400, 800, 1000, 1200)] int numberOfConcurrentTasks)
        {
            long startTime = DateTime.Now.Ticks;
            IMemoizer<string, long, string> memoizer =
                ReallySlowNetworkInvocation1a.Cache().CachePolicy(null).InstrumentWith(Console.WriteLine).GetMemoizer();
            for (int i = 0; i < NUMBER_OF_ITERATIONS; ++i)
            {
                // Arrange
                TwoPhaseExecutor twoPhaseExecutor = new TwoPhaseExecutor(numberOfConcurrentTasks);
                VeryExpensiveMemoizedServiceCallTask_Memoizer[] tasks = new VeryExpensiveMemoizedServiceCallTask_Memoizer[numberOfConcurrentTasks];
                for (int j = 0; j < numberOfConcurrentTasks; ++j)
                    tasks[j] = new VeryExpensiveMemoizedServiceCallTask_Memoizer(twoPhaseExecutor.Barrier, memoizer, "MultiThreadedMemoizedInvocation_Memoizer", 15L);

                // Act
                for (int j = 0; j < numberOfConcurrentTasks; ++j)
                    tasks[j].Start();
                twoPhaseExecutor.Start();

                // Assert
                for (int j = 0; j < numberOfConcurrentTasks; ++j)
                    Assert.That(tasks[j].Result, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "MultiThreadedMemoizedInvocation_Memoizer" + 15L));
            }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Assert.That(durationInMilliseconds, Is.InRange(NETWORK_RESPONSE_LATENCY_IN_MILLIS, 2600L));
            Console.WriteLine(
                "MultiThreadedMemoizedInvocation_Memoizer: " + NUMBER_OF_ITERATIONS + " rounds with " + numberOfConcurrentTasks + " concurrent, identical, memoized method invocations:" +
                " " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms non-cached method latency took " + durationInMilliseconds + " ms" +
                " (should take " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " > " + durationInMilliseconds + " > 2600 ms).");
        }


        class ClearCacheTask_Memoizer : AbstractTwoPhaseExecutorThread
        {
            static int TASK_COUNTER;
            readonly IMemoizer<string, long, string> memoizer;

            public ClearCacheTask_Memoizer(Barrier barrier, IMemoizer<string, long, string> memoizer)
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
                                 ((IClearable)this.memoizer).Clear();
                             };

                if (Instrumentation)
                    Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]");
            }
        }


        //        [Ignore("delebil...")]
        //        [Test, ExpectedException(typeof(FormatException), ExpectedMessage = "Hour and minute string must be given as HH:mm", MatchType = MessageMatch.Contains)]
        //        public void GetDateTimeFromHourAndMinuteString_ShouldFailIfStringParamIsNotOfCorrectFormat(
        //            [Values(null, "", "  ", "18:39:00", "1839", "18 39", "18'39", "18''39", "18,39", "18.39", "18;39", "18-39", "01.10.2010 18:39")] string hourMinuteString)
        //        {
        ////            DateTimeHelper.GetDateTimeFromHourAndMinuteString(DateTime.Now, hourMinuteString);
        //        }


        /// <summary>
        /// Concurrent invocation of the memoizer
        /// </summary>
        //[Ignore("Temporary disabled...")]
        [Test]
        public void MultiThreadedMemoizedInvocationWithClearing_Memoizer(
            [Values(1, 2, 10, 30, 60, 100, 200, 400, 800, 1000, 1200)] int numberOfConcurrentTasks)
        {
            //for (int i = 0; i < NUMBER_OF_ITERATIONS; ++i)
            //{
            //const int METHOD_NUMBER_OF_CONCURRENT_TASKS = 50;
            long startTime = DateTime.Now.Ticks;
            IMemoizer<string, long, string> memoizer = TypicalDatabaseInvocation1a.GetMemoizer();
            // Arrange
            TwoPhaseExecutor twoPhaseExecutor = new TwoPhaseExecutor(numberOfConcurrentTasks + 3); // + 3 clearing tasks
            //twoPhaseExecutor.Instrumentation = true;
            VeryExpensiveMemoizedServiceCallTask_Memoizer[] tasks = new VeryExpensiveMemoizedServiceCallTask_Memoizer[numberOfConcurrentTasks];

            ClearCacheTask_Memoizer clearingTask1 = new ClearCacheTask_Memoizer(twoPhaseExecutor.Barrier, memoizer);
            //clearingTask1.Instrumentation = true;

            for (int j = 0; j < numberOfConcurrentTasks; ++j)
            {
                tasks[j] = new VeryExpensiveMemoizedServiceCallTask_Memoizer(twoPhaseExecutor.Barrier, memoizer, "MultiThreadedMemoizedInvocationWithClearing_Memoizer", 15L);
                //tasks[j].Instrumentation = true;
            }

            ClearCacheTask_Memoizer clearingTask2 = new ClearCacheTask_Memoizer(twoPhaseExecutor.Barrier, memoizer);
            //clearingTask2.Instrumentation = true;
            ClearCacheTask_Memoizer clearingTask3 = new ClearCacheTask_Memoizer(twoPhaseExecutor.Barrier, memoizer);
            //clearingTask3.Instrumentation = true;

            // Act
            clearingTask1.Start();

            for (int j = 0; j < numberOfConcurrentTasks; ++j)
                tasks[j].Start();

            clearingTask2.Start();
            clearingTask3.Start();
            twoPhaseExecutor.Start();

            // Assert
            for (int j = 0; j < numberOfConcurrentTasks; ++j)
                Assert.That(tasks[j].Result, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "MultiThreadedMemoizedInvocationWithClearing_Memoizer" + 15L));
            //}
            //long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            //Assert.That(durationInMilliseconds, Is.GreaterThan(METHOD_RESPONSE_LATENCY_IN_MILLIS * NUMBER_OF_ITERATIONS));// - 2000L)); // -2000 due to possible CLR optomizations
            //Assert.That(durationInMilliseconds, Is.LessThan(METHOD_RESPONSE_LATENCY_IN_MILLIS * NUMBER_OF_ITERATIONS + METHOD_RESPONSE_LATENCY_IN_MILLIS));
            //Console.WriteLine("MultiThreadedMemoizedInvocationWithClearing_Memoizer: " + NUMBER_OF_ITERATIONS + " * " + NUMBER_OF_CONCURRENT_TASKS + " concurrent, identical method invocations (latency: " + METHOD_RESPONSE_LATENCY_IN_MILLIS + " ms) took " + durationInMilliseconds + " ms");

            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;

            // The more threads, the more contention overhard, and the longer this test will take.
            // Just a silly empiric hack emulating the CLR resource contention behaviour...
            int threadContentionFactor = 1;
            if (numberOfConcurrentTasks > 100)
                threadContentionFactor += 2;
            if (numberOfConcurrentTasks > 200)
                threadContentionFactor += 2;
            if (numberOfConcurrentTasks > 300)
                threadContentionFactor += 2;
            if (numberOfConcurrentTasks > 400)
                threadContentionFactor += 2;
            if (numberOfConcurrentTasks > 500)
                threadContentionFactor += 2;
            if (numberOfConcurrentTasks > 600)
                threadContentionFactor += 2;
            if (numberOfConcurrentTasks > 700)
                threadContentionFactor += 2;
            if (numberOfConcurrentTasks > 800)
                threadContentionFactor += 2;
            if (numberOfConcurrentTasks > 900)
                threadContentionFactor += 2;
            if (numberOfConcurrentTasks > 1000)
                threadContentionFactor += 2;

            int numberOfMemoizerElementsCleared = memoizer.NumberOfElementsCleared;
            int numberOfMemoizerInvocations = memoizer.NumberOfTimesInvoked;
            int numberOfMemoizerNoCachedExecutions = memoizer.NumberOfTimesNoCacheInvoked;

            Assert.That(numberOfMemoizerInvocations, Is.GreaterThanOrEqualTo(numberOfMemoizerNoCachedExecutions));
            Assert.That(numberOfMemoizerElementsCleared, Is.LessThanOrEqualTo(numberOfMemoizerNoCachedExecutions));

            //int totalExpectedLatencyInMillis = DATABASE_RESPONSE_LATENCY_IN_MILLIS + (memoizer.NumberOfElementsCleared * DATABASE_RESPONSE_LATENCY_IN_MILLIS);
            int minimumExpectedLatencyInMillis = DATABASE_RESPONSE_LATENCY_IN_MILLIS;
            //int maximumExpectedLatencyInMillis = (numberOfMemoizerNoCachedExecutions * DATABASE_RESPONSE_LATENCY_IN_MILLIS) + (DATABASE_RESPONSE_LATENCY_IN_MILLIS / 2);
            int maximumExpectedLatencyInMillis = DATABASE_RESPONSE_LATENCY_IN_MILLIS + ((numberOfMemoizerNoCachedExecutions * DATABASE_RESPONSE_LATENCY_IN_MILLIS) * threadContentionFactor);

            //Assert.That(durationInMilliseconds, Is.InRange(DATABASE_RESPONSE_LATENCY_IN_MILLIS, DATABASE_RESPONSE_LATENCY_IN_MILLIS+20L));
            Console.WriteLine(
                "MultiThreadedMemoizedInvocationWithClearing_Memoizer: " + numberOfConcurrentTasks + " concurrent, identical, memoized method invocations.");// +
            ///*" " + NUMBER_OF_ITERATIONS + " iterations*/ " 1 iteration with " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms latency and " + numberOfMemoizerNoCachedExecutions + " function executions took " + durationInMilliseconds + " ms" +
            //" (should take [" + minimumExpectedLatencyInMillis + " < " + durationInMilliseconds + " < " + maximumExpectedLatencyInMillis + "] ms).");
            Console.WriteLine(
                "MultiThreadedMemoizedInvocationWithClearing_Memoizer: " + // + numberOfConcurrentTasks + " concurrent, identical, memoized method invocations." +
                /*" " + NUMBER_OF_ITERATIONS + " iterations*/ "1 iteration with " + DATABASE_RESPONSE_LATENCY_IN_MILLIS + " ms latency and " + numberOfMemoizerNoCachedExecutions + " function execution(s) took " + durationInMilliseconds + " ms" +
                " (should take [" + minimumExpectedLatencyInMillis + " < " + durationInMilliseconds + " < " + maximumExpectedLatencyInMillis + "] ms) (threadContentionFactor=" + threadContentionFactor + ").");

            Assert.That(durationInMilliseconds, Is.GreaterThan(minimumExpectedLatencyInMillis));
            Assert.That(durationInMilliseconds, Is.LessThan(maximumExpectedLatencyInMillis));
            //}
        }


        static public readonly Func<long, long> FIBONACCI = (arg => arg <= 1 ? arg : FIBONACCI(arg - 1) + FIBONACCI(arg - 2));

        //Memoizer<long, long> MEMOIZED_FIBONACCI = new Memoizer<long, long>("fibonacci", FIBONACCI).InstrumentUsing(Console.WriteLine);
        //Memoizer<long, long> MEMOIZED_FIBONACCI_INSTRUMENTED = new Memoizer<long, long>("fibonacci", (arg => arg < 2 ? arg : FIBONACCI(arg - 1) + FIBONACCI(arg - 2)));
        //MEMOIZED_FIBONACCI_INSTRUMENTED.InstrumentWith(Console.WriteLine);
        //readonly Func<long, long> MEMOIZED_FIBONACCI = FIBONACCI.MemoizedFunc();
        readonly IMemoizer<long, long> MEMOIZED_FIBONACCI_IINVOCABLE = FIBONACCI.Cache().GetMemoizer();

        [Test]
        public void FibonacciNumbers(
            [Values(1, 4, 8, 12, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40)] int numberOfFibonacciArguments)
        //[Values(6)] int numberOfFibonacciArguments)
        {
            long startTime = DateTime.Now.Ticks;
            Console.WriteLine("Fibonacci(" + numberOfFibonacciArguments + ") =");
            for (int i = 0; i < numberOfFibonacciArguments; ++i) { Console.Write(FIBONACCI(i) + " "); }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Console.WriteLine();
            Console.WriteLine("Took " + durationInMilliseconds + " ms");
        }

        //// TODO: Memoization of recursive functions not yet supported
        //[Test]
        //public void MemoizedFibonacciNumbers_Nope(
        //    [Values(1, 4, 8, 12, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40)] int numberOfFibonacciArguments)
        ////[Values(6)] int numberOfFibonacciArguments)
        //{
        //    long startTime = DateTime.Now.Ticks;
        //    Console.WriteLine("(Memoized [Func.Hash=" + MEMOIZED_FIBONACCI.GetHashCode() + "]) Fibonacci(" + numberOfFibonacciArguments + ") =");
        //    for (int i = 0; i < numberOfFibonacciArguments; ++i) { Console.Write(MEMOIZED_FIBONACCI(i) + " "); }
        //    long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
        //    Console.WriteLine();
        //    Console.WriteLine("Took " + durationInMilliseconds + " ms");
        //}

        // TODO: Memoization of recursive functions not yet supported
        [Test]
        public void MemoizedFibonacciNumbers_KindOf(
            [Values(1, 4, 8, 12, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40)] int numberOfFibonacciArguments)
        //[Values(6)] int numberOfFibonacciArguments)
        {
            long startTime = DateTime.Now.Ticks;
            Console.WriteLine("(Memoized [IInvocable.Hash=" + MEMOIZED_FIBONACCI_IINVOCABLE.GetHashCode() + "]) Fibonacci(" + numberOfFibonacciArguments + ") =");
            for (int i = 0; i < numberOfFibonacciArguments; ++i) { Console.Write(MEMOIZED_FIBONACCI_IINVOCABLE.InvokeWith(i) + " "); }
            long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Console.WriteLine();
            Console.WriteLine("Took " + durationInMilliseconds + " ms");
        }
        #endregion


        // TODO: test when caching with complex objects as parameters

        // TODO: check the behaviour of the MemoryCache name parameter (Gets or sets the name of a particular cache configuration) via multiple memoizer instances at the same time

    }
    #endregion
}
#pragma warning restore 618
#endregion
