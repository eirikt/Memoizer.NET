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
    public enum TimeUnit { Milliseconds, Seconds, Minutes, Hours, Days }

    #region Func extension methods
    public static class FuncExtensionMethods
    {
        public static MemoizerBuilder<TParam1, TResult> Memoize<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TResult>(functionToBeMemoized);
        }
        public static MemoizerBuilder<TParam1, TParam2, TResult> Memoize<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TParam2, TResult>(functionToBeMemoized);
        }
        public static MemoizerBuilder<TParam1, TParam2, TParam3, TResult> Memoize<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized);
        }
        public static MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> Memoize<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized);
        }

        public static Func<TParam1, TResult> MemoizedFunc<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TResult>(functionToBeMemoized).Function;
        }
        public static Func<TParam1, TParam2, TResult> MemoizedFunc<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TParam2, TResult>(functionToBeMemoized).Function;
        }
        public static Func<TParam1, TParam2, TParam3, TResult> MemoizedFunc<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized).Function;
        }
        public static Func<TParam1, TParam2, TParam3, TParam4, TResult> MemoizedFunc<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized).Function;
        }

        public static TResult MemoizedInvoke<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized, TParam1 arg1)
        {
            return new MemoizerBuilder<TParam1, TResult>(functionToBeMemoized).Get().InvokeWith(arg1);
        }
        public static TResult MemoizedInvoke<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized, TParam1 arg1, TParam2 arg2)
        {
            return new MemoizerBuilder<TParam1, TParam2, TResult>(functionToBeMemoized).Get().InvokeWith(arg1, arg2);
        }
        public static TResult MemoizedInvoke<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized, TParam1 arg1, TParam2 arg2, TParam3 arg3)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized).Get().InvokeWith(arg1, arg2, arg3);
        }
        public static TResult MemoizedInvoke<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized, TParam1 arg1, TParam2 arg2, TParam3 arg3, TParam4 arg4)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized).Get().InvokeWith(arg1, arg2, arg3, arg4);
        }
    }
    #endregion

    #region MemoizerBuilder<TParam1, TResult>
    public class MemoizerBuilder<TParam1, TResult>
    {
        static readonly LazyMemoizer<Func<TParam1, TResult>, Memoizer<TParam1, TResult>> MEMOIZER_MEMOIZER =
            new LazyMemoizer<Func<TParam1, TResult>, Memoizer<TParam1, TResult>>
                (f => new Memoizer<TParam1, TResult>(f));

        public MemoizerBuilder(Func<TParam1, TResult> functionToBeMemoized)
        {
            this.function = functionToBeMemoized;
        }

        readonly Func<TParam1, TResult> function;
        internal Func<TParam1, TResult> Function
        {
            get { return this.function; }
        }

        CacheItemPolicy cacheItemPolicy;
        public MemoizerBuilder<TParam1, TResult> CachePolicy(CacheItemPolicy cacheItemPolicy)
        {
            this.cacheItemPolicy = cacheItemPolicy;
            return this;
        }
        public MemoizerBuilder_AwaitingExpirationUnit<TParam1, TResult> KeepItemsCachedFor(long cacheItemExpiration)
        {
            return new MemoizerBuilder_AwaitingExpirationUnit<TParam1, TResult>(cacheItemExpiration, this);
        }

        Action<String> loggingMethod;
        internal Action<String> LoggingMethod
        {
            get { return this.loggingMethod; }
        }
        public MemoizerBuilder<TParam1, TResult> InstrumentWith(Action<String> loggingAction)
        {
            this.loggingMethod = loggingAction;
            return this;
        }

        public IInvocable<TParam1, TResult> Get()
        {
            //Memoizer<TParam1, TResult> memoizer = new Memoizer<TParam1, TResult>(Function); // Not memoized memoizer one-line creation
            Memoizer<TParam1, TResult> memoizer = MEMOIZER_MEMOIZER.InvokeWith(this.function);

            if (this.cacheItemPolicy != null) { memoizer.CacheItemPolicy(this.cacheItemPolicy); }
            if (this.loggingMethod != null) { memoizer.InstrumentWith(this.loggingMethod); }
            return memoizer;
        }
    }


    public class MemoizerBuilder_AwaitingExpirationUnit<TParam1, TResult>
    {
        readonly long cacheItemExpiration;
        readonly MemoizerBuilder<TParam1, TResult> memoizerBuilder;

        public MemoizerBuilder_AwaitingExpirationUnit(long cacheItemExpiration, MemoizerBuilder<TParam1, TResult> memoizerBuilder)
        {
            this.cacheItemExpiration = cacheItemExpiration;
            this.memoizerBuilder = memoizerBuilder;
        }

        MemoizerBuilder<TParam1, TResult> InjectCacheItemPolicy(TimeUnit timeUnit)
        {
            CacheItemPolicy cacheItemPolicy = default(CacheItemPolicy);
            switch (timeUnit)
            {
                case TimeUnit.Milliseconds:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMilliseconds(cacheItemExpiration) };
                    break;
                case TimeUnit.Seconds:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromSeconds(cacheItemExpiration) };
                    break;
                case TimeUnit.Minutes:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(cacheItemExpiration) };
                    break;
                case TimeUnit.Hours:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(cacheItemExpiration) };
                    break;
                case TimeUnit.Days:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromDays(cacheItemExpiration) };
                    break;
            }
            this.memoizerBuilder.CachePolicy(cacheItemPolicy);
            return this.memoizerBuilder;
        }
        public MemoizerBuilder<TParam1, TResult> Milliseconds { get { return InjectCacheItemPolicy(TimeUnit.Milliseconds); } }
        public MemoizerBuilder<TParam1, TResult> Seconds { get { return InjectCacheItemPolicy(TimeUnit.Seconds); } }
        public MemoizerBuilder<TParam1, TResult> Minutes { get { return InjectCacheItemPolicy(TimeUnit.Minutes); } }
        public MemoizerBuilder<TParam1, TResult> Hours { get { return InjectCacheItemPolicy(TimeUnit.Hours); } }
        public MemoizerBuilder<TParam1, TResult> Days { get { return InjectCacheItemPolicy(TimeUnit.Days); } }
    }
    #endregion

    #region MemoizerBuilder<TParam1, TParam2, TResult>
    public class MemoizerBuilder<TParam1, TParam2, TResult>
    {
        static readonly LazyMemoizer<Func<TParam1, TParam2, TResult>, Memoizer<TParam1, TParam2, TResult>> MEMOIZER_MEMOIZER =
            new LazyMemoizer<Func<TParam1, TParam2, TResult>, Memoizer<TParam1, TParam2, TResult>>
                (f => new Memoizer<TParam1, TParam2, TResult>(f));

        public MemoizerBuilder(Func<TParam1, TParam2, TResult> functionToBeMemoized) { this.function = functionToBeMemoized; }

        readonly Func<TParam1, TParam2, TResult> function;
        internal Func<TParam1, TParam2, TResult> Function { get { return this.function; } }

        CacheItemPolicy cacheItemPolicy;
        public MemoizerBuilder<TParam1, TParam2, TResult> CachePolicy(CacheItemPolicy cacheItemPolicy)
        {
            this.cacheItemPolicy = cacheItemPolicy;
            return this;
        }
        public MemoizerBuilder_AwaitingExpirationUnit<TParam1,TParam2, TResult> KeepItemsCachedFor(long cacheItemExpiration)
        {
            return new MemoizerBuilder_AwaitingExpirationUnit<TParam1,TParam2, TResult>(cacheItemExpiration, this);
        }

        Action<String> loggingMethod;
        internal Action<String> LoggingMethod { get { return this.loggingMethod; } }
        public MemoizerBuilder<TParam1, TParam2, TResult> InstrumentWith(Action<String> loggingAction)
        {
            this.loggingMethod = loggingAction;
            return this;
        }

        public IInvocable<TParam1, TParam2, TResult> Get()
        {
            Memoizer<TParam1, TParam2, TResult> memoizer = MEMOIZER_MEMOIZER.InvokeWith(this.function);
            if (this.cacheItemPolicy != null) { memoizer.CacheItemPolicy(this.cacheItemPolicy); }
            if (this.loggingMethod != null) { memoizer.InstrumentWith(this.loggingMethod); }
            return memoizer;
        }
    }


    public class MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TResult>
    {
        readonly long cacheItemExpiration;
        readonly MemoizerBuilder<TParam1, TParam2, TResult> memoizerBuilder;

        public MemoizerBuilder_AwaitingExpirationUnit(long cacheItemExpiration, MemoizerBuilder<TParam1, TParam2, TResult> memoizerBuilder)
        {
            this.cacheItemExpiration = cacheItemExpiration;
            this.memoizerBuilder = memoizerBuilder;
        }

        MemoizerBuilder<TParam1, TParam2, TResult> InjectCacheItemPolicy(TimeUnit timeUnit)
        {
            CacheItemPolicy cacheItemPolicy = default(CacheItemPolicy);
            switch (timeUnit)
            {
                case TimeUnit.Milliseconds:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMilliseconds(cacheItemExpiration) };
                    break;
                case TimeUnit.Seconds:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromSeconds(cacheItemExpiration) };
                    break;
                case TimeUnit.Minutes:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(cacheItemExpiration) };
                    break;
                case TimeUnit.Hours:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(cacheItemExpiration) };
                    break;
                case TimeUnit.Days:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromDays(cacheItemExpiration) };
                    break;
            }
            this.memoizerBuilder.CachePolicy(cacheItemPolicy);
            return this.memoizerBuilder;
        }
        public MemoizerBuilder<TParam1, TParam2, TResult> Milliseconds { get { return InjectCacheItemPolicy(TimeUnit.Milliseconds); } }
        public MemoizerBuilder<TParam1, TParam2, TResult> Seconds { get { return InjectCacheItemPolicy(TimeUnit.Seconds); } }
        public MemoizerBuilder<TParam1, TParam2, TResult> Minutes { get { return InjectCacheItemPolicy(TimeUnit.Minutes); } }
        public MemoizerBuilder<TParam1, TParam2, TResult> Hours { get { return InjectCacheItemPolicy(TimeUnit.Hours); } }
        public MemoizerBuilder<TParam1, TParam2, TResult> Days { get { return InjectCacheItemPolicy(TimeUnit.Days); } }
    }
    #endregion

    #region MemoizerBuilder<TParam1, TParam2, TParam3, TResult>
    public class MemoizerBuilder<TParam1, TParam2, TParam3, TResult>
    {
        static readonly LazyMemoizer<Func<TParam1, TParam2, TParam3, TResult>, Memoizer<TParam1, TParam2, TParam3, TResult>> MEMOIZER_MEMOIZER =
            new LazyMemoizer<Func<TParam1, TParam2, TParam3, TResult>, Memoizer<TParam1, TParam2, TParam3, TResult>>
                (f => new Memoizer<TParam1, TParam2, TParam3, TResult>(f));

        public MemoizerBuilder(Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized) { this.function = functionToBeMemoized; }

        readonly Func<TParam1, TParam2, TParam3, TResult> function;
        internal Func<TParam1, TParam2, TParam3, TResult> Function { get { return this.function; } }

        CacheItemPolicy cacheItemPolicy;
        public MemoizerBuilder<TParam1, TParam2, TParam3, TResult> CachePolicy(CacheItemPolicy cacheItemPolicy)
        {
            this.cacheItemPolicy = cacheItemPolicy;
            return this;
        }
        public MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2,TParam3, TResult> KeepItemsCachedFor(long cacheItemExpiration)
        {
            return new MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TResult>(cacheItemExpiration, this);
        }

        Action<String> loggingMethod;
        internal Action<String> LoggingMethod { get { return this.loggingMethod; } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TResult> InstrumentWith(Action<String> loggingAction)
        {
            this.loggingMethod = loggingAction;
            return this;
        }

        public IInvocable<TParam1, TParam2, TParam3, TResult> Get()
        {
            Memoizer<TParam1, TParam2, TParam3, TResult> memoizer = MEMOIZER_MEMOIZER.InvokeWith(this.function);
            if (this.cacheItemPolicy != null) { memoizer.CacheItemPolicy(this.cacheItemPolicy); }
            if (this.loggingMethod != null) { memoizer.InstrumentWith(this.loggingMethod); }
            return memoizer;
        }
    }


    public class MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TResult>
    {
        readonly long cacheItemExpiration;
        readonly MemoizerBuilder<TParam1, TParam2, TParam3, TResult> memoizerBuilder;

        public MemoizerBuilder_AwaitingExpirationUnit(long cacheItemExpiration, MemoizerBuilder<TParam1, TParam2, TParam3, TResult> memoizerBuilder)
        {
            this.cacheItemExpiration = cacheItemExpiration;
            this.memoizerBuilder = memoizerBuilder;
        }

        MemoizerBuilder<TParam1, TParam2, TParam3, TResult> InjectCacheItemPolicy(TimeUnit timeUnit)
        {
            CacheItemPolicy cacheItemPolicy = default(CacheItemPolicy);
            switch (timeUnit)
            {
                case TimeUnit.Milliseconds:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMilliseconds(cacheItemExpiration) };
                    break;
                case TimeUnit.Seconds:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromSeconds(cacheItemExpiration) };
                    break;
                case TimeUnit.Minutes:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(cacheItemExpiration) };
                    break;
                case TimeUnit.Hours:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(cacheItemExpiration) };
                    break;
                case TimeUnit.Days:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromDays(cacheItemExpiration) };
                    break;
            }
            this.memoizerBuilder.CachePolicy(cacheItemPolicy);
            return this.memoizerBuilder;
        }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TResult> Milliseconds { get { return InjectCacheItemPolicy(TimeUnit.Milliseconds); } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TResult> Seconds { get { return InjectCacheItemPolicy(TimeUnit.Seconds); } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TResult> Minutes { get { return InjectCacheItemPolicy(TimeUnit.Minutes); } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TResult> Hours { get { return InjectCacheItemPolicy(TimeUnit.Hours); } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TResult> Days { get { return InjectCacheItemPolicy(TimeUnit.Days); } }
    }
    #endregion

    #region MemoizerBuilder<TParam1, TParam2, TParam3,TParam4, TResult>
    public class MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>
    {
        static readonly LazyMemoizer<Func<TParam1, TParam2, TParam3, TParam4, TResult>, Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>> MEMOIZER_MEMOIZER =
            new LazyMemoizer<Func<TParam1, TParam2, TParam3, TParam4, TResult>, Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>>
                (f => new Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>(f));

        public MemoizerBuilder(Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized) { this.function = functionToBeMemoized; }

        readonly Func<TParam1, TParam2, TParam3, TParam4, TResult> function;
        internal Func<TParam1, TParam2, TParam3, TParam4, TResult> Function { get { return this.function; } }

        CacheItemPolicy cacheItemPolicy;
        public MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> CachePolicy(CacheItemPolicy cacheItemPolicy)
        {
            this.cacheItemPolicy = cacheItemPolicy;
            return this;
        }
        public MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TParam4, TResult> KeepItemsCachedFor(long cacheItemExpiration)
        {
            return new MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TParam4, TResult>(cacheItemExpiration, this);
        }

        Action<String> loggingMethod;
        internal Action<String> LoggingMethod { get { return this.loggingMethod; } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> InstrumentWith(Action<String> loggingAction)
        {
            this.loggingMethod = loggingAction;
            return this;
        }

        public IInvocable<TParam1, TParam2, TParam3, TParam4, TResult> Get()
        {
            Memoizer<TParam1, TParam2, TParam3, TParam4, TResult> memoizer = MEMOIZER_MEMOIZER.InvokeWith(this.function);
            if (this.cacheItemPolicy != null) { memoizer.CacheItemPolicy(this.cacheItemPolicy); }
            if (this.loggingMethod != null) { memoizer.InstrumentWith(this.loggingMethod); }
            return memoizer;
        }
    }


    public class MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TParam4, TResult>
    {
        readonly long cacheItemExpiration;
        readonly MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> memoizerBuilder;

        public MemoizerBuilder_AwaitingExpirationUnit(long cacheItemExpiration, MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> memoizerBuilder)
        {
            this.cacheItemExpiration = cacheItemExpiration;
            this.memoizerBuilder = memoizerBuilder;
        }

        MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> InjectCacheItemPolicy(TimeUnit timeUnit)
        {
            CacheItemPolicy cacheItemPolicy = default(CacheItemPolicy);
            switch (timeUnit)
            {
                case TimeUnit.Milliseconds:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMilliseconds(cacheItemExpiration) };
                    break;
                case TimeUnit.Seconds:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromSeconds(cacheItemExpiration) };
                    break;
                case TimeUnit.Minutes:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(cacheItemExpiration) };
                    break;
                case TimeUnit.Hours:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(cacheItemExpiration) };
                    break;
                case TimeUnit.Days:
                    cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromDays(cacheItemExpiration) };
                    break;
            }
            this.memoizerBuilder.CachePolicy(cacheItemPolicy);
            return this.memoizerBuilder;
        }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> Milliseconds { get { return InjectCacheItemPolicy(TimeUnit.Milliseconds); } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> Seconds { get { return InjectCacheItemPolicy(TimeUnit.Seconds); } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> Minutes { get { return InjectCacheItemPolicy(TimeUnit.Minutes); } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> Hours { get { return InjectCacheItemPolicy(TimeUnit.Hours); } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> Days { get { return InjectCacheItemPolicy(TimeUnit.Days); } }
    }
    #endregion
}
