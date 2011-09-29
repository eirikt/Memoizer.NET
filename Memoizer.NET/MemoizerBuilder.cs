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

    #region Func extension methods
    public static class FuncExtensionMethods
    {
        //public static Func<TParam, TResult> Memoize<TParam, TResult>(this Func<TParam, TResult> functionToBeMemoized)
        //{
        //    Memoizer<TResult, TParam> memoizer = new Memoizer<TResult, TParam>(functionToBeMemoized);
        //    Func<TParam, TResult> proxyFunction = new Func<TParam, TResult>(delegate(TParam arg)
        //    {
        //        //return functionToBeMemoized.Invoke(arg);
        //        //return (TResult)functionToBeMemoized.DynamicInvoke(arg);
        //        return memoizer.InvokeWith(arg);
        //    });

        //    return proxyFunction;
        //}

        //// TODO: memoize/cache the memoized function as well :-D
        //public static Func<TParam, TResult> Memoize<TParam, TResult>(this Func<TParam, TResult> functionToBeMemoized)
        //{
        //    Memoizer<TResult, TParam> memoizer = new Memoizer<TResult, TParam>(functionToBeMemoized);
        //    Func<TParam, TResult> proxyFunction =
        //        new Func<TParam, TResult>(delegate(TParam arg)
        //            {
        //                return memoizer.InvokeWith(arg);
        //            });
        //    return proxyFunction;
        //}
        //public static Func<TParam1, TParam2, TResult> Memoize<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized)
        //{
        //    return new Memoizer<TResult, TParam1, TParam2>(functionToBeMemoized).InvokeWith;
        //}
        ////public static Func<TParam1, TParam2, TParam3, TResult> Memoize<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized) { return new Memoizer<TResult, TParam1, TParam2, TParam3>(functionToBeMemoized).InvokeWith; }
        ////public static Func<TParam1, TParam2, TParam3, TParam4, TResult> Memoize<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized) { return new Memoizer<TResult, TParam1, TParam2, TParam3, TParam4>(functionToBeMemoized).InvokeWith; }

        public static MemoizerBuilder<TResult, TParam> Memoize<TParam, TResult>(this Func<TParam, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TResult, TParam>(functionToBeMemoized);
        }

        public static Func<TParam, TResult> MemoizedFunc<TParam, TResult>(this Func<TParam, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TResult, TParam>(functionToBeMemoized).Function;
        }

        public static TResult MemoizedInvoke<TParam, TResult>(this Func<TParam, TResult> functionToBeMemoized, TParam arg)
        {
            return new MemoizerBuilder<TResult, TParam>(functionToBeMemoized).Get().InvokeWith(arg);
        }
    }
    #endregion

    //#region Memoizer extension methods
    //public static class MemoizerExtensionMethods
    //{
    //    public static Memoizer<TParam, TResult> InstrumentWith<TParam, TResult>(this Memoizer<TParam, TResult> memoizerToInstrument, Action<String> instrumenter)
    //    {
    //        memoizerToInstrument.LoggingMethod = instrumenter;
    //        return memoizerToInstrument;
    //    }
    //}
    //#endregion

    public class MemoizerBuilder<TResult, TParam>
    {
        static Memoizer<TResult, TParam> CreateMemoizer(Func<TParam, TResult> f) { return new Memoizer<TResult, TParam>(f); }
        static readonly LazyMemoizer<Memoizer<TResult, TParam>, Func<TParam, TResult>> MEMOIZER_MEMOIZER =
            new LazyMemoizer<Memoizer<TResult, TParam>, Func<TParam, TResult>>(CreateMemoizer);

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

        public Memoizer<TResult, TParam> Get()
        {
            //Memoizer<TResult, TParam> memoizer = new Memoizer<TResult, TParam>(Function); // Not memoized memoizer one-line creation
            Memoizer<TResult, TParam> memoizer = MEMOIZER_MEMOIZER.InvokeWith(this.function);

            if (this.loggingMethod != null) { memoizer.InstrumentWith(this.loggingMethod); }
            return memoizer;

        }
    }

    //class MemoizerReadyForBuilding<TResult, TParam>
    //{
    //    Memoizer<TResult, TParam> memoizer;

    //    MemoizerReadyForBuilding(){}

    //    public Memoizer<TResult, TParam> Get()
    //    {
    //        return this.memoizer;
    //    }
    //}
}
