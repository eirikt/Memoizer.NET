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

    #region Func extension methods
    public static class FuncExtensionMethods
    {
        public static MemoizerBuilder<TResult, TParam1> Memoize<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TResult, TParam1>(functionToBeMemoized);
        }
        public static MemoizerBuilder<TResult, TParam1, TParam2> Memoize<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TResult, TParam1, TParam2>(functionToBeMemoized);
        }

        public static Func<TParam1, TResult> MemoizedFunc<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TResult, TParam1>(functionToBeMemoized).Function;
        }
        public static Func<TParam1, TParam2, TResult> MemoizedFunc<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TResult, TParam1, TParam2>(functionToBeMemoized).Function;
        }

        public static TResult MemoizedInvoke<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized, TParam1 arg)
        {
            return new MemoizerBuilder<TResult, TParam1>(functionToBeMemoized).Get().InvokeWith(arg);
        }
        public static TResult MemoizedInvoke<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized, TParam1 arg1, TParam2 arg2)
        {
            return new MemoizerBuilder<TResult, TParam1, TParam2>(functionToBeMemoized).Get().InvokeWith(arg1, arg2);
        }
    }
    #endregion

    public class MemoizerBuilder<TResult, TParam>
    {
        static readonly LazyMemoizer<Memoizer<TResult, TParam>, Func<TParam, TResult>> MEMOIZER_MEMOIZER =
            new LazyMemoizer<Memoizer<TResult, TParam>, Func<TParam, TResult>>(
                f => new Memoizer<TResult, TParam>(f)
        );

        public MemoizerBuilder(Func<TParam, TResult> functionToBeMemoized)
        {
            this.function = functionToBeMemoized;
        }

        readonly Func<TParam, TResult> function;
        internal Func<TParam, TResult> Function
        {
            get { return this.function; }
        }

        Action<String> loggingMethod;
        internal Action<String> LoggingMethod
        {
            get { return this.loggingMethod; }
        }

        public MemoizerBuilder<TResult, TParam> InstrumentWith(Action<String> loggingAction)
        {
            this.loggingMethod = loggingAction;
            return this;
        }

        public IInvocable<TResult, TParam> Get()
        {
            //Memoizer<TResult, TParam> memoizer = new Memoizer<TResult, TParam>(Function); // Not memoized memoizer one-line creation
            Memoizer<TResult, TParam> memoizer = MEMOIZER_MEMOIZER.InvokeWith(this.function);

            if (this.loggingMethod != null) { memoizer.InstrumentWith(this.loggingMethod); }
            return memoizer;
        }
    }


    public class MemoizerBuilder<TResult, TParam1, TParam2>
    {
        static readonly LazyMemoizer<Memoizer<TResult, TParam1, TParam2>, Func<TParam1, TParam2, TResult>> MEMOIZER_MEMOIZER =
            new LazyMemoizer<Memoizer<TResult, TParam1, TParam2>, Func<TParam1, TParam2, TResult>>(
                f => new Memoizer<TResult, TParam1, TParam2>(f)
        );

        public MemoizerBuilder(Func<TParam1, TParam2, TResult> functionToBeMemoized) { this.function = functionToBeMemoized; }

        readonly Func<TParam1, TParam2, TResult> function;
        internal Func<TParam1, TParam2, TResult> Function { get { return this.function; } }

        CacheItemPolicy cacheItemPolicy;
        public MemoizerBuilder<TResult, TParam1, TParam2> CachePolicy(CacheItemPolicy cacheItemPolicy)
        {
            this.cacheItemPolicy = cacheItemPolicy;
            return this;
        }

        Action<String> loggingMethod;
        internal Action<String> LoggingMethod { get { return this.loggingMethod; } }

        public MemoizerBuilder<TResult, TParam1, TParam2> InstrumentWith(Action<String> loggingAction)
        {
            this.loggingMethod = loggingAction;
            return this;
        }

        public IInvocable<TResult, TParam1, TParam2> Get()
        {
            //Memoizer<TResult, TParam> memoizer = new Memoizer<TResult, TParam>(Function); // Not memoized memoizer one-line creation
            Memoizer<TResult, TParam1, TParam2> memoizer = MEMOIZER_MEMOIZER.InvokeWith(this.function);

            if (this.cacheItemPolicy != null) { memoizer.UseCacheItemPolicy(this.cacheItemPolicy); }
            if (this.loggingMethod != null) { memoizer.InstrumentWith(this.loggingMethod); }
            return memoizer;
        }
    }
}
