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
            Action.DynamicInvoke();

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
    [Obsolete("Dynamic version replaces this one")]
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


    [Obsolete("Dynamic version replaces this one")]
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


    // TODO: Feasable...?? YESS!
    /*public*/
    internal class DynamicTwoPhaseExecutorThread : AbstractTwoPhaseExecutorThread
    {
        static int TASK_COUNTER;

        internal readonly dynamic invocable;
        internal readonly dynamic originalInvocable;
        internal readonly dynamic[] args;

        internal dynamic Result { get; private set; }
        //public bool IsMemoizerClearingThread { get; private set; }

        internal DynamicTwoPhaseExecutorThread(dynamic invocable,
                                                            dynamic originalInvocable,
                                               dynamic[] args,
                                               Barrier barrier = default(Barrier),
            //bool isMemoizerClearing = false,
                                               bool instrumentation = false,
                                               string tag = null)
            : base(barrier, instrumentation, tag)
        {
            this.invocable = invocable;
            this.originalInvocable = originalInvocable;
            this.args = args;
            //  IsMemoizerClearingThread = isMemoizerClearing;
            TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
            ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);

            Action = () => Result = this.invocable.DynamicInvoke(this.args);

            //this.Action = new Action(delegate()
            //{
            //    //Result = (((Func<string, long, string>)this.invocable).CachedInvoke((string)Args[0], (long)Args[1]));
            //    //Result = (((Func<string, long, string>)this.invocable).CachedDynamicInvoke((string)Args[0], (long)Args[1]));
            //    Result = new MemoizerFactory(invocable).GetMemoizer().InvokeWith(Args);
            //});

            if (Instrumented)
                if (IsTagged)
                    Console.WriteLine(GetType().Namespace + "." + GetType().Name + " #" + TaskNumber + " ['" + Tag + "'] created... [(Possible) TwoPhaseExecutor.Barrier participant #" + ParticipantNumber + "]");
                else
                    Console.WriteLine(GetType().Namespace + "." + GetType().Name + " #" + TaskNumber + " created... [(Possible) TwoPhaseExecutor.Barrier participant #" + ParticipantNumber + "]");
        }
    }


    public static partial class FuncExtensionMethods
    {
        public static TwoPhaseExecutionContext CreateExecutionContext<TParam1, TParam2, TResult>(
            this Func<TParam1, TParam2, TResult> functionToBeMemoized,
            dynamic[] args = null,
            int numberOfConcurrentThreadsWitinhEachIteration = 1,
            int numberOfIterations = 1,
            bool concurrent = true,
            bool memoize = false,
            bool memoizerClearing = false, // Remove!
            bool idempotentFunction = true, // Remove...?
            long functionLatency = default(long), // Remove?
            bool instrumentation = false,
            string tag = null
            )
        {
            return new TwoPhaseExecutionContext(functionToBeMemoized,
                                                args,
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


    public /*partial*/ class TwoPhaseExecutionContext
    {
        public const int NUMBER_OF_WARM_UP_ITERATIONS = 4;
        //}


        //public partial class TwoPhaseExecutionContext<TParam1, TParam2, TResult>
        //{

        //        //                     1                                2               3                4                            5                     6
        //        (numberOfConcurrentThreadsWitinhEachIteration, functionToBeExecuted, memoize, isMemoizerClearingTask, functionLatencyInMilliseconds, instrumentation)

        //class Ghjj<V,W,T> where T : Func<V,W>
        class FunctionExecutionContext
        {
            //internal bool functionContextExecuted = false;

            //internal T FunctionToBeExecuted { get; set; }
            internal dynamic FunctionToBeExecuted { get; set; }
            internal Type FunctionToBeExecutedType { get; set; }
            internal dynamic[] Args { get; set; }
            internal int NumberOfConcurrentThreadsWitinhEachIteration { get; set; }
            internal bool IsMemoized { get; set; }
            internal bool IsMemoizerClearingTask { get; set; }
            internal long LatencyInMilliseconds { get; set; }
            internal bool CalculatedLatency { get; set; }
            internal bool Instrumentation { get; set; }
            internal string Tag { get; set; }

            public override string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("[");
                if (!string.IsNullOrEmpty(Tag))
                {
                    //stringBuilder.Append("#");
                    stringBuilder.Append(Tag);
                    stringBuilder.Append(": ");
                }
                stringBuilder.Append(NumberOfConcurrentThreadsWitinhEachIteration);
                stringBuilder.Append(NumberOfConcurrentThreadsWitinhEachIteration == 1
                    ? " concurrent thread, "
                    : " concurrent threads, ");
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

                // TODO: include this...?
                //stringBuilder.Append(" (expected function invocations: " + GetExpectedFunctionInvocationCountFor(this.twoPhaseExecutionContext.) + ")");

                if (Instrumentation)
                    stringBuilder.Append(", logging");
                stringBuilder.Append("]");
                return stringBuilder.ToString();
            }
        }

        //readonly List<FunctionExecutionContext> functionsToBeExecuted;
        readonly IDictionary<string, FunctionExecutionContext> functionsToBeExecuted;

        //readonly int numberOfIterations;

        // TODO: ...and get rid of these, I guess
        //readonly bool concurrent;
        //readonly bool memoize;
        bool isIdempotentContext;
        readonly bool instrumentation;

        TwoPhaseExecutor twoPhaseExecutor;

        //IList<FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>> workerThreads;
        IList<DynamicTwoPhaseExecutorThread> workerThreads;


        TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet;// = new TwoPhaseExecutionContextResultSet(this);


        private IDictionary<string, Tuple<long, object>> resultMatrix;
        private long totalLatency;
        private int functionInvocationCounts;


        //internal TwoPhaseExecutionContext(Func<TParam1, TParam2, TResult> functionToBeExecuted,
        internal TwoPhaseExecutionContext(dynamic functionToBeExecuted,
                                          dynamic[] args,
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
                    FunctionToBeExecuted = functionToBeExecuted,
                    FunctionToBeExecutedType = functionToBeExecuted.GetType(),
                    Args = args,
                    NumberOfConcurrentThreadsWitinhEachIteration = numberOfConcurrentThreadsWitinhEachIteration,
                    IsMemoized = memoize,
                    IsMemoizerClearingTask = isMemoizerClearing,
                    LatencyInMilliseconds = functionLatencyInMilliseconds,
                    CalculatedLatency = functionLatencyInMilliseconds == default(long),
                    Instrumentation = instrumentation,
                    Tag = tag
                };
            this.functionsToBeExecuted =
                new Dictionary<string, FunctionExecutionContext>(1) { { HashHelper.CreateFunctionHash(functionToBeExecuted, args), functionExecutionContext } };
            //this.functionsToBeExecuted.Add(functionToBeExecuted, functionExecutionContext);
            //this.functionsToBeExecuted.Add(MemoizerHelper.CreateFunctionHash(functionToBeExecuted), functionExecutionContext);

            //this.numberOfIterations = numberOfIterations;
            NumberOfIterations = numberOfIterations;

            //this.concurrent = concurrent;
            //this.memoize = memoize;
            this.isIdempotentContext = idempotentFunction;
            //LatencyInMilliseconds = functionLatencyInMilliseconds;
            this.instrumentation = instrumentation;


            this.twoPhaseExecutionContextResultSet = new TwoPhaseExecutionContextResultSet(this);
        }

        internal bool IsExecuted { get; private set; }

        //internal int NumberOfIterations { get { return this.numberOfIterations; } }
        public int NumberOfIterations { get; private set; }

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
        public long MaximumExpectedLatencyInMilliseconds { get; private set; }
        public long MinimumExpextedLatencyInMilliseconds { get; private set; }

        //internal bool IsConcurrent { get { return this.concurrent; } }
        //internal bool IsMemoized
        //{
        //    get
        //    {
        //        if (NumberOfFunctions > 1) { throw new NotImplementedException("N/A for multiple function contexts"); }
        //        return this.functionsToBeExecuted[0].IsMemoized;//.this.memoize;
        //    }
        //}
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

            else if (NumberOfConcurrentWorkerThreads <= 4)
            {
                threadContentionFactor = /*6.0d;*/ 20;
            }
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
            return Convert.ToInt64(NumberOfConcurrentWorkerThreads * threadContentionFactor);
        }


        //public TwoPhaseExecutionContext<TParam1, TParam2, TResult> And(TwoPhaseExecutionContext<TParam1, TParam2, TResult> anotherTwoPhaseExecutionContext)
        public TwoPhaseExecutionContext And(TwoPhaseExecutionContext anotherTwoPhaseExecutionContext)
        {
            TwoPhaseExecutionContext mergedTwoPhaseExecutionContext = this.MemberwiseClone() as TwoPhaseExecutionContext;

            //if (this.NumberOfConcurrentWorkerThreads == 1 || anotherTwoPhaseExecutionContext.NumberOfConcurrentWorkerThreads == 1)
            //    this.IsMergedWithOneOrMoreSingleThreadContexts = true;

            try
            {
                foreach (var functionToBeExecuted in anotherTwoPhaseExecutionContext.functionsToBeExecuted)
                    mergedTwoPhaseExecutionContext.functionsToBeExecuted.Add(functionToBeExecuted.Key, functionToBeExecuted.Value);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Oops: " + e);
            }
            if (!anotherTwoPhaseExecutionContext.isIdempotentContext)
                mergedTwoPhaseExecutionContext.isIdempotentContext = false;

            mergedTwoPhaseExecutionContext.twoPhaseExecutionContextResultSet = new TwoPhaseExecutionContextResultSet(mergedTwoPhaseExecutionContext);

            return mergedTwoPhaseExecutionContext;
        }


        public TwoPhaseExecutionContext Having(int iterations)
        {
            this.NumberOfIterations = iterations;

            this.twoPhaseExecutionContextResultSet = new TwoPhaseExecutionContextResultSet(this);
            //{
            //    this.twoPhaseExecutionContext = twoPhaseExecutionContext;
            //    this.twoPhaseExecutionContextResults = new TwoPhaseExecutionContextResult[this.twoPhaseExecutionContext.NumberOfIterations];
            //    this.StopWatch = new StopWatch();
            //}
            return this;
        }


        [Obsolete("To be removed, I guess...")]
        // TODO: automatic arguments
        //public void Test() { TestUsingArguments(default(TParam1), default(TParam2)); }
        //public void Test() { TestUsingArguments(1234L, "SomeString"); }
        //public void Test() { TestUsingArguments("SomeString", 1234L); }
        public void Test() { TestUsingDefaultValues(); }


        [Obsolete("To be removed, I guess...")]
        public void TestUsingDefaultValues()
        {
            dynamic[] defaultValues = null;
            foreach (var functionExecutionContext in this.functionsToBeExecuted)
            {
                Type[] genericArguments = functionExecutionContext.Value.FunctionToBeExecuted.GetType().GetGenericArguments();
                defaultValues = new dynamic[genericArguments.Length - 1]; // -1 since the last generic parameter is the TResult
                for (int j = 0; j < genericArguments.Length - 1; ++j)
                {
                    Type genericArgument = genericArguments[j];
                    if (genericArgument.IsValueType)
                        defaultValues[j] = Activator.CreateInstance(genericArgument);
                    else if (genericArgument == typeof(String))
                        defaultValues[j] = default(String);
                }
            }
            TestUsingArguments(defaultValues);
        }


        [Obsolete("To be removed, I guess...")]
        // TODO: automatic arguments
        //public void TestUsingRandomArgumentsFrom(ISet<TParam1> arg1Set, ISet<TParam2> arg2Set) { throw new NotImplementedException(); }
        public void TestUsingRandomArgumentsFrom(ISet<dynamic> setOfArguments) { throw new NotImplementedException(); }


        [Obsolete("Use one of the two above")]
        //public void TestUsingArguments(TParam1 arg1, TParam2 arg2)
        public void TestUsingArguments(params dynamic[] args)
        {
            // Execute context
            //TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult> twoPhaseExecutionContextResultSet = Execute(arg1, arg2);
            TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet = Execute(args);

            // Write report
            Console.WriteLine(twoPhaseExecutionContextResultSet.Report);

            // Assert latency (everything in milliseconds)
            long duration = twoPhaseExecutionContextResultSet.StopWatch.DurationInMilliseconds;
            if (duration > MaximumExpectedLatencyInMilliseconds)
                throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Latency violation! [to slow...]");
            if (duration < MinimumExpextedLatencyInMilliseconds)
                throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Latency violation! [too fast!?]");

            //// Assert results
            //for (int i = 0; i < this.numberOfIterations; ++i)
            //{
            //    for (int j = 0; j < this.twoPhaseExecutor.NumberOfParticipatingWorkerThreads; ++j)
            //    {
            //        dynamic result = twoPhaseExecutionContextResultSet[i, j].Result;
            //        //if (typeof(TResult) == typeof(String))
            //        //{
            //        //try
            //        //{
            //        if (twoPhaseExecutionContextResultSet[i, j].IsMemoizerClearingThread) { continue; }

            //        //if (args[0] == null) { if (default(TParam1) == null) { continue; } }
            //        /*else {*/
            //        //if (args[0].Equals(1234L)) { continue; } //}

            //        //if (arg2 == null) { if (default(TParam2) == null) { continue; } }
            //        /*else {*/
            //        //if (args[1].Equals("SomeString")) { continue; } //}

            //        if (!((result as string).Contains(args[0].ToString()) && (result as string).Contains(args[1].ToString())))
            //            throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Result violation!");
            //        //}
            //        //catch (Exception) { throw new ApplicationException("Internal casting..."); }
            //        //}
            //        //else
            //        //    throw new ApplicationException("Only String type results are supported ...so far");
            //    }
            //    //}
            //}
        }


        [Obsolete("Arguments is now part of the individual contexts")]
        //public TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult> Execute(TParam1 arg1, TParam2 arg2)
        public TwoPhaseExecutionContextResultSet Execute(params dynamic[] args)
        //internal TwoPhaseExecutionContextResultSet Execute(params dynamic[] args)
        {
            #region Idempotency
            if (!this.isIdempotentContext) { throw new NotImplementedException("Non-idempotent function are kind of N/A in this context... I think"); }
            #endregion

            #region Expected latency
            foreach (var functionExecutionContext in this.functionsToBeExecuted)
            {
                if (functionExecutionContext.Value.LatencyInMilliseconds == default(long))
                {
                    long accumulatedLatencyMeasurement = 0;
                    StopWatch stopWatch = new StopWatch();
                    for (int i = 0; i < NUMBER_OF_WARM_UP_ITERATIONS; ++i)
                    {
                        stopWatch.Start();

                        Type[] genericArguments = functionExecutionContext.Value.FunctionToBeExecuted.GetType().GetGenericArguments();
                        object[] onTheFlyArguments = new object[genericArguments.Length - 1]; // -1 since the last generic parameter is the TResult
                        for (int j = 0; j < genericArguments.Length - 1; ++j)
                        {
                            Type genericArgument = genericArguments[j];
                            if (genericArgument.IsValueType)
                                onTheFlyArguments[j] = Activator.CreateInstance(genericArgument);
                            else if (genericArgument == typeof(String))
                                onTheFlyArguments[j] = default(String);
                        }
                        functionExecutionContext.Value.FunctionToBeExecuted.DynamicInvoke(onTheFlyArguments);

                        accumulatedLatencyMeasurement += stopWatch.DurationInMilliseconds;
                    }
                    functionExecutionContext.Value.LatencyInMilliseconds = accumulatedLatencyMeasurement / NUMBER_OF_WARM_UP_ITERATIONS;

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
            TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet = new TwoPhaseExecutionContextResultSet(this);
            twoPhaseExecutionContextResultSet.StopWatch.Start();
            //for (int i = 0; i < this.numberOfIterations; ++i)
            for (int i = 0; i < NumberOfIterations; ++i)
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
                            ////this.workerThreads.Add(new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>(function: ((Func<TParam1, TParam2, TResult>)function.Value.FunctionToBeExecuted).CachedInvoke,
                            ////                                                                                 barrier: twoPhaseExecutor.Barrier,
                            ////                                                                                 instrumentation: function.Value.Instrumentation,
                            ////                                                                                 tag: function.Value.Tag));

                            var functionToBeExecuted = function.Value.FunctionToBeExecuted;

                            Func<dynamic, dynamic, dynamic> func = new Func<dynamic, dynamic, dynamic>(delegate(dynamic arg1, dynamic arg2)
                            {
                                return new MemoizerFactory(functionToBeExecuted).GetMemoizer().InvokeWith(new object[] { arg1, arg2 });
                                //return functionToBeExecuted.CachedDynamicInvoke(new object[] { arg11, arg22 });
                            });
                            DynamicTwoPhaseExecutorThread d = new DynamicTwoPhaseExecutorThread(invocable: func, //*/functionToBeExecuted,//.DynamicCachedInvoke,
                                originalInvocable: functionsToBeExecuted,
                                                                                                args: args,
                                                                                                barrier: twoPhaseExecutor.Barrier,
                                                                                                instrumentation: function.Value.Instrumentation,
                                                                                                tag: function.Value.Tag);
                            //{
                            //    Args = args
                            //};
                            this.workerThreads.Add(d);
                        }
                        else
                        {
                            //this.workerThreads.Add(new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>(function: ((Func<TParam1, TParam2, TResult>)function.Value.FunctionToBeExecuted),
                            //                                                                                 barrier: twoPhaseExecutor.Barrier,
                            //                                                                                 instrumentation: function.Value.Instrumentation,
                            //                                                                                 tag: function.Value.Tag));
                            //this.workerThreads.Add(new DynamicTwoPhaseExecutorThread(invocable: function.Value.FunctionToBeExecuted,
                            //                                                         barrier: twoPhaseExecutor.Barrier,
                            //                                                         instrumentation: function.Value.Instrumentation,
                            //                                                         tag: function.Value.Tag));
                            DynamicTwoPhaseExecutorThread d = new DynamicTwoPhaseExecutorThread(invocable: function.Value.FunctionToBeExecuted,
                                originalInvocable: function.Value.FunctionToBeExecuted,
                                                                                                args: args,
                                                                                                barrier: twoPhaseExecutor.Barrier,
                                                                                                instrumentation: function.Value.Instrumentation,
                                                                                                tag: function.Value.Tag);
                            //{
                            //    Args = args
                            //};
                            this.workerThreads.Add(d);
                        }

                TwoPhaseExecutionContextResult twoPhaseExecutionContextResult = new TwoPhaseExecutionContextResult(NumberOfConcurrentWorkerThreads);
                for (int j = 0; j < NumberOfConcurrentWorkerThreads; ++j)
                {
                    //    //workerThreads[j].Arg1 = arg1;
                    //    //workerThreads[j].Arg2 = arg2;
                    //    workerThreads[j].Args = args; //new dynamic[] { args };
                    workerThreads[j].Start();
                    twoPhaseExecutionContextResult.WorkerThreads[j] = workerThreads[j];
                }
                twoPhaseExecutor.Start();

                //for (int j = 0; j < NumberOfConcurrentWorkerThreads; ++j)
                //    twoPhaseExecutionContextResult.WorkerThread[j] = workerThreads[j];

                twoPhaseExecutionContextResultSet.ExecutionResult[i] = twoPhaseExecutionContextResult;
            }
            twoPhaseExecutionContextResultSet.StopWatch.Stop();
            IsExecuted = true;

            return twoPhaseExecutionContextResultSet;
            #endregion
        }


        public TwoPhaseExecutionContext Execute(bool report = true)
        //public TwoPhaseExecutionContextResultSet Execute(bool report = true)
        //internal TwoPhaseExecutionContextResultSet Execute(bool report = true)
        {
            #region Report [before]
            if (report) { Console.WriteLine(new TwoPhaseExecutionContextResultSet(this).Report); }
            #endregion

            #region Idempotency
            if (!this.isIdempotentContext) { throw new NotImplementedException("Non-idempotent function are kind of N/A in this context... I think"); }
            #endregion

            #region Expected latency);
            //Console.WriteLine("--- Expected latency ---");
            foreach (var functionExecutionContext in this.functionsToBeExecuted.Values)
            {
                if (functionExecutionContext.LatencyInMilliseconds == default(long))
                {
                    long accumulatedLatencyMeasurement = 0;
                    StopWatch stopWatch = new StopWatch();
                    for (int i = 0; i < NUMBER_OF_WARM_UP_ITERATIONS; ++i)
                    {
                        stopWatch.Start();

                        Type[] genericArguments = functionExecutionContext.FunctionToBeExecuted.GetType().GetGenericArguments();
                        object[] defaultValueArguments = new object[genericArguments.Length - 1]; // -1 since the last generic parameter is the TResult
                        for (int j = 0; j < genericArguments.Length - 1; ++j)
                        {
                            Type genericArgument = genericArguments[j];
                            if (genericArgument.IsValueType)
                                defaultValueArguments[j] = Activator.CreateInstance(genericArgument);
                            else if (genericArgument == typeof(String))
                                defaultValueArguments[j] = default(String);
                        }
                        functionExecutionContext.FunctionToBeExecuted.DynamicInvoke(defaultValueArguments);

                        accumulatedLatencyMeasurement += stopWatch.DurationInMilliseconds;
                    }
                    functionExecutionContext.LatencyInMilliseconds = accumulatedLatencyMeasurement / NUMBER_OF_WARM_UP_ITERATIONS;

                    if (MinimumExpextedLatencyInMilliseconds == default(long))
                        MinimumExpextedLatencyInMilliseconds = functionExecutionContext.LatencyInMilliseconds;
                    else if (functionExecutionContext.LatencyInMilliseconds < MinimumExpextedLatencyInMilliseconds)
                        MinimumExpextedLatencyInMilliseconds = functionExecutionContext.LatencyInMilliseconds;
                }

                long expectedLatency = functionExecutionContext.LatencyInMilliseconds + CalculateContentionOverheadInMilliseconds();

                if (functionExecutionContext.IsMemoized)
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
            //Console.WriteLine("--- Two-phased execution ---");
            //TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet = new TwoPhaseExecutionContextResultSet(this);
            this.twoPhaseExecutionContextResultSet.StopWatch.Start();
            //for (int i = 0; i < this.numberOfIterations; ++i)
            for (int i = 0; i < NumberOfIterations; ++i)
            {
                this.twoPhaseExecutor = new TwoPhaseExecutor(NumberOfConcurrentWorkerThreads, this.instrumentation);

                //this.workerThreads = new List<FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>>(NumberOfConcurrentWorkerThreads);
                this.workerThreads = new List<DynamicTwoPhaseExecutorThread>(NumberOfConcurrentWorkerThreads);

                foreach (var functionExecutionContext in functionsToBeExecuted.Values)
                {
                    Type[] genericArguments;

                    for (int j = 0; j < functionExecutionContext.NumberOfConcurrentThreadsWitinhEachIteration; ++j)
                    {

                        int numberOfFunctionArguments;

                        if (functionExecutionContext.IsMemoized)
                        {
                            //this.workerThreads.Add(new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>(function: ((Func<TParam1, TParam2, TResult>)function.Value.FunctionToBeExecuted).CachedInvoke,
                            //                                                                                 barrier: twoPhaseExecutor.Barrier,
                            //                                                                                 instrumentation: function.Value.Instrumentation,
                            //                                                                                 tag: function.Value.Tag));
                            //localFunctionExecutionContext = functionExecutionContext;
                            //dynamic functionToBeExecuted = function.Value.FunctionToBeExecuted;
                            //var memFunctionToBeExecuted = new MemoizerFactory(functionToBeExecuted).GetMemoizer().InvokeWith(new object[] { arg1, arg2 });
                            //var act = new Action(new MemoizerFactory(functionToBeExecuted).GetMemoizer().InvokeWith(new object[]{arg1,arg2}));

                            genericArguments = functionExecutionContext.FunctionToBeExecuted.GetType().GetGenericArguments();
                            numberOfFunctionArguments = genericArguments.Length - 1; // -1 since the last generic parameter is the TResult
                            //for (int j = 0; j < genericArguments.Length - 1; ++j)
                            switch (numberOfFunctionArguments)
                            {
                                case 0: throw new NotImplementedException();
                                case 1: throw new NotImplementedException();
                                case 2:
                                    ////Func<dynamic, dynamic, dynamic> act = new Func<dynamic, dynamic, dynamic>(delegate(dynamic arg1, dynamic arg2)
                                    ////{
                                    ////    return functionToBeExecuted.CachedDynamicInvoke(new object[] { functionExecutionContext.Args });
                                    ////});
                                    //Func<dynamic, dynamic, dynamic> func =
                                    //    delegate
                                    //    {
                                    //        //return functionExecutionContext.FunctionToBeExecuted.CachedInvoke(new dynamic[] { functionExecutionContext.Args });
                                    //        //return functionExecutionContext.FunctionToBeExecuted.CachedDynamicInvoke(new dynamic[] { functionExecutionContext.Args });
                                    //        return functionExecutionContext.FunctionToBeExecuted.CachedDynamicInvoke(functionExecutionContext.Args[0], functionExecutionContext.Args[1]);
                                    //    };
                                    ////this.workerThreads.Add(new DynamicTwoPhaseExecutorThread(invocable: func, //functionToBeExecuted.DynamicCachedInvoke,
                                    ////                                                         barrier: twoPhaseExecutor.Barrier,
                                    ////                                                         instrumentation: function.Instrumentation,
                                    ////                                                         tag: function.Tag));
                                    //DynamicTwoPhaseExecutorThread d = new DynamicTwoPhaseExecutorThread(invocable: func, //functionToBeExecuted.DynamicCachedInvoke,
                                    //                                                                    barrier: twoPhaseExecutor.Barrier,
                                    //                                                                    instrumentation: function.Instrumentation,
                                    //                                                                    tag: function.Tag);
                                    //d.Args = functionExecutionContext.Args;


                                    //var functionToBeExecuted = function.Value.FunctionToBeExecuted;

                                    //Func<dynamic, dynamic, dynamic> func = new Func<dynamic, dynamic, dynamic>(delegate(dynamic arg1, dynamic arg2)
                                    //{
                                    //    return new MemoizerFactory(functionExecutionContext.FunctionToBeExecuted).GetMemoizer().InvokeWith(new object[] { arg1, arg2 });
                                    //});
                                    FunctionExecutionContext context = functionExecutionContext;
                                    Func<dynamic, dynamic, dynamic> func = (arg1, arg2) =>
                                        new MemoizerFactory(context.FunctionToBeExecuted).GetMemoizer().InvokeWith(new object[] { arg1, arg2 });

                                    DynamicTwoPhaseExecutorThread dynamicTwoPhaseExecutorThread =
                                        new DynamicTwoPhaseExecutorThread(invocable: func,
                                                                          originalInvocable: functionExecutionContext.FunctionToBeExecuted,
                                                                          args: functionExecutionContext.Args,
                                                                          barrier: twoPhaseExecutor.Barrier,
                                                                          instrumentation: functionExecutionContext.Instrumentation,
                                                                          tag: functionExecutionContext.Tag);
                                    //{
                                    //    Args = functionExecutionContext.Args
                                    //};

                                    this.workerThreads.Add(dynamicTwoPhaseExecutorThread);

                                    ////for (int j = 0; j < NumberOfConcurrentWorkerThreads; ++j)
                                    dynamicTwoPhaseExecutorThread.Start();

                                    break;

                                case 3: throw new NotImplementedException();
                            }
                            //Func<dynamic, dynamic, dynamic> act = new Func<dynamic, dynamic, dynamic>(delegate(dynamic arg11, dynamic arg22)
                            //{
                            //    //return new MemoizerFactory(functionToBeExecuted).GetMemoizer().InvokeWith(new object[] { arg11, arg22 });
                            //    return functionToBeExecuted.CachedDynamicInvoke(new object[] { functionExecutionContext.Args });
                            //});
                            //this.workerThreads.Add(new DynamicTwoPhaseExecutorThread(invocable: act,//functionToBeExecuted.DynamicCachedInvoke,
                            //                                                         barrier: twoPhaseExecutor.Barrier,
                            //                                                         instrumentation: function.Instrumentation,
                            //                                                         tag: function.Tag));
                        }
                        else
                        {
                            //this.workerThreads.Add(new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>(function: ((Func<TParam1, TParam2, TResult>)function.Value.FunctionToBeExecuted),
                            //                                                                                 barrier: twoPhaseExecutor.Barrier,
                            //                                                                                 instrumentation: function.Value.Instrumentation,
                            //                                                                                 tag: function.Value.Tag));
                            //throw new NotImplementedException();
                            //this.workerThreads.Add(new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>(function: ((Func<TParam1, TParam2, TResult>)function.Value.FunctionToBeExecuted).CachedInvoke,
                            //                                                                                 barrier: twoPhaseExecutor.Barrier,
                            //                                                                                 instrumentation: function.Value.Instrumentation,
                            //                                                                                 tag: function.Value.Tag));
                            //localFunctionExecutionContext = functionExecutionContext;
                            //dynamic functionToBeExecuted = function.Value.FunctionToBeExecuted;
                            //var memFunctionToBeExecuted = new MemoizerFactory(functionToBeExecuted).GetMemoizer().InvokeWith(new object[] { arg1, arg2 });
                            //var act = new Action(new MemoizerFactory(functionToBeExecuted).GetMemoizer().InvokeWith(new object[]{arg1,arg2}));

                            genericArguments = functionExecutionContext.FunctionToBeExecuted.GetType().GetGenericArguments();
                            numberOfFunctionArguments = genericArguments.Length - 1; // -1 since the last generic parameter is the TResult
                            //for (int j = 0; j < genericArguments.Length - 1; ++j)
                            switch (numberOfFunctionArguments)
                            {
                                case 0: throw new NotImplementedException();
                                case 1: throw new NotImplementedException();
                                case 2:
                                    DynamicTwoPhaseExecutorThread dynamicTwoPhaseExecutorThread =
                                        new DynamicTwoPhaseExecutorThread(invocable: functionExecutionContext.FunctionToBeExecuted,
                                                                          originalInvocable: functionExecutionContext.FunctionToBeExecuted,
                                                                          args: functionExecutionContext.Args,
                                                                          barrier: twoPhaseExecutor.Barrier,
                                                                          instrumentation: functionExecutionContext.Instrumentation,
                                                                          tag: functionExecutionContext.Tag);
                                    //{
                                    //    Args = functionExecutionContext.Args
                                    //};
                                    this.workerThreads.Add(dynamicTwoPhaseExecutorThread);
                                    dynamicTwoPhaseExecutorThread.Start();
                                    break;

                                case 3: throw new NotImplementedException();
                            }
                        }
                    }

                    //for (int j = 0; j < NumberOfConcurrentWorkerThreads; ++j)
                    //{
                    //    //workerThreads[j].Arg1 = arg1;
                    //    //workerThreads[j].Arg2 = arg2;
                    //    //workerThreads[j].Args = args; //new dynamic[] { args };
                    //    workerThreads[j].Start();
                    //}
                }
                twoPhaseExecutor.Start();

                TwoPhaseExecutionContextResult twoPhaseExecutionContextResult = new TwoPhaseExecutionContextResult(NumberOfConcurrentWorkerThreads);
                for (int j = 0; j < NumberOfConcurrentWorkerThreads; ++j)
                    twoPhaseExecutionContextResult.WorkerThreads[j] = workerThreads[j];

                this.twoPhaseExecutionContextResultSet.ExecutionResult[i] = twoPhaseExecutionContextResult;
            }
            this.twoPhaseExecutionContextResultSet.StopWatch.Stop();
            IsExecuted = true;
            #endregion

            #region Report [after]
            if (report) { Console.WriteLine(this.twoPhaseExecutionContextResultSet.Report); }
            #endregion
            //return twoPhaseExecutionContextResultSet;
            return this;
        }


        //public long GetExpectedFunctionInvocationCountFor(object function, params object[] args)
        //{
        //    //FunctionExecutionContext functionExecutionContext = this.functionsToBeExecuted[function];
        //    //FunctionExecutionContext functionExecutionContext = this.functionsToBeExecuted[MemoizerHelper.CreateFunctionHash(function)];
        //    FunctionExecutionContext functionExecutionContext = this.functionsToBeExecuted[HashHelper.CreateFunctionHash(function, args)];
        //    if (functionExecutionContext.IsMemoized) { return 1 + NUMBER_OF_WARM_UP_ITERATIONS; }
        //    return (functionExecutionContext.NumberOfConcurrentThreadsWitinhEachIteration * NumberOfIterations) + NUMBER_OF_WARM_UP_ITERATIONS;
        //}

        public long GetExpectedFunctionInvocationCountFor(object function)
        {
            long expectedInvocations = 0;
            foreach (var functionExecutionContextKeyValuePair in this.functionsToBeExecuted)
            {
                string[] funcIdArray = functionExecutionContextKeyValuePair.Key.Split(new[] { "@" }, StringSplitOptions.None);
                var funcHash = HashHelper.CreateFunctionHash(function);
                var funcHash0 = funcHash.Split(new[] { "@" }, StringSplitOptions.None);
                if (funcHash0[0] == funcIdArray[0])
                    if (functionExecutionContextKeyValuePair.Value.CalculatedLatency)
                        if (functionExecutionContextKeyValuePair.Value.IsMemoized)
                            expectedInvocations += 1 + NUMBER_OF_WARM_UP_ITERATIONS;
                        else
                            expectedInvocations += (functionExecutionContextKeyValuePair.Value.NumberOfConcurrentThreadsWitinhEachIteration * NumberOfIterations) + NUMBER_OF_WARM_UP_ITERATIONS;
                    else
                        if (functionExecutionContextKeyValuePair.Value.IsMemoized)
                            expectedInvocations += 1;
                        else
                            expectedInvocations += functionExecutionContextKeyValuePair.Value.NumberOfConcurrentThreadsWitinhEachIteration * NumberOfIterations;
            }
            return expectedInvocations;
        }


        //public TwoPhaseExecutionContext Expect(IDictionary<string, Tuple<long, object>> resultMatrix,
        //                                       long totalLatency,
        //                                       ref int functionInvocationCounts)
        //{
        //    this.resultMatrix = resultMatrix;
        //    this.totalLatency=totalLatency;
        //    this.functionInvocationCounts = functionInvocationCounts;
        //    return this;
        //}

        public void Verify(IDictionary<string, object> expectedResults,
                           long expectedMinimumLatency = default(long),
                           long expectedMaximumLatency = default(long),
                           IDictionary<string, long> actualFunctionInvocationCounts = default(IDictionary<string, long>))
        {
            //StringBuilder reportBuilder = new StringBuilder();

            // Assert results of the executed functions
            TwoPhaseExecutionContextResult[] t = this.twoPhaseExecutionContextResultSet.ExecutionResult;
            //Assert.That(t.Length, Is.EqualTo(1));
            //if (t.Length != expectedResults.)
            //{
            //    throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Latency violation! TOO FAST" + Environment.NewLine + reportBuilder);

            //}
            for (int i = 0; i < NumberOfIterations; ++i)
            {
                TwoPhaseExecutionContextResult twoPhaseExecutionContextResult = t[i];
                DynamicTwoPhaseExecutorThread[] dynamicTwoPhaseExecutorThreads = twoPhaseExecutionContextResult.WorkerThreads;
                //Assert.That(dynamicTwoPhaseExecutorThreads.Length, Is.EqualTo(1 + 1 * 2));
                for (int j = 0; j < dynamicTwoPhaseExecutorThreads.Length; ++j)
                {
                    DynamicTwoPhaseExecutorThread dynamicTwoPhaseExecutorThread = dynamicTwoPhaseExecutorThreads[j];

                    object actualResult = dynamicTwoPhaseExecutorThread.Result;

                    //foreach (var functionExecutionContext in this.functionsToBeExecuted.Values)
                    //{
                    //    //long expectedCount = GetExpectedFunctionInvocationCountFor(functionExecutionContext.FunctionToBeExecuted);
                    //    dynamic actualCount;
                    //    string funcHash2 = HashHelper.CreateFunctionHash(functionExecutionContext.FunctionToBeExecuted, functionExecutionContext.Args);
                    //    expectedResults.TryGetValue(funcHash2, out actualCount);
                    //    int iu = 0;
                    //}

                    string funcHash = HashHelper.CreateFunctionHash(dynamicTwoPhaseExecutorThread.originalInvocable, dynamicTwoPhaseExecutorThread.args);
                    dynamic expectedResult;
                    expectedResults.TryGetValue(funcHash, out expectedResult);

                    if (!expectedResult.Equals(actualResult))
                    {
                        StringBuilder reportBuilder = new StringBuilder();
                        reportBuilder.Append("Expected result: " + expectedResult);
                        reportBuilder.Append(Environment.NewLine);
                        reportBuilder.Append("Actual result: " + actualResult);
                        throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Violation in result" + Environment.NewLine + reportBuilder);
                    }

                }
                //tt.Start(); // System.Threading.ThreadStateException : Thread is running or terminated; it cannot restart.

                //var res_0_0 = twoPhaseExecutionContextResultSet[0, 0].Result;
                //var res_0_1 = twoPhaseExecutionContextResultSet[0, 1].Result;
                //var res_0_2 = twoPhaseExecutionContextResultSet[0, 2].Result;
                ////var res_0_3 = twoPhaseExecutionContextResultSet[0, 3].Result; // Exception: ...
                ////var res_1_0 = twoPhaseExecutionContextResultSet[1, 0].Result; // Exception: ...
                //Assert.That(res_0_0, Is.EqualTo("VeryExpensiveMethodResponseForyo13"));
                //Assert.That(res_0_1, Is.EqualTo("VeryExpensiveMethodResponseForyoyo1313"));
                //Assert.That(res_0_2, Is.EqualTo("VeryExpensiveMethodResponseForyoyo1313"));
            }

            // Assert latency/duration
            long duration = this.twoPhaseExecutionContextResultSet.StopWatch.DurationInMilliseconds;

            if (duration > MaximumExpectedLatencyInMilliseconds)
            {
                StringBuilder reportBuilder = new StringBuilder();
                reportBuilder.Append("Duration: " + duration + " ms | " + this.twoPhaseExecutionContextResultSet.StopWatch.DurationInTicks + " ticks");
                reportBuilder.Append(" (should take ");
                reportBuilder.Append(MinimumExpextedLatencyInMilliseconds);
                reportBuilder.Append(" <= ");
                reportBuilder.Append(duration);
                reportBuilder.Append(" <= ");
                reportBuilder.Append(MaximumExpectedLatencyInMilliseconds);
                reportBuilder.Append(" ms) [Internal demand]");
                throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Latency violation! TOO SLOW" + Environment.NewLine + reportBuilder);
            }
            if (duration < MinimumExpextedLatencyInMilliseconds)
            {
                StringBuilder reportBuilder = new StringBuilder();
                reportBuilder.Append("Duration: " + duration + " ms | " + this.twoPhaseExecutionContextResultSet.StopWatch.DurationInTicks + " ticks");
                reportBuilder.Append(" (should take ");
                reportBuilder.Append(MinimumExpextedLatencyInMilliseconds);
                reportBuilder.Append(" <= ");
                reportBuilder.Append(duration);
                reportBuilder.Append(" <= ");
                reportBuilder.Append(MaximumExpectedLatencyInMilliseconds);
                reportBuilder.Append(" ms) [Internal demand]");
                throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Latency violation! TOO FAST" + Environment.NewLine + reportBuilder);
            }
            if (duration > expectedMaximumLatency)
            {
                StringBuilder reportBuilder = new StringBuilder();
                reportBuilder.Append("Duration: " + duration + " ms | " + this.twoPhaseExecutionContextResultSet.StopWatch.DurationInTicks + " ticks");
                reportBuilder.Append(" (should take ");
                reportBuilder.Append(expectedMinimumLatency);
                reportBuilder.Append(" <= ");
                reportBuilder.Append(duration);
                reportBuilder.Append(" <= ");
                reportBuilder.Append(expectedMaximumLatency);
                reportBuilder.Append(" ms) [External demand]");
                throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Latency violation, TOO SLOW!" + Environment.NewLine + reportBuilder);
            }
            if (duration < expectedMinimumLatency)
            {
                StringBuilder reportBuilder = new StringBuilder();

                reportBuilder.Append("Duration: " + duration + " ms | " + this.twoPhaseExecutionContextResultSet.StopWatch.DurationInTicks + " ticks");
                reportBuilder.Append(" (should take ");
                reportBuilder.Append(expectedMinimumLatency);
                reportBuilder.Append(" <= ");
                reportBuilder.Append(duration);
                reportBuilder.Append(" <= ");
                reportBuilder.Append(expectedMaximumLatency);
                reportBuilder.Append(" ms) [External demand]");
                throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Latency violation! TOO FAST" + Environment.NewLine + reportBuilder);
            }


            // Assert number of invocations (mostly useful when memoizing...)
            if (actualFunctionInvocationCounts != default(IDictionary<string, long>))
            {
                foreach (var functionExecutionContext in this.functionsToBeExecuted.Values)
                {
                    long expectedCount = GetExpectedFunctionInvocationCountFor(functionExecutionContext.FunctionToBeExecuted);
                    long actualCount;
                    actualFunctionInvocationCounts.TryGetValue(HashHelper.CreateFunctionHash(functionExecutionContext.FunctionToBeExecuted), out actualCount);

                    StringBuilder reportBuilder = new StringBuilder();
                    reportBuilder.Append("Expected function [id=" + HashHelper.CreateFunctionHash(functionExecutionContext.FunctionToBeExecuted) + "] invocations:\t" + expectedCount);
                    reportBuilder.Append(Environment.NewLine);
                    reportBuilder.Append("Actual function [id=" + HashHelper.CreateFunctionHash(functionExecutionContext.FunctionToBeExecuted) + "] invocations:\t\t" + actualCount);
                    //Console.WriteLine(Environment.NewLine + reportBuilder);
                    if (expectedCount != actualCount) { throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Violation in number of function invocations" + Environment.NewLine + reportBuilder); }
                }
            }

            // More asserts ...?

        }
    }


    public class TwoPhaseExecutionContextResult
    {
        //readonly FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[] funcTwoPhaseExecutorThreads;
        readonly DynamicTwoPhaseExecutorThread[] funcTwoPhaseExecutorThreads;

        internal TwoPhaseExecutionContextResult(int numberOfParticipatingWorkerThreads)
        {
            //this.funcTwoPhaseExecutorThreads = new FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[numberOfParticipatingWorkerThreads];
            this.funcTwoPhaseExecutorThreads = new DynamicTwoPhaseExecutorThread[numberOfParticipatingWorkerThreads];
        }

        //internal FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[] WorkerThreads { get { return this.funcTwoPhaseExecutorThreads; } }
        internal DynamicTwoPhaseExecutorThread[] WorkerThreads { get { return this.funcTwoPhaseExecutorThreads; } }
        //public DynamicTwoPhaseExecutorThread[] WorkerThreads { get { return this.funcTwoPhaseExecutorThreads; } }
    }


    //public class TwoPhaseExecutionContextResultSet<TParam1, TParam2, TResult>
    public class TwoPhaseExecutionContextResultSet
    //internal class TwoPhaseExecutionContextResultSet
    {
        //readonly TwoPhaseExecutionContext<TParam1, TParam2, TResult> twoPhaseExecutionContext;
        //readonly TwoPhaseExecutionContextResult<TParam1, TParam2, TResult>[] twoPhaseExecutionContextResults;
        readonly TwoPhaseExecutionContext twoPhaseExecutionContext;
        readonly TwoPhaseExecutionContextResult[] twoPhaseExecutionContextResults;

        public StopWatch StopWatch { get; private set; }

        internal TwoPhaseExecutionContextResultSet(TwoPhaseExecutionContext twoPhaseExecutionContext)
        {
            this.twoPhaseExecutionContext = twoPhaseExecutionContext;
            this.twoPhaseExecutionContextResults = new TwoPhaseExecutionContextResult[this.twoPhaseExecutionContext.NumberOfIterations];
            this.StopWatch = new StopWatch();
        }

        internal string Report
        {
            get
            {
                {
                    StringBuilder reportBuilder = new StringBuilder(GetType().FullName + ": ");
                    reportBuilder.Append(this.twoPhaseExecutionContext.NumberOfIterations);
                    reportBuilder.Append(this.twoPhaseExecutionContext.NumberOfIterations == 1 ? " round " : " rounds of ");
                    reportBuilder.Append(this.twoPhaseExecutionContext.FunctionListing);

                    if (this.twoPhaseExecutionContext.IsExecuted)
                    {
                        reportBuilder.Append(" having approx. " + this.twoPhaseExecutionContext.LatencyListing + " ms latency - took " + this.StopWatch.DurationInMilliseconds + " ms | " + this.StopWatch.DurationInTicks + " ticks");
                        reportBuilder.Append(" (should take ");
                        reportBuilder.Append(this.twoPhaseExecutionContext.MinimumExpextedLatencyInMilliseconds);
                        reportBuilder.Append(" <= ");
                        reportBuilder.Append(StopWatch.DurationInMilliseconds);
                        reportBuilder.Append(" <= ");
                        reportBuilder.Append(this.twoPhaseExecutionContext.MaximumExpectedLatencyInMilliseconds);
                        reportBuilder.Append(" ms)");
                        if (this.twoPhaseExecutionContext.NumberOfConcurrentWorkerThreads == 1) reportBuilder.Append(" (extra 1-thread-only-latency-expectation penalty added...)");
                        if (this.twoPhaseExecutionContext.IsMergedWithOneOrMoreSingleThreadContexts) reportBuilder.Append(" (extra 1-thread-only-latency-expectation penalty added...)");
                        //reportBuilder.Append(" (expected function invocations: " + GetExpectedFunctionInvocationCountFor(this.twoPhaseExecutionContext.) + ")");
                    }
                    else
                        reportBuilder.Append(" (context not yet executed)");

                    return reportBuilder.ToString();
                }
            }
        }

        //public TwoPhaseExecutionContextResult[] ExecutionResult { get { return this.twoPhaseExecutionContextResults; } }
        internal TwoPhaseExecutionContextResult[] ExecutionResult { get { return this.twoPhaseExecutionContextResults; } }

        //public FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult> this[int iterationIndex, int concurrentThreadIndex]
        //public int this[int iterationIndex, int concurrentThreadIndex]
        //{ }

        //public FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult> this[int iterationIndex, int concurrentThreadIndex]
        internal DynamicTwoPhaseExecutorThread this[int iterationIndex, int concurrentThreadIndex]
        {
            get
            {
                if (this.twoPhaseExecutionContextResults == null || this.twoPhaseExecutionContextResults.Length < 1)
                    throw new Exception("No TwoPhaseExecutionContextResults are available");
                if (iterationIndex > this.twoPhaseExecutionContextResults.Length - 1 && this.twoPhaseExecutionContextResults.Length == 1)
                    throw new Exception("Result set contains only 1 iteration... Really no point is asking for iteration #2 (zero-based) then, is it?");
                if (iterationIndex > this.twoPhaseExecutionContextResults.Length - 1)
                    throw new Exception("Result set contains only " + this.twoPhaseExecutionContextResults.Length + " iterations... Really no point is asking for iteration #" + (iterationIndex + 1) + " (zero-based) then, is it?");

                TwoPhaseExecutionContextResult twoPhaseExecutionContextResult = this.twoPhaseExecutionContextResults[iterationIndex];

                //FuncTwoPhaseExecutorThread<TParam1, TParam2, TResult>[] funcTwoPhaseExecutorThreads = twoPhaseExecutionContextResult.WorkerThread;
                DynamicTwoPhaseExecutorThread[] funcTwoPhaseExecutorThreads = twoPhaseExecutionContextResult.WorkerThreads;
                if (funcTwoPhaseExecutorThreads == null || funcTwoPhaseExecutorThreads.Length < 1)
                    throw new Exception("No FuncTwoPhaseExecutorThreads (worker threads) are available");
                if (concurrentThreadIndex > funcTwoPhaseExecutorThreads.Length - 1 && funcTwoPhaseExecutorThreads.Length == 1)
                    throw new Exception("Result set contains only 1 worker thread... Really no point is asking for thread #2 (zero-based) then, is it?");
                if (concurrentThreadIndex > funcTwoPhaseExecutorThreads.Length - 1)
                    throw new Exception("Result set contains only " + funcTwoPhaseExecutorThreads.Length + " worker threads... Really no point is asking for thread #" + (concurrentThreadIndex + 1) + " (zero-based) then, is it?");

                return twoPhaseExecutionContextResult.WorkerThreads[concurrentThreadIndex];
            }
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
