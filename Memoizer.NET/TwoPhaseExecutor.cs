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

namespace Memoizer.NET
{
    /// <remarks>
    /// A class for synchronized execution of a number of worker threads.
    /// All participating worker threads must be a <code>AbstractTwoPhaseExecutorThread</code>-derived instance. 
    /// <p/>
    /// ... 
    /// </remarks>
    public sealed class TwoPhaseExecutor
    {
        /// <summary>
        /// The thread barrier.
        /// Must be distributed to all participating worker threads.
        /// </summary>
        public Barrier Barrier { get; private set; }

        /// <summary>
        /// The overall number of participating worker threads in this two-phase execution. 
        /// </summary>
        public int NumberOfParticipants { get; private set; }

        /// <summary>
        /// If set to <code>true</code>, info will be written to console.
        /// Default is <code>false</code>.
        /// </summary>
        public bool Instrumentation { get; set; }

        public TwoPhaseExecutor(int numberOfParticipants, bool instrumentation = false)
        {
            if (numberOfParticipants < 0) { throw new ArgumentException("Number of participating worker threads cannot be less than zero"); }
            if (numberOfParticipants < 1) { Console.WriteLine("No worker threads are attending..."); }

            NumberOfParticipants = numberOfParticipants;
            Instrumentation = instrumentation;
            if (Instrumentation)
                Console.WriteLine("Phase 0: Creating barrier, managing at most " + NumberOfParticipants + " phased worker threads, + 1 main thread");
            Barrier = new Barrier((NumberOfParticipants + 1), barrier =>
            {
                if (Instrumentation)
                {
                    switch (barrier.CurrentPhaseNumber)
                    {
                        case 0:
                            Console.WriteLine("Phase 1: releasing all worker threads simultaneously");
                            break;

                        case 1:
                            Console.WriteLine("Phase 2: all worker threads have finished; cleaning up and terminating all threads");
                            break;

                        default:
                            throw new NotSupportedException("Unknown phase (" + barrier.CurrentPhaseNumber + ") entered...");
                    }
                }
            });
        }

        public void Start()
        {
            if (Instrumentation)
            {
                Console.WriteLine(NumberOfParticipants < 1
                    ? "Main thread: Arriving at 1st barrier rendevouz - releasing it immediately as it is the only participating thread..."
                    : "Main thread: Arriving at 1st barrier rendevouz - probably as one of the last ones, releasing all worker threads simultaneously when all have reach 1st barrier...");
            }
            Barrier.SignalAndWait(Timeout.Infinite);

            if (Instrumentation)
            {
                Console.WriteLine(NumberOfParticipants < 1
                    ? "Main thread: Arriving at 1st barrier rendevouz - continuing immediately as it is the only participating thread..."
                    : "Main thread: Arriving at 2nd barrier rendevouz - probably as one of the first ones, waiting for all worker threads to complete...");
            }
            Barrier.SignalAndWait(Timeout.Infinite);
        }
    }


    public static class BarrierExtensionMethods
    {
        public static string GetInfo(this Barrier barrier)
        {
            if (barrier == null) { throw new ArgumentException("Barrier parameter cannot null"); }
            return "Barrier phase is " + barrier.CurrentPhaseNumber + ", remaining participants are " + (barrier.ParticipantsRemaining) + " of a total of " + (barrier.ParticipantCount - 1) + " (plus main thread)";
        }
    }


    /// <remarks>
    /// Abstract base class for <code>TwoPhaseExecutor</code> worker/task threads.
    /// </remarks>
    public abstract class AbstractTwoPhaseExecutorThread
    {
        readonly Thread thread;

        /// <summary>
        /// The thread barrier.
        /// Should be the same instance as created by the main <code>TwoPhaseExecutor</code> object,
        /// fetched and constructor-injected into these class instances.
        /// </summary>
        protected Barrier Barrier { get; private set; }

        static int EXECUTION_INDEX_COUNTER;

        /// <summary>
        /// The overall number/index of executed actions in the two-phase execution.
        /// </summary>
        protected int ExecutionIndex { get; private set; }

        protected static int PARTICIPANT_COUNTER;

        /// <summary>
        /// The overall participant number of this worker/task in the two-phase execution.
        /// Must be set by the concrete subclass implementations.
        /// <br/>
        /// Example of setting:<br/>
        /// <code>ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);</code>
        /// </summary>
        protected int ParticipantNumber { get; set; }

        /// <summary>
        /// The task instance number amongst all workers/tasks <i>of the same type</i>, participating in this the two-phase execution.
        /// Must be set by the concrete subclass implementations.
        /// <br/>
        /// Example of setting:<br/>
        /// <code>
        /// static int TASK_COUNTER;
        /// ...
        /// TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
        /// </code>
        /// </summary>
        protected int TaskNumber { get; set; }

        /// <summary>
        /// The worker/task action.
        /// </summary>
        protected Action Action { get; set; }

        /// <summary>
        /// If set to <code>true</code>, info will be written to console.
        /// Default is <code>false</code>.
        /// </summary>
        public bool Instrumentation { get; set; }

        public string ThreadInfo
        {
            get
            {
                return
                    "OS thread ID=" + this.thread.ManagedThreadId + ", " +
                    "Managed thread ID=" + this.thread.GetHashCode() + "/" + this.thread.ManagedThreadId;
            }
        }

        void GetPhasedAction()
        {
            if (Instrumentation)
                Console.WriteLine("Barrier participating task #" + ParticipantNumber + ": Arriving at 1st barrier rendevouz... [" + Barrier.GetInfo() + "]");
            Barrier.SignalAndWait(Timeout.Infinite);

            ExecutionIndex = Interlocked.Increment(ref EXECUTION_INDEX_COUNTER);
            Action.Invoke();

            if (Instrumentation)
                Console.WriteLine("Barrier participating task #" + ParticipantNumber + ": Arriving at 2nd barrier rendevouz... [" + Barrier.GetInfo() + "]");
            Barrier.SignalAndWait(Timeout.Infinite);
        }

        protected AbstractTwoPhaseExecutorThread(Barrier sharedBarrier, bool instrumentation = false)
        {
            Barrier = sharedBarrier;
            Instrumentation = instrumentation;
            this.thread = new Thread(GetPhasedAction);
        }

        public void Start()
        {
            //if(Instrumentation)
            //Console.WriteLine("Barrier participating task #" + ParticipantNumber + ": " + this.thread.ThreadState);
            this.thread.Start();
            //if(Instrumentation)
            //Console.WriteLine("Barrier participating task #" + ParticipantNumber + ": " + this.thread.ThreadState);
        }
    }


    /// <remarks>
    /// Example concrete implementation of an <code>AbstractTwoPhaseExecutorThread</code> worker/task thread.
    /// E.g. do notice how to correctly get hold of the <code>TaskNumber</code> and <code>ParticipantNumber</code> properties.
    /// </remarks>
    public class TrivialTask : AbstractTwoPhaseExecutorThread
    {
        static int TASK_COUNTER;

        public TrivialTask(Barrier barrier)
            : base(barrier, true)
        {
            TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
            ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);
            Action = () => Console.WriteLine("Barrier participant #" + ParticipantNumber + " [invocation #" + ExecutionIndex + "] [" + ThreadInfo + "]");
            if (Instrumentation)
                Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]");
        }
    }

    #region New TwoPhaseExecutor API
    public static partial class FuncExtensionMethods
    {
        public static TwoPhaseExecutionContext<TParam1, TParam2, TResult> CreateExecutionContext<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized, int numberOfIterations, int numberOfConcurrentThreadsWitinhEachIteration)
        {
            return new TwoPhaseExecutionContext<TParam1, TParam2, TResult>(functionToBeMemoized, numberOfIterations, numberOfConcurrentThreadsWitinhEachIteration);
        }
    }


    public class TwoPhaseExecutionContext<TParam1, TParam2, TResult>
    {
        readonly Func<TParam1, TParam2, TResult> functionToBeExecuted;
        readonly int numberOfIterations;
        readonly int numberOfConcurrentThreadsWitinhEachIteration;

        public TwoPhaseExecutionContext(Func<TParam1, TParam2, TResult> functionToBeExecuted, int numberOfIterations, int numberOfConcurrentThreadsWitinhEachIteration)
        {
            if (numberOfIterations < 0) { throw new ArgumentException("Number-of-iteration parameter ('numberOfIterations') cannot be a negative number"); }
            if (numberOfConcurrentThreadsWitinhEachIteration < 0) { throw new ArgumentException("Number-of-worker-threads parameter ('numberOfConcurrentThreadsWitinhEachIteration') cannot be a negative number"); }
            this.functionToBeExecuted = functionToBeExecuted;
            this.numberOfIterations = numberOfIterations;
            this.numberOfConcurrentThreadsWitinhEachIteration = numberOfConcurrentThreadsWitinhEachIteration;
        }


        public TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult> Execute(TParam1 arg1, TParam2 arg2)
        {
            TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult> twoPhaseExecutionContextResultSet = new TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult>(this.numberOfIterations);
            twoPhaseExecutionContextResultSet.StopWatch.Start();

            for (int i = 0; i < this.numberOfIterations; ++i)
            {
                // Arrange
                TwoPhaseExecutor twoPhaseExecutor = new TwoPhaseExecutor(this.numberOfConcurrentThreadsWitinhEachIteration);
                twoPhaseExecutor.Instrumentation = true;

                FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[] workerThread = new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[twoPhaseExecutor.NumberOfParticipants];
                for (int j = 0; j < twoPhaseExecutor.NumberOfParticipants; ++j)
                {
                    workerThread[j] = new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>(this.functionToBeExecuted, arg1, arg2, twoPhaseExecutor.Barrier);
                    workerThread[j].Instrumentation = true;
                }

                // Act
                for (int j = 0; j < twoPhaseExecutor.NumberOfParticipants; ++j)
                {
                    workerThread[j].Start();
                }
                twoPhaseExecutor.Start();


                //// Assert
                //for (int j = 0; j < numberOfConcurrentTasks; ++j)

                //    Assert.That(tasks[j].Result, Is.EqualTo(METHOD_RESPONSE_ELEMENT + "MultiThreadedMemoizedInvocation_Memoizer" + 15L));

                TwoPhaseExecutionContextResult<TParam1, TParam2, TResult> twoPhaseExecutionContextResult = new TwoPhaseExecutionContextResult<TParam1, TParam2, TResult>(twoPhaseExecutor.NumberOfParticipants);
                for (int j = 0; j < twoPhaseExecutor.NumberOfParticipants; ++j)
                {
                    twoPhaseExecutionContextResult.WorkerThread[j] = workerThread[j];
                }
                //twoPhaseExecutor.Start();

                twoPhaseExecutionContextResultSet.ExecutionResult[i] = twoPhaseExecutionContextResult;

            }
            //long durationInMilliseconds = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            //Assert.That(durationInMilliseconds, Is.InRange(NETWORK_RESPONSE_LATENCY_IN_MILLIS, 2600L));
            //Console.WriteLine(
            //    "MultiThreadedMemoizedInvocation_Memoizer: " + NUMBER_OF_ITERATIONS + " rounds with " + numberOfConcurrentTasks + " concurrent, identical, memoized method invocations:" +
            //    " " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " ms non-cached method latency took " + durationInMilliseconds + " ms" +
            //    " (should take " + NETWORK_RESPONSE_LATENCY_IN_MILLIS + " > " + durationInMilliseconds + " > 2600 ms).");

            twoPhaseExecutionContextResultSet.StopWatch.Stop();

            return twoPhaseExecutionContextResultSet;
        }
    }


    public class FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult> : AbstractTwoPhaseExecutorThread
    {
        static int TASK_COUNTER;

        readonly Func<TParam1, TParam2, TResult> function;

        public TResult Result { get; private set; }

        internal FuncTwoPhaseExecutorThread(Func<TParam1, TParam2, TResult> function, TParam1 arg1, TParam2 arg2, Barrier barrier)
            : base(barrier)
        {
            this.TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
            this.ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);
            this.function = function;
            this.Action = () => this.Result = this.function.Invoke(arg1, arg2);

            if (Instrumentation)
                Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]");
        }
    }



    public class TwoPhaseExecutionContextResult<TParam1, TParam2, TResult>
    {
        readonly FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[] funcTwoPhaseExecutorThreads;


        internal TwoPhaseExecutionContextResult(int numberOfParticipants)
        {
            this.funcTwoPhaseExecutorThreads = new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[numberOfParticipants];
        }


        internal FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[] WorkerThread
        {
            get
            {
                return this.funcTwoPhaseExecutorThreads;
            }
        }


        public TResult Result
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }



    public class TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult>
    {
        public StopWatch StopWatch { get; private set; }

        readonly TwoPhaseExecutionContextResult<TParam1, TParam2, TResult>[] twoPhaseExecutionContextResults;

        internal TwoPhaseExecutionContextResultSet(int numberOfIterations)
        {
            this.StopWatch = new StopWatch();
            this.twoPhaseExecutionContextResults = new TwoPhaseExecutionContextResult<TParam1, TParam2, TResult>[numberOfIterations];
        }


        internal TwoPhaseExecutionContextResult<TParam1, TParam2, TResult>[] ExecutionResult
        {
            get { return this.twoPhaseExecutionContextResults; }
            //set { throw new NotImplementedException(); }
        }

        public FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult> this[int iterationIndex, int concurrentThreadIndex]
        {
            get
            {
                if (this.twoPhaseExecutionContextResults == null || this.twoPhaseExecutionContextResults.Length < 1)
                {
                    //return null;
                    throw new Exception("No TwoPhaseExecutionContextResult results are available...");
                }

                if (this.twoPhaseExecutionContextResults.Length < iterationIndex+1)
                {
                    throw new Exception("Result set contains only "+this.twoPhaseExecutionContextResults.Length+" iterations... No point is asking for iteration "+iterationIndex+" then, is it?");
                }





                TwoPhaseExecutionContextResult<TParam1, TParam2, TResult> twoPhaseExecutionContextResult = this.twoPhaseExecutionContextResults[iterationIndex];

                FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[] funcTwoPhaseExecutorThreads = twoPhaseExecutionContextResult.WorkerThread;
                if (funcTwoPhaseExecutorThreads == null || funcTwoPhaseExecutorThreads.Length < 1)
                {
                    return null;
                }
                FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult> funcTwoPhaseExecutorThread = twoPhaseExecutionContextResult.WorkerThread[concurrentThreadIndex];


                return funcTwoPhaseExecutorThread;

            }
        }
    }


    public class StopWatch
    {
        long startTime, stopTime;

        public long DurationInMilliseconds
        {
            get
            {
                if (this.stopTime != default(long)) { return (this.stopTime - this.startTime) / TimeSpan.TicksPerMillisecond; }
                return (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            }
        }
        internal void Start() { this.startTime = DateTime.Now.Ticks; }
        internal void Stop() { this.stopTime = DateTime.Now.Ticks; }
    }
    #endregion
}
