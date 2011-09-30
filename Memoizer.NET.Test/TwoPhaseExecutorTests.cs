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

//#pragma warning disable 162
namespace Memoizer.NET.Test
{

    /// <summary>
    /// Well, not really a test... rather an instrumented demo
    /// </summary>
    [TestFixture]
    class TwoPhaseExecutorTests
    {

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
                    new ExpensiveTask(twoPhaseExecutor.Barrier).Start();
                }
            else
                for (int i = 0; i < NUMBER_OF_PARTICIPANTS; ++i)
                    new ExpensiveTask(twoPhaseExecutor.Barrier).Start();

            // 3. Start the two-phase executor
            twoPhaseExecutor.Start();
        }
    }


    public class ExpensiveTask : AbstractTwoPhaseExecutorThread
    {
        static int TASK_COUNTER;

        public ExpensiveTask(Barrier barrier) : base(barrier, true)
        {
            TaskNumber = Interlocked.Increment(ref TASK_COUNTER);
            ParticipantNumber = Interlocked.Increment(ref PARTICIPANT_COUNTER);
            Action = () => Console.WriteLine("Barrier participant #" + ParticipantNumber + " [invocation #" + ExecutionIndex + "] [" + TwoPhaseExecutor.GetThreadInfo() + "]");

            if (Instrumentation)
                Console.WriteLine(this.GetType().Name + " #" + TaskNumber + " created... [(possible) barrier participant #" + ParticipantNumber + "]");
        }
    }
}
