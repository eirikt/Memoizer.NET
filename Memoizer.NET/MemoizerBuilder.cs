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
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Serialization;

namespace Memoizer.NET
{
    #region Func extension methods
    public static class FuncExtensionMethods
    {
        #region CreateMemoizer()
        public static IMemoizer<TParam1, TResult> CreateMemoizer<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TResult>(functionToBeMemoized).CreateMemoizer();
        }
        public static IMemoizer<TParam1, TParam2, TResult> CreateMemoizer<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TParam2, TResult>(functionToBeMemoized).CreateMemoizer();
        }
        public static IMemoizer<TParam1, TParam2, TParam3, TResult> CreateMemoizer<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized).CreateMemoizer();
        }
        public static IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult> CreateMemoizer<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized).GetMemoizer();
        }
        #endregion

        #region GetMemoizer()
        public static IMemoizer<TParam1, TResult> GetMemoizer<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TResult>(functionToBeMemoized).GetMemoizer();
        }
        public static IMemoizer<TParam1, TParam2, TResult> GetMemoizer<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TParam2, TResult>(functionToBeMemoized).GetMemoizer();
        }
        public static IMemoizer<TParam1, TParam2, TParam3, TResult> GetMemoizer<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized).GetMemoizer();
        }
        public static IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult> GetMemoizer<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized).GetMemoizer();
        }
        #endregion

        #region Cache([as long as the CLR is alive])
        public static MemoizerBuilder<TParam1, TResult> Cache<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TResult>(functionToBeMemoized);
        }
        public static MemoizerBuilder<TParam1, TParam2, TResult> Cache<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TParam2, TResult>(functionToBeMemoized);
        }
        public static MemoizerBuilder<TParam1, TParam2, TParam3, TResult> Cache<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized);
        }
        public static MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> Cache<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized);
        }
        #endregion

        #region CacheFor(int expirationValue)
        public static MemoizerBuilder_AwaitingExpirationUnit<TParam1, TResult> CacheFor<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized, int expirationValue)
        {
            return new MemoizerBuilder<TParam1, TResult>(functionToBeMemoized).KeepElementsCachedFor(expirationValue);
        }
        public static MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TResult> CacheFor<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized, int expirationValue)
        {
            return new MemoizerBuilder<TParam1, TParam2, TResult>(functionToBeMemoized).KeepElementsCachedFor(expirationValue);
        }
        public static MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TResult> CacheFor<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized, int expirationValue)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized).KeepElementsCachedFor(expirationValue);
        }
        public static MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TParam4, TResult> CacheFor<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized, int expirationValue)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized).KeepElementsCachedFor(expirationValue);
        }
        #endregion

        // Hmm, what's the point...
        //public static Func<TParam1, TResult> MemoizedFunc<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized)
        //{
        //    return new MemoizerBuilder<TParam1, TResult>(functionToBeMemoized).Function;
        //}
        //public static Func<TParam1, TParam2, TResult> MemoizedFunc<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized)
        //{
        //    return new MemoizerBuilder<TParam1, TParam2, TResult>(functionToBeMemoized).Function;
        //}
        //public static Func<TParam1, TParam2, TParam3, TResult> MemoizedFunc<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized)
        //{
        //    return new MemoizerBuilder<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized).Function;
        //}
        //public static Func<TParam1, TParam2, TParam3, TParam4, TResult> MemoizedFunc<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized)
        //{
        //    return new MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized).Function;
        //}

        #region CachedInvoke(TParam... args)
        public static TResult CachedInvoke<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized, TParam1 arg1)
        {
            return new MemoizerBuilder<TParam1, TResult>(functionToBeMemoized).GetMemoizer().InvokeWith(arg1);
        }
        public static TResult CachedInvoke<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized, TParam1 arg1, TParam2 arg2)
        {
            return new MemoizerBuilder<TParam1, TParam2, TResult>(functionToBeMemoized).GetMemoizer().InvokeWith(arg1, arg2);
        }
        public static TResult CachedInvoke<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized, TParam1 arg1, TParam2 arg2, TParam3 arg3)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized).GetMemoizer().InvokeWith(arg1, arg2, arg3);
        }
        public static TResult CachedInvoke<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized, TParam1 arg1, TParam2 arg2, TParam3 arg3, TParam4 arg4)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized).GetMemoizer().InvokeWith(arg1, arg2, arg3, arg4);
        }
        #endregion
    }
    #endregion


    public enum ExpirationType { Relative, Absolute }


    public enum TimeUnit { Milliseconds, Seconds, Minutes, Hours, Days }


    #region CacheItemPolicyBuilder
    public class CacheItemPolicyBuilder
    {
        // TODO: ...
        public static CacheItemPolicy CreateCacheItemPolicy(ExpirationType expirationType, int expirationValue, TimeUnit expirationTimeUnit)
        {
            return null;
        }
    }
    #endregion

    #region MemoizerHelper
    public class MemoizerHelper
    {
        public static string CreateParameterHash(params object[] args) { return args.Aggregate(String.Empty, (current, arg) => current + arg.GetHashCode().ToString()); }

        static readonly ObjectIDGenerator OBJECT_ID_GENERATOR = new ObjectIDGenerator();
        public static string CreateMemoizerBuilderHash<TParam1, TResult>(MemoizerBuilder<TParam1, TResult> memoizerBuilder)
        {
            Func<TParam1, TResult> func = memoizerBuilder.Function;
            CacheItemPolicy cacheItemPolicy = memoizerBuilder.CacheItemPolicy;
            Action<String> loggerAction = memoizerBuilder.LoggerAction;

            bool firstTime;
            long funcId = OBJECT_ID_GENERATOR.GetId(func, out firstTime);

            long cacheItemPolicyHashCode = cacheItemPolicy == null ? 0 : cacheItemPolicy.GetHashCode();

            return funcId.ToString() + cacheItemPolicyHashCode.ToString();
        }

        public static string CreateMemoizerBuilderHash<TParam1, TParam2, TResult>(MemoizerBuilder<TParam1, TParam2, TResult> memoizerBuilder)
        {
            throw new NotImplementedException();
        }

        public static string CreateMemoizerBuilderHash<TParam1, TParam2, TParam3, TResult>(MemoizerBuilder<TParam1, TParam2, TParam3, TResult> memoizerBuilder)
        {
            throw new NotImplementedException();
        }

        public static string CreateMemoizerBuilderHash<TParam1, TParam2, TParam3, TParam4, TResult>(MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> memoizerBuilder)
        {
            throw new NotImplementedException();
        }

        public static string CreateMethodHash(object source) { return CreateParameterHash(source); }

        //public static string CreateMethodHash(Type sourceClass, string methodName) { return CreateParameterHash(sourceClass.ToString(), methodName); }
    }
    #endregion

    #region MemoizerBuilder<TParam1, TResult>
    /// <summary>
    /// Not thread-safe memoizer builder.
    /// </summary>
    public class MemoizerBuilder<TParam1, TResult> //: IInvocable<TParam1, TResult>
    {
        static readonly LazyMemoizer<Func<TParam1, TResult>, Memoizer<TParam1, TResult>> MEMOIZER_MEMOIZER =
            new LazyMemoizer<Func<TParam1, TResult>, Memoizer<TParam1, TResult>>
                (f => new Memoizer<TParam1, TResult>(f));

        readonly Func<TParam1, TResult> function;
        CacheItemPolicy cacheItemPolicy;
        Action<String> loggerMethod;

        internal MemoizerBuilder(Func<TParam1, TResult> functionToBeMemoized, int expirationValue = 0) { this.function = functionToBeMemoized; }

        internal Func<TParam1, TResult> Function
        {
            get { return this.function; }
        }

        internal CacheItemPolicy CacheItemPolicy
        {
            get { return this.cacheItemPolicy; }
        }

        internal Action<String> LoggerAction
        {
            get { return this.loggerMethod; }
        }

        public MemoizerBuilder<TParam1, TResult> CachePolicy(CacheItemPolicy cacheItemPolicy)
        {
            this.cacheItemPolicy = cacheItemPolicy;
            return this;
        }
        public MemoizerBuilder_AwaitingExpirationUnit<TParam1, TResult> KeepElementsCachedFor(long cacheItemExpiration)
        {
            return new MemoizerBuilder_AwaitingExpirationUnit<TParam1, TResult>(cacheItemExpiration, this);
        }

        /// <summary>
        /// Mutable logging action.
        /// </summary>
        public MemoizerBuilder<TParam1, TResult> InstrumentWith(Action<String> loggingAction)
        {
            this.loggerMethod = loggingAction;
            return this;
        }

        /// <summary>
        /// Force creation and <i>not</i> caching of created memoizer instance.
        /// </summary>
        public IMemoizer<TParam1, TResult> CreateMemoizer()
        {
            return GetMemoizer(false);
        }

        public IMemoizer<TParam1, TResult> GetMemoizer(bool cacheAndShareMemoizerInstance = true)
        {
            Memoizer<TParam1, TResult> memoizer =
                cacheAndShareMemoizerInstance ?
                MEMOIZER_MEMOIZER.InvokeWith(this.function) :
                new Memoizer<TParam1, TResult>(this.function, this.cacheItemPolicy);

            // TODO: replace above with:
            //System.Runtime.Caching.CacheItemPolicy cacheItemPolicy = CacheItemPolicyBuilder.CreateCacheItemPolicy(this.cacheItemExpirationType, this.cacheItemExpirationValue, this.cacheItemExpirationTimeUnit);
            //Memoizer<TParam1, TResult> memoizer =
            //    cacheAndShareMemoizerInstance ?
            //    MEMOIZER_MEMOIZER.InvokeWith(this.function) :
            //    new Memoizer<TParam1, TResult>(this.function, cacheItemPolicy);

            /*if (this.cacheItemPolicy != null) {*/
            //memoizer.CacheItemPolicy(this.cacheItemPolicy); /*}*/
            /*if (this.loggingMethod != null) {*/
            //memoizer.InstrumentWith(this.loggingMethod); /*}*/
            return memoizer;
        }

        //public TResult InvokeWith(TParam1 someId) { return GetMemoizer().InvokeWith(someId); }
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

        readonly Func<TParam1, TParam2, TResult> function;
        CacheItemPolicy cacheItemPolicy;
        Action<String> loggingMethod;

        internal MemoizerBuilder(Func<TParam1, TParam2, TResult> functionToBeMemoized) { this.function = functionToBeMemoized; }

        public MemoizerBuilder<TParam1, TParam2, TResult> CachePolicy(CacheItemPolicy cacheItemPolicy)
        {
            this.cacheItemPolicy = cacheItemPolicy;
            return this;
        }

        public MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TResult> KeepElementsCachedFor(long cacheItemExpiration)
        {
            return new MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TResult>(cacheItemExpiration, this);
        }

        public MemoizerBuilder<TParam1, TParam2, TResult> InstrumentWith(Action<String> loggingAction)
        {
            this.loggingMethod = loggingAction;
            return this;
        }

        public IMemoizer<TParam1, TParam2, TResult> CreateMemoizer() { return GetMemoizer(false); }

        public IMemoizer<TParam1, TParam2, TResult> GetMemoizer(bool cacheAndShareMemoizerInstance = true)
        {
            Memoizer<TParam1, TParam2, TResult> memoizer =
                cacheAndShareMemoizerInstance ? MEMOIZER_MEMOIZER.InvokeWith(this.function) : new Memoizer<TParam1, TParam2, TResult>(this.function);
            memoizer.CacheItemPolicy(this.cacheItemPolicy);
            memoizer.InstrumentWith(this.loggingMethod);
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

        readonly Func<TParam1, TParam2, TParam3, TResult> function;
        CacheItemPolicy cacheItemPolicy;
        Action<String> loggingMethod;

        internal MemoizerBuilder(Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized, int expirationValue = 0) { this.function = functionToBeMemoized; }

        public MemoizerBuilder<TParam1, TParam2, TParam3, TResult> CachePolicy(CacheItemPolicy cacheItemPolicy)
        {
            this.cacheItemPolicy = cacheItemPolicy;
            return this;
        }

        public MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TResult> KeepElementsCachedFor(long cacheItemExpiration)
        {
            return new MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TResult>(cacheItemExpiration, this);
        }

        public MemoizerBuilder<TParam1, TParam2, TParam3, TResult> InstrumentWith(Action<String> loggingAction)
        {
            this.loggingMethod = loggingAction;
            return this;
        }

        public IMemoizer<TParam1, TParam2, TParam3, TResult> CreateMemoizer() { return GetMemoizer(false); }

        public IMemoizer<TParam1, TParam2, TParam3, TResult> GetMemoizer(bool cacheAndShareMemoizerInstance = true)
        {
            Memoizer<TParam1, TParam2, TParam3, TResult> memoizer =
                cacheAndShareMemoizerInstance ? MEMOIZER_MEMOIZER.InvokeWith(this.function) : new Memoizer<TParam1, TParam2, TParam3, TResult>(this.function);
            memoizer.CacheItemPolicy(this.cacheItemPolicy);
            memoizer.InstrumentWith(this.loggingMethod);
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

        readonly Func<TParam1, TParam2, TParam3, TParam4, TResult> function;
        CacheItemPolicy cacheItemPolicy;
        Action<String> loggingMethod;

        internal MemoizerBuilder(Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized, int expirationValue = 0) { this.function = functionToBeMemoized; }

        public MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> CachePolicy(CacheItemPolicy cacheItemPolicy)
        {
            this.cacheItemPolicy = cacheItemPolicy;
            return this;
        }

        public MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TParam4, TResult> KeepElementsCachedFor(long cacheItemExpiration)
        {
            return new MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TParam4, TResult>(cacheItemExpiration, this);
        }

        public MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> InstrumentWith(Action<String> loggingAction)
        {
            this.loggingMethod = loggingAction;
            return this;
        }

        public IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult> CreateMemoizer() { return GetMemoizer(false); }

        public IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult> GetMemoizer(bool cacheAndShareMemoizerInstance = true)
        {
            Memoizer<TParam1, TParam2, TParam3, TParam4, TResult> memoizer =
                cacheAndShareMemoizerInstance ? MEMOIZER_MEMOIZER.InvokeWith(this.function) : new Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>(this.function);
            memoizer.CacheItemPolicy(this.cacheItemPolicy);
            memoizer.InstrumentWith(this.loggingMethod);
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
