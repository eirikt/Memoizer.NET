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

namespace Memoizer.NET
{

    #region LazyMemoizer
    // TODO: some decent documentation would have been appropriate...
    public class LazyMemoizer<TResult, TParam> : IInvocable<TResult, TParam>
    {
        readonly bool doInstrumentInvocations;

        readonly string methodId;
        readonly Type invokingType;
        readonly string nameOfMethodToBeMemoized;
        readonly Action<string> loggingMethod;

        readonly Lazy<Memoizer<TResult, TParam>> lazyInitializer;

        public LazyMemoizer(string methodId, Func<TParam, TResult> methodToBeMemoized, bool doInstrumentInvocations = false, Action<string> loggingMethod = null)
        {
            if (string.IsNullOrEmpty(methodId)) { throw new ArgumentException("A hash of the method to be memoized must be provided"); }
            if (methodToBeMemoized == null) { throw new ArgumentException("Method to be memoized is missing"); }
            this.methodId = methodId;
            this.doInstrumentInvocations = doInstrumentInvocations;
            this.loggingMethod = loggingMethod;
            this.lazyInitializer = new Lazy<Memoizer<TResult, TParam>>(() =>
                new Memoizer<TResult, TParam>(this.methodId, methodToBeMemoized), true);
        }

        public LazyMemoizer(Type invokingType, string nameOfMethodToBeMemoized, Func<TParam, TResult> methodToBeMemoized, bool doInstrumentInvocations = false, Action<string> loggingMethod = null)
        {
            if (invokingType == null) { throw new ArgumentException("Type of invoking class is missing"); }
            if (string.IsNullOrEmpty(nameOfMethodToBeMemoized)) { throw new ArgumentException("Name of method to be memoized is missing"); }
            if (methodToBeMemoized == null) { throw new ArgumentException("Method to be memoized is missing"); }
            this.invokingType = invokingType;
            this.nameOfMethodToBeMemoized = nameOfMethodToBeMemoized;
            this.doInstrumentInvocations = doInstrumentInvocations;
            this.loggingMethod = loggingMethod;
            this.lazyInitializer = new Lazy<Memoizer<TResult, TParam>>(() =>
                new Memoizer<TResult, TParam>(MemoizerHelper.CreateMethodHash(invokingType, nameOfMethodToBeMemoized), methodToBeMemoized), true);
        }

        public TResult InvokeWith(TParam param)
        {
            if (!doInstrumentInvocations)
                return this.lazyInitializer.Value.InvokeWith(param);

            long startTime = DateTime.Now.Ticks;
            TResult retVal = this.lazyInitializer.Value.InvokeWith(param);
            string localMethodId;
            if (this.invokingType == null || string.IsNullOrEmpty(this.nameOfMethodToBeMemoized))
                localMethodId = this.methodId;
            else
                localMethodId = this.invokingType.Name + "." + this.nameOfMethodToBeMemoized;

            string retValLogVersion;
            if (retVal is string)
                retValLogVersion = "'" + retVal + "'";
            else if (retVal == null)
                retValLogVersion = "null";
            else
                retValLogVersion = retVal.ToString();

            long durationInTicks = DateTime.Now.Ticks - startTime;
            string logMessage = localMethodId + "(" + param + ") :: " + retValLogVersion + " [duration " + durationInTicks + " ticks (" + durationInTicks / 10000 + " ms)]";
            if (this.loggingMethod != null)
                this.loggingMethod(logMessage);
            else
                Console.WriteLine(logMessage);

            return retVal;
        }
    }

    // TODO: create classes with more parameters
    #endregion
}
