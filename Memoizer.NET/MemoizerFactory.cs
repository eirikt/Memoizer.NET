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
using System.Runtime.Serialization;

namespace Memoizer.NET
{
    #region CacheItemPolicyFactory
    static class CacheItemPolicyFactory
    {
        internal static CacheItemPolicy CreateCacheItemPolicy(ExpirationType expirationType, int expirationValue, TimeUnit expirationTimeUnit)
        {
            CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
            switch (expirationType)
            {
                case ExpirationType.Relative:
                    TimeSpan timeSpan;
                    switch (expirationTimeUnit)
                    {
                        case TimeUnit.Milliseconds:
                            timeSpan = new TimeSpan(0, 0, 0, 0, expirationValue);
                            cacheItemPolicy.SlidingExpiration = timeSpan;
                            break;
                        case TimeUnit.Seconds:
                            timeSpan = new TimeSpan(0, 0, 0, expirationValue, 0);
                            cacheItemPolicy.SlidingExpiration = timeSpan;
                            break;
                        case TimeUnit.Minutes:
                            timeSpan = new TimeSpan(0, 0, expirationValue, 0, 0);
                            cacheItemPolicy.SlidingExpiration = timeSpan;
                            break;
                        case TimeUnit.Hours:
                            timeSpan = new TimeSpan(0, expirationValue, 0, 0, 0);
                            cacheItemPolicy.SlidingExpiration = timeSpan;
                            break;
                        case TimeUnit.Days:
                            timeSpan = new TimeSpan(expirationValue, 0, 0, 0, 0);
                            cacheItemPolicy.SlidingExpiration = timeSpan;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            return cacheItemPolicy;
        }
    }
    #endregion

    #region MemoizerHelper
    public static class MemoizerHelper
    {
        static internal readonly int[] PRIMES = new[] { 31, 37, 43, 47, 59, 61, 71, 73, 89, 97, 101, 103, 113, 127, 131, 137 };

        static readonly ObjectIDGenerator OBJECT_ID_GENERATOR = new ObjectIDGenerator();


        internal static long GetObjectId(object arg, ref bool firstTime)
        {
            return OBJECT_ID_GENERATOR.GetId(arg, out firstTime);
        }


        public static string CreateParameterHash(params object[] args)
        //public static string CreateParameterHash(bool possiblySharedMemoizerConfig = true, params object[] args)
        {
            if (args == null)
                return "NULLARGARRAY";

            if (args.Length == 1)
                return args[0] == null ? "NULLARG" : args[0].GetHashCode().ToString();

            int retVal;
            if (args[0] == null)
                retVal = Int32.MinValue;
            else
                retVal = args[0].GetHashCode() * PRIMES[0];

            for (int i = 1; i < args.Length; ++i)
            {
                if (args[i] == null)
                    retVal = retVal * PRIMES[i] + Int32.MaxValue;
                else
                    retVal = retVal * PRIMES[i] + args[i].GetHashCode();
            }

            return retVal.ToString();
        }


        public static long CreateFunctionHash(object func)
        {
            bool firstTime = false;
            return GetObjectId(func, ref firstTime);
        }
    }
    #endregion

    #region MemoizerFactory<TResult>
    public class MemoizerFactory<TResult>
    {
        readonly Func<TResult> function;

        internal ExpirationType ExpirationType { get; set; }
        internal int ExpirationValue { get; set; }
        internal TimeUnit ExpirationTimeUnit { get; set; }

        Action<String> loggerMethod;


        internal MemoizerFactory(Func<TResult> functionToBeMemoized)
        {
            this.function = functionToBeMemoized;
        }

        internal Func<TResult> Function
        {
            get { return this.function; }
        }

        internal Action<String> LoggerAction
        {
            get { return this.loggerMethod; }
        }

        MemoizerConfiguration MemoizerConfiguration
        {
            get { return new MemoizerConfiguration(this.function, this.ExpirationType, this.ExpirationValue, this.ExpirationTimeUnit, this.loggerMethod); }
        }

        public MemoizerFactory_AwaitingExpirationUnit<TResult> KeepItemsCachedFor(int cacheExpirationValue)
        {
            return new MemoizerFactory_AwaitingExpirationUnit<TResult>(cacheExpirationValue, this);
        }

        public MemoizerFactory<TResult> InstrumentWith(Action<String> loggingAction)
        {
            this.loggerMethod = loggingAction;
            return this;
        }

        /// <summary>
        /// Force creation and <i>not</i> caching/sharing (via memoizer^2 registry) of created memoizer instance.
        /// </summary>
        public IMemoizer<TResult> CreateMemoizer()
        {
            return GetMemoizer(cachedAndSharedMemoizerInstance: false);
        }

        public IMemoizer<TResult> GetMemoizer(bool cachedAndSharedMemoizerInstance = true)
        {
            return cachedAndSharedMemoizerInstance
                ? MemoizerRegistry<TResult>.LAZY_MEMOIZER_REGISTRY.Value.InvokeWith(this.MemoizerConfiguration)
                : new Memoizer<TResult>(this.MemoizerConfiguration, shared: false);
        }
    }


    public class MemoizerFactory_AwaitingExpirationUnit<TResult>
    {
        readonly ExpirationType expirationType = ExpirationType.Relative;
        readonly int expirationValue;

        readonly MemoizerFactory<TResult> memoizerFactory;

        public MemoizerFactory_AwaitingExpirationUnit(int expirationValue, MemoizerFactory<TResult> memoizerFactory)
        {
            this.expirationValue = expirationValue;
            this.memoizerFactory = memoizerFactory;
        }

        MemoizerFactory<TResult> ConfigureCacheItemPolicyWithTimeUnit(TimeUnit timeUnit)
        {
            this.memoizerFactory.ExpirationType = this.expirationType;
            this.memoizerFactory.ExpirationValue = this.expirationValue;
            this.memoizerFactory.ExpirationTimeUnit = timeUnit;

            return this.memoizerFactory;
        }

        public MemoizerFactory<TResult> Milliseconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Milliseconds); } }
        public MemoizerFactory<TResult> Seconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Seconds); } }
        public MemoizerFactory<TResult> Minutes { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Minutes); } }
        public MemoizerFactory<TResult> Hours { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Hours); } }
        public MemoizerFactory<TResult> Days { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Days); } }
    }
    #endregion

    #region MemoizerFactory<TParam1, TResult>
    // TODO: rename to MemoizerConfig ...? It's just a better class name for a public class
    public class MemoizerFactory<TParam1, TResult>
    {
        readonly Func<TParam1, TResult> function;

        internal ExpirationType ExpirationType { get; set; }
        internal int ExpirationValue { get; set; }
        internal TimeUnit ExpirationTimeUnit { get; set; }

        Action<String> loggerMethod;

        internal MemoizerFactory(Func<TParam1, TResult> functionToBeMemoized) { this.function = functionToBeMemoized; }
        internal Func<TParam1, TResult> Function { get { return this.function; } }
        internal Action<String> LoggerAction { get { return this.loggerMethod; } }
        MemoizerConfiguration MemoizerConfiguration { get { return new MemoizerConfiguration(this.function, this.ExpirationType, this.ExpirationValue, this.ExpirationTimeUnit, this.loggerMethod); } }
        public MemoizerFactory_AwaitingExpirationUnit<TParam1, TResult> KeepItemsCachedFor(int cacheExpirationValue) { return new MemoizerFactory_AwaitingExpirationUnit<TParam1, TResult>(cacheExpirationValue, this); }
        public MemoizerFactory<TParam1, TResult> InstrumentWith(Action<String> loggingAction)
        {
            this.loggerMethod = loggingAction;
            return this;
        }

        /// <summary>
        /// Force creation and <i>not</i> caching/sharing (via memoizer^2 registry) of created memoizer instance.
        /// </summary>
        public IMemoizer<TParam1, TResult> CreateMemoizer() { return GetMemoizer(cachedAndSharedMemoizerInstance: false); }

        public IMemoizer<TParam1, TResult> GetMemoizer(bool cachedAndSharedMemoizerInstance = true)
        {
            return cachedAndSharedMemoizerInstance
                ? MemoizerRegistry<TParam1, TResult>.LAZY_MEMOIZER_REGISTRY.Value.InvokeWith(this.MemoizerConfiguration)
                : new Memoizer<TParam1, TResult>(this.MemoizerConfiguration, shared: false);
        }
    }


    public class MemoizerFactory_AwaitingExpirationUnit<TParam1, TResult>
    {
        readonly ExpirationType expirationType = ExpirationType.Relative;
        readonly int expirationValue;

        readonly MemoizerFactory<TParam1, TResult> memoizerFactory;

        public MemoizerFactory_AwaitingExpirationUnit(int expirationValue, MemoizerFactory<TParam1, TResult> memoizerFactory)
        {
            this.expirationValue = expirationValue;
            this.memoizerFactory = memoizerFactory;
        }

        MemoizerFactory<TParam1, TResult> ConfigureCacheItemPolicyWithTimeUnit(TimeUnit timeUnit)
        {
            this.memoizerFactory.ExpirationType = this.expirationType;
            this.memoizerFactory.ExpirationValue = this.expirationValue;
            this.memoizerFactory.ExpirationTimeUnit = timeUnit;

            return this.memoizerFactory;
        }

        public MemoizerFactory<TParam1, TResult> Milliseconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Milliseconds); } }
        public MemoizerFactory<TParam1, TResult> Seconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Seconds); } }
        public MemoizerFactory<TParam1, TResult> Minutes { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Minutes); } }
        public MemoizerFactory<TParam1, TResult> Hours { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Hours); } }
        public MemoizerFactory<TParam1, TResult> Days { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Days); } }
    }
    #endregion

    #region MemoizerFactory<TParam1, TParam2, TResult>
    public class MemoizerFactory<TParam1, TParam2, TResult>
    {
        readonly Func<TParam1, TParam2, TResult> function;
        internal Func<TParam1, TParam2, TResult> Function { get { return this.function; } }

        internal ExpirationType ExpirationType { get; set; }
        internal int ExpirationValue { get; set; }
        internal TimeUnit ExpirationTimeUnit { get; set; }

        Action<String> loggerMethod;
        internal Action<String> LoggerAction { get { return this.loggerMethod; } }

        MemoizerConfiguration MemoizerConfiguration { get { return new MemoizerConfiguration(this.function, this.ExpirationType, this.ExpirationValue, this.ExpirationTimeUnit, this.loggerMethod); } }
        internal MemoizerFactory(Func<TParam1, TParam2, TResult> functionToBeMemoized) { this.function = functionToBeMemoized; }
        public MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TResult> KeepItemsCachedFor(int cacheExpirationValue) { return new MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TResult>(cacheExpirationValue, this); }
        public MemoizerFactory<TParam1, TParam2, TResult> InstrumentWith(Action<String> loggerMethod)
        {
            this.loggerMethod = loggerMethod;
            return this;
        }
        public IMemoizer<TParam1, TParam2, TResult> CreateMemoizer() { return GetMemoizer(false); }
        public IMemoizer<TParam1, TParam2, TResult> GetMemoizer(bool cacheAndShareMemoizerInstance = true)
        {
            return cacheAndShareMemoizerInstance
                ? MemoizerRegistry<TParam1, TParam2, TResult>.LAZY_MEMOIZER_REGISTRY.Value.InvokeWith(this.MemoizerConfiguration)
                : new Memoizer<TParam1, TParam2, TResult>(this.MemoizerConfiguration);
        }
    }


    public class MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TResult>
    {
        readonly ExpirationType expirationType = ExpirationType.Relative;
        readonly int expirationValue;

        readonly MemoizerFactory<TParam1, TParam2, TResult> memoizerFactory;

        public MemoizerFactory_AwaitingExpirationUnit(int expirationValue, MemoizerFactory<TParam1, TParam2, TResult> memoizerFactory)
        {
            this.expirationValue = expirationValue;
            this.memoizerFactory = memoizerFactory;
        }

        MemoizerFactory<TParam1, TParam2, TResult> ConfigureCacheItemPolicyWithTimeUnit(TimeUnit timeUnit)
        {
            this.memoizerFactory.ExpirationType = this.expirationType;
            this.memoizerFactory.ExpirationValue = this.expirationValue;
            this.memoizerFactory.ExpirationTimeUnit = timeUnit;

            return this.memoizerFactory;
        }

        public MemoizerFactory<TParam1, TParam2, TResult> Milliseconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Milliseconds); } }
        public MemoizerFactory<TParam1, TParam2, TResult> Seconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Seconds); } }
        public MemoizerFactory<TParam1, TParam2, TResult> Minutes { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Minutes); } }
        public MemoizerFactory<TParam1, TParam2, TResult> Hours { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Hours); } }
        public MemoizerFactory<TParam1, TParam2, TResult> Days { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Days); } }
    }
    #endregion

    #region MemoizerFactory<TParam1, TParam2, TParam3, TResult>
    public class MemoizerFactory<TParam1, TParam2, TParam3, TResult>
    {
        readonly Func<TParam1, TParam2, TParam3, TResult> function;
        internal Func<TParam1, TParam2, TParam3, TResult> Function { get { return this.function; } }

        internal ExpirationType ExpirationType { get; set; }
        internal int ExpirationValue { get; set; }
        internal TimeUnit ExpirationTimeUnit { get; set; }

        Action<String> loggerMethod;
        internal Action<String> LoggerAction { get { return this.loggerMethod; } }

        MemoizerConfiguration MemoizerConfiguration { get { return new MemoizerConfiguration(this.function, this.ExpirationType, this.ExpirationValue, this.ExpirationTimeUnit, this.loggerMethod); } }
        internal MemoizerFactory(Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized) { this.function = functionToBeMemoized; }
        public MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TResult> KeepItemsCachedFor(int cacheExpirationValue) { return new MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TResult>(cacheExpirationValue, this); }
        public MemoizerFactory<TParam1, TParam2, TParam3, TResult> InstrumentWith(Action<String> loggerMethod)
        {
            this.loggerMethod = loggerMethod;
            return this;
        }
        public IMemoizer<TParam1, TParam2, TParam3, TResult> CreateMemoizer() { return GetMemoizer(false); }
        public IMemoizer<TParam1, TParam2, TParam3, TResult> GetMemoizer(bool cacheAndShareMemoizerInstance = true)
        {
            return cacheAndShareMemoizerInstance
                ? MemoizerRegistry<TParam1, TParam2, TParam3, TResult>.LAZY_MEMOIZER_REGISTRY.Value.InvokeWith(this.MemoizerConfiguration)
                : new Memoizer<TParam1, TParam2, TParam3, TResult>(this.MemoizerConfiguration);
        }
    }


    public class MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TResult>
    {
        readonly ExpirationType expirationType = ExpirationType.Relative;
        readonly int expirationValue;

        readonly MemoizerFactory<TParam1, TParam2, TParam3, TResult> memoizerFactory;

        public MemoizerFactory_AwaitingExpirationUnit(int expirationValue, MemoizerFactory<TParam1, TParam2, TParam3, TResult> memoizerFactory)
        {
            this.expirationValue = expirationValue;
            this.memoizerFactory = memoizerFactory;
        }

        MemoizerFactory<TParam1, TParam2, TParam3, TResult> ConfigureCacheItemPolicyWithTimeUnit(TimeUnit timeUnit)
        {
            this.memoizerFactory.ExpirationType = this.expirationType;
            this.memoizerFactory.ExpirationValue = this.expirationValue;
            this.memoizerFactory.ExpirationTimeUnit = timeUnit;

            return this.memoizerFactory;
        }

        public MemoizerFactory<TParam1, TParam2, TParam3, TResult> Milliseconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Milliseconds); } }
        public MemoizerFactory<TParam1, TParam2, TParam3, TResult> Seconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Seconds); } }
        public MemoizerFactory<TParam1, TParam2, TParam3, TResult> Minutes { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Minutes); } }
        public MemoizerFactory<TParam1, TParam2, TParam3, TResult> Hours { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Hours); } }
        public MemoizerFactory<TParam1, TParam2, TParam3, TResult> Days { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Days); } }
    }
    #endregion

    #region MemoizerFactory<TParam1, TParam2,TParam3, TParam3,TParam4, TResult>
    public class MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult>
    {

        readonly Func<TParam1, TParam2, TParam3, TParam4, TResult> function;
        internal Func<TParam1, TParam2, TParam3, TParam4, TResult> Function { get { return this.function; } }

        internal ExpirationType ExpirationType { get; set; }
        internal int ExpirationValue { get; set; }
        internal TimeUnit ExpirationTimeUnit { get; set; }

        Action<String> loggerMethod;
        internal Action<String> LoggerAction { get { return this.loggerMethod; } }

        MemoizerConfiguration MemoizerConfiguration { get { return new MemoizerConfiguration(this.function, this.ExpirationType, this.ExpirationValue, this.ExpirationTimeUnit, this.loggerMethod); } }
        internal MemoizerFactory(Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized) { this.function = functionToBeMemoized; }
        public MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TParam4, TResult> KeepItemsCachedFor(int cacheExpirationValue) { return new MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TParam4, TResult>(cacheExpirationValue, this); }
        public MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult> InstrumentWith(Action<String> loggerMethod)
        {
            this.loggerMethod = loggerMethod;
            return this;
        }
        public IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult> CreateMemoizer() { return GetMemoizer(false); }
        public IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult> GetMemoizer(bool cacheAndShareMemoizerInstance = true)
        {
            return cacheAndShareMemoizerInstance
                ? MemoizerRegistry<TParam1, TParam2, TParam3, TParam4, TResult>.LAZY_MEMOIZER_REGISTRY.Value.InvokeWith(this.MemoizerConfiguration)
                : new Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>(this.MemoizerConfiguration);
        }
    }


    public class MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TParam4, TResult>
    {
        readonly ExpirationType expirationType = ExpirationType.Relative;
        readonly int expirationValue;

        readonly MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult> memoizerFactory;

        public MemoizerFactory_AwaitingExpirationUnit(int expirationValue, MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult> memoizerFactory)
        {
            this.expirationValue = expirationValue;
            this.memoizerFactory = memoizerFactory;
        }

        MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult> ConfigureCacheItemPolicyWithTimeUnit(TimeUnit timeUnit)
        {
            this.memoizerFactory.ExpirationType = this.expirationType;
            this.memoizerFactory.ExpirationValue = this.expirationValue;
            this.memoizerFactory.ExpirationTimeUnit = timeUnit;

            return this.memoizerFactory;
        }

        public MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult> Milliseconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Milliseconds); } }
        public MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult> Seconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Seconds); } }
        public MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult> Minutes { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Minutes); } }
        public MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult> Hours { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Hours); } }
        public MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult> Days { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Days); } }
    }
    #endregion
}
