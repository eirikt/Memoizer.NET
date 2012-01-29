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

            Instrumentation = instrumentation;
            if (Instrumentation) { Console.WriteLine(GetType().FullName + ": Phase 0: Creating barrier, managing at most " + numberOfParticipants + " phased worker threads, + 1 main thread"); }
            Barrier = new Barrier((numberOfParticipants + 1), barrier =>
            {
                if (Instrumentation)
                    switch (barrier.CurrentPhaseNumber)
                    {
                        case 0:
                            Console.WriteLine(GetType().FullName + ": Phase 1: releasing all worker threads simultaneously");
                            break;

                        case 1:
                            Console.WriteLine(GetType().FullName + ": Phase 2: all worker threads have finished; cleaning up and terminating all threads");
                            break;

                        default:
                            throw new NotSupportedException("Unknown phase (" + barrier.CurrentPhaseNumber + ") entered...");
                    }
            });
        }

        public void Start()
        {
            if (Instrumentation)
            {
                Console.WriteLine(NumberOfParticipatingWorkerThreads < 1
                    ? GetType().FullName + ": Main thread: Arriving at 1st barrier rendevouz - releasing it immediately as it is the only participating thread..."
                    : GetType().FullName + ": Main thread: Arriving at 1st barrier rendevouz - probably as one of the last ones, releasing all worker threads simultaneously when all have reach 1st barrier...");
            }
            Barrier.SignalAndWait(Timeout.Infinite);

            if (Instrumentation)
            {
                Console.WriteLine(NumberOfParticipatingWorkerThreads < 1
                    ? GetType().FullName + ": Main thread: Arriving at 1st barrier rendevouz - continuing immediately as it is the only participating thread..."
                    : GetType().FullName + ": Main thread: Arriving at 2nd barrier rendevouz - probably as one of the first ones, waiting for all worker threads to complete...");
            }
            Barrier.SignalAndWait(Timeout.Infinite);
        }
    }


    public static class BarrierExtensionMethods
    {
        public static string GetInfo(this Barrier barrier)
        {
            if (barrier == null) { throw new ArgumentException("Barrier parameter cannot null"); }
            return /*barrier.GetType().FullName + ":*/ "Barrier phase is " + barrier.CurrentPhaseNumber + ", remaining participants are " + (barrier.ParticipantsRemaining) + " of a total of " + (barrier.ParticipantCount - 1) + " (plus main thread)";
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
        public bool Instrumented { get; set; }

        /// <summary>
        /// Explicit tag for recognizing this particular thread when instrumenting execution.
        /// </summary>
        public string Tag { get; set; }

        public bool IsTagged { get { return !string.IsNullOrEmpty(Tag); } }

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
            if (Instrumented) { Console.WriteLine("Memoizer.NET.TwoPhaseExecutor.Barrier participating thread #" + ParticipantNumber + ": Arriving at 1st barrier rendevouz... [" + Barrier.GetInfo() + "]"); }
            try { Barrier.SignalAndWait(Timeout.Infinite); }
            catch (ThreadAbortException e)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.ExceptionState);
            }

            ExecutionIndex = Interlocked.Increment(ref EXECUTION_INDEX_COUNTER);
            Action.Invoke();

            if (Instrumented) { Console.WriteLine("Memoizer.NET.TwoPhaseExecutor.Barrier participating thread #" + ParticipantNumber + ": Arriving at 2nd barrier rendevouz... [" + Barrier.GetInfo() + "]"); }
            try { Barrier.SignalAndWait(Timeout.Infinite); }
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

        protected AbstractTwoPhaseExecutorThread(Barrier sharedBarrier, bool instrumentation = false, string tag = null)
        {
            Barrier = sharedBarrier;
            Instrumented = instrumentation;
            Tag = tag;
            this.thread = new Thread(GetPhasedAction);
        }

        public void Start()
        {
            if (Instrumented) { Console.WriteLine("Memoizer.NET.TwoPhaseExecutor.Barrier participating thread #" + ParticipantNumber + " thread state: " + this.thread.ThreadState); }
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
            if (Instrumented) { Console.WriteLine("Memoizer.NET.TwoPhaseExecutor.Barrier participating thread #" + ParticipantNumber + " thread state: " + this.thread.ThreadState); }
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
            if (Instrumented) { Console.WriteLine(GetType().Namespace + "." + GetType().Name + " #" + TaskNumber + " created... [(Possible) TwoPhaseExecutor.Barrier participant #" + ParticipantNumber + "]"); }
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
            if (Instrumented) { Console.WriteLine(GetType().Namespace + "." + GetType().Name + " #" + TaskNumber + " created... [(Possible) TwoPhaseExecutor.Barrier participant #" + ParticipantNumber + "]"); }
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
                                            bool instrumentation = false,
                                            string tag = null)
            : base(barrier, instrumentation, tag)
        {
            this.function = function;
            IsMemoizerClearingThread = isMemoizerClearing;
            TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
            ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);

            // Don't know about this...
            //if (IsMemoizerClearingThread)
            //{
            //    if (Arg1 != null && Arg1.Equals(default(TParam1)) || Arg2 != null && Arg2.Equals(default(TParam2)))
            //        Action = () => this.function.RemoveFromCache(Arg1, Arg2);
            //    else
            //        Action = () => this.function.UnMemoize();
            //}
            //else
            Action = () => Result = this.function.Invoke(Arg1, Arg2);

            if (Instrumented)
                if (IsTagged)
                    Console.WriteLine(GetType().Namespace + "." + GetType().Name + " #" + TaskNumber + " ['" + Tag + "'] created... [(Possible) TwoPhaseExecutor.Barrier participant #" + ParticipantNumber + "]");
                else
                    Console.WriteLine(GetType().Namespace + "." + GetType().Name + " #" + TaskNumber + " created... [(Possible) TwoPhaseExecutor.Barrier participant #" + ParticipantNumber + "]");
        }
    }


    // TODO: Feasable...??
    public class DynamicTwoPhaseExecutorThread : AbstractTwoPhaseExecutorThread
    {
        static int TASK_COUNTER;
        readonly dynamic invocable;

        public dynamic Result { get; private set; }
        public dynamic[] Args { get; set; }
        public bool IsMemoizerClearingThread { get; private set; }

        internal DynamicTwoPhaseExecutorThread(dynamic invocable,
                                               Barrier barrier = default(Barrier),
                                               bool isMemoizerClearing = false,
                                               bool instrumentation = false,
                                               string tag = null)
            : base(barrier, instrumentation, tag)
        {
            this.invocable = invocable;
            IsMemoizerClearingThread = isMemoizerClearing;
            TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
            ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);

            Action = () => Result = this.invocable.DynamicInvoke(Args);
            //Action = () => Result = this.invocable.CachedInvoke(Args);
            //Action = () => Result = new MemoizerFactory(this.invocable).GetMemoizer().InvokeWith(Args);

            //var functionToBeExecuted = function.Value.FunctionToBeExecuted;
            //dynamic functionToBeExecuted = function.Value.FunctionToBeExecuted;
            //var memFunctionToBeExecuted = 

            if (Instrumented)
                if (IsTagged)
                    Console.WriteLine(GetType().Namespace + "." + GetType().Name + " #" + TaskNumber + " ['" + Tag + "'] created... [(Possible) TwoPhaseExecutor.Barrier participant #" + ParticipantNumber + "]");
                else
                    Console.WriteLine(GetType().Namespace + "." + GetType().Name + " #" + TaskNumber + " created... [(Possible) TwoPhaseExecutor.Barrier participant #" + ParticipantNumber + "]");
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
            bool memoizerClearing = false,
            bool idempotentFunction = true,
            long functionLatency = default(long),
            bool instrumentation = false,
            string tag = null)
        {
            return new TwoPhaseExecutionContext<TParam1, TParam2, TResult>(
                functionToBeMemoized,
                numberOfConcurrentThreadsWitinhEachIteration,
                numberOfIterations,
                concurrent,
                memoize,
                memoizerClearing,
                idempotentFunction,
                functionLatency,
                instrumentation,
                tag);
        }
    }


    public partial class TwoPhaseExecutionContext
    {
        public const int NUMBER_OF_WARM_UP_ITERATIONS = 4;
    }


    public partial class TwoPhaseExecutionContext<TParam1, TParam2, TResult>
    {

        //        //                     1                                2               3                4                            5                     6
        //        (numberOfConcurrentThreadsWitinhEachIteration, functionToBeExecuted, memoize, isMemoizerClearingTask, functionLatencyInMilliseconds, instrumentation)

        //class Ghjj<V,W,T> where T : Func<V,W>
        class FunctionExecutionContext
        {
            internal int NumberOfConcurrentThreadsWitinhEachIteration { get; set; }
            //internal T FunctionToBeExecuted { get; set; }
            internal dynamic FunctionToBeExecuted { get; set; }
            internal Type FunctionToBeExecutedType { get; set; }
            internal bool IsMemoized { get; set; }
            internal bool IsMemoizerClearingTask { get; set; }
            internal long LatencyInMilliseconds { get; set; }
            internal bool Instrumentation { get; set; }
            internal string Tag { get; set; }

            public override string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("[");
                if (!string.IsNullOrEmpty(Tag))
                {
                    stringBuilder.Append("#");
                    stringBuilder.Append(Tag);
                    stringBuilder.Append(": ");
                }
                stringBuilder.Append(NumberOfConcurrentThreadsWitinhEachIteration);
                stringBuilder.Append(" concurrent threads, ");
                if (IsMemoizerClearingTask)
                    stringBuilder.Append("mem-clearing");
                else
                {
                    if (IsMemoized)
                        stringBuilder.Append("mem");
                    else
                        stringBuilder.Append("non-mem");
                }
                //stringBuilder.Append(IsMemoizerClearingTask);
                //stringBuilder.Append(LatencyInMilliseconds);
                if (Instrumentation)
                    stringBuilder.Append(" logging");
                stringBuilder.Append("]");
                return stringBuilder.ToString();
            }
        }

        //readonly List<FunctionExecutionContext> functionsToBeExecuted;
        readonly IDictionary<long, FunctionExecutionContext> functionsToBeExecuted;

        readonly int numberOfIterations;

        // TODO: ...and get rid of these, I guess
        //readonly bool concurrent;
        //readonly bool memoize;
        bool isIdempotentContext;
        readonly bool instrumentation;

        TwoPhaseExecutor twoPhaseExecutor;

        //IList<FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>> workerThreads;
        IList<DynamicTwoPhaseExecutorThread> workerThreads;


        internal TwoPhaseExecutionContext(Func<TParam1, TParam2, TResult> functionToBeExecuted,
                                          int numberOfConcurrentThreadsWitinhEachIteration,
                                          int numberOfIterations,
                                          bool concurrent,
                                          bool memoize,
                                          bool isMemoizerClearing,
                                          bool idempotentFunction,
                                          long functionLatencyInMilliseconds,
                                          bool instrumentation,
                                          string tag)
        {
            if (numberOfIterations < 0) { throw new ArgumentException("Number-of-iteration parameter ('numberOfIterations') cannot be a negative number"); }
            if (numberOfConcurrentThreadsWitinhEachIteration < 0) { throw new ArgumentException("Number-of-worker-threads parameter ('numberOfConcurrentThreadsWitinhEachIteration') cannot be a negative number"); }

            FunctionExecutionContext functionExecutionContext =
                new FunctionExecutionContext
                {
                    NumberOfConcurrentThreadsWitinhEachIteration = numberOfConcurrentThreadsWitinhEachIteration,
                    FunctionToBeExecuted = functionToBeExecuted,
                    FunctionToBeExecutedType = functionToBeExecuted.GetType(),
                    IsMemoized = memoize,
                    IsMemoizerClearingTask = isMemoizerClearing,
                    LatencyInMilliseconds = functionLatencyInMilliseconds,
                    Instrumentation = instrumentation,
                    Tag = tag
                };
            this.functionsToBeExecuted =
                new Dictionary<long, FunctionExecutionContext>(1) { { HashHelper.CreateFunctionHash(functionToBeExecuted), functionExecutionContext } };
            //this.functionsToBeExecuted.Add(functionToBeExecuted, functionExecutionContext);
            //this.functionsToBeExecuted.Add(MemoizerHelper.CreateFunctionHash(functionToBeExecuted), functionExecutionContext);

            this.numberOfIterations = numberOfIterations;

            //this.concurrent = concurrent;
            //this.memoize = memoize;
            this.isIdempotentContext = idempotentFunction;
            //LatencyInMilliseconds = functionLatencyInMilliseconds;
            this.instrumentation = instrumentation;
        }

        internal int NumberOfIterations { get { return this.numberOfIterations; } }

        internal int NumberOfConcurrentWorkerThreads { get { return this.functionsToBeExecuted.Aggregate(0, (current, functionExecutionContext) => current + functionExecutionContext.Value.NumberOfConcurrentThreadsWitinhEachIteration); } }

        internal long NumberOfFunctions { get { return this.functionsToBeExecuted.Count; } }
        internal string FunctionListing
        {
            get
            {
                StringBuilder functionListingBuilder = new StringBuilder();
                functionListingBuilder.Append("{");
                foreach (var functionExecutionContext in functionsToBeExecuted.Values)
                {
                    functionListingBuilder.Append(functionExecutionContext.ToString());
                    functionListingBuilder.Append(", ");
                }
                functionListingBuilder.Remove(functionListingBuilder.Length - 2, 2);
                functionListingBuilder.Append("}");
                return functionListingBuilder.ToString();
            }
        }
        internal string LatencyListing
        {
            get
            {
                StringBuilder latencyListingBuilder = new StringBuilder();
                latencyListingBuilder.Append("{");
                foreach (var functionExecutionContext in functionsToBeExecuted.Values)
                {
                    latencyListingBuilder.Append(functionExecutionContext.LatencyInMilliseconds);
                    latencyListingBuilder.Append(", ");
                }
                latencyListingBuilder.Remove(latencyListingBuilder.Length - 2, 2);
                latencyListingBuilder.Append("}");
                return latencyListingBuilder.ToString();
            }
        }
        //internal bool IsMergedWithOneOrMoreSingleThreadContexts { get; set; }
        internal bool IsMergedWithOneOrMoreSingleThreadContexts { get { return functionsToBeExecuted.Any(functionExecutionContext => functionExecutionContext.Value.NumberOfConcurrentThreadsWitinhEachIteration == 1); } }

        //internal long LatencyInMilliseconds { get; private set; }
        internal long MaximumExpectedLatencyInMilliseconds { get; private set; }
        internal long MinimumExpextedLatencyInMilliseconds { get; private set; }

        //internal bool IsConcurrent { get { return this.concurrent; } }
        internal bool IsMemoized
        {
            get
            {
                if (NumberOfFunctions > 1) { throw new NotImplementedException("N/A for multiple function contexts"); }
                return this.functionsToBeExecuted[0].IsMemoized;//.this.memoize;
            }
        }
        //internal bool IsInstrumented { get { return this.instrumentation; } }


        //public int ExpectedFunctionInvocationCount
        //{
        //    get
        //    {
        //        if (NumberOfFunctions > 1) { throw new NotImplementedException("N/A for multiple function contexts"); }
        //        if (IsMemoized) { return 1 + TwoPhaseExecutionContext.NUMBER_OF_WARM_UP_ITERATIONS; }
        //        return (NumberOfConcurrentWorkerThreads * NumberOfIterations) + TwoPhaseExecutionContext.NUMBER_OF_WARM_UP_ITERATIONS;
        //    }
        //}


        /// <summary>
        /// Just some empirically calculated contention overhead...
        /// </summary>
        internal long CalculateContentionOverheadInMilliseconds()
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
            //if (this.NumberOfConcurrentWorkerThreads == 1 || anotherTwoPhaseExecutionContext.NumberOfConcurrentWorkerThreads == 1)
            //    this.IsMergedWithOneOrMoreSingleThreadContexts = true;

            foreach (var functionToBeExecuted in anotherTwoPhaseExecutionContext.functionsToBeExecuted)
            {
                this.functionsToBeExecuted.Add(functionToBeExecuted.Key, functionToBeExecuted.Value);
            }

            if (!anotherTwoPhaseExecutionContext.isIdempotentContext)
                this.isIdempotentContext = false;

            return this;
        }


        // TODO: automatic arguments
        public void Test() { TestUsingArguments(default(TParam1), default(TParam2)); }


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
            long duration = twoPhaseExecutionContextResultSet.StopWatch.DurationInMilliseconds;
            if (duration > MaximumExpectedLatencyInMilliseconds)
                throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Latency violation! [to slow...]");
            if (duration < MinimumExpextedLatencyInMilliseconds)
                throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Latency violation! [too fast!?]");

            // Assert results
            for (int i = 0; i < this.numberOfIterations; ++i)
            {
                for (int j = 0; j < this.twoPhaseExecutor.NumberOfParticipatingWorkerThreads; ++j)
                {
                    TResult result = twoPhaseExecutionContextResultSet[i, j].Result;
                    if (typeof(TResult) == typeof(String))
                    {
                        //try
                        //{
                        if (twoPhaseExecutionContextResultSet[i, j].IsMemoizerClearingThread) { continue; }

                        if (arg1 == null) { if (default(TParam1) == null) { continue; } }
                        else { if (arg1.Equals(default(TParam1))) { continue; } }

                        if (arg2 == null) { if (default(TParam2) == null) { continue; } }
                        else { if (arg2.Equals(default(TParam2))) { continue; } }

                        if (!((result as string).Contains(arg1.ToString()) && (result as string).Contains(arg2.ToString())))
                            throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Result violation!");
                        //}
                        //catch (Exception) { throw new ApplicationException("Internal casting..."); }
                    }
                    else
                        throw new ApplicationException("Only String type results are supported ...so far");
                }
            }
        }


        public TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult> Execute(TParam1 arg1, TParam2 arg2)
        {
            #region Idempotency
            if (!this.isIdempotentContext) { throw new NotImplementedException("Non-idempotent function are kind of N/A in this context... I think"); }
            #endregion

            #region Expected latency
            foreach (var functionExecutionContext in this.functionsToBeExecuted)
            {
                //long latency = functionExecutionContext.LatencyInMilliseconds;
                //if (latency == default(long))
                if (functionExecutionContext.Value.LatencyInMilliseconds == default(long))
                {
                    long accumulatedLatencyMeasurement = 0;
                    StopWatch stopWatch = new StopWatch();
                    for (int i = 0; i < TwoPhaseExecutionContext.NUMBER_OF_WARM_UP_ITERATIONS; ++i)
                    {
                        stopWatch.Start();
                        ((Func<TParam1, TParam2, TResult>)functionExecutionContext.Value.FunctionToBeExecuted).Invoke(default(TParam1), default(TParam2));
                        accumulatedLatencyMeasurement += stopWatch.DurationInMilliseconds;
                    }
                    functionExecutionContext.Value.LatencyInMilliseconds = accumulatedLatencyMeasurement / TwoPhaseExecutionContext.NUMBER_OF_WARM_UP_ITERATIONS;

                    if (MinimumExpextedLatencyInMilliseconds == default(long))
                        MinimumExpextedLatencyInMilliseconds = functionExecutionContext.Value.LatencyInMilliseconds;
                    else if (functionExecutionContext.Value.LatencyInMilliseconds < MinimumExpextedLatencyInMilliseconds)
                        MinimumExpextedLatencyInMilliseconds = functionExecutionContext.Value.LatencyInMilliseconds;
                }

                long expectedLatency = functionExecutionContext.Value.LatencyInMilliseconds + CalculateContentionOverheadInMilliseconds();

                if (functionExecutionContext.Value.IsMemoized)
                {
                    if (expectedLatency > MaximumExpectedLatencyInMilliseconds)
                        MaximumExpectedLatencyInMilliseconds = expectedLatency;

                    if (expectedLatency < MinimumExpextedLatencyInMilliseconds)
                        MinimumExpextedLatencyInMilliseconds = expectedLatency;
                }
                else
                {
                    expectedLatency *= NumberOfIterations;

                    if (expectedLatency > MaximumExpectedLatencyInMilliseconds)
                        MaximumExpectedLatencyInMilliseconds = expectedLatency;

                    if (expectedLatency < MinimumExpextedLatencyInMilliseconds)
                        MinimumExpextedLatencyInMilliseconds = expectedLatency;
                }
            }
            #endregion

            #region Two-phased execution
            TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult> twoPhaseExecutionContextResultSet = new TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult>(this);
            twoPhaseExecutionContextResultSet.StopWatch.Start();
            for (int i = 0; i < this.numberOfIterations; ++i)
            {
                this.twoPhaseExecutor = new TwoPhaseExecutor(NumberOfConcurrentWorkerThreads, this.instrumentation);

                //this.workerThreads = new List<FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>>(NumberOfConcurrentWorkerThreads);
                this.workerThreads = new List<DynamicTwoPhaseExecutorThread>(NumberOfConcurrentWorkerThreads);

                foreach (var function in functionsToBeExecuted)
                    for (int j = 0; j < function.Value.NumberOfConcurrentThreadsWitinhEachIteration; ++j)
                        //if (function.Value.IsMemoizerClearingTask)
                        //    this.workerThreads.Add(new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>(function: ((Func<TParam1, TParam2, TResult>)function.Value.FunctionToBeExecuted),
                        //                                                                                     barrier: twoPhaseExecutor.Barrier,
                        //                                                                                     isMemoizerClearing: true,
                        //                                                                                     instrumentation: function.Value.Instrumentation,
                        //                                                                                     tag: function.Value.Tag));
                        //else
                        if (function.Value.IsMemoized)
                        {
                            //this.workerThreads.Add(new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>(function: ((Func<TParam1, TParam2, TResult>)function.Value.FunctionToBeExecuted).CachedInvoke,
                            //                                                                                 barrier: twoPhaseExecutor.Barrier,
                            //                                                                                 instrumentation: function.Value.Instrumentation,
                            //                                                                                 tag: function.Value.Tag));
                            var functionToBeExecuted = function.Value.FunctionToBeExecuted;
                            //dynamic functionToBeExecuted = function.Value.FunctionToBeExecuted;
                            //var memFunctionToBeExecuted = new MemoizerFactory(functionToBeExecuted).GetMemoizer().InvokeWith(new object[] { arg1, arg2 });
                            //var act = new Action(new MemoizerFactory(functionToBeExecuted).GetMemoizer().InvokeWith(new object[]{arg1,arg2}));
                            Func<dynamic, dynamic, dynamic> act = new Func<dynamic, dynamic, dynamic>(delegate(dynamic arg11, dynamic arg22)
                            {
                                return new MemoizerFactory(functionToBeExecuted).GetMemoizer().InvokeWith(new object[] { arg11, arg22 });
                            });
                            this.workerThreads.Add(new DynamicTwoPhaseExecutorThread(invocable: act,//functionToBeExecuted.DynamicCachedInvoke,
                                                                                     barrier: twoPhaseExecutor.Barrier,
                                                                                     instrumentation: function.Value.Instrumentation,
                                                                                     tag: function.Value.Tag));
                        }
                        else
                            //this.workerThreads.Add(new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>(function: ((Func<TParam1, TParam2, TResult>)function.Value.FunctionToBeExecuted),
                            //                                                                                 barrier: twoPhaseExecutor.Barrier,
                            //                                                                                 instrumentation: function.Value.Instrumentation,
                            //                                                                                 tag: function.Value.Tag));
                            this.workerThreads.Add(new DynamicTwoPhaseExecutorThread(invocable: function.Value.FunctionToBeExecuted,
                                                                                     barrier: twoPhaseExecutor.Barrier,
                                                                                     instrumentation: function.Value.Instrumentation,
                                                                                     tag: function.Value.Tag));
                for (int j = 0; j < NumberOfConcurrentWorkerThreads; ++j)
                {
                    //workerThreads[j].Arg1 = arg1;
                    //workerThreads[j].Arg2 = arg2;
                    workerThreads[j].Args = new dynamic[] { arg1, arg2 };
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
            #endregion
        }


        public long GetExpectedFunctionInvocationCountFor(object function)
        {
            //FunctionExecutionContext functionExecutionContext = this.functionsToBeExecuted[function];
            //FunctionExecutionContext functionExecutionContext = this.functionsToBeExecuted[MemoizerHelper.CreateFunctionHash(function)];
            FunctionExecutionContext functionExecutionContext = this.functionsToBeExecuted[HashHelper.CreateFunctionHash(function)];
            if (functionExecutionContext.IsMemoized) { return 1 + TwoPhaseExecutionContext.NUMBER_OF_WARM_UP_ITERATIONS; }
            return (functionExecutionContext.NumberOfConcurrentThreadsWitinhEachIteration * NumberOfIterations) + TwoPhaseExecutionContext.NUMBER_OF_WARM_UP_ITERATIONS;
        }
    }


    public class TwoPhaseExecutionContextResult<TParam1, TParam2, TResult>
    {
        //readonly FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[] funcTwoPhaseExecutorThreads;
        readonly DynamicTwoPhaseExecutorThread[] funcTwoPhaseExecutorThreads;

        internal TwoPhaseExecutionContextResult(int numberOfParticipatingWorkerThreads)
        {
            //this.funcTwoPhaseExecutorThreads = new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[numberOfParticipatingWorkerThreads];
            this.funcTwoPhaseExecutorThreads = new DynamicTwoPhaseExecutorThread[numberOfParticipatingWorkerThreads];
        }

        //internal FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[] WorkerThread { get { return this.funcTwoPhaseExecutorThreads; } }
        internal DynamicTwoPhaseExecutorThread[] WorkerThread { get { return this.funcTwoPhaseExecutorThreads; } }
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

        //public FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult> this[int iterationIndex, int concurrentThreadIndex]
        public DynamicTwoPhaseExecutorThread this[int iterationIndex, int concurrentThreadIndex]
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

                //FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[] funcTwoPhaseExecutorThreads = twoPhaseExecutionContextResult.WorkerThread;
                DynamicTwoPhaseExecutorThread[] funcTwoPhaseExecutorThreads = twoPhaseExecutionContextResult.WorkerThread;
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
            reportBuilder.Append(this.twoPhaseExecutionContext.NumberOfIterations);
            reportBuilder.Append(this.twoPhaseExecutionContext.NumberOfIterations == 1 ? " round " : " rounds of ");
            reportBuilder.Append(this.twoPhaseExecutionContext.FunctionListing);
            reportBuilder.Append(" having approx. " + this.twoPhaseExecutionContext.LatencyListing + " ms latency - took " + this.StopWatch.DurationInMilliseconds + " ms | " + this.StopWatch.DurationInTicks + " ticks");
            reportBuilder.Append(" (should take ");
            reportBuilder.Append(this.twoPhaseExecutionContext.MinimumExpextedLatencyInMilliseconds);
            reportBuilder.Append(" <= ");
            reportBuilder.Append(StopWatch.DurationInMilliseconds);
            reportBuilder.Append(" <= ");
            reportBuilder.Append(this.twoPhaseExecutionContext.MaximumExpectedLatencyInMilliseconds);
            reportBuilder.Append(" ms)");
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
