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

                if (Instrumented)
                    Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]");
            }
        }

        #region New API
        [Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = "Number-of-iteration parameter ('numberOfIterations') cannot be a negative number", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_IterationParameterCannotBeNegativeNumber(
            [Values(-10, -1)] int numberOfIterations,
            [Values(1)] int numberOfConcurrentWorkerThreads)
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            myFunc.CreateExecutionContext(numberOfIterations: numberOfIterations, numberOfConcurrentThreadsWitinhEachIteration: numberOfConcurrentWorkerThreads);
        }


        [Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = "Number-of-worker-threads parameter ('numberOfConcurrentThreadsWitinhEachIteration') cannot be a negative number", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_NumberOfConcurrentWorkerThreadsParameterCannotBeNegativeNumber(
            [Values(1)] int numberOfIterations,
            [Values(-10, -1)] int numberOfConcurrentWorkerThreads)
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            myFunc.CreateExecutionContext(numberOfIterations: numberOfIterations, numberOfConcurrentThreadsWitinhEachIteration: numberOfConcurrentWorkerThreads);
        }


        [Test, ExpectedException(typeof(Exception), ExpectedMessage = "No TwoPhaseExecutionContextResults are available", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_ZeroIterationParameter(
            [Values(0)] int numberOfIterations,
            [Values(1)] int numberOfConcurrentWorkerThreads)
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext(numberOfIterations: numberOfIterations, numberOfConcurrentThreadsWitinhEachIteration: numberOfConcurrentWorkerThreads);
            // TODO: new API...
            /*TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet =*/ twoPhaseExecutionContext.Execute("Jabadabadoo", 888L); // Hva med en liste med parametre, forskjellige for hver concurrent thread
            //Assert.That(twoPhaseExecutionContextResultSet[0, 0], Is.Null);
        }


        [Test, ExpectedException(typeof(Exception), ExpectedMessage = "No FuncTwoPhaseExecutorThreads (worker threads) are available", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_ZeroNumberOfConcurrentWorkerThreadsParameter(
            [Values(1)] int numberOfIterations,
            [Values(0)] int numberOfConcurrentWorkerThreads)
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext(numberOfIterations: numberOfIterations, numberOfConcurrentThreadsWitinhEachIteration: numberOfConcurrentWorkerThreads);
            // TODO: new API...
            /*TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet =*/ twoPhaseExecutionContext.Execute("Jabadabadoo", 888L); // Hva med en liste med parametre, forskjellige for hver concurrent thread
            //Assert.That(twoPhaseExecutionContextResultSet[0, 0], Is.Null);
        }


        [Test, ExpectedException(typeof(Exception), ExpectedMessage = "Result set contains only 1 iteration... Really no point is asking for iteration #2 (zero-based) then, is it?", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_OutOfBoundsErrorMessages1()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext(numberOfIterations: 1, numberOfConcurrentThreadsWitinhEachIteration: 2);
            // TODO: new API...
            /*TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet =*/ twoPhaseExecutionContext.Execute("Jabadabadoo", 888L); // Hva med en liste med parametre, forskjellige for hver concurrent thread
            //var nonExistingIterationResultSet = twoPhaseExecutionContextResultSet[1, 0];
        }


        [Test, ExpectedException(typeof(Exception), ExpectedMessage = "Result set contains only 3 iterations... Really no point is asking for iteration #20 (zero-based) then, is it?", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_OutOfBoundsErrorMessages2()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext(numberOfIterations: 3, numberOfConcurrentThreadsWitinhEachIteration: 2);
            // TODO: new API...
            /*TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet = */twoPhaseExecutionContext.Execute("Jabadabadoo", 888L); // Hva med en liste med parametre, forskjellige for hver concurrent thread
            //var nonExistingIterationResultSet = twoPhaseExecutionContextResultSet[19, 0];
        }


        [Test, ExpectedException(typeof(Exception), ExpectedMessage = "Result set contains only 1 worker thread... Really no point is asking for thread #2 (zero-based) then, is it?", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_OutOfBoundsErrorMessages3()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext(numberOfIterations: 2, numberOfConcurrentThreadsWitinhEachIteration: 1);
            // TODO: new API...
            /*TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet = */twoPhaseExecutionContext.Execute("Jabadabadoo", 888L); // Hva med en liste med parametre, forskjellige for hver concurrent thread
            //var nonExistingWorkerThreadResultSet = twoPhaseExecutionContextResultSet[1, 1];
        }


        [Test, ExpectedException(typeof(Exception), ExpectedMessage = "Result set contains only 10 worker threads... Really no point is asking for thread #100 (zero-based) then, is it?", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_OutOfBoundsErrorMessages4()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext(numberOfIterations: 3, numberOfConcurrentThreadsWitinhEachIteration: 10);
            // TODO: new API...
            /*TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet =*/ twoPhaseExecutionContext.Execute("Jabadabadoo", 888L); // Hva med en liste med parametre, forskjellige for hver concurrent thread
            //var nonExistingWorkerThreadResultSet = twoPhaseExecutionContextResultSet[1, 99];
        }
        #endregion
    }
}
