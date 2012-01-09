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
using System.Linq;
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
        public Barrier Barrier { get; internal set; }

        /// <summary>
        /// The number of participating worker threads in this two-phase execution. 
        /// </summary>
        public int NumberOfParticipatingWorkerThreads { get { return Barrier.ParticipantCount - 1; } }

        /// <summary>
        /// If set to <code>true</code>, info will be written to console.
        /// Default is <code>false</code>.
        /// </summary>
        public bool Instrumentation { get; set; }

        public bool IsMerged { get; set; }

        public TwoPhaseExecutor(int numberOfParticipants, bool instrumentation = false)
        {
            if (numberOfParticipants < 0) { throw new ArgumentException("Number of participating worker threads cannot be less than zero"); }
            if (numberOfParticipants < 1) { Console.WriteLine("No worker threads are attending..."); }

            //NumberOfParticipants = numberOfParticipants;
            Instrumentation = instrumentation;
            if (Instrumentation)
                Console.WriteLine("Phase 0: Creating barrier, managing at most " + numberOfParticipants + " phased worker threads, + 1 main thread");
            Barrier = new Barrier((numberOfParticipants + 1), barrier =>
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
                Console.WriteLine(NumberOfParticipatingWorkerThreads < 1
                    ? "Main thread: Arriving at 1st barrier rendevouz - releasing it immediately as it is the only participating thread..."
                    : "Main thread: Arriving at 1st barrier rendevouz - probably as one of the last ones, releasing all worker threads simultaneously when all have reach 1st barrier...");
            }
            Barrier.SignalAndWait(Timeout.Infinite);

            if (Instrumentation)
            {
                Console.WriteLine(NumberOfParticipatingWorkerThreads < 1
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
        protected internal Barrier Barrier { get; internal set; }

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

        // The System.Threading.ThreadStart delegate 
        void GetPhasedAction()
        {
            if (Instrumentation)
                Console.WriteLine("Barrier participating task #" + ParticipantNumber + ": Arriving at 1st barrier rendevouz... [" + Barrier.GetInfo() + "]");

            try
            {
                Barrier.SignalAndWait(Timeout.Infinite);
            }
            catch (ThreadAbortException e)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.ExceptionState);
            }

            ExecutionIndex = Interlocked.Increment(ref EXECUTION_INDEX_COUNTER);
            Action.Invoke();

            if (Instrumentation)
                Console.WriteLine("Barrier participating task #" + ParticipantNumber + ": Arriving at 2nd barrier rendevouz... [" + Barrier.GetInfo() + "]");
            try
            {
                Barrier.SignalAndWait(Timeout.Infinite);
            }
            catch (ThreadAbortException e)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.ExceptionState);
            }

            //// Working...? / Necessary...?
            //if (Instrumentation)
            //    Console.WriteLine(this.thread.ThreadState);
            //this.thread.Abort();
            //this.thread = null;
        }

        protected AbstractTwoPhaseExecutorThread(Barrier sharedBarrier, bool instrumentation = false)
        {
            Barrier = sharedBarrier;
            Instrumentation = instrumentation;
            this.thread = new Thread(GetPhasedAction);
        }

        public void Start()
        {
            if (Instrumentation) { Console.WriteLine("Barrier participating task #" + ParticipantNumber + ": " + this.thread.ThreadState); }
            //if (this.thread.ThreadState == ThreadState.Running)
            //{
            //    this.thread.Abort();
            //    this.thread = null;
            //    this.thread = new Thread(GetPhasedAction);
            //}
            //if (this.thread.ThreadState == ThreadState.WaitSleepJoin)
            //{
            //    this.thread.Interrupt();
            //    this.thread.Abort();
            //    this.thread = null;
            //    this.thread = new Thread(GetPhasedAction);
            //}
            //if (this.thread.ThreadState == ThreadState.Stopped)
            //{
            //    this.thread = null;
            //    this.thread = new Thread(GetPhasedAction);
            //}
            this.thread.Start();
            if (Instrumentation) { Console.WriteLine("Barrier participating task #" + ParticipantNumber + ": " + this.thread.ThreadState); }
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
        public TParam1 Arg1 { get; set; }
        public TParam2 Arg2 { get; set; }
        public bool IsMemoizerClearingThread { get; private set; }

        internal FuncTwoPhaseExecutorThread(Func<TParam1, TParam2, TResult> function,
                                            Barrier barrier = default(Barrier),
                                            bool isMemoizerClearing = false,
                                            bool instrumentation = false)
            : base(barrier, instrumentation)
        {
            this.function = function;
            IsMemoizerClearingThread = isMemoizerClearing;
            TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
            ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);
            if (IsMemoizerClearingThread)
            {
                if (Arg1 != null && Arg1.Equals(default(TParam1)) || Arg2 != null && Arg2.Equals(default(TParam2)))
                    Action = () => this.function.RemoveFromCache(Arg1, Arg2);
                else
                    Action = () => this.function.UnMemoize();
            }
            else
                Action = () => Result = this.function.Invoke(Arg1, Arg2);

            if (Instrumentation) { Console.WriteLine(GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]"); }
        }
    }


    public static partial class FuncExtensionMethods
    {
        public static TwoPhaseExecutionContext<TParam1, TParam2, TResult> CreateExecutionContext<TParam1, TParam2, TResult>(
            this Func<TParam1, TParam2, TResult> functionToBeMemoized,
            int numberOfConcurrentThreadsWitinhEachIteration,
            int numberOfIterations = 1,
            bool concurrent = true,
            bool memoize = false,
            bool memoizerClearingTask = false,
            long functionLatency = default(long),
            bool instrumentation = false)
        {
            return new TwoPhaseExecutionContext<TParam1, TParam2, TResult>(
                functionToBeMemoized,
                numberOfConcurrentThreadsWitinhEachIteration,
                numberOfIterations,
                concurrent,
                memoize,
                memoizerClearingTask,
                functionLatency,
                instrumentation);
        }
    }


    public class TwoPhaseExecutionContext<TParam1, TParam2, TResult>
    {
        // TODO: rewrite to proper internal class
        readonly List<Tuple<int, Func<TParam1, TParam2, TResult>, bool, bool, long, bool>> functionsToBeExecuted;

        readonly int numberOfIterations;

        // TODO: ...and get rid of these, I guess
        readonly bool concurrent;
        readonly bool memoize;
        readonly bool instrumentation;

        TwoPhaseExecutor twoPhaseExecutor;

        IList<FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>> workerThreads;

        internal TwoPhaseExecutionContext(Func<TParam1, TParam2, TResult> functionToBeExecuted,
                                         int numberOfConcurrentThreadsWitinhEachIteration,
                                         int numberOfIterations,
                                         bool concurrent,
                                         bool memoize,
                                         bool isMemoizerClearingTask,
                                         long functionLatencyInMilliseconds,
                                         bool instrumentation)
        {
            if (numberOfIterations < 0) { throw new ArgumentException("Number-of-iteration parameter ('numberOfIterations') cannot be a negative number"); }
            if (numberOfConcurrentThreadsWitinhEachIteration < 0) { throw new ArgumentException("Number-of-worker-threads parameter ('numberOfConcurrentThreadsWitinhEachIteration') cannot be a negative number"); }

            this.functionsToBeExecuted = new List<Tuple<int, Func<TParam1, TParam2, TResult>, bool, bool, long, bool>>
            {
                new Tuple<int, Func<TParam1, TParam2, TResult>, bool, bool, long, bool>
                    //                     1                                2               3                4                            5                     6
                    (numberOfConcurrentThreadsWitinhEachIteration, functionToBeExecuted, memoize, isMemoizerClearingTask, functionLatencyInMilliseconds, instrumentation)
            };

            this.numberOfIterations = numberOfIterations;

            this.concurrent = concurrent;
            this.memoize = memoize;
            LatencyInMilliseconds = functionLatencyInMilliseconds;
            this.instrumentation = instrumentation;
        }

        internal int NumberOfIterations { get { return this.numberOfIterations; } }

        internal int NumberOfConcurrentWorkerThreads { get { return this.functionsToBeExecuted.Aggregate(0, (current, tuple) => current + tuple.Item1); } }

        internal long LatencyInMilliseconds { get; private set; }

        internal bool IsConcurrent { get { return this.concurrent; } }
        internal bool IsMemoized { get { return this.memoize; } }
        internal bool IsInstrumented { get { return this.instrumentation; } }

        internal bool IsMergedWithOneOrMoreSingleThreadContexts { get; set; }

        /// <summary>
        /// Just some empirically calculated contention overhead...
        /// </summary>
        internal long CalculateOverheadInMillisecondsFor()
        {
            double threadContentionFactor;
            if (NumberOfConcurrentWorkerThreads == 1)
                //if (IsMemoized)
                //    threadContentionFactor = 75.0d; // Overhead for memoizer init)
                //else
                threadContentionFactor = 50.0d; // Overhead

            else if (NumberOfConcurrentWorkerThreads <= 4) { threadContentionFactor = 6.0d; }
            else if (NumberOfConcurrentWorkerThreads <= 20) { threadContentionFactor = 4.0d; }
            else if (NumberOfConcurrentWorkerThreads <= 50) { threadContentionFactor = 2.0d; }
            else if (NumberOfConcurrentWorkerThreads <= 200) { threadContentionFactor = 1.6d; }
            else { threadContentionFactor = 1.2d; }

            if (NumberOfConcurrentWorkerThreads > 1)
            {
                if (IsMergedWithOneOrMoreSingleThreadContexts)
                {
                    // Extra overhead for merged context (multiple functions)
                    return Convert.ToInt64(NumberOfConcurrentWorkerThreads * threadContentionFactor + 50d);
                }
            }
            return Convert.ToInt64(NumberOfConcurrentWorkerThreads * threadContentionFactor); ;
        }


        public TwoPhaseExecutionContext<TParam1, TParam2, TResult> And(TwoPhaseExecutionContext<TParam1, TParam2, TResult> anotherTwoPhaseExecutionContext)
        {
            if (this.NumberOfConcurrentWorkerThreads == 1 || anotherTwoPhaseExecutionContext.NumberOfConcurrentWorkerThreads == 1)
                this.IsMergedWithOneOrMoreSingleThreadContexts = true;

            this.functionsToBeExecuted.AddRange(anotherTwoPhaseExecutionContext.functionsToBeExecuted);

            return this;
        }


        // TODO: automatic arguments
        public void Test()
        {
            TestUsingArguments(default(TParam1), default(TParam2));
        }


        // TODO: automatic arguments
        public void TestUsingRandomArguments(ISet<TParam1> arg1Set, ISet<TParam2> arg2Set) { throw new NotImplementedException(); }


        [Obsolete("Use one of the two above")]
        public void TestUsingArguments(TParam1 arg1, TParam2 arg2)
        {
            // Execute context
            TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult> twoPhaseExecutionContextResultSet = Execute(arg1, arg2);

            // Write report
            Console.WriteLine(twoPhaseExecutionContextResultSet.GetReport());

            // Assert latency (everything in milliseconds)
            double minimumLatency = this.numberOfIterations * LatencyInMilliseconds;
            if (memoize) { minimumLatency = 1 * LatencyInMilliseconds; }

            long duration = twoPhaseExecutionContextResultSet.StopWatch.DurationInMilliseconds;

            long calculatedOverhead = CalculateOverheadInMillisecondsFor();

            double maximumLatency = minimumLatency + calculatedOverhead;

            if (duration < minimumLatency)
                throw new ApplicationException("Latency violation...");
            if (duration > maximumLatency)
                throw new ApplicationException("Latency violation...");

            // Assert results
            for (int i = 0; i < this.numberOfIterations; ++i)
            {
                for (int j = 0; j < this.twoPhaseExecutor.NumberOfParticipatingWorkerThreads; ++j)
                {
                    TResult result = twoPhaseExecutionContextResultSet[i, j].Result;
                    if (typeof(TResult) == typeof(String))
                    {
                        try
                        {
                            if (twoPhaseExecutionContextResultSet[i, j].IsMemoizerClearingThread) { continue; }

                            if (arg1 == null) { if (default(TParam1) == null) { continue; } }
                            else { if (arg1.Equals(default(TParam1))) { continue; } }

                            if (arg2 == null) { if (default(TParam2) == null) { continue; } }
                            else { if (arg2.Equals(default(TParam2))) { continue; } }

                            if (!((result as string).Contains(arg1.ToString()) && (result as string).Contains(arg2.ToString())))
                                throw new ApplicationException("Result violation...");
                        }
                        catch (Exception) { throw new ApplicationException("Internal casting..."); }
                    }
                    else
                        throw new ApplicationException("Only String type results are supported ...so far");
                }
            }
        }


        public TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult> Execute(TParam1 arg1, TParam2 arg2)
        {
            if (LatencyInMilliseconds == default(long))
                // First, dry-running all functions to measure approx. latency... the slowest one of them that is
                foreach (Tuple<int, Func<TParam1, TParam2, TResult>, bool, bool, long, bool> function in functionsToBeExecuted)
                {
                    long latency = function.Item5;
                    if (latency == default(long))
                    {
                        const int iterations = 4;
                        long accumulatedLatencyMeasurement = 0;
                        StopWatch stopWatch = new StopWatch();
                        for (int i = 0; i < iterations; ++i)
                        {
                            stopWatch.Start();
                            function.Item2.Invoke(default(TParam1), default(TParam2));
                            accumulatedLatencyMeasurement += stopWatch.DurationInMilliseconds;
                        }
                        long meanLatencyMeasurement = accumulatedLatencyMeasurement / iterations;
                        if (meanLatencyMeasurement > LatencyInMilliseconds)
                            LatencyInMilliseconds = meanLatencyMeasurement;
                    }
                    else
                        if (latency > LatencyInMilliseconds)
                            LatencyInMilliseconds = latency;
                }

            TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult> twoPhaseExecutionContextResultSet = new TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult>(this);
            twoPhaseExecutionContextResultSet.StopWatch.Start();
            for (int i = 0; i < this.numberOfIterations; ++i)
            {
                this.twoPhaseExecutor = new TwoPhaseExecutor(NumberOfConcurrentWorkerThreads, this.instrumentation);

                this.workerThreads = new List<FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>>(NumberOfConcurrentWorkerThreads);

                foreach (Tuple<int, Func<TParam1, TParam2, TResult>, bool, bool, long, bool> tuple in functionsToBeExecuted)
                    for (int j = 0; j < tuple.Item1; ++j)
                        if (tuple.Item4)
                            this.workerThreads.Add(new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>(function: tuple.Item2,
                                                                                                             barrier: twoPhaseExecutor.Barrier,
                                                                                                             isMemoizerClearing: true,
                                                                                                             instrumentation: tuple.Item6));
                        else
                            if (tuple.Item3)
                                this.workerThreads.Add(new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>(function: tuple.Item2.CachedInvoke,
                                                                                                                 barrier: twoPhaseExecutor.Barrier,
                                                                                                                 instrumentation: tuple.Item6));
                            else
                                this.workerThreads.Add(new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>(function: tuple.Item2,
                                                                                                                 barrier: twoPhaseExecutor.Barrier,
                                                                                                                 instrumentation: tuple.Item6));
                for (int j = 0; j < NumberOfConcurrentWorkerThreads; ++j)
                {
                    workerThreads[j].Arg1 = arg1;
                    workerThreads[j].Arg2 = arg2;
                    workerThreads[j].Start();
                }
                twoPhaseExecutor.Start();

                TwoPhaseExecutionContextResult<TParam1, TParam2, TResult> twoPhaseExecutionContextResult = new TwoPhaseExecutionContextResult<TParam1, TParam2, TResult>(NumberOfConcurrentWorkerThreads);
                for (int j = 0; j < NumberOfConcurrentWorkerThreads; ++j)
                    twoPhaseExecutionContextResult.WorkerThread[j] = workerThreads[j];

                twoPhaseExecutionContextResultSet.ExecutionResult[i] = twoPhaseExecutionContextResult;
            }
            twoPhaseExecutionContextResultSet.StopWatch.Stop();

            return twoPhaseExecutionContextResultSet;
        }
    }


    public class TwoPhaseExecutionContextResult<TParam1, TParam2, TResult>
    {
        readonly FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[] funcTwoPhaseExecutorThreads;

        internal TwoPhaseExecutionContextResult(int numberOfParticipatingWorkerThreads)
        {
            this.funcTwoPhaseExecutorThreads = new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[numberOfParticipatingWorkerThreads];
        }

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

        internal TwoPhaseExecutionContextResult<TParam1, TParam2, TResult>[] ExecutionResult { get { return this.twoPhaseExecutionContextResults; } }

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
            //bool isConcurrentExecution = this.twoPhaseExecutionContext.IsConcurrent;

            StringBuilder reportBuilder = new StringBuilder();

            reportBuilder.Append(this.twoPhaseExecutionContext.NumberOfIterations);
            reportBuilder.Append(this.twoPhaseExecutionContext.NumberOfIterations == 1 ? " round " : " rounds ");
            reportBuilder.Append("of " + this.twoPhaseExecutionContext.NumberOfConcurrentWorkerThreads + " ");

            reportBuilder.Append(this.twoPhaseExecutionContext.IsConcurrent ? "concurrent" : "sequential");

            reportBuilder.Append(this.twoPhaseExecutionContext.IsMemoized ? ", memoized" : ", non-memoized");
            //reportBuilder.Append(", identical method invocations ");
            reportBuilder.Append(" method invocations");

            reportBuilder.Append(" having approx. " + this.twoPhaseExecutionContext.LatencyInMilliseconds + " ms latency - took " + this.StopWatch.DurationInMilliseconds + " ms | " + this.StopWatch.DurationInTicks + " ticks");

            reportBuilder.Append(" (should take ");

            if (this.twoPhaseExecutionContext.IsMemoized)
                reportBuilder.Append(1 * this.twoPhaseExecutionContext.LatencyInMilliseconds);
            else
                reportBuilder.Append(this.twoPhaseExecutionContext.NumberOfIterations * this.twoPhaseExecutionContext.LatencyInMilliseconds);

            reportBuilder.Append(" <= ");
            reportBuilder.Append(StopWatch.DurationInMilliseconds);
            reportBuilder.Append(" <= ");

            if (this.twoPhaseExecutionContext.IsMemoized)
                reportBuilder.Append((1 * this.twoPhaseExecutionContext.LatencyInMilliseconds) + this.twoPhaseExecutionContext.CalculateOverheadInMillisecondsFor());//this.twoPhaseExecutionContext.NumberOfConcurrentWorkerThreads)));
            else
                reportBuilder.Append(((this.twoPhaseExecutionContext.NumberOfIterations * this.twoPhaseExecutionContext.LatencyInMilliseconds) + this.twoPhaseExecutionContext.CalculateOverheadInMillisecondsFor()));//this.twoPhaseExecutionContext.NumberOfConcurrentWorkerThreads)));

            reportBuilder.Append(")");
            if (this.twoPhaseExecutionContext.NumberOfConcurrentWorkerThreads == 1) reportBuilder.Append(" (extra 1-thread-only latency expectation penalty added...)");
            if (this.twoPhaseExecutionContext.IsMergedWithOneOrMoreSingleThreadContexts) reportBuilder.Append(" (extra 1-thread-only latency expectation penalty added...)");

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
