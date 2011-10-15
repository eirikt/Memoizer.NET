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
using System.Reflection;
using System.Runtime.Caching;
using System.Runtime.Serialization;

namespace Memoizer.NET
{
    #region Func extension methods
    public static class FuncExtensionMethods
    {
        // TODO: ...
        //#region CreateMemoizer()
        //public static IMemoizer<TParam1, TResult> CreateMemoizer<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized)
        //{
        //    return new MemoizerBuilder<TParam1, TResult>(functionToBeMemoized).CreateMemoizer();
        //}
        //public static IMemoizer<TParam1, TParam2, TResult> CreateMemoizer<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized)
        //{
        //    return new MemoizerBuilder<TParam1, TParam2, TResult>(functionToBeMemoized).CreateMemoizer();
        //}
        //public static IMemoizer<TParam1, TParam2, TParam3, TResult> CreateMemoizer<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized)
        //{
        //    return new MemoizerBuilder<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized).CreateMemoizer();
        //}
        //public static IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult> CreateMemoizer<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized)
        //{
        //    return new MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized).GetMemoizer();
        //}
        //#endregion

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

        #region Memoize() [go into memoizer builder mode]
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
        #endregion

        #region CacheFor(int expirationValue)
        public static MemoizerBuilder_AwaitingExpirationUnit<TParam1, TResult> CacheFor<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized, int expirationValue)
        {
            return new MemoizerBuilder<TParam1, TResult>(functionToBeMemoized).KeepItemsCachedFor(expirationValue);
        }
        public static MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TResult> CacheFor<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized, int expirationValue)
        {
            return new MemoizerBuilder<TParam1, TParam2, TResult>(functionToBeMemoized).KeepItemsCachedFor(expirationValue);
        }
        public static MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TResult> CacheFor<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized, int expirationValue)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized).KeepItemsCachedFor(expirationValue);
        }
        public static MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TParam4, TResult> CacheFor<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized, int expirationValue)
        {
            return new MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized).KeepItemsCachedFor(expirationValue);
        }
        #endregion

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
        public static CacheItemPolicy CreateCacheItemPolicy(ExpirationType expirationType, int expirationValue, TimeUnit expirationTimeUnit)
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

    #region ReflectionHelper
    public class ReflectionHelper
    {
        public static object GetProperty(object source, string propertyName)
        {
            PropertyInfo propertyInfo = source.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo == null) { throw new ArgumentException("Could not find a property with the name '" + propertyName + "' in the class '" + source.GetType().Namespace + "." + source.GetType().Name + "'"); }
            return propertyInfo.GetValue(source, null);
        }
    }
    #endregion

    #region MemoizerHelper (mostly reflection-based for the time being...)
    public class MemoizerHelper
    {
        public static int[] PRIMES = new[] { 31, 37, 43, 47, 59, 61, 71, 73, 89, 97, 101, 103, 113, 127, 131, 137 };

        static readonly ObjectIDGenerator OBJECT_ID_GENERATOR = new ObjectIDGenerator();


        public static string CreateMemoizerBuilderHash(object memoizerBuilder)
        {
            var func = ReflectionHelper.GetProperty(memoizerBuilder, "Function");
            var expirationType = ReflectionHelper.GetProperty(memoizerBuilder, "ExpirationType");
            var expirationValue = ReflectionHelper.GetProperty(memoizerBuilder, "ExpirationValue");
            var expirationTimeUnit = ReflectionHelper.GetProperty(memoizerBuilder, "ExpirationTimeUnit");

            bool firstTime;
            long funcId = OBJECT_ID_GENERATOR.GetId(func, out firstTime);

            return CreateParameterHash(funcId, expirationType, expirationValue, expirationTimeUnit);
        }


        public static string CreateParameterHash(params object[] args)
        {
            if (args == null) { throw new ArgumentException("Argument array cannot be null"); }

            if (args.Length == 1)
            {
                if (args[0].GetType().Name.StartsWith("MemoizerBuilder"))
                    return CreateMemoizerBuilderHash(args[0]);

                if (args[0].GetType().Name.StartsWith("Func"))
                {
                    bool firstTime;
                    long funcId = OBJECT_ID_GENERATOR.GetId(args[0], out firstTime);
                    return funcId.ToString();
                }
                return args[0].GetHashCode().ToString();
            }

            int retVal = 0;
            for (int i = 0; i < args.Length; ++i)
                retVal = retVal * PRIMES[i] + args[i].GetHashCode();

            return retVal.ToString();
        }


        public static string CreateFunctionHash(object source) { return CreateParameterHash(source); }
    }
    #endregion

    #region MemoizerBuilder<TParam1, TResult>
    public class MemoizerBuilder<TParam1, TResult>
    {
        static Func<MemoizerBuilder<TParam1, TResult>, Memoizer<TParam1, TResult>> CREATE_MEMOIZER_FROM_BUILDER =
            new Func<MemoizerBuilder<TParam1, TResult>, Memoizer<TParam1, TResult>>(
                delegate(MemoizerBuilder<TParam1, TResult> memoizerBuilder)
                {
                    Console.WriteLine("Creating Memoizer<TParam1, TResult> from MemoizerBuilder<TParam1, TResult> [hash=" + MemoizerHelper.CreateMemoizerBuilderHash(memoizerBuilder) + "]...");
                    return new Memoizer<TParam1, TResult>(memoizerBuilder);
                });

        static Func<Memoizer<MemoizerBuilder<TParam1, TResult>, Memoizer<TParam1, TResult>>> CREATE_MEMOIZER_MEMOIZER =
            new Func<Memoizer<MemoizerBuilder<TParam1, TResult>, Memoizer<TParam1, TResult>>>(
                delegate()
                {
                    Console.WriteLine("Creating Memoizer for Memoizer items with key MemoizerBuilder<TParam1, TResult>...");
                    return new Memoizer<MemoizerBuilder<TParam1, TResult>, Memoizer<TParam1, TResult>>(CREATE_MEMOIZER_FROM_BUILDER);
                });

        static Lazy<Memoizer<MemoizerBuilder<TParam1, TResult>, Memoizer<TParam1, TResult>>> LAZY_MEMOIZER_MEMOIZER =
            new Lazy<Memoizer<MemoizerBuilder<TParam1, TResult>, Memoizer<TParam1, TResult>>>(
                CREATE_MEMOIZER_MEMOIZER,
                isThreadSafe: typeof(Memoizer<MemoizerBuilder<TParam1, TResult>, Memoizer<TParam1, TResult>>) is IThreadSafe);


        readonly Func<TParam1, TResult> function;

        internal ExpirationType ExpirationType { get; set; }
        internal int ExpirationValue { get; set; }
        internal TimeUnit ExpirationTimeUnit { get; set; }

        Action<String> loggerMethod;


        internal MemoizerBuilder(Func<TParam1, TResult> functionToBeMemoized)
        {
            this.function = functionToBeMemoized;
        }

        internal Func<TParam1, TResult> Function
        {
            get { return this.function; }
        }

        internal Action<String> LoggerAction
        {
            get { return this.loggerMethod; }
        }


        public MemoizerBuilder_AwaitingExpirationUnit<TParam1, TResult> KeepItemsCachedFor(int cacheExpirationValue)
        {
            return new MemoizerBuilder_AwaitingExpirationUnit<TParam1, TResult>(cacheExpirationValue, this);
        }

        public MemoizerBuilder<TParam1, TResult> InstrumentWith(Action<String> loggingAction)
        {
            this.loggerMethod = loggingAction;
            return this;
        }

        /// <summary>
        /// Force creation and <i>not</i> caching/sharing of created memoizer instance.
        /// </summary>
        public IMemoizer<TParam1, TResult> CreateMemoizer()
        {
            return GetMemoizer(cacheAndShareMemoizerInstance: false);
        }

        public IMemoizer<TParam1, TResult> GetMemoizer(bool cacheAndShareMemoizerInstance = true)
        {
            return cacheAndShareMemoizerInstance ? LAZY_MEMOIZER_MEMOIZER.Value.InvokeWith(this) : new Memoizer<TParam1, TResult>(this);
        }
    }


    public class MemoizerBuilder_AwaitingExpirationUnit<TParam1, TResult>
    {
        readonly ExpirationType expirationType = ExpirationType.Relative;
        readonly int expirationValue;

        readonly MemoizerBuilder<TParam1, TResult> memoizerBuilder;

        public MemoizerBuilder_AwaitingExpirationUnit(int expirationValue, MemoizerBuilder<TParam1, TResult> memoizerBuilder)
        {
            this.expirationValue = expirationValue;
            this.memoizerBuilder = memoizerBuilder;
        }

        MemoizerBuilder<TParam1, TResult> ConfigureCacheItemPolicyWithTimeUnit(TimeUnit timeUnit)
        {
            this.memoizerBuilder.ExpirationType = this.expirationType;
            this.memoizerBuilder.ExpirationValue = this.expirationValue;
            this.memoizerBuilder.ExpirationTimeUnit = timeUnit;

            return this.memoizerBuilder;
        }

        public MemoizerBuilder<TParam1, TResult> Milliseconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Milliseconds); } }
        public MemoizerBuilder<TParam1, TResult> Seconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Seconds); } }
        public MemoizerBuilder<TParam1, TResult> Minutes { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Minutes); } }
        public MemoizerBuilder<TParam1, TResult> Hours { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Hours); } }
        public MemoizerBuilder<TParam1, TResult> Days { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Days); } }
    }
    #endregion

    #region MemoizerBuilder<TParam1, TParam2, TResult>
    public class MemoizerBuilder<TParam1, TParam2, TResult>
    {
        static readonly Lazy<Memoizer<MemoizerBuilder<TParam1, TParam2, TResult>, Memoizer<TParam1, TParam2, TResult>>> LAZY_MEMOIZER_MEMOIZER =
            new Lazy<Memoizer<MemoizerBuilder<TParam1, TParam2, TResult>, Memoizer<TParam1, TParam2, TResult>>>(
                () => new Memoizer<MemoizerBuilder<TParam1, TParam2, TResult>, Memoizer<TParam1, TParam2, TResult>>(
                    memoizerBuilder => new Memoizer<TParam1, TParam2, TResult>(memoizerBuilder)),
                isThreadSafe: typeof(Memoizer<MemoizerBuilder<TParam1, TParam2, TResult>, Memoizer<TParam1, TParam2, TResult>>) is IThreadSafe);

        readonly Func<TParam1, TParam2, TResult> function;
        internal Func<TParam1, TParam2, TResult> Function { get { return this.function; } }

        internal ExpirationType ExpirationType { get; set; }
        internal int ExpirationValue { get; set; }
        internal TimeUnit ExpirationTimeUnit { get; set; }

        Action<String> loggerMethod;
        internal Action<String> LoggerAction { get { return this.loggerMethod; } }

        internal MemoizerBuilder(Func<TParam1, TParam2, TResult> functionToBeMemoized)
        {
            this.function = functionToBeMemoized;
        }

        public MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TResult> KeepItemsCachedFor(int cacheExpirationValue)
        {
            return new MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TResult>(cacheExpirationValue, this);
        }

        public MemoizerBuilder<TParam1, TParam2, TResult> InstrumentWith(Action<String> loggerMethod)
        {
            this.loggerMethod = loggerMethod;
            return this;
        }

        public IMemoizer<TParam1, TParam2, TResult> CreateMemoizer() { return GetMemoizer(false); }

        public IMemoizer<TParam1, TParam2, TResult> GetMemoizer(bool cacheAndShareMemoizerInstance = true)
        {
            return cacheAndShareMemoizerInstance ? LAZY_MEMOIZER_MEMOIZER.Value.InvokeWith(this) : new Memoizer<TParam1, TParam2, TResult>(this);
        }
    }


    public class MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TResult>
    {
        readonly ExpirationType expirationType = ExpirationType.Relative;
        readonly int expirationValue;

        readonly MemoizerBuilder<TParam1, TParam2, TResult> memoizerBuilder;

        public MemoizerBuilder_AwaitingExpirationUnit(int expirationValue, MemoizerBuilder<TParam1, TParam2, TResult> memoizerBuilder)
        {
            this.expirationValue = expirationValue;
            this.memoizerBuilder = memoizerBuilder;
        }

        MemoizerBuilder<TParam1, TParam2, TResult> ConfigureCacheItemPolicyWithTimeUnit(TimeUnit timeUnit)
        {
            this.memoizerBuilder.ExpirationType = this.expirationType;
            this.memoizerBuilder.ExpirationValue = this.expirationValue;
            this.memoizerBuilder.ExpirationTimeUnit = timeUnit;

            return this.memoizerBuilder;
        }

        public MemoizerBuilder<TParam1, TParam2, TResult> Milliseconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Milliseconds); } }
        public MemoizerBuilder<TParam1, TParam2, TResult> Seconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Seconds); } }
        public MemoizerBuilder<TParam1, TParam2, TResult> Minutes { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Minutes); } }
        public MemoizerBuilder<TParam1, TParam2, TResult> Hours { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Hours); } }
        public MemoizerBuilder<TParam1, TParam2, TResult> Days { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Days); } }
    }
    #endregion

    #region MemoizerBuilder<TParam1, TParam2, TParam3, TResult>
    public class MemoizerBuilder<TParam1, TParam2, TParam3, TResult>
    {
        static readonly Lazy<Memoizer<MemoizerBuilder<TParam1, TParam2, TParam3, TResult>, Memoizer<TParam1, TParam2, TParam3, TResult>>> LAZY_MEMOIZER_MEMOIZER =
            new Lazy<Memoizer<MemoizerBuilder<TParam1, TParam2, TParam3, TResult>, Memoizer<TParam1, TParam2, TParam3, TResult>>>(
                () => new Memoizer<MemoizerBuilder<TParam1, TParam2, TParam3, TResult>, Memoizer<TParam1, TParam2, TParam3, TResult>>(
                    memoizerBuilder => new Memoizer<TParam1, TParam2, TParam3, TResult>(memoizerBuilder)),
                isThreadSafe: typeof(Memoizer<MemoizerBuilder<TParam1, TParam2, TParam3, TResult>, Memoizer<TParam1, TParam2, TParam3, TResult>>) is IThreadSafe);

        readonly Func<TParam1, TParam2, TParam3, TResult> function;
        internal Func<TParam1, TParam2, TParam3, TResult> Function { get { return this.function; } }

        internal ExpirationType ExpirationType { get; set; }
        internal int ExpirationValue { get; set; }
        internal TimeUnit ExpirationTimeUnit { get; set; }

        Action<String> loggerMethod;
        internal Action<String> LoggerAction { get { return this.loggerMethod; } }

        internal MemoizerBuilder(Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized)
        {
            this.function = functionToBeMemoized;
        }

        public MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TResult> KeepItemsCachedFor(int cacheExpirationValue)
        {
            return new MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TResult>(cacheExpirationValue, this);
        }

        public MemoizerBuilder<TParam1, TParam2, TParam3, TResult> InstrumentWith(Action<String> loggerMethod)
        {
            this.loggerMethod = loggerMethod;
            return this;
        }

        public IMemoizer<TParam1, TParam2, TParam3, TResult> CreateMemoizer() { return GetMemoizer(false); }

        public IMemoizer<TParam1, TParam2, TParam3, TResult> GetMemoizer(bool cacheAndShareMemoizerInstance = true)
        {
            return cacheAndShareMemoizerInstance ? LAZY_MEMOIZER_MEMOIZER.Value.InvokeWith(this) : new Memoizer<TParam1, TParam2, TParam3, TResult>(this);
        }
    }


    public class MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TResult>
    {
        readonly ExpirationType expirationType = ExpirationType.Relative;
        readonly int expirationValue;

        readonly MemoizerBuilder<TParam1, TParam2, TParam3, TResult> memoizerBuilder;

        public MemoizerBuilder_AwaitingExpirationUnit(int expirationValue, MemoizerBuilder<TParam1, TParam2, TParam3, TResult> memoizerBuilder)
        {
            this.expirationValue = expirationValue;
            this.memoizerBuilder = memoizerBuilder;
        }

        MemoizerBuilder<TParam1, TParam2, TParam3, TResult> ConfigureCacheItemPolicyWithTimeUnit(TimeUnit timeUnit)
        {
            this.memoizerBuilder.ExpirationType = this.expirationType;
            this.memoizerBuilder.ExpirationValue = this.expirationValue;
            this.memoizerBuilder.ExpirationTimeUnit = timeUnit;

            return this.memoizerBuilder;
        }

        public MemoizerBuilder<TParam1, TParam2, TParam3, TResult> Milliseconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Milliseconds); } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TResult> Seconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Seconds); } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TResult> Minutes { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Minutes); } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TResult> Hours { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Hours); } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TResult> Days { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Days); } }
    }
    #endregion

    #region MemoizerBuilder<TParam1, TParam2,TParam3, TParam3,TParam4, TResult>
    public class MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>
    {
        static readonly Lazy<Memoizer<MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>, Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>>> LAZY_MEMOIZER_MEMOIZER =
            new Lazy<Memoizer<MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>, Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>>>(
                () => new Memoizer<MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>, Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>>(
                    memoizerBuilder => new Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>(memoizerBuilder)),
                isThreadSafe: typeof(Memoizer<MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult>, Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>>) is IThreadSafe);

        readonly Func<TParam1, TParam2, TParam3, TParam4, TResult> function;
        internal Func<TParam1, TParam2, TParam3, TParam4, TResult> Function { get { return this.function; } }

        internal ExpirationType ExpirationType { get; set; }
        internal int ExpirationValue { get; set; }
        internal TimeUnit ExpirationTimeUnit { get; set; }

        Action<String> loggerMethod;
        internal Action<String> LoggerAction { get { return this.loggerMethod; } }

        internal MemoizerBuilder(Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized)
        {
            this.function = functionToBeMemoized;
        }

        public MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TParam4, TResult> KeepItemsCachedFor(int cacheExpirationValue)
        {
            return new MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TParam4, TResult>(cacheExpirationValue, this);
        }

        public MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> InstrumentWith(Action<String> loggerMethod)
        {
            this.loggerMethod = loggerMethod;
            return this;
        }

        public IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult> CreateMemoizer() { return GetMemoizer(false); }

        public IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult> GetMemoizer(bool cacheAndShareMemoizerInstance = true)
        {
            return cacheAndShareMemoizerInstance ? LAZY_MEMOIZER_MEMOIZER.Value.InvokeWith(this) : new Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>(this);
        }
    }


    public class MemoizerBuilder_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TParam4, TResult>
    {
        readonly ExpirationType expirationType = ExpirationType.Relative;
        readonly int expirationValue;

        readonly MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> memoizerBuilder;

        public MemoizerBuilder_AwaitingExpirationUnit(int expirationValue, MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> memoizerBuilder)
        {
            this.expirationValue = expirationValue;
            this.memoizerBuilder = memoizerBuilder;
        }

        MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> ConfigureCacheItemPolicyWithTimeUnit(TimeUnit timeUnit)
        {
            this.memoizerBuilder.ExpirationType = this.expirationType;
            this.memoizerBuilder.ExpirationValue = this.expirationValue;
            this.memoizerBuilder.ExpirationTimeUnit = timeUnit;

            return this.memoizerBuilder;
        }

        public MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> Milliseconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Milliseconds); } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> Seconds { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Seconds); } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> Minutes { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Minutes); } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> Hours { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Hours); } }
        public MemoizerBuilder<TParam1, TParam2, TParam3, TParam4, TResult> Days { get { return ConfigureCacheItemPolicyWithTimeUnit(TimeUnit.Days); } }
    }
    #endregion
}
