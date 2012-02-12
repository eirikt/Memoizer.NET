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

        ///// <summary>
        ///// Well, not really a test... rather an instrumented demo
        ///// </summary>
        ////[Ignore("Temporary disabled...")]
        //[Test]
        //public void Test()
        //{
        //    int NUMBER_OF_PARTICIPANTS = 6;

        //    // 1. Create the two-phase executor
        //    TwoPhaseExecutor twoPhaseExecutor = new TwoPhaseExecutor(NUMBER_OF_PARTICIPANTS);
        //    twoPhaseExecutor.Instrumentation = true;

        //    // 2. Create and start all worker threads
        //    if (NUMBER_OF_PARTICIPANTS % 2 == 0)
        //        for (int i = 0; i < NUMBER_OF_PARTICIPANTS / 2; ++i)
        //        {
        //            new TrivialTask(twoPhaseExecutor.Barrier).Start();
        //            new SomeOtherTask(twoPhaseExecutor.Barrier).Start();
        //        }
        //    else
        //        for (int i = 0; i < NUMBER_OF_PARTICIPANTS; ++i)
        //            new SomeOtherTask(twoPhaseExecutor.Barrier).Start();

        //    // 3. Start the two-phase executor
        //    twoPhaseExecutor.Start();
        //}

        //class SomeOtherTask : AbstractTwoPhaseExecutorThread
        //{
        //    static int TASK_COUNTER;

        //    public SomeOtherTask(Barrier barrier)
        //        : base(barrier, true)
        //    {
        //        TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
        //        ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);
        //        Action = () => Console.WriteLine("Barrier participant #" + ParticipantNumber + " [invocation #" + ExecutionIndex + "] [" + ThreadInfo + "]");

        //        if (Instrumented)
        //            Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]");
        //    }
        //}

        #region New API
        [Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = "Number-of-iteration parameter ('iterations') cannot be a negative number", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_IterationParameterCannotBeNegativeNumber(
            [Values(-10, -1)] int numberOfIterations,
            [Values(1)] int numberOfConcurrentWorkerThreads)
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            myFunc.CreateExecutionContext(iterations: numberOfIterations, threads: numberOfConcurrentWorkerThreads);
        }


        [Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = "Number-of-worker-threads parameter ('threads') cannot be a negative number", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_NumberOfConcurrentWorkerThreadsParameterCannotBeNegativeNumber(
            [Values(1)] int numberOfIterations,
            [Values(-10, -1)] int numberOfConcurrentWorkerThreads)
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            myFunc.CreateExecutionContext(iterations: numberOfIterations, threads: numberOfConcurrentWorkerThreads);
        }


        [Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = "Number-of-arguments parameter ('args') does not match the function signature", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_NumberOfArgumentsParameterMustMatchTheFunctionSignature()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            myFunc.CreateExecutionContext(args: new dynamic[] { "cowabunga" });
        }


        [Test, ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Execution context is not yet executed", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_VerifyingAnUnExecutedContextShouldThrowException()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            myFunc.CreateExecutionContext().Verify();
        }


        [Test]
        public void TwoPhaseExecutionContext_DefaultExecution()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            myFunc.CreateExecutionContext().Execute().Verify();
            // TODO: asserts...?
        }


        [Test]
        public void TwoPhaseExecutionContext_Equals()
        {
            Func<string, long, string> func1 = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext t1 = func1.CreateExecutionContext();
            Assert.That(t1, Is.EqualTo(t1));
            Assert.That(t1, Is.SameAs(t1));
            
            TwoPhaseExecutionContext t2 = func1.CreateExecutionContext(threads: 1);
            Assert.That(t1, Is.EqualTo(t2));
            Assert.That(t1, Is.Not.SameAs(t2));

            t2 = func1.CreateExecutionContext(threads: 2);
            Assert.That(t1, Is.Not.EqualTo(t2));

            t2 = func1.CreateExecutionContext(args: new object[] { "rihanna", 23L }, threads: 2);
            TwoPhaseExecutionContext t3 = func1.CreateExecutionContext(args: new object[] { "gaga", 23L }, threads: 2);
            Assert.That(t2, Is.Not.EqualTo(t3));

            
            Func<string, long, string> func2 = MemoizerTests.TypicalDatabaseStaticInvocation;
            TwoPhaseExecutionContext t4 = func2.CreateExecutionContext(threads: 1);
            Assert.That(t1, Is.Not.EqualTo(t4));
            
            t4 = func1.CreateExecutionContext(threads: 2);
            Assert.That(t1, Is.Not.EqualTo(t4));

        }


        [Test]
        public void TwoPhaseExecutionContext_GetExpectedFunctionInvocationCountFor()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext();

            Assert.That(twoPhaseExecutionContext.GetExpectedFunctionInvocationCountFor(myFunc), Is.EqualTo(TwoPhaseExecutionContext.NUMBER_OF_WARM_UP_ITERATIONS + 1));
            twoPhaseExecutionContext.Having(iterations: 3);
            Assert.That(twoPhaseExecutionContext.GetExpectedFunctionInvocationCountFor(myFunc), Is.EqualTo(TwoPhaseExecutionContext.NUMBER_OF_WARM_UP_ITERATIONS + 3));
            twoPhaseExecutionContext.Having(iterations: 2);
            Assert.That(twoPhaseExecutionContext.GetExpectedFunctionInvocationCountFor(myFunc), Is.EqualTo(TwoPhaseExecutionContext.NUMBER_OF_WARM_UP_ITERATIONS + 2));

            const int THREADS = 5000;
            twoPhaseExecutionContext = myFunc.CreateExecutionContext(threads: THREADS);

            Assert.That(twoPhaseExecutionContext.GetExpectedFunctionInvocationCountFor(myFunc), Is.EqualTo(TwoPhaseExecutionContext.NUMBER_OF_WARM_UP_ITERATIONS + 1 * THREADS));
            twoPhaseExecutionContext.Having(iterations: 3);
            Assert.That(twoPhaseExecutionContext.GetExpectedFunctionInvocationCountFor(myFunc), Is.EqualTo(TwoPhaseExecutionContext.NUMBER_OF_WARM_UP_ITERATIONS + 3 * THREADS));
            twoPhaseExecutionContext.Having(iterations: 2);
            Assert.That(twoPhaseExecutionContext.GetExpectedFunctionInvocationCountFor(myFunc), Is.EqualTo(TwoPhaseExecutionContext.NUMBER_OF_WARM_UP_ITERATIONS + 2 * THREADS));

            twoPhaseExecutionContext = myFunc.CreateExecutionContext(threads: THREADS, memoize: true);

            Assert.That(twoPhaseExecutionContext.GetExpectedFunctionInvocationCountFor(myFunc), Is.EqualTo(TwoPhaseExecutionContext.NUMBER_OF_WARM_UP_ITERATIONS + 1));
            twoPhaseExecutionContext.Having(iterations: 3);
            Assert.That(twoPhaseExecutionContext.GetExpectedFunctionInvocationCountFor(myFunc), Is.EqualTo(TwoPhaseExecutionContext.NUMBER_OF_WARM_UP_ITERATIONS + 1));
            twoPhaseExecutionContext.Having(iterations: 2);
            Assert.That(twoPhaseExecutionContext.GetExpectedFunctionInvocationCountFor(myFunc), Is.EqualTo(TwoPhaseExecutionContext.NUMBER_OF_WARM_UP_ITERATIONS + 1));
        }


        [Test]
        public void TwoPhaseExecutionContext_MinimumExpextedLatencyInMilliseconds()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext();
            Assert.That(twoPhaseExecutionContext.MinimumExpectedLatencyInMilliseconds, Is.EqualTo(default(long)));
            twoPhaseExecutionContext.Execute();
            Assert.That(twoPhaseExecutionContext.MinimumExpectedLatencyInMilliseconds, Is.GreaterThan(MemoizerTests.NETWORK_RESPONSE_LATENCY_IN_MILLIS));
        }


        [Test]
        public void TwoPhaseExecutionContext_MaximumExpectedLatencyInMilliseconds()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext();
            Assert.That(twoPhaseExecutionContext.MaximumExpectedLatencyInMilliseconds, Is.EqualTo(default(long)));
            twoPhaseExecutionContext.Execute();
            Assert.That(twoPhaseExecutionContext.MaximumExpectedLatencyInMilliseconds, Is.GreaterThan(MemoizerTests.NETWORK_RESPONSE_LATENCY_IN_MILLIS));
            Assert.That(twoPhaseExecutionContext.MaximumExpectedLatencyInMilliseconds, Is.LessThan(MemoizerTests.NETWORK_RESPONSE_LATENCY_IN_MILLIS + 200L));
        }


        [Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = "TwoPhaseExecutionContext parameter cannot be null", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_MergingWithNullShouldTerminateExecution()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext();
            twoPhaseExecutionContext.And(null);
        }


        [Test]
        public void TwoPhaseExecutionContext_MergingWithItselfShouldGiveTheSameObject()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext();
            TwoPhaseExecutionContext mergedContext = twoPhaseExecutionContext.And(twoPhaseExecutionContext);

            Assert.That(mergedContext.NumberOfIterations, Is.EqualTo(twoPhaseExecutionContext.NumberOfIterations));
            Assert.That(mergedContext.NumberOfFunctions, Is.EqualTo(twoPhaseExecutionContext.NumberOfFunctions));
            Assert.That(mergedContext.NumberOfConcurrentWorkerThreads, Is.EqualTo(twoPhaseExecutionContext.NumberOfConcurrentWorkerThreads));
            Assert.That(mergedContext, Is.EqualTo(twoPhaseExecutionContext));
            Assert.That(mergedContext, Is.SameAs(twoPhaseExecutionContext));
        }


        [Test]
        public void TwoPhaseExecutionContext_MergingWithEqualObjectShouldGiveTheSameObject()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext1 = myFunc.CreateExecutionContext();
            TwoPhaseExecutionContext twoPhaseExecutionContext2 = myFunc.CreateExecutionContext();
            TwoPhaseExecutionContext mergedContext = twoPhaseExecutionContext1.And(twoPhaseExecutionContext2);

            Assert.That(mergedContext, Is.EqualTo(twoPhaseExecutionContext1));
            Assert.That(mergedContext, Is.EqualTo(twoPhaseExecutionContext2));
            Assert.That(mergedContext, Is.SameAs(twoPhaseExecutionContext1));
            Assert.That(mergedContext, Is.Not.SameAs(twoPhaseExecutionContext2));

            mergedContext = twoPhaseExecutionContext2.And(twoPhaseExecutionContext1);

            Assert.That(mergedContext, Is.EqualTo(twoPhaseExecutionContext1));
            Assert.That(mergedContext, Is.EqualTo(twoPhaseExecutionContext2));
            Assert.That(mergedContext, Is.Not.SameAs(twoPhaseExecutionContext1));
            Assert.That(mergedContext, Is.SameAs(twoPhaseExecutionContext2));
        }







        [Test, ExpectedException(typeof(Exception), ExpectedMessage = "No TwoPhaseExecutionContextResults are available", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_ZeroIterationParameter(
            [Values(0)] int numberOfIterations,
            [Values(1)] int numberOfConcurrentWorkerThreads)
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext(iterations: numberOfIterations, threads: numberOfConcurrentWorkerThreads);
            // TODO: new API...
            ///*TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet =*/ twoPhaseExecutionContext.Execute("Jabadabadoo", 888L); // Hva med en liste med parametre, forskjellige for hver concurrent thread
            ////Assert.That(twoPhaseExecutionContextResultSet[0, 0], Is.Null);
        }


        [Test, ExpectedException(typeof(Exception), ExpectedMessage = "No FuncTwoPhaseExecutorThreads (worker threads) are available", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_ZeroNumberOfConcurrentWorkerThreadsParameter(
            [Values(1)] int numberOfIterations,
            [Values(0)] int numberOfConcurrentWorkerThreads)
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext(iterations: numberOfIterations, threads: numberOfConcurrentWorkerThreads);
            // TODO: new API...
            ///*TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet =*/ twoPhaseExecutionContext.Execute("Jabadabadoo", 888L); // Hva med en liste med parametre, forskjellige for hver concurrent thread
            ////Assert.That(twoPhaseExecutionContextResultSet[0, 0], Is.Null);
        }


        [Test, ExpectedException(typeof(Exception), ExpectedMessage = "Result set contains only 1 iteration... Really no point is asking for iteration #2 (zero-based) then, is it?", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_OutOfBoundsErrorMessages1()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext(iterations: 1, threads: 2);
            // TODO: new API...
            ///*TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet =*/ twoPhaseExecutionContext.Execute("Jabadabadoo", 888L); // Hva med en liste med parametre, forskjellige for hver concurrent thread
            ////var nonExistingIterationResultSet = twoPhaseExecutionContextResultSet[1, 0];
        }


        [Test, ExpectedException(typeof(Exception), ExpectedMessage = "Result set contains only 3 iterations... Really no point is asking for iteration #20 (zero-based) then, is it?", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_OutOfBoundsErrorMessages2()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext(iterations: 3, threads: 2);
            // TODO: new API...
            ///*TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet = */twoPhaseExecutionContext.Execute("Jabadabadoo", 888L); // Hva med en liste med parametre, forskjellige for hver concurrent thread
            ////var nonExistingIterationResultSet = twoPhaseExecutionContextResultSet[19, 0];
        }


        [Test, ExpectedException(typeof(Exception), ExpectedMessage = "Result set contains only 1 worker thread... Really no point is asking for thread #2 (zero-based) then, is it?", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_OutOfBoundsErrorMessages3()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext(iterations: 2, threads: 1);
            // TODO: new API...
            ///*TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet = */twoPhaseExecutionContext.Execute("Jabadabadoo", 888L); // Hva med en liste med parametre, forskjellige for hver concurrent thread
            ////var nonExistingWorkerThreadResultSet = twoPhaseExecutionContextResultSet[1, 1];
        }


        [Test, ExpectedException(typeof(Exception), ExpectedMessage = "Result set contains only 10 worker threads... Really no point is asking for thread #100 (zero-based) then, is it?", MatchType = MessageMatch.Exact)]
        public void TwoPhaseExecutionContext_OutOfBoundsErrorMessages4()
        {
            Func<string, long, string> myFunc = MemoizerTests.ReallySlowNetworkStaticInvocation;
            TwoPhaseExecutionContext twoPhaseExecutionContext = myFunc.CreateExecutionContext(iterations: 3, threads: 10);
            // TODO: new API...
            ///*TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet =*/ twoPhaseExecutionContext.Execute("Jabadabadoo", 888L); // Hva med en liste med parametre, forskjellige for hver concurrent thread
            ////var nonExistingWorkerThreadResultSet = twoPhaseExecutionContextResultSet[1, 99];
        }
        #endregion
    }
}
