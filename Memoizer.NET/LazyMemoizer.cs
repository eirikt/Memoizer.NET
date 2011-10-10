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
using System.Runtime.Caching;

namespace Memoizer.NET
{

    // TODO: to be removed, I guess
    class LazyMemoizer<TParam1, TResult> //: IMemoizer<TParam1, TResult>, IThreadSafe
    {
        static readonly bool IS_THREAD_SAFE = typeof(LazyMemoizer<TParam1, TResult>) is IThreadSafe;

        readonly Lazy<Memoizer<TParam1, TResult>> lazyInitializer;

        // CacheItemPolicy CacheItemPolicy { get; private set; }
        //Action<string> LoggingMethod { get; set; }
        // String MethodName { get; private set; }
        //bool InstrumentInvocations { get { return LoggingMethod != null; } }

        internal LazyMemoizer(Func<TParam1, TResult> methodToBeMemoized, CacheItemPolicy cacheItemPolicy = null)
        {
            if (methodToBeMemoized == null) { throw new ArgumentException("Method to be memoized is missing"); }
            this.lazyInitializer = new Lazy<Memoizer<TParam1, TResult>>(() =>
                new Memoizer<TParam1, TResult>(methodToBeMemoized, cacheItemPolicy), IS_THREAD_SAFE);
        }

        internal TResult InvokeWith(TParam1 param1)
        {
            //if (!InstrumentInvocations)
            //    return this.lazyInitializer.Value.InvokeWith(param1);

            //long startTime = DateTime.Now.Ticks;
            TResult retVal = this.lazyInitializer.Value.InvokeWith(param1);
            //string localMethodId;
            //if (this.invokingType == null || string.IsNullOrEmpty(this.nameOfMethodToBeMemoized))
            //    localMethodId = this.methodId;
            //else
            //    localMethodId = this.invokingType.Name + "." + this.nameOfMethodToBeMemoized;

            //string retValLogVersion;
            //if (retVal is string)
            //    retValLogVersion = "'" + retVal + "'";
            //else if (retVal == null)
            //    retValLogVersion = "null";
            //else
            //    retValLogVersion = retVal.ToString();

            //long durationInTicks = DateTime.Now.Ticks - startTime;
            //string methodId = string.IsNullOrEmpty(MethodName) ? "<unknown method name/ID>" : MethodName;
            //LoggingMethod(methodId + "(" + param + ") :: " + retValLogVersion + " [duration " + durationInTicks + " ticks (~" + durationInTicks / 10000 + " ms)]");

            return retVal;
        }
    }
}
