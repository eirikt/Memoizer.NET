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
    /// <p/>
    /// ... 
    /// </remarks>
    // TODO: Rename to PhasedExecutor!
    public sealed class TwoPhaseExecutor
    {
        /// <summary>
        /// The thread barrier.
        /// Must be distributed to all participating worker threads.
        /// </summary>
        public Barrier Barrier { get; private set; }

        /// <summary>
        /// The number of participating worker threads in this two-phase execution. 
        /// </summary>
        internal int NumberOfParticipatingWorkerThreads { get { return Barrier.ParticipantCount - 1; } }

        /// <summary>
        /// If set to <code>true</code>, info will be written to console.
        /// Default is <code>false</code>.
        /// </summary>
        public bool Instrumentation { get; set; }

        public bool IsMerged { get; set; }


        public TwoPhaseExecutor(int numberOfParticipants, bool instrumentation = false)
        {
            if (numberOfParticipants < 0) { throw new ArgumentException("Number of participating worker threads cannot be less than zero"); }
            //if (numberOfParticipants < 1) { Console.WriteLine("No worker threads are attending..."); }

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
        public bool Instrumented { get; private set; }

        /// <summary>
        /// Explicit tag for recognizing this particular thread when instrumenting execution.
        /// </summary>
        public string Tag { get; private set; }

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

    public class DynamicTwoPhaseExecutorThread : AbstractTwoPhaseExecutorThread
    {
        static int TASK_COUNTER;

        internal readonly dynamic invocable;
        internal readonly dynamic originalInvocable;
        internal readonly dynamic[] args;

        public dynamic Result
        {
            get;
            private set;
        }

        internal DynamicTwoPhaseExecutorThread(dynamic invocable,
                                               dynamic originalInvocable,
                                               dynamic[] args,
                                               Barrier barrier = default(Barrier),
                                               bool instrumentation = false,
                                               string tag = null)
            : base(barrier, instrumentation, tag)
        {
            this.invocable = invocable;
            this.originalInvocable = originalInvocable;
            this.args = args;
            TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
            ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);

            Action = () => Result = this.invocable.DynamicInvoke(this.args);

            if (Instrumented)
                if (IsTagged)
                    Console.WriteLine(GetType().Namespace + "." + GetType().Name + " #" + TaskNumber + " ['" + Tag + "'] created... [(Possible) TwoPhaseExecutor.Barrier participant #" + ParticipantNumber + "]");
                else
                    Console.WriteLine(GetType().Namespace + "." + GetType().Name + " #" + TaskNumber + " created... [(Possible) TwoPhaseExecutor.Barrier participant #" + ParticipantNumber + "]");
        }
    }


    // TODO: extend to Action and up-to four Func input arguments

    public static partial class FuncExtensionMethods
    {
        const int DEFAULT_NUMBER_OF_ITERATIONS = 1;
        const int DEFAULT_NUMBER_OF_WORKER_THREADS = 1;


        public static TwoPhaseExecutionContext CreatePhasedExecutionContext<TResult>(
            this Func<TResult> function,
            dynamic[] args = null,
            int threads = DEFAULT_NUMBER_OF_WORKER_THREADS,
            int iterations = DEFAULT_NUMBER_OF_ITERATIONS,
            bool report = false,
            bool concurrent = true,
            bool memoize = false,
            long functionLatency = default(long),
            //bool ignoreLatency = false,
            bool instrumentation = false,
            string tag = null,
            Action<string> loggingMethod = null)
        {
            return new TwoPhaseExecutionContext(function, args, threads, iterations, report, concurrent, memoize, false, functionLatency, instrumentation, tag, loggingMethod);
        }


        public static TwoPhaseExecutionContext CreateExecutionContext<TParam1, TParam2, TResult>(
            this Func<TParam1, TParam2, TResult> functionToBeMemoized,
            dynamic[] args = null,
            //int numberOfConcurrentThreadsWitinhEachIteration = 1,
            //int numberOfIterations = 1,
            int threads = 1,
            int iterations = 1,
            bool concurrent = true,
            bool memoize = false,
            //bool memoizerClearing = false, // Remove!
            bool idempotent = true, // Remove...?
            long functionLatency = default(long), // Remove...?
            bool instrumentation = false,
            string tag = null
            )
        {
            return new TwoPhaseExecutionContext(functionToBeMemoized,
                                                args,
                                                threads,
                                                iterations,
                                                true,
                                                concurrent,
                                                memoize,
                // memoizerClearing,
                                                idempotent,
                                                functionLatency,
                //false,
                                                instrumentation,
                                                tag,
                                                Console.WriteLine
                                                );
        }
    }





    // TODO: rename to ConcurrentInvocationContext ...?
    public class TwoPhaseExecutionContext
    {
        public const int NUMBER_OF_WARM_UP_ITERATIONS = 4;


        // TODO: rename to InvocationContext ...?
        class FunctionExecutionContext
        {
            //internal bool functionContextExecuted = false;

            //internal T FunctionToBeExecuted { get; set; }
            internal dynamic FunctionToBeExecuted { get; set; }
            internal Type FunctionToBeExecutedType { get; set; }
            internal dynamic[] Args { get; set; }
            internal bool DefaultArguments { get; set; }
            internal int NumberOfConcurrentThreadsWitinhEachIteration { get; set; }
            internal bool IsMemoized { get; set; }
            //internal bool IsMemoizerClearingTask { get; set; }
            internal long LatencyInMilliseconds { get; set; }
            internal bool CalculatedLatency { get; set; }
            //internal bool IgnoreLatency { get; set; }
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

                Type[] genericArguments = FunctionToBeExecuted.GetType().GetGenericArguments();
                if (genericArguments.Length <= 1)
                    stringBuilder.Append("no args, ");
                else
                    if (DefaultArguments)
                        stringBuilder.Append("default args, ");

                //if (IsMemoizerClearingTask)
                //    stringBuilder.Append("mem-clearing");
                //else
                //{

                //if (CalculatedLatency)
                //  stringBuilder.Append("~" + LatencyInMilliseconds + " ms latency");

                if (LatencyInMilliseconds != default(long))
                    if (CalculatedLatency)
                        stringBuilder.Append("~" + LatencyInMilliseconds + " ms latency");
                    else
                        stringBuilder.Append(LatencyInMilliseconds + " ms latency");
                else
                    stringBuilder.Append("latency unknown");

                if (IsMemoized)
                    stringBuilder.Append(", memoized");
                //else
                //    stringBuilder.Append(", non-mem");

                //}
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


        internal TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet;// = new TwoPhaseExecutionContextResultSet(this);

        private Action<string> loggingMethod;

        internal bool MeasureLatency { get; set; }
        internal bool AssertLatency { get; set; }

        //private IDictionary<string, Tuple<long, object>> resultMatrix;
        //private long totalLatency;
        //private int functionInvocationCounts;


        //internal TwoPhaseExecutionContext(Func<TParam1, TParam2, TResult> functionToBeExecuted,
        //internal TwoPhaseExecutionContext(dynamic function,
        //                                  dynamic[] args, 
        //                                  int numberOfConcurrentThreadsWitinhEachIteration,
        //                                  int numberOfIterations,
        //                                  bool concurrent,
        //                                  bool memoize,
        //                                  long functionLatencyInMilliseconds,
        //                                  bool instrumentation,
        //                                  string tag)
        //    :this((dynamic)function, (dynamic[])args, numberOfConcurrentThreadsWitinhEachIteration, numberOfIterations,concurrent, memoize, true,functionLatencyInMilliseconds, instrumentation, tag)
        //{ }

        internal TwoPhaseExecutionContext(dynamic functionToBeExecuted,
                                          dynamic[] args,
                                          int numberOfConcurrentThreadsWitinhEachIteration,
                                          int numberOfIterations,
            bool report,
                                          bool concurrent,
                                          bool memoize,
            //bool isMemoizerClearing,
                                          bool idempotentFunction,
                                          long functionLatencyInMilliseconds,
            //bool ignoreLatency,
                                          bool instrumentation,
                                          string tag,
                                          Action<string> loggingMethod// = null//new Action<string>(delegate(string logLine){Console.WriteLine(logLine);});
            )
        {
            if (numberOfIterations < 1) { throw new ArgumentException("Number-of-iterations parameter ('iterations') cannot be less than 1"); }
            if (numberOfConcurrentThreadsWitinhEachIteration < 1) { throw new ArgumentException("Number-of-worker-threads parameter ('threads') cannot be less than 1"); }

            //if (numberOfIterations < 1) { throw new ArgumentException("No iterations are declared - context cannot be executed"); }
            //if (numberOfConcurrentThreadsWitinhEachIteration < 1) { throw new ArgumentException("No worker threads are declared - context cannot be executed"); }


            Type[] genericArguments = functionToBeExecuted.GetType().GetGenericArguments();
            //if (genericArguments.Length < 1)
            //{

            //}
            //else
            //{
            if (args != null)
            {
                if (genericArguments.Length - 1 != args.Length) // -1 since the last generic parameter is the TResult
                    throw new ArgumentException("Number-of-arguments parameter ('args') does not match the function signature");
            }
            else
            {
                DefaultArguments = true;
                object[] defaultValueArguments = new object[genericArguments.Length - 1]; // -1 since the last generic parameter is the TResult
                for (int i = 0; i < genericArguments.Length - 1; ++i)
                {
                    Type genericArgument = genericArguments[i];
                    if (genericArgument.IsValueType)
                        defaultValueArguments[i] = Activator.CreateInstance(genericArgument);
                    else if (genericArgument == typeof(String))
                        defaultValueArguments[i] = default(String);
                }
                args = defaultValueArguments;
            }
            //}

            FunctionExecutionContext functionExecutionContext = new FunctionExecutionContext
            {
                FunctionToBeExecuted = functionToBeExecuted,
                FunctionToBeExecutedType = functionToBeExecuted.GetType(),
                Args = args,
                DefaultArguments = DefaultArguments,
                NumberOfConcurrentThreadsWitinhEachIteration = numberOfConcurrentThreadsWitinhEachIteration,
                IsMemoized = memoize,
                //IsMemoizerClearingTask = isMemoizerClearing,
                LatencyInMilliseconds = functionLatencyInMilliseconds,
                //CalculatedLatency = functionLatencyInMilliseconds == default(long),
                CalculatedLatency = false,
                //IgnoreLatency = ignoreLatency,
                Instrumentation = instrumentation,
                Tag = tag
            };

            NumberOfIterations = numberOfIterations;

            this.functionsToBeExecuted = new Dictionary<string, FunctionExecutionContext>(1) { { HashHelper.CreateFunctionHash(functionToBeExecuted, args), functionExecutionContext } };
            this.isIdempotentContext = idempotentFunction;
            this.instrumentation = instrumentation;
            this.twoPhaseExecutionContextResultSet = new TwoPhaseExecutionContextResultSet(this);

            this.loggingMethod = loggingMethod == default(Action<string>)
                ? logLine => Console.WriteLine(logLine)
                : loggingMethod;

            if (report)
                this.loggingMethod(this.twoPhaseExecutionContextResultSet.Report);
        }


        internal bool DefaultArguments { get; private set; }

        internal bool IsExecuted { get; private set; }

        //internal int NumberOfIterations { get { return this.numberOfIterations; } }
        public int NumberOfIterations { get; private set; }

        public int NumberOfConcurrentWorkerThreads { get { return this.functionsToBeExecuted.Aggregate(0, (current, functionExecutionContext) => current + functionExecutionContext.Value.NumberOfConcurrentThreadsWitinhEachIteration); } }

        public long NumberOfFunctions { get { return this.functionsToBeExecuted.Count; } }

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
        public long MinimumExpectedLatencyInMilliseconds { get; private set; }

        public PhasedExecutionContextResult Results
        {
            get { return new PhasedExecutionContextResult(this); }
        }

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
            if (anotherTwoPhaseExecutionContext == default(TwoPhaseExecutionContext)) { throw new ArgumentException("TwoPhaseExecutionContext parameter cannot be null"); }

            if (this.Equals(anotherTwoPhaseExecutionContext))
            {
                return this;
            }

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
                this.loggingMethod("Oops: " + e);
            }
            if (!anotherTwoPhaseExecutionContext.isIdempotentContext)
                mergedTwoPhaseExecutionContext.isIdempotentContext = false;

            mergedTwoPhaseExecutionContext.twoPhaseExecutionContextResultSet = new TwoPhaseExecutionContextResultSet(mergedTwoPhaseExecutionContext);

            return mergedTwoPhaseExecutionContext;
        }


        [Obsolete("Include this as Execute (named) parameters")]
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





        public TwoPhaseExecutionContext Execute(bool measureLatency = false, bool assertLatency = false, bool report = true)
        {
            MeasureLatency = measureLatency;
            AssertLatency = assertLatency;

            //#region Report [before]
            //if (report) { this.loggingMethod(new TwoPhaseExecutionContextResultSet(this).Report); }
            //#endregion

            //#region Idempotency
            //if (!this.isIdempotentContext) { throw new NotImplementedException("Non-idempotent function are kind of N/A in this context... I think"); }
            //#endregion

            #region Expected latency
            if (MeasureLatency)
            {
                //this.loggingMethod("--- Expected latency ---");
                foreach (var functionExecutionContext in this.functionsToBeExecuted.Values)
                {
                    //if (!functionExecutionContext.IgnoreLatency && functionExecutionContext.LatencyInMilliseconds == default(long))
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

                        if (MinimumExpectedLatencyInMilliseconds == default(long))
                            MinimumExpectedLatencyInMilliseconds = functionExecutionContext.LatencyInMilliseconds;
                        else if (functionExecutionContext.LatencyInMilliseconds < MinimumExpectedLatencyInMilliseconds)
                            MinimumExpectedLatencyInMilliseconds = functionExecutionContext.LatencyInMilliseconds;
                    }

                    long expectedLatency = functionExecutionContext.LatencyInMilliseconds + CalculateContentionOverheadInMilliseconds();

                    if (functionExecutionContext.IsMemoized)
                    {
                        if (expectedLatency > MaximumExpectedLatencyInMilliseconds)
                            MaximumExpectedLatencyInMilliseconds = expectedLatency;

                        if (expectedLatency < MinimumExpectedLatencyInMilliseconds)
                            MinimumExpectedLatencyInMilliseconds = expectedLatency;
                    }
                    else
                    {
                        if (NumberOfIterations < 1)
                            expectedLatency *= 1; // Too avoid expectedLatency == 0 and MaximumExpectedLatencyInMilliseconds == 0
                        else
                            expectedLatency *= NumberOfIterations;

                        if (expectedLatency > MaximumExpectedLatencyInMilliseconds)
                            MaximumExpectedLatencyInMilliseconds = expectedLatency;

                        if (expectedLatency < MinimumExpectedLatencyInMilliseconds)
                            MinimumExpectedLatencyInMilliseconds = expectedLatency;
                    }
                    functionExecutionContext.CalculatedLatency = true;
                }
            }
            #endregion

            #region Two-phased execution
            //this.loggingMethod("--- Two-phased execution ---");
            //TwoPhaseExecutionContextResultSet twoPhaseExecutionContextResultSet = new TwoPhaseExecutionContextResultSet(this);
            this.twoPhaseExecutionContextResultSet.StopWatch.Start();
            for (int i = 0; i < NumberOfIterations; ++i)
            {
                this.twoPhaseExecutor = new TwoPhaseExecutor(NumberOfConcurrentWorkerThreads, this.instrumentation);
                this.workerThreads = new List<DynamicTwoPhaseExecutorThread>(NumberOfConcurrentWorkerThreads);

                foreach (var functionExecutionContext in functionsToBeExecuted.Values)
                {
                    Type[] genericArguments;

                    for (int j = 0; j < functionExecutionContext.NumberOfConcurrentThreadsWitinhEachIteration; ++j)
                    {
                        int numberOfFunctionArguments;

                        FunctionExecutionContext context;
                        DynamicTwoPhaseExecutorThread dynamicTwoPhaseExecutorThread;

                        if (functionExecutionContext.IsMemoized)
                        {
                            genericArguments = functionExecutionContext.FunctionToBeExecuted.GetType().GetGenericArguments();
                            numberOfFunctionArguments = genericArguments.Length - 1; // -1 since the last generic parameter is the TResult

                            switch (numberOfFunctionArguments)
                            {
                                case 0: throw new NotImplementedException();
                                case 1: throw new NotImplementedException();
                                case 2:
                                    if (functionExecutionContext.Args == null || functionExecutionContext.Args.Count() != 2) { throw new ApplicationException("Missing args"); }
                                    context = functionExecutionContext;
                                    Func<dynamic, dynamic, dynamic> func2 = (arg1, arg2) =>
                                        new MemoizerFactory(context.FunctionToBeExecuted).GetMemoizer().InvokeWith(new object[] { arg1, arg2 });

                                    dynamicTwoPhaseExecutorThread =
                                       new DynamicTwoPhaseExecutorThread(invocable: func2,
                                                                         originalInvocable: functionExecutionContext.FunctionToBeExecuted,
                                                                         args: functionExecutionContext.Args,
                                                                         barrier: twoPhaseExecutor.Barrier,
                                                                         instrumentation: functionExecutionContext.Instrumentation,
                                                                         tag: functionExecutionContext.Tag);

                                    this.workerThreads.Add(dynamicTwoPhaseExecutorThread);

                                    dynamicTwoPhaseExecutorThread.Start();

                                    break;

                                case 3: throw new NotImplementedException();
                            }
                        }
                        else
                        {
                            genericArguments = functionExecutionContext.FunctionToBeExecuted.GetType().GetGenericArguments();
                            numberOfFunctionArguments = genericArguments.Length - 1; // -1 since the last generic parameter is the TResult
                            switch (numberOfFunctionArguments)
                            {
                                case 0:
                                    //if (functionExecutionContext.Args == null)
                                    //{
                                    //    throw new ApplicationException("Missing args");
                                    //}
                                    //if (functionExecutionContext.Args.Count() != 2)
                                    //{
                                    //    throw new ApplicationException("Missing args");
                                    //}
                                    dynamicTwoPhaseExecutorThread =
                                       new DynamicTwoPhaseExecutorThread(invocable: functionExecutionContext.FunctionToBeExecuted,
                                                                         originalInvocable: functionExecutionContext.FunctionToBeExecuted,
                                                                         args: functionExecutionContext.Args,
                                                                         barrier: twoPhaseExecutor.Barrier,
                                                                         instrumentation: functionExecutionContext.Instrumentation,
                                                                         tag: functionExecutionContext.Tag);
                                    this.workerThreads.Add(dynamicTwoPhaseExecutorThread);
                                    dynamicTwoPhaseExecutorThread.Start();
                                    break;

                                case 1: throw new NotImplementedException();
                                case 2:
                                    if (functionExecutionContext.Args == null || functionExecutionContext.Args.Count() != 2) { throw new ApplicationException("Missing args"); }
                                    dynamicTwoPhaseExecutorThread =
                                       new DynamicTwoPhaseExecutorThread(invocable: functionExecutionContext.FunctionToBeExecuted,
                                                                         originalInvocable: functionExecutionContext.FunctionToBeExecuted,
                                                                         args: functionExecutionContext.Args,
                                                                         barrier: twoPhaseExecutor.Barrier,
                                                                         instrumentation: functionExecutionContext.Instrumentation,
                                                                         tag: functionExecutionContext.Tag);
                                    this.workerThreads.Add(dynamicTwoPhaseExecutorThread);
                                    dynamicTwoPhaseExecutorThread.Start();
                                    break;

                                case 3: throw new NotImplementedException();
                            }
                        }
                    }
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
            if (report) { this.loggingMethod(this.twoPhaseExecutionContextResultSet.Report); }
            #endregion

            return this;
        }


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





        public TwoPhaseExecutionContext Verify(bool report = true,
                                               bool listResults = false,
                                               IDictionary<string, object> expectedResults = default(IDictionary<string, object>),
                                               long expectedMinimumLatency = 0L,
                                               long expectedMaximumLatency = Int64.MaxValue,
                                               IDictionary<string, long> actualFunctionInvocationCounts = default(IDictionary<string, long>))
        {
            if (!IsExecuted) { throw new InvalidOperationException("Execution context is not yet executed"); }

            StringBuilder reportBuilder = new StringBuilder();
            StringBuilder errorReportBuilder = new StringBuilder();

            // Assert results of the executed functions
            reportBuilder.Append("PhasedExecutor: ");
            reportBuilder.Append(NumberOfIterations);
            reportBuilder.Append(NumberOfIterations == 1 ? " round of " : " rounds of ");
            reportBuilder.Append(FunctionListing);

            //if (IsExecuted)
            //{
            //    reportBuilder.Append(/*" having approx. " + LatencyListing + " ms latency - t*/" took " + this.twoPhaseExecutionContextResultSet.StopWatch.DurationInMilliseconds + " ms | " + this.twoPhaseExecutionContextResultSet.StopWatch.DurationInTicks + " ticks");

            //    if (MeasureLatency)
            //    {
            //        reportBuilder.Append(" (should take [");
            //        reportBuilder.Append(MinimumExpectedLatencyInMilliseconds);
            //        reportBuilder.Append(" <= ");
            //        reportBuilder.Append(this.twoPhaseExecutionContextResultSet.StopWatch.DurationInMilliseconds);
            //        reportBuilder.Append(" <= ");
            //        reportBuilder.Append(MaximumExpectedLatencyInMilliseconds);
            //        reportBuilder.Append("] ms)");
            //        //if (this.twoPhaseExecutionContext.NumberOfConcurrentWorkerThreads == 1) reportBuilder.Append(" (extra 1-thread-only-latency-expectation penalty added...)");
            //        if (IsMergedWithOneOrMoreSingleThreadContexts) reportBuilder.Append(" (extra 1-thread-only-latency-expectation penalty added...)");
            //        //reportBuilder.Append(" (expected function invocations: " + GetExpectedFunctionInvocationCountFor(this.twoPhaseExecutionContext.) + ")");
            //        //}
            //        //else
            //        //    reportBuilder.Append(" (context not yet executed)");
            //    }
            //    //else
            //    //    reportBuilder.Append(" (no latency...)");

            //    if (expectedResults == default(IDictionary<string, object>) || expectedResults.Count <= 0)
            //    {
            //        reportBuilder.Append(" (no expected results given)");

            //        if (listResults)
            //            reportBuilder.Append(Environment.NewLine);
            //    }
            //}

            if (expectedResults == default(IDictionary<string, object>) || expectedResults.Count <= 0)
            {
                // Expected results not given
                reportBuilder.Append(" expected results not given");

                if (listResults)
                {
                    reportBuilder.Append(Environment.NewLine);

                    TwoPhaseExecutionContextResult[] twoPhaseExecutionContextResults = this.twoPhaseExecutionContextResultSet.ExecutionResult;
                    if (twoPhaseExecutionContextResults.Length == 0)
                        reportBuilder.Append("No results available");
                    else
                    {
                        reportBuilder.Append("Result listing:");

                        for (int i = 0; i < NumberOfIterations; ++i)
                        {
                            TwoPhaseExecutionContextResult twoPhaseExecutionContextResult = twoPhaseExecutionContextResults[i];
                            DynamicTwoPhaseExecutorThread[] dynamicTwoPhaseExecutorThreads = twoPhaseExecutionContextResult.WorkerThreads;
                            long numberOfConcurrentThreadsForThisParticularFuncContext = dynamicTwoPhaseExecutorThreads.Length;
                            for (int j = 0; j < numberOfConcurrentThreadsForThisParticularFuncContext; ++j)
                            {
                                DynamicTwoPhaseExecutorThread dynamicTwoPhaseExecutorThread = dynamicTwoPhaseExecutorThreads[j];

                                object actualResult = dynamicTwoPhaseExecutorThread.Result;

                                reportBuilder.Append(Environment.NewLine);
                                reportBuilder.Append("\t");
                                reportBuilder.Append("Result [" + i + "][" + j + "]: " + actualResult);
                            }
                        }
                    }
                }
                else
                {

                }
            }
            else
            {
                TwoPhaseExecutionContextResult[] twoPhaseExecutionContextResults = this.twoPhaseExecutionContextResultSet.ExecutionResult;

                for (int i = 0; i < NumberOfIterations; ++i)
                {
                    TwoPhaseExecutionContextResult twoPhaseExecutionContextResult = twoPhaseExecutionContextResults[i];
                    DynamicTwoPhaseExecutorThread[] dynamicTwoPhaseExecutorThreads = twoPhaseExecutionContextResult.WorkerThreads;
                    for (int j = 0; j < NumberOfConcurrentWorkerThreads/*dynamicTwoPhaseExecutorThreads.Length*/; ++j)
                    {
                        DynamicTwoPhaseExecutorThread dynamicTwoPhaseExecutorThread = dynamicTwoPhaseExecutorThreads[j];

                        object actualResult = dynamicTwoPhaseExecutorThread.Result;

                        string funcHash = HashHelper.CreateFunctionHash(dynamicTwoPhaseExecutorThread.originalInvocable, dynamicTwoPhaseExecutorThread.args);
                        dynamic expectedResult;
                        expectedResults.TryGetValue(funcHash, out expectedResult);

                        if (!actualResult.Equals(expectedResult))
                        {
                            //StringBuilder resultErrorReportBuilder = new StringBuilder();
                            if (expectedResult == null)
                                errorReportBuilder.Append("Expected result: [no expected results found for action with hash '" + HashHelper.CreateFunctionHash(dynamicTwoPhaseExecutorThread.originalInvocable, dynamicTwoPhaseExecutorThread.args) + "']");
                            else
                                errorReportBuilder.Append("Expected result: " + expectedResult);
                            errorReportBuilder.Append(Environment.NewLine);
                            errorReportBuilder.Append("Actual result  : " + actualResult);
                            throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Violation in result" + Environment.NewLine + errorReportBuilder);
                        }
                    }
                }
            }

            if (AssertLatency)
            {
                // Assert latency/duration
                long duration = this.twoPhaseExecutionContextResultSet.StopWatch.DurationInMilliseconds;

                //StringBuilder errorReportBuilder = new StringBuilder();
                //reportBuilder.Append("Duration: " + duration + " ms | " + this.twoPhaseExecutionContextResultSet.StopWatch.DurationInTicks + " ticks");

                //if (!MeasureLatency)
                //{

                //}
                //else
                //{
                if (duration > MaximumExpectedLatencyInMilliseconds)
                {
                    errorReportBuilder.Append(" (should take ");
                    errorReportBuilder.Append(MinimumExpectedLatencyInMilliseconds);
                    errorReportBuilder.Append(" <= ");
                    errorReportBuilder.Append(duration);
                    errorReportBuilder.Append(" <= ");
                    errorReportBuilder.Append(MaximumExpectedLatencyInMilliseconds);
                    errorReportBuilder.Append(" ms) [Internal demand]");
                    throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Latency violation! TOO SLOW" + Environment.NewLine + errorReportBuilder);

                }
                if (duration < MinimumExpectedLatencyInMilliseconds)
                {
                    //StringBuilder errorReportBuilder = new StringBuilder();
                    //errorReportBuilder.Append("Duration: " + duration + " ms | " + this.twoPhaseExecutionContextResultSet.StopWatch.DurationInTicks + " ticks");
                    errorReportBuilder.Append(" (should take ");
                    errorReportBuilder.Append(MinimumExpectedLatencyInMilliseconds);
                    errorReportBuilder.Append(" <= ");
                    errorReportBuilder.Append(duration);
                    errorReportBuilder.Append(" <= ");
                    errorReportBuilder.Append(MaximumExpectedLatencyInMilliseconds);
                    errorReportBuilder.Append(" ms) [Internal demand]");
                    throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Latency violation! TOO FAST" + Environment.NewLine + errorReportBuilder);
                }
                if (duration > expectedMaximumLatency)
                {
                    //StringBuilder errorReportBuilder = new StringBuilder();
                    //errorReportBuilder.Append("Duration: " + duration + " ms | " + this.twoPhaseExecutionContextResultSet.StopWatch.DurationInTicks + " ticks");
                    errorReportBuilder.Append(" (should take ");
                    errorReportBuilder.Append(expectedMinimumLatency);
                    errorReportBuilder.Append(" <= ");
                    errorReportBuilder.Append(duration);
                    errorReportBuilder.Append(" <= ");
                    errorReportBuilder.Append(expectedMaximumLatency);
                    errorReportBuilder.Append(" ms) [External demand]");
                    throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Latency violation, TOO SLOW!" + Environment.NewLine + errorReportBuilder);
                }
                if (duration < expectedMinimumLatency)
                {
                    //StringBuilder errorReportBuilder = new StringBuilder();

                    //errorReportBuilder.Append("Duration: " + duration + " ms | " + this.twoPhaseExecutionContextResultSet.StopWatch.DurationInTicks + " ticks");
                    errorReportBuilder.Append(" (should take ");
                    errorReportBuilder.Append(expectedMinimumLatency);
                    errorReportBuilder.Append(" <= ");
                    errorReportBuilder.Append(duration);
                    errorReportBuilder.Append(" <= ");
                    errorReportBuilder.Append(expectedMaximumLatency);
                    errorReportBuilder.Append(" ms) [External demand]");
                    throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Latency violation! TOO FAST" + Environment.NewLine + errorReportBuilder);
                }
                //}
            }
            else
            {
                reportBuilder.Append(", latency boundaries demands not given");
            }


            // Assert number of invocations (mostly useful when memoizing...)
            if (actualFunctionInvocationCounts != default(IDictionary<string, long>))
            {
                foreach (var functionExecutionContext in this.functionsToBeExecuted.Values)
                {
                    long expectedCount = GetExpectedFunctionInvocationCountFor(functionExecutionContext.FunctionToBeExecuted);
                    long actualCount;
                    actualFunctionInvocationCounts.TryGetValue(HashHelper.CreateFunctionHash(functionExecutionContext.FunctionToBeExecuted), out actualCount);

                    //StringBuilder numberOfInvocationsErrorReportBuilder = new StringBuilder();
                    errorReportBuilder.Append("Expected function [id=" + HashHelper.CreateFunctionHash(functionExecutionContext.FunctionToBeExecuted) + "] invocations:\t" + expectedCount);
                    errorReportBuilder.Append(Environment.NewLine);
                    errorReportBuilder.Append("Actual function [id=" + HashHelper.CreateFunctionHash(functionExecutionContext.FunctionToBeExecuted) + "] invocations:\t\t" + actualCount);
                    //this.loggingMethod(Environment.NewLine + errorReportBuilder);
                    if (expectedCount != actualCount) { throw new ApplicationException("Memoizer.NET.TwoPhaseExecutor: Violation in number of function invocations" + Environment.NewLine + errorReportBuilder); }
                }
            }


            // More asserts ...?


            if (report) { this.loggingMethod(reportBuilder.ToString()); }

            return this;
        }


        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (var functionExecutionContext in functionsToBeExecuted.Values)
            {
                bool firstTime = false;
                long objectId = HashHelper.GetObjectId(functionExecutionContext.FunctionToBeExecuted, ref firstTime);

                int parameterHash = 0;

                if (functionExecutionContext.Args == null || functionExecutionContext.Args.Length == 0)
                    parameterHash += 0;

                else if (functionExecutionContext.Args.Length == 1)
                    if (functionExecutionContext.Args[0] == null)
                        parameterHash += 0;
                    else
                        parameterHash += functionExecutionContext.Args[0].GetHashCode();
                else
                    parameterHash += Convert.ToInt32(HashHelper.CreateParameterHash(functionExecutionContext.Args));

                hashCode += Convert.ToInt32(objectId) + parameterHash + functionExecutionContext.NumberOfConcurrentThreadsWitinhEachIteration;
            }
            return hashCode;
        }


        public override bool Equals(object otherObject)
        {
            if (ReferenceEquals(null, otherObject)) { return false; }
            if (ReferenceEquals(this, otherObject)) { return true; }
            TwoPhaseExecutionContext otherTwoPhaseExecutionContext = otherObject as TwoPhaseExecutionContext;
            if (otherTwoPhaseExecutionContext == null) { return false; }
            return this.GetHashCode().Equals(otherTwoPhaseExecutionContext.GetHashCode());
        }


        public override string ToString()
        {
            return this.twoPhaseExecutionContextResultSet.Report;
        }
    }





    internal class TwoPhaseExecutionContextResult
    {
        readonly DynamicTwoPhaseExecutorThread[] funcTwoPhaseExecutorThreads;

        internal TwoPhaseExecutionContextResult(int numberOfParticipatingWorkerThreads)
        {
            this.funcTwoPhaseExecutorThreads = new DynamicTwoPhaseExecutorThread[numberOfParticipatingWorkerThreads];
        }

        internal DynamicTwoPhaseExecutorThread[] WorkerThreads { get { return this.funcTwoPhaseExecutorThreads; } }
    }





    internal class TwoPhaseExecutionContextResultSet
    {
        readonly TwoPhaseExecutionContext twoPhaseExecutionContext;
        internal readonly TwoPhaseExecutionContextResult[] twoPhaseExecutionContextResults;

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
                    StringBuilder reportBuilder = new StringBuilder();
                    reportBuilder.Append("PhasedExecutor: ");
                    reportBuilder.Append(this.twoPhaseExecutionContext.NumberOfIterations);
                    reportBuilder.Append(this.twoPhaseExecutionContext.NumberOfIterations == 1 ? " round of " : " rounds of ");
                    reportBuilder.Append(this.twoPhaseExecutionContext.FunctionListing);

                    if (!this.twoPhaseExecutionContext.IsExecuted)
                    {
                        reportBuilder.Append(" - not yet executed");
                    }
                    if (this.twoPhaseExecutionContext.IsExecuted && this.twoPhaseExecutionContext.MeasureLatency)
                    {
                        reportBuilder.Append(" having approx. " + this.twoPhaseExecutionContext.LatencyListing + " ms latency");//" - took " + this.StopWatch.DurationInMilliseconds + " ms | " + this.StopWatch.DurationInTicks + " ticks");
                        reportBuilder.Append(" (should take [");
                        reportBuilder.Append(this.twoPhaseExecutionContext.MinimumExpectedLatencyInMilliseconds);
                        reportBuilder.Append(", ");
                        reportBuilder.Append(this.twoPhaseExecutionContext.MaximumExpectedLatencyInMilliseconds);
                        reportBuilder.Append("] ms)");
                        if (this.twoPhaseExecutionContext.IsMergedWithOneOrMoreSingleThreadContexts) reportBuilder.Append(" (extra 1-thread-only-latency-expectation penalty added...)");
                    }
                    else
                        if (this.twoPhaseExecutionContext.IsExecuted && !this.twoPhaseExecutionContext.MeasureLatency)
                        {
                            reportBuilder.Append(" took " + this.StopWatch.DurationInMilliseconds + " ms | " + this.StopWatch.DurationInTicks + " ticks");
                        }
                        else
                        {
                            if (this.twoPhaseExecutionContext.NumberOfIterations < 1)
                            {
                                reportBuilder.Append(" (no iterations are declared - context cannot be executed!)");
                            }
                        }
                    if (this.twoPhaseExecutionContext.NumberOfConcurrentWorkerThreads < 1)
                        reportBuilder.Append(" (no worker threads are declared - context cannot be executed!)");

                    return reportBuilder.ToString();
                }
            }
        }

        internal TwoPhaseExecutionContextResult[] ExecutionResult { get { return this.twoPhaseExecutionContextResults; } }

        internal DynamicTwoPhaseExecutorThread this[int iterationIndex, int concurrentThreadIndex]
        {
            get
            {
                if (this.twoPhaseExecutionContextResults == null || this.twoPhaseExecutionContextResults.Length < 1)
                    throw new ApplicationException("No TwoPhaseExecutionContextResults are available");
                if (iterationIndex > this.twoPhaseExecutionContextResults.Length - 1 && this.twoPhaseExecutionContextResults.Length == 1)
                    throw new ApplicationException("Result set contains only 1 iteration... Really no point is asking for iteration #2 (zero-based) then, is it?");
                if (iterationIndex > this.twoPhaseExecutionContextResults.Length - 1)
                    throw new ApplicationException("Result set contains only " + this.twoPhaseExecutionContextResults.Length + " iterations... Really no point is asking for iteration #" + (iterationIndex + 1) + " (zero-based) then, is it?");

                TwoPhaseExecutionContextResult twoPhaseExecutionContextResult = this.twoPhaseExecutionContextResults[iterationIndex];

                DynamicTwoPhaseExecutorThread[] funcTwoPhaseExecutorThreads = twoPhaseExecutionContextResult.WorkerThreads;
                if (funcTwoPhaseExecutorThreads == null || funcTwoPhaseExecutorThreads.Length < 1)
                    throw new ApplicationException("No FuncTwoPhaseExecutorThreads (worker threads) are available");
                if (concurrentThreadIndex > funcTwoPhaseExecutorThreads.Length - 1 && funcTwoPhaseExecutorThreads.Length == 1)
                    throw new ApplicationException("Result set contains only 1 worker thread... Really no point is asking for thread #2 (zero-based) then, is it?");
                if (concurrentThreadIndex > funcTwoPhaseExecutorThreads.Length - 1)
                    throw new ApplicationException("Result set contains only " + funcTwoPhaseExecutorThreads.Length + " worker threads... Really no point is asking for thread #" + (concurrentThreadIndex + 1) + " (zero-based) then, is it?");

                return twoPhaseExecutionContextResult.WorkerThreads[concurrentThreadIndex];
            }
        }
    }


    public class PhasedExecutionContextResult
    {
        readonly TwoPhaseExecutionContext twoPhaseExecutionContext;

        public PhasedExecutionContextResult(TwoPhaseExecutionContext twoPhaseExecutionContext)
        {
            this.twoPhaseExecutionContext = twoPhaseExecutionContext;
        }


        public IList<object> this[int iterationIndex]
        {
            get
            {
                if (this.twoPhaseExecutionContext.twoPhaseExecutionContextResultSet.twoPhaseExecutionContextResults == null || this.twoPhaseExecutionContext.twoPhaseExecutionContextResultSet.twoPhaseExecutionContextResults.Length < 1)
                    throw new ApplicationException("No results are available");
                if (iterationIndex > this.twoPhaseExecutionContext.twoPhaseExecutionContextResultSet.twoPhaseExecutionContextResults.Length - 1 && this.twoPhaseExecutionContext.twoPhaseExecutionContextResultSet.twoPhaseExecutionContextResults.Length == 1)
                    throw new ApplicationException("Result set contains only 1 iteration... Really no point is asking for iteration #2 (zero-based) then, is it?");
                if (iterationIndex > this.twoPhaseExecutionContext.twoPhaseExecutionContextResultSet.twoPhaseExecutionContextResults.Length - 1)
                    throw new ApplicationException("Result set contains only " + this.twoPhaseExecutionContext.twoPhaseExecutionContextResultSet.twoPhaseExecutionContextResults.Length + " iterations... Really no point is asking for iteration #" + (iterationIndex + 1) + " (zero-based) then, is it?");

                TwoPhaseExecutionContextResult twoPhaseExecutionContextResult = this.twoPhaseExecutionContext.twoPhaseExecutionContextResultSet.twoPhaseExecutionContextResults[iterationIndex];
                if (twoPhaseExecutionContextResult == null)
                    throw new ApplicationException("No results are available");

                List<object> results = new List<dynamic>(twoPhaseExecutionContextResult.WorkerThreads.Length);
                foreach (DynamicTwoPhaseExecutorThread workerThread in twoPhaseExecutionContextResult.WorkerThreads)
                    results.Add(workerThread.Result);

                return results;
            }
        }


        public object this[int iterationIndex, int concurrentThreadIndex]
        {
            get
            {
                if (this.twoPhaseExecutionContext.twoPhaseExecutionContextResultSet.twoPhaseExecutionContextResults == null || this.twoPhaseExecutionContext.twoPhaseExecutionContextResultSet.twoPhaseExecutionContextResults.Length < 1)
                    throw new ApplicationException("No results are available");
                if (iterationIndex > this.twoPhaseExecutionContext.twoPhaseExecutionContextResultSet.twoPhaseExecutionContextResults.Length - 1 && this.twoPhaseExecutionContext.twoPhaseExecutionContextResultSet.twoPhaseExecutionContextResults.Length == 1)
                    throw new ApplicationException("Result set contains only 1 iteration... Really no point is asking for iteration #2 (zero-based) then, is it?");
                if (iterationIndex > this.twoPhaseExecutionContext.twoPhaseExecutionContextResultSet.twoPhaseExecutionContextResults.Length - 1)
                    throw new ApplicationException("Result set contains only " + this.twoPhaseExecutionContext.twoPhaseExecutionContextResultSet.twoPhaseExecutionContextResults.Length + " iterations... Really no point is asking for iteration #" + (iterationIndex + 1) + " (zero-based) then, is it?");

                TwoPhaseExecutionContextResult twoPhaseExecutionContextResult = this.twoPhaseExecutionContext.twoPhaseExecutionContextResultSet.twoPhaseExecutionContextResults[iterationIndex];

                DynamicTwoPhaseExecutorThread[] funcTwoPhaseExecutorThreads = twoPhaseExecutionContextResult.WorkerThreads;
                if (funcTwoPhaseExecutorThreads == null || funcTwoPhaseExecutorThreads.Length < 1)
                    throw new ApplicationException("No worker threads are available");
                if (concurrentThreadIndex > funcTwoPhaseExecutorThreads.Length - 1 && funcTwoPhaseExecutorThreads.Length == 1)
                    throw new ApplicationException("Result set contains only 1 worker thread... Really no point is asking for thread #2 (zero-based) then, is it?");
                if (concurrentThreadIndex > funcTwoPhaseExecutorThreads.Length - 1)
                    throw new ApplicationException("Result set contains only " + funcTwoPhaseExecutorThreads.Length + " worker threads... Really no point is asking for thread #" + (concurrentThreadIndex + 1) + " (zero-based) then, is it?");

                List<object> results = new List<dynamic>(twoPhaseExecutionContextResult.WorkerThreads.Length);
                foreach (DynamicTwoPhaseExecutorThread workerThread in twoPhaseExecutionContextResult.WorkerThreads)
                    results.Add(workerThread.Result);

                return results;
            }
        }


        public long Count
        {
            get
            {
                return this.twoPhaseExecutionContext.NumberOfIterations;
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
