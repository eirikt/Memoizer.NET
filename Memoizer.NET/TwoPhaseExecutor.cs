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
using System.Text;
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
        /// The worker thread action.
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
    public class ActionTwoPhaseExecutorThread<TParam1> : AbstractTwoPhaseExecutorThread
    {
        static int TASK_COUNTER;

        public ActionTwoPhaseExecutorThread(Action<TParam1> action, TParam1 arg1, Barrier barrier, bool instrumentation = false)
            : base(barrier, instrumentation)
        {
            TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
            ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);
            Action = () => action.Invoke(arg1);
            if (Instrumentation)
                Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]");
        }
    }


    public class FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult> : AbstractTwoPhaseExecutorThread
    {
        static int TASK_COUNTER;
        readonly Func<TParam1, TParam2, TResult> function;
        public TResult Result { get; private set; }
        internal FuncTwoPhaseExecutorThread(Func<TParam1, TParam2, TResult> function, TParam1 arg1, TParam2 arg2, Barrier barrier, bool instrumentation = false)
            : base(barrier, instrumentation)
        {
            this.function = function;
            TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
            ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);
            Action = () => Result = this.function.Invoke(arg1, arg2);
            if (Instrumentation) { Console.WriteLine(GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]"); }
        }
    }


    public static partial class FuncExtensionMethods
    {
        // TODO: add boolean report flags: sequential/concurrent, memoized/non-memoized, instrumentation/quiet,  ...
        public static TwoPhaseExecutionContext<TParam1, TParam2, TResult> CreateExecutionContext<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized, int numberOfIterations, int numberOfConcurrentThreadsWitinhEachIteration, bool instrumentation = false)
        {
            return new TwoPhaseExecutionContext<TParam1, TParam2, TResult>(functionToBeMemoized, numberOfIterations, numberOfConcurrentThreadsWitinhEachIteration);
        }
    }


    public class TwoPhaseExecutionContext<TParam1, TParam2, TResult>
    {
        readonly Func<TParam1, TParam2, TResult> functionToBeExecuted;
        readonly int numberOfIterations;
        readonly int numberOfConcurrentThreadsWitinhEachIteration;
        readonly bool instrumentation;

        public TwoPhaseExecutionContext(Func<TParam1, TParam2, TResult> functionToBeExecuted, int numberOfIterations, int numberOfConcurrentThreadsWitinhEachIteration, bool instrumentation = false)
        {
            if (numberOfIterations < 0) { throw new ArgumentException("Number-of-iteration parameter ('numberOfIterations') cannot be a negative number"); }
            if (numberOfConcurrentThreadsWitinhEachIteration < 0) { throw new ArgumentException("Number-of-worker-threads parameter ('numberOfConcurrentThreadsWitinhEachIteration') cannot be a negative number"); }
            this.functionToBeExecuted = functionToBeExecuted;
            this.numberOfIterations = numberOfIterations;
            this.numberOfConcurrentThreadsWitinhEachIteration = numberOfConcurrentThreadsWitinhEachIteration;
            this.instrumentation = instrumentation;
        }

        internal int NumberOfIterations { get { return this.numberOfIterations; } }

        internal int NumberOfConcurrentWorkerThreads { get { return this.numberOfConcurrentThreadsWitinhEachIteration; } }

        public long LatencyInMilliseconds { get; private set; }

        public long CalculateOverheadInMillisecondsFor(int numberOfConcurrentWorkerThreads)
        {
            double threadContentionFactor;
            if (LatencyInMilliseconds > 200)
            {
                if (numberOfConcurrentWorkerThreads == 1) { threadContentionFactor = 10.0d; } // 10 ms overhead
                else if (numberOfConcurrentWorkerThreads < 20) { threadContentionFactor = 3.0d; }
                else if (numberOfConcurrentWorkerThreads < 50) { threadContentionFactor = 2.0d; }
                else if (numberOfConcurrentWorkerThreads < 200) { threadContentionFactor = 1.6d; }
                else if (numberOfConcurrentWorkerThreads < 500) { threadContentionFactor = 1.2d; }
                else { threadContentionFactor = 1.05d; }
            }
            else
            {
                if (numberOfConcurrentWorkerThreads == 1) { threadContentionFactor = 50.0d; } // 50 ms overhead
                else if (numberOfConcurrentWorkerThreads < 20) { threadContentionFactor = 10.0d; }
                else if (numberOfConcurrentWorkerThreads < 50) { threadContentionFactor = 5.0d; }
                else if (numberOfConcurrentWorkerThreads < 200) { threadContentionFactor = 2.0d; }
                else if (numberOfConcurrentWorkerThreads < 500) { threadContentionFactor = 1.2d; }
                else { threadContentionFactor = 1.05d; }
            }
            return Convert.ToInt64(numberOfConcurrentWorkerThreads * threadContentionFactor); ;
        }

        public TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult> Execute(TParam1 arg1, TParam2 arg2)
        {
            // First, dry-running function to measure latency...
            StopWatch stopWatch = new StopWatch();
            this.functionToBeExecuted.Invoke(default(TParam1), default(TParam2));
            LatencyInMilliseconds = stopWatch.DurationInMilliseconds;

            TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult> twoPhaseExecutionContextResultSet = new TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult>(this);
            twoPhaseExecutionContextResultSet.StopWatch.Start();
            for (int i = 0; i < this.numberOfIterations; ++i)
            {
                TwoPhaseExecutor twoPhaseExecutor = new TwoPhaseExecutor(this.numberOfConcurrentThreadsWitinhEachIteration, this.instrumentation);

                FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[] workerThreads = new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[twoPhaseExecutor.NumberOfParticipants];
                for (int j = 0; j < twoPhaseExecutor.NumberOfParticipants; ++j)
                    workerThreads[j] = new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>(this.functionToBeExecuted, arg1, arg2, twoPhaseExecutor.Barrier, this.instrumentation);

                for (int j = 0; j < twoPhaseExecutor.NumberOfParticipants; ++j) { workerThreads[j].Start(); }
                twoPhaseExecutor.Start();

                TwoPhaseExecutionContextResult<TParam1, TParam2, TResult> twoPhaseExecutionContextResult = new TwoPhaseExecutionContextResult<TParam1, TParam2, TResult>(twoPhaseExecutor.NumberOfParticipants);
                for (int j = 0; j < twoPhaseExecutor.NumberOfParticipants; ++j) { twoPhaseExecutionContextResult.WorkerThread[j] = workerThreads[j]; }

                twoPhaseExecutionContextResultSet.ExecutionResult[i] = twoPhaseExecutionContextResult;
            }
            twoPhaseExecutionContextResultSet.StopWatch.Stop();

            return twoPhaseExecutionContextResultSet;
        }


        public void Test(TParam1 arg1, TParam2 arg2)
        {
            // Execute context
            TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult> twoPhaseExecutionContextResultSet = Execute(arg1, arg2);

            // Write report
            Console.WriteLine(twoPhaseExecutionContextResultSet.GetReport());

            // Check results
            for (int i = 0; i < this.numberOfIterations; ++i)
            {
                for (int j = 0; j < this.numberOfConcurrentThreadsWitinhEachIteration; ++j)
                {
                    TResult result = twoPhaseExecutionContextResultSet[i, j].Result;
                    if (typeof(TResult) == typeof(String))
                    {
                        try
                        {
                            if (!((result as string).Contains(arg1.ToString()) && (result as string).Contains(arg2.ToString())))
                            {
                                throw new Exception("Result violation...");
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Internal casting...");
                        }
                    }
                }

                // Check latency
                if (!(twoPhaseExecutionContextResultSet.StopWatch.DurationInMilliseconds >= this.numberOfIterations * LatencyInMilliseconds &&
                    twoPhaseExecutionContextResultSet.StopWatch.DurationInMilliseconds <= this.numberOfIterations * LatencyInMilliseconds + CalculateOverheadInMillisecondsFor(this.numberOfConcurrentThreadsWitinhEachIteration)))

                    throw new Exception("Latency violation...");

            }
        }
    }


    public class TwoPhaseExecutionContextResult<TParam1, TParam2, TResult>
    {
        readonly FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[] funcTwoPhaseExecutorThreads;
        internal TwoPhaseExecutionContextResult(int numberOfParticipants) { this.funcTwoPhaseExecutorThreads = new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[numberOfParticipants]; }
        internal FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[] WorkerThread { get { return this.funcTwoPhaseExecutorThreads; } }
    }


    public class TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult>
    {
        readonly TwoPhaseExecutionContext<TParam1, TParam2, TResult> twoPhaseExecutionContext;
        readonly TwoPhaseExecutionContextResult<TParam1, TParam2, TResult>[] twoPhaseExecutionContextResults;

        public StopWatch StopWatch { get; private set; }

        internal TwoPhaseExecutionContextResultSet(TwoPhaseExecutionContext<TParam1, TParam2, TResult> twoPhaseExecutionContext)
        {
            this.twoPhaseExecutionContext = twoPhaseExecutionContext;
            this.twoPhaseExecutionContextResults = new TwoPhaseExecutionContextResult<TParam1, TParam2, TResult>[this.twoPhaseExecutionContext.NumberOfIterations];
            this.StopWatch = new StopWatch();
        }

        internal TwoPhaseExecutionContextResult<TParam1, TParam2, TResult>[] ExecutionResult
        {
            get { return this.twoPhaseExecutionContextResults; }
        }

        public FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult> this[int iterationIndex, int concurrentThreadIndex]
        {
            get
            {
                if (this.twoPhaseExecutionContextResults == null || this.twoPhaseExecutionContextResults.Length < 1)
                    throw new Exception("No TwoPhaseExecutionContextResults are available");
                if (iterationIndex > this.twoPhaseExecutionContextResults.Length - 1 && this.twoPhaseExecutionContextResults.Length == 1)
                    throw new Exception("Result set contains only 1 iteration... Really no point is asking for iteration #2 (zero-based) then, is it?");
                if (iterationIndex > this.twoPhaseExecutionContextResults.Length - 1)
                    throw new Exception("Result set contains only " + this.twoPhaseExecutionContextResults.Length + " iterations... Really no point is asking for iteration #" + (iterationIndex + 1) + " (zero-based) then, is it?");

                TwoPhaseExecutionContextResult<TParam1, TParam2, TResult> twoPhaseExecutionContextResult = this.twoPhaseExecutionContextResults[iterationIndex];

                FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[] funcTwoPhaseExecutorThreads = twoPhaseExecutionContextResult.WorkerThread;
                if (funcTwoPhaseExecutorThreads == null || funcTwoPhaseExecutorThreads.Length < 1)
                    throw new Exception("No FuncTwoPhaseExecutorThreads (worker threads) are available");
                if (concurrentThreadIndex > funcTwoPhaseExecutorThreads.Length - 1 && funcTwoPhaseExecutorThreads.Length == 1)
                    throw new Exception("Result set contains only 1 worker thread... Really no point is asking for thread #2 (zero-based) then, is it?");
                if (concurrentThreadIndex > funcTwoPhaseExecutorThreads.Length - 1)
                    throw new Exception("Result set contains only " + funcTwoPhaseExecutorThreads.Length + " worker threads... Really no point is asking for thread #" + (concurrentThreadIndex + 1) + " (zero-based) then, is it?");

                return twoPhaseExecutionContextResult.WorkerThread[concurrentThreadIndex];
            }
        }

        public string GetReport()
        {
            StringBuilder reportBuilder = new StringBuilder();
            
            reportBuilder.Append(this.twoPhaseExecutionContext.NumberOfIterations + " rounds with " + this.twoPhaseExecutionContext.NumberOfConcurrentWorkerThreads + " concurrent, identical, memoized method invocations:");
            
            reportBuilder.Append(" having " + this.twoPhaseExecutionContext.LatencyInMilliseconds + " ms method latency - took " + this.StopWatch.DurationInMilliseconds + " ms | " + this.StopWatch.DurationInTicks + " ticks");

            reportBuilder.Append(" (should take ");
            reportBuilder.Append(this.twoPhaseExecutionContext.NumberOfIterations * this.twoPhaseExecutionContext.LatencyInMilliseconds));
            reportBuilder.Append(" <= ");
            reportBuilder.Append(StopWatch.DurationInMilliseconds);
            reportBuilder.Append(" =< ");
            reportBuilder.Append(((this.twoPhaseExecutionContext.NumberOfIterations * this.twoPhaseExecutionContext.LatencyInMilliseconds) + this.twoPhaseExecutionContext.CalculateOverheadInMillisecondsFor(this.twoPhaseExecutionContext.NumberOfConcurrentWorkerThreads)));
            reportBuilder.Append(")");
            
            return reportBuilder.ToString();
        }
    }


    public class StopWatch
    {
        long startTime, stopTime;
        public StopWatch() { Start(); }
        public long DurationInTicks
        {
            get
            {
                if (this.stopTime != default(long)) { return this.stopTime - this.startTime; }
                return DateTime.Now.Ticks - startTime;
            }
        }
        public long DurationInMilliseconds { get { return DurationInTicks / TimeSpan.TicksPerMillisecond; } }
        public void Start()
        {
            //Console.WriteLine("Starting stop watch ...NOW!");
            this.startTime = DateTime.Now.Ticks;
        }
        public void Stop()
        {
            this.stopTime = DateTime.Now.Ticks;
            //Console.WriteLine("Stop watch ...Stopped NOW!");
        }
    }
    #endregion
}
