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
    class TwoPhaseExecutorTests
    {
        /// <summary>
        /// Well, not really a test... rather an instrumented demo
        /// </summary>
        //[Ignore("Temporary disabled...")]
        [Test]
        public void Test()
        {
            int NUMBER_OF_PARTICIPANTS = 6;

            // 1. Create the two-phase executor
            TwoPhaseExecutor twoPhaseExecutor = new TwoPhaseExecutor(NUMBER_OF_PARTICIPANTS);
            twoPhaseExecutor.Instrumentation = true;

            // 2. Create and start all worker threads
            if (NUMBER_OF_PARTICIPANTS % 2 == 0)
                for (int i = 0; i < NUMBER_OF_PARTICIPANTS / 2; ++i)
                {
                    new TrivialTask(twoPhaseExecutor.Barrier).Start();
                    new SomeOtherTask(twoPhaseExecutor.Barrier).Start();
                }
            else
                for (int i = 0; i < NUMBER_OF_PARTICIPANTS; ++i)
                    new SomeOtherTask(twoPhaseExecutor.Barrier).Start();

            // 3. Start the two-phase executor
            twoPhaseExecutor.Start();
        }

        class SomeOtherTask : AbstractTwoPhaseExecutorThread
        {
            static int TASK_COUNTER;

            public SomeOtherTask(Barrier barrier)
                : base(barrier, true)
            {
                TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
                ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);
                Action = () => Console.WriteLine("Barrier participant #" + ParticipantNumber + " [invocation #" + ExecutionIndex + "] [" + ThreadInfo + "]");

                if (Instrumentation)
                    Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]");
            }
        }


        [Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = "Number-of-iteration parameter ('numberOfIterations') cannot be a negative number", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_IterationParameterCannotBeNegativeNumber(
            [Values(-10, -1)] int numberOfIterations,
            [Values(1)] int numberOfConcurrentWorkerThreads)
        {
            //////long startTime = DateTime.Now.Ticks;
            ////IMemoizer<string, long, string> myMemoizer = ReallySlowNetworkInvocation1a.CreateMemoizer();

            //////Action<string, long> myConcurrentAction = (arg1, arg2) => memoizer.InvokeWith(arg1, arg2);
            ////Func<string, long, string> myConcurrentFunc = (arg1, arg2) => myMemoizer.InvokeWith(arg1, arg2);

            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            ////Func<string, long, string> myConcurrentFunc = (arg1, arg2) => ReallySlowNetworkInvocation1a.CreateMemoizer().InvokeWith(arg1, arg2);

            /*TwoPhaseExecutionContext<string, long, string> twoPhaseExecutionContext =*/
            myFunc.CreateExecutionContext(numberOfIterations, numberOfConcurrentWorkerThreads);
            ////TwoPhaseExecutionContextResultSet<string> twoPhaseExecutionContext = Execute(numberOfIterations, numberOfConcurrentTasks, "Jabadabadoo", 888L); // Hva med en slik en like greit...?
            ////TwoPhaseExecutionContext twoPhaseExecutionContext = myConcurrentFunc.Execute(NUMBER_OF_ITERATIONS).TimelyIterations.And(numberOfConcurrentTasks).SpcelyIterations;
            /*TwoPhaseExecutionContextResultSet<string, long, string> twoPhaseExecutionContextResultSet =*/
            //twoPhaseExecutionContext.Execute("Jabadabadoo", 888L); // Hva med en liste med parametre, forskjellige for hver concurrent thread

            //Assert.That(twoPhaseExecutionContextResultSet[0, 0].Result, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "Jabadabadoo" + 888L));
            //Assert.That(twoPhaseExecutionContextResultSet.StopWatch.DurationInMilliseconds, Is.InRange(NETWORK_RESPONSE_LATENCY_IN_MILLIS, 2600L));

            //Console.WriteLine("MultiThreadedMemoizedInvocation_Memoizer_NewAndEasyVersion: " + numberOfIterations + " rounds with " + numberOfConcurrentTasks + " concurrent, identical, non-memoized method invocations...");
            ////Console.WriteLine("MultiThreadedMemoizedInvocation_Memoizer_NewAndEasyVersion: " + numberOfIterations + " rounds with " + numberOfConcurrentTasks + " concurrent, identical, memoized method invocations...");
        }


        [Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = "Number-of-worker-threads parameter ('numberOfConcurrentThreadsWitinhEachIteration') cannot be a negative number", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_NumberOfConcurrentWorkerThreadsParameterCannotBeNegativeNumber(
            [Values(1)] int numberOfIterations,
            [Values(-10,-1)] int numberOfConcurrentWorkerThreads)
        {
            //////long startTime = DateTime.Now.Ticks;
            ////IMemoizer<string, long, string> myMemoizer = ReallySlowNetworkInvocation1a.CreateMemoizer();

            //////Action<string, long> myConcurrentAction = (arg1, arg2) => memoizer.InvokeWith(arg1, arg2);
            ////Func<string, long, string> myConcurrentFunc = (arg1, arg2) => myMemoizer.InvokeWith(arg1, arg2);

            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            ////Func<string, long, string> myConcurrentFunc = (arg1, arg2) => ReallySlowNetworkInvocation1a.CreateMemoizer().InvokeWith(arg1, arg2);

            /*TwoPhaseExecutionContext<string, long, string> twoPhaseExecutionContext =*/
            myFunc.CreateExecutionContext(numberOfIterations, numberOfConcurrentWorkerThreads);
            ////TwoPhaseExecutionContextResultSet<string> twoPhaseExecutionContext = Execute(numberOfIterations, numberOfConcurrentTasks, "Jabadabadoo", 888L); // Hva med en slik en like greit...?
            ////TwoPhaseExecutionContext twoPhaseExecutionContext = myConcurrentFunc.Execute(NUMBER_OF_ITERATIONS).TimelyIterations.And(numberOfConcurrentTasks).SpcelyIterations;
            /*TwoPhaseExecutionContextResultSet<string, long, string> twoPhaseExecutionContextResultSet =*/
            //twoPhaseExecutionContext.Execute("Jabadabadoo", 888L); // Hva med en liste med parametre, forskjellige for hver concurrent thread

            //Assert.That(twoPhaseExecutionContextResultSet[0, 0].Result, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "Jabadabadoo" + 888L));
            //Assert.That(twoPhaseExecutionContextResultSet.StopWatch.DurationInMilliseconds, Is.InRange(NETWORK_RESPONSE_LATENCY_IN_MILLIS, 2600L));

            //Console.WriteLine("MultiThreadedMemoizedInvocation_Memoizer_NewAndEasyVersion: " + numberOfIterations + " rounds with " + numberOfConcurrentTasks + " concurrent, identical, non-memoized method invocations...");
            ////Console.WriteLine("MultiThreadedMemoizedInvocation_Memoizer_NewAndEasyVersion: " + numberOfIterations + " rounds with " + numberOfConcurrentTasks + " concurrent, identical, memoized method invocations...");
        }


        [Test]
        public void TwoPhaseExecutionContext_ZeroIterationParameter(
            [Values(0)] int numberOfIterations,
            [Values(1)] int numberOfConcurrentWorkerThreads)
        {
            ////long startTime = DateTime.Now.Ticks;
            //IMemoizer<string, long, string> myMemoizer = ReallySlowNetworkInvocation1a.CreateMemoizer();

            ////Action<string, long> myConcurrentAction = (arg1, arg2) => memoizer.InvokeWith(arg1, arg2);
            //Func<string, long, string> myConcurrentFunc = (arg1, arg2) => myMemoizer.InvokeWith(arg1, arg2);

            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            //            Func<string, long, string> myConcurrentFunc = (arg1, arg2) => ReallySlowNetworkInvocation1a.Invoke(arg1, arg2);
            //Func<string, long, string> myConcurrentFunc = (arg1, arg2) => ReallySlowNetworkInvocation1a.CreateMemoizer().InvokeWith(arg1, arg2);

            TwoPhaseExecutionContext<string, long, string> twoPhaseExecutionContext = myFunc.CreateExecutionContext(numberOfIterations, numberOfConcurrentWorkerThreads);
            //TwoPhaseExecutionContextResultSet<string> twoPhaseExecutionContext = Execute(numberOfIterations, numberOfConcurrentTasks, "Jabadabadoo", 888L); // Hva med en slik en like greit...?
            //TwoPhaseExecutionContext twoPhaseExecutionContext = myConcurrentFunc.Execute(NUMBER_OF_ITERATIONS).TimelyIterations.And(numberOfConcurrentTasks).SpcelyIterations;
            TwoPhaseExecutionContextResultSet<string, long, string> twoPhaseExecutionContextResultSet = twoPhaseExecutionContext.Execute("Jabadabadoo", 888L); // Hva med en liste med parametre, forskjellige for hver concurrent thread

            Assert.That(twoPhaseExecutionContextResultSet[0, 0], Is.Null);
            //            Assert.That(twoPhaseExecutionContextResultSet.StopWatch.DurationInMilliseconds, Is.LInRange(NETWORK_RESPONSE_LATENCY_IN_MILLIS, 2600L));

            //Console.WriteLine("MultiThreadedMemoizedInvocation_Memoizer_NewAndEasyVersion: " + numberOfIterations + " rounds with " + numberOfConcurrentTasks + " concurrent, identical, non-memoized method invocations...");
            //Console.WriteLine("MultiThreadedMemoizedInvocation_Memoizer_NewAndEasyVersion: " + numberOfIterations + " rounds with " + numberOfConcurrentTasks + " concurrent, identical, memoized method invocations...");
        }



        [Test]
        public void TwoPhaseExecutionContext_ZeroNumberOfConcurrentWorkerThreadsParameter(
            [Values(1)] int numberOfIterations,
            [Values(0)] int numberOfConcurrentWorkerThreads)
        {
            ////long startTime = DateTime.Now.Ticks;
            //IMemoizer<string, long, string> myMemoizer = ReallySlowNetworkInvocation1a.CreateMemoizer();

            ////Action<string, long> myConcurrentAction = (arg1, arg2) => memoizer.InvokeWith(arg1, arg2);
            //Func<string, long, string> myConcurrentFunc = (arg1, arg2) => myMemoizer.InvokeWith(arg1, arg2);

            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            //            Func<string, long, string> myConcurrentFunc = (arg1, arg2) => ReallySlowNetworkInvocation1a.Invoke(arg1, arg2);
            //Func<string, long, string> myConcurrentFunc = (arg1, arg2) => ReallySlowNetworkInvocation1a.CreateMemoizer().InvokeWith(arg1, arg2);

            TwoPhaseExecutionContext<string, long, string> twoPhaseExecutionContext = myFunc.CreateExecutionContext(numberOfIterations, numberOfConcurrentWorkerThreads);
            //TwoPhaseExecutionContextResultSet<string> twoPhaseExecutionContext = Execute(numberOfIterations, numberOfConcurrentTasks, "Jabadabadoo", 888L); // Hva med en slik en like greit...?
            //TwoPhaseExecutionContext twoPhaseExecutionContext = myConcurrentFunc.Execute(NUMBER_OF_ITERATIONS).TimelyIterations.And(numberOfConcurrentTasks).SpcelyIterations;
            TwoPhaseExecutionContextResultSet<string, long, string> twoPhaseExecutionContextResultSet = twoPhaseExecutionContext.Execute("Jabadabadoo", 888L); // Hva med en liste med parametre, forskjellige for hver concurrent thread

            Assert.That(twoPhaseExecutionContextResultSet[0, 0], Is.Null);
            //            Assert.That(twoPhaseExecutionContextResultSet.StopWatch.DurationInMilliseconds, Is.LInRange(NETWORK_RESPONSE_LATENCY_IN_MILLIS, 2600L));

            //Console.WriteLine("MultiThreadedMemoizedInvocation_Memoizer_NewAndEasyVersion: " + numberOfIterations + " rounds with " + numberOfConcurrentTasks + " concurrent, identical, non-memoized method invocations...");
            //Console.WriteLine("MultiThreadedMemoizedInvocation_Memoizer_NewAndEasyVersion: " + numberOfIterations + " rounds with " + numberOfConcurrentTasks + " concurrent, identical, memoized method invocations...");
        }



    }
}
