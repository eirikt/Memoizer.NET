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
    /// A class for synchronized execution of a number of worker/task threads.
    /// All participating worker/task threads must be a <code>AbstractTwoPhaseExecutorThread</code>-derived instance. 
    /// </remarks>
    public sealed class TwoPhaseExecutor
    {
        /// <summary>
        /// The thread barrier.
        /// Must be distributed to all participating worker/task threads.
        /// </summary>
        public Barrier Barrier { get; private set; }

        /// <summary>
        /// The overall number of participating workers/tasks in this two-phase execution. 
        /// </summary>
        public int NumberOfParticipants { get; private set; }

        /// <summary>
        /// If set to <code>true</code>, info will be written to console.
        /// Default is <code>false</code>.
        /// </summary>
        public bool Instrumentation { get; set; }

        public TwoPhaseExecutor(int numberOfParticipants, bool instrumentation = false)
        {
            if (numberOfParticipants < 1)
                throw new ArgumentException("Number of participating tasks must be a non-zero natural number");

            NumberOfParticipants = numberOfParticipants;
            Instrumentation = instrumentation;
            if (Instrumentation)
                Console.WriteLine("Phase 0: Creating barrier, managing at most " + NumberOfParticipants + " phased tasks, + 1 main thread");
            Barrier = new Barrier((NumberOfParticipants + 1), barrier =>
            {
                if (Instrumentation)
                {
                    switch (barrier.CurrentPhaseNumber)
                    {
                        case 0:
                            Console.WriteLine("Phase 1: releasing all tasks simultaneously");
                            break;

                        case 1:
                            Console.WriteLine("Phase 2: all tasks have finished; cleaning up and terminating all task threads");
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
                Console.WriteLine("Main thread: Arriving at 1st barrier rendevouz - probably as one of the last ones, releasing all tasks when all have reach 1st barrier...");
            Barrier.SignalAndWait(Timeout.Infinite);

            if (Instrumentation)
                Console.WriteLine("Main thread: Arriving at 2nd barrier rendevouz - probably as one of the first ones, waiting for all tasks to complete...");
            Barrier.SignalAndWait(Timeout.Infinite);
        }

#pragma warning disable 612,618
        public static string GetThreadInfo()
        {
            return
                "OS thread ID=" + AppDomain.GetCurrentThreadId() + ", " +
                "Managed thread ID=" + Thread.CurrentThread.GetHashCode() + "/" + Thread.CurrentThread.ManagedThreadId;
        }
    }
#pragma warning restore 612,618


    public static class BarrierExtensionMethods
    {
        public static string GetInfo(this Barrier barrier)
        {
            return "Barrier phase is " + barrier.CurrentPhaseNumber + ", remaining participants are " + (barrier.ParticipantsRemaining) + " of a total of " + (barrier.ParticipantCount - 1) + " (plus main thread)";
        }
    }


    /// <remarks>
    /// Abstract base class for <code>TwoPhaseExecutor</code> worker/task threads.
    /// </remarks>
    public abstract class AbstractTwoPhaseExecutorThread
    {
        Thread thread;

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
    /// E.g. do notice how to correctly get hold of <code>TaskNumer</code> and <code>ParticipantNumber</code>.
    /// </remarks>
    public class TrivialTask : AbstractTwoPhaseExecutorThread
    {
        static int TASK_COUNTER;

        public TrivialTask(Barrier barrier)
            : base(barrier, true)
        {
            TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
            ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);
            Action = () => Console.WriteLine("Barrier participant #" + ParticipantNumber + " [invocation #" + ExecutionIndex + "] [" + TwoPhaseExecutor.GetThreadInfo() + "]");
            if (Instrumentation)
                Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]");
        }
    }
}
