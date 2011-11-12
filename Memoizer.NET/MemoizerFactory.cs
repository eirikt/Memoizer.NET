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
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Caching;
using System.Runtime.Serialization;
using System.Threading.Tasks;


namespace Memoizer.NET
{
    #region Func extension methods
    public static class FuncExtensionMethods
    {
        // TODO: ...
        //#region CreateMemoizer()
        //public static IMemoizer<TParam1, TResult> CreateMemoizer<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized)
        //{
        //    return new MemoizerFactory<TParam1, TResult>(functionToBeMemoized).CreateMemoizer();
        //}
        //public static IMemoizer<TParam1, TParam2, TResult> CreateMemoizer<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized)
        //{
        //    return new MemoizerFactory<TParam1, TParam2, TResult>(functionToBeMemoized).CreateMemoizer();
        //}
        //public static IMemoizer<TParam1, TParam2, TParam3, TResult> CreateMemoizer<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized)
        //{
        //    return new MemoizerFactory<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized).CreateMemoizer();
        //}
        //public static IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult> CreateMemoizer<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized)
        //{
        //    return new MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized).GetMemoizer();
        //}
        //#endregion

        #region GetMemoizer()
        public static IMemoizer<TParam1, TResult> GetMemoizer<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized)
        {
            return new MemoizerFactory<TParam1, TResult>(functionToBeMemoized).GetMemoizer();
        }
        public static IMemoizer<TParam1, TParam2, TResult> GetMemoizer<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized)
        {
            return new MemoizerFactory<TParam1, TParam2, TResult>(functionToBeMemoized).GetMemoizer();
        }
        public static IMemoizer<TParam1, TParam2, TParam3, TResult> GetMemoizer<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized)
        {
            return new MemoizerFactory<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized).GetMemoizer();
        }
        public static IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult> GetMemoizer<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized)
        {
            return new MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized).GetMemoizer();
        }
        #endregion

        #region Memoize() [go into memoizer builder mode]
        public static MemoizerFactory<TParam1, TResult> Memoize<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized)
        {
            return new MemoizerFactory<TParam1, TResult>(functionToBeMemoized);
        }
        public static MemoizerFactory<TParam1, TParam2, TResult> Memoize<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized)
        {
            return new MemoizerFactory<TParam1, TParam2, TResult>(functionToBeMemoized);
        }
        public static MemoizerFactory<TParam1, TParam2, TParam3, TResult> Memoize<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized)
        {
            return new MemoizerFactory<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized);
        }
        public static MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult> Memoize<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized)
        {
            return new MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized);
        }
        #endregion

        #region UnMemoize() [remove all memoizers using this particular Func from memoizer^2 registry]
        public static void UnMemoize<TParam1, TResult>(this Func<TParam1, TResult> functionToUnMemoize)
        {
            MemoizerFactory<TParam1, TResult>.RemoveMemoizersFromRegistryHavingFunction(functionToUnMemoize);
        }
        public static void UnMemoize<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized)
        {
            throw new NotImplementedException();
            //MemoizerFactory<TParam1, TParam2, TResult>.RemoveFunction(functionToUnMemoize);
        }
        public static void UnMemoize<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized)
        {
            throw new NotImplementedException();
            //MemoizerFactory<TParam1, TParam2, TParam3, TResult>.RemoveFunction(functionToUnMemoize);
        }
        public static void UnMemoize<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized)
        {
            throw new NotImplementedException();
            //MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult>.RemoveFunction(functionToUnMemoize);
        }
        #endregion

        #region CacheFor(int expirationValue)
        public static MemoizerFactory_AwaitingExpirationUnit<TParam1, TResult> CacheFor<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized, int expirationValue)
        {
            return new MemoizerFactory<TParam1, TResult>(functionToBeMemoized).KeepItemsCachedFor(expirationValue);
        }
        public static MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TResult> CacheFor<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized, int expirationValue)
        {
            return new MemoizerFactory<TParam1, TParam2, TResult>(functionToBeMemoized).KeepItemsCachedFor(expirationValue);
        }
        public static MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TResult> CacheFor<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized, int expirationValue)
        {
            return new MemoizerFactory<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized).KeepItemsCachedFor(expirationValue);
        }
        public static MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TParam4, TResult> CacheFor<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized, int expirationValue)
        {
            return new MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized).KeepItemsCachedFor(expirationValue);
        }
        #endregion

        #region CachedInvoke(TParam... args)
        public static TResult CachedInvoke<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized, TParam1 arg1)
        {
            return new MemoizerFactory<TParam1, TResult>(functionToBeMemoized).GetMemoizer().InvokeWith(arg1);
        }
        public static TResult CachedInvoke<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized, TParam1 arg1, TParam2 arg2)
        {
            return new MemoizerFactory<TParam1, TParam2, TResult>(functionToBeMemoized).GetMemoizer().InvokeWith(arg1, arg2);
        }
        public static TResult CachedInvoke<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized, TParam1 arg1, TParam2 arg2, TParam3 arg3)
        {
            return new MemoizerFactory<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized).GetMemoizer().InvokeWith(arg1, arg2, arg3);
        }
        public static TResult CachedInvoke<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized, TParam1 arg1, TParam2 arg2, TParam3 arg3, TParam4 arg4)
        {
            return new MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized).GetMemoizer().InvokeWith(arg1, arg2, arg3, arg4);
        }
        #endregion

        #region RemoveFromCache() [remove this particular cached invocation from all memoizers using this Func from memoizer^2 registry]
        public static void RemoveFromCache<TParam1, TResult>(this Func<TParam1, TResult> memoizedFunction, TParam1 arg1ForRemoval)
        {
            MemoizerFactory<TParam1, TResult>.RemoveMemoizersFromRegistryHavingFunction(memoizedFunction);
        }
        #endregion
    }
    #endregion

    #region Enums
    public enum ExpirationType { Relative, Absolute }

    public enum TimeUnit { Milliseconds, Seconds, Minutes, Hours, Days }
    #endregion

    #region CacheItemPolicyFactory
    public class CacheItemPolicyFactory
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
            PropertyInfo propertyInfo = source.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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

        internal static long GetObjectId(object arg, ref bool firstTime)
        {
            return OBJECT_ID_GENERATOR.GetId(arg, out firstTime);
        }


        //public static string CreateMemoizerHash(object memoizerConfig)
        //{
        //    var func = ReflectionHelper.GetProperty(memoizerConfig, "Function");
        //    var expirationType = ReflectionHelper.GetProperty(memoizerConfig, "ExpirationType");
        //    var expirationValue = Reflection[0]Helper.GetProperty(memoizerConfig, "ExpirationValue");
        //    var expirationTimeUnit = ReflectionHelper.GetProperty(memoizerConfig, "ExpirationTimeUnit");

        //    bool firstTime = false;
        //    string funcId = GetObjectId(func, ref firstTime);

        //    return CreateParameterHash(funcId, expirationType, expirationValue, expirationTimeUnit);
        //}


        public static string CreateParameterHash(params object[] args)
        //public static string CreateParameterHash(bool possiblySharedMemoizerConfig = true, params object[] args)
        {
            if (args == null) { throw new ArgumentException("Argument array cannot be null"); }

            if (args.Length == 1)
            {
                //if (args[0].GetType().Name.StartsWith("MemoizerConfiguration"))
                //{
                //    //return CreateMemoizerHash(args[0]);
                //    return args[0].GetHashCode().ToString();
                //}

                ////if (args[0].GetType().Name.StartsWith("Func"))
                ////    return GetObjectId(args[0]);

                return args[0].GetHashCode().ToString();
            }

            int retVal = args[0].GetHashCode() * PRIMES[0];
            for (int i = 1; i < args.Length; ++i)
                retVal = retVal * PRIMES[i] + args[i].GetHashCode();

            return retVal.ToString();
        }


        public static long CreateFunctionHash(object func)
        {
            bool firstTime = false;
            return GetObjectId(func, ref firstTime);
        }


        // TODO: common method for Func removal
        //internal static void RemoveFunction<T>(object functionToUnMemoize, Lazy<T> LAZY_MEMOIZER_MEMOIZER):T is IMemoizer
        //{
        //    foreach (KeyValuePair<string, object> keyValuePair in LAZY_MEMOIZER_MEMOIZER.Value.cache)
        //    {
        //        lock (keyValuePair.Value)
        //        {
        //            string memoizerConfigHashCode2 = keyValuePair.Key.PadLeft(10, '0');
        //            string memoizerConfigHashCode = memoizerConfigHashCode2.Substring(0, 5);

        //            bool firstTime = false;
        //            long functionToUnMemoizeHashCode_long = MemoizerHelper.GetObjectId(functionToUnMemoize, ref firstTime);
        //            if (functionToUnMemoizeHashCode_long > 21474) { throw new InvalidOperationException("Memoizer.NET supports only 21474 different Func references..."); }
        //            string functionToUnMemoizeHashCode = functionToUnMemoizeHashCode_long.ToString().PadLeft(5, '0');

        //            if (functionToUnMemoizeHashCode == memoizerConfigHashCode)
        //            {
        //                // Dispose of the MemoryCache backing object
        //                PropertyInfo propertyInfo = keyValuePair.Value.GetType().GetProperty("Result");
        //                if (propertyInfo == null)
        //                    throw new ArgumentException("Could not find a property with the name 'Result'");

        //                var memoizer = propertyInfo.GetValue(keyValuePair.Value, null);

        //                MethodInfo method = memoizer.GetType().GetMethod("Dispose");//, BindingFlags.Public);
        //                if (method == null)
        //                    throw new ArgumentException("Could not find a method with the name 'Dispose'");

        //                method.Invoke(memoizer, null);

        //                // Remove the memoizer from the memoizer^2 registry
        //                LAZY_MEMOIZER_MEMOIZER.Value.cache.Remove(keyValuePair.Key);
        //            }
        //        }
        //    }
        //}
    }
    #endregion

    #region MemoizerFactory<TParam1, TResult>
    // TODO: rename to MemoizerConfig ...? It's just a better class name for a public class
    public class MemoizerFactory<TParam1, TResult>
    {
        #region CLR-wide shared memoizer of memoizers
        // Static delegate for creating a memoizer with TParam1 as key type, and TResult as item type, from a MemoizerFactory instance
        static readonly Func<MemoizerConfiguration, Memoizer<TParam1, TResult>> CREATE_MEMOIZER_FROM_CONFIG =
            new Func<MemoizerConfiguration, Memoizer<TParam1, TResult>>(
                delegate(MemoizerConfiguration memoizerConfig)
                {
                    //Console.WriteLine("Creating Memoizer<TParam1, TResult> from MemoizerFactory<TParam1, TResult> [hash=" + MemoizerHelper.CreateMemoizerFactoryHash(memoizerFactory) + "]...");
                    return new Memoizer<TParam1, TResult>(memoizerConfig);
                });

        // Static delegate for creating a memoizer with MemoizerFactory as key type, and Memoizer as item type, from function CREATE_MEMOIZER_FROM_FACTORY above
        static readonly Func<Memoizer<MemoizerConfiguration, Memoizer<TParam1, TResult>>> CREATE_MEMOIZER_MEMOIZER =
            new Func<Memoizer<MemoizerConfiguration, Memoizer<TParam1, TResult>>>(
                delegate()
                {
                    //Console.WriteLine("Creating Memoizer for Memoizer items with key MemoizerFactory<TParam1, TResult>...");
                    return new Memoizer<MemoizerConfiguration, Memoizer<TParam1, TResult>>(CREATE_MEMOIZER_FROM_CONFIG);
                });

        // Static lazy-loaded memoizer of memoizers, from function above
        internal static readonly Lazy<Memoizer<MemoizerConfiguration, Memoizer<TParam1, TResult>>> LAZY_MEMOIZER_MEMOIZER =
            new Lazy<Memoizer<MemoizerConfiguration, Memoizer<TParam1, TResult>>>(
                CREATE_MEMOIZER_MEMOIZER,
                isThreadSafe: typeof(IMemoizer<MemoizerConfiguration, IMemoizer<TParam1, TResult>>) is IThreadSafe);
        #endregion

        /// <summary>
        /// Lock object for removal of the memoizer from the memoizer^2 registry.
        /// </summary>
        //static readonly object @lock = new Object();

        //internal static IList<Memoizer<TParam1, TResult>> FindMemoizersInRegistryHavingFunction(Func<TParam1, TResult> functionToRemoveCachedValueFrom)
        //{
        //    IList<Memoizer<TParam1, TResult>> memoizerList = new List<Memoizer<TParam1, TResult>>();
        //    foreach (KeyValuePair<string, object> keyValuePair in LAZY_MEMOIZER_MEMOIZER.Value.cache)
        //    {
        //        string memoizerConfigHashCode2 = keyValuePair.Key.PadLeft(10, '0');
        //        string memoizerConfigHashCode = memoizerConfigHashCode2.Substring(0, 5);

        //        bool firstTime = false;
        //        long functionToRemoveCachedValueFromHashCode_long = MemoizerHelper.GetObjectId(functionToRemoveCachedValueFrom, ref firstTime);
        //        if (functionToRemoveCachedValueFromHashCode_long > 21474) { throw new InvalidOperationException("Memoizer.NET supports only 21474 different Func references..."); }
        //        string functionToRemoveCachedValueFromHashCode = functionToRemoveCachedValueFromHashCode_long.ToString().PadLeft(5, '0');

        //        if (functionToRemoveCachedValueFromHashCode == memoizerConfigHashCode)
        //            memoizerList.Add((Memoizer<TParam1, TResult>)keyValuePair.Value);
        //    }
        //    return memoizerList;
        //}

        ////internal static IEnumerable<KeyValuePair<string, IMemoizer<TParam1, TResult>>> FindMemoizersInRegistryHavingFunction(Func<TParam1, TResult> functionToRemoveCachedValueFrom)
        //internal static IEnumerable<KeyValuePair<string, object>> FindMemoizersInRegistryHavingFunction(Func<TParam1, TResult> functionToRemoveCachedValueFrom)
        //{
        //    //IList<KeyValuePair<string, IMemoizer<TParam1, TResult>>> memoizerList = new List<KeyValuePair<string, IMemoizer<TParam1, TResult>>>();
        //    IList<KeyValuePair<string, object>> memoizerList = new List<KeyValuePair<string, object>>();
        //    //LAZY_MEMOIZER_MEMOIZER.Value.cache.GetEnum
        //    foreach (KeyValuePair<string, object> keyValuePair in LAZY_MEMOIZER_MEMOIZER.Value.cache)
        //    //foreach (KeyValuePair<string, IMemoizer<TParam1, TResult>> keyValuePair in LAZY_MEMOIZER_MEMOIZER.Value.cache)
        //    //for (int i = 0; i < LAZY_MEMOIZER_MEMOIZER.Value.cache.GetCount(); )
        //    {
        //        string memoizerConfigHashCode2 = keyValuePair.Key.PadLeft(10, '0');
        //        string memoizerConfigHashCode = memoizerConfigHashCode2.Substring(0, 5);

        //        bool firstTime = false;
        //        long functionToRemoveCachedValueFromHashCode_long = MemoizerHelper.GetObjectId(functionToRemoveCachedValueFrom, ref firstTime);
        //        if (functionToRemoveCachedValueFromHashCode_long > 21474) { throw new InvalidOperationException("Memoizer.NET supports only 21474 different Func references..."); }
        //        string functionToRemoveCachedValueFromHashCode = functionToRemoveCachedValueFromHashCode_long.ToString().PadLeft(5, '0');

        //        if (functionToRemoveCachedValueFromHashCode == memoizerConfigHashCode)
        //            //memoizerList.Add((KeyValuePair<string, IMemoizer<TParam1, TResult>>)keyValuePair);
        //            memoizerList.Add(keyValuePair);
        //    }
        //    return memoizerList;
        //}

        /// <summary>
        /// Coupled with the <code>MemoizerConfiguration.GetHashCode()</code> method.
        /// </summary>
        /// <param name="function">The memoized function to look for</param>
        /// <returns>An enumeration of keys pointing to memoizer instances in the memoizer^2 registry, having the given function</returns>
        internal static IEnumerable<string> FindMemoizerKeysInRegistryHavingFunction(Func<TParam1, TResult> function)
        {
            IList<string> memoizerKeyList = new List<string>();
            foreach (KeyValuePair<string, object> keyValuePair in LAZY_MEMOIZER_MEMOIZER.Value.cache)
            {
                string memoizerConfigHashCode2 = keyValuePair.Key.PadLeft(10, '0');
                string memoizerConfigHashCode = memoizerConfigHashCode2.Substring(0, 5);

                bool firstTime = false;
                long functionToRemoveCachedValueFromHashCode_long = MemoizerHelper.GetObjectId(function, ref firstTime);
                if (functionToRemoveCachedValueFromHashCode_long > 21474) { throw new InvalidOperationException("Memoizer.NET supports only 21474 different Func references..."); }
                string functionToRemoveCachedValueFromHashCode = functionToRemoveCachedValueFromHashCode_long.ToString().PadLeft(5, '0');

                if (functionToRemoveCachedValueFromHashCode == memoizerConfigHashCode)
                    memoizerKeyList.Add(keyValuePair.Key);
            }
            return memoizerKeyList;
        }

        /// <summary>
        /// Remove all memoizer instances having the given Func, from the memoizer^2 registry
        /// </summary>
        /// <param name="functionToUnMemoize">The memoized function to remove from the memoizer^2 registry</param>
        /// <returns>Number of memoizer instances removed from memoizer^2 registry</returns>
        internal static int RemoveMemoizersFromRegistryHavingFunction(Func<TParam1, TResult> functionToUnMemoize)
        {
            int numberOfMemoizersRemoved = 0;
            IEnumerable<string> memoizerKeyList = FindMemoizerKeysInRegistryHavingFunction(functionToUnMemoize);
            foreach (var memoizerKey in memoizerKeyList)
            {
                Task<Memoizer<TParam1, TResult>> cacheValueTask = (Task<Memoizer<TParam1, TResult>>)LAZY_MEMOIZER_MEMOIZER.Value.cache.Get(memoizerKey);
                IMemoizer<TParam1, TResult> memoizer = cacheValueTask.Result;
                memoizer.Dispose();
                LAZY_MEMOIZER_MEMOIZER.Value.cache.Remove(memoizerKey);
                ++numberOfMemoizersRemoved;
            }
            //foreach (KeyValuePair<string, object> keyValuePair in LAZY_MEMOIZER_MEMOIZER.Value.cache)
            //{
            //    lock (keyValuePair.Value)
            //    {
            //        string memoizerConfigHashCode2 = keyValuePair.Key.PadLeft(10, '0');
            //        string memoizerConfigHashCode = memoizerConfigHashCode2.Substring(0, 5);

            //        bool firstTime = false;
            //        long functionToUnMemoizeHashCode_long = MemoizerHelper.GetObjectId(functionToUnMemoize, ref firstTime);
            //        if (functionToUnMemoizeHashCode_long > 21474) { throw new InvalidOperationException("Memoizer.NET supports only 21474 different Func references..."); }
            //        string functionToUnMemoizeHashCode = functionToUnMemoizeHashCode_long.ToString().PadLeft(5, '0');

            //        if (functionToUnMemoizeHashCode == memoizerConfigHashCode)
            //        {
            //            // Dispose of the MemoryCache backing object
            //            PropertyInfo propertyInfo = keyValuePair.Value.GetType().GetProperty("Result");
            //            if (propertyInfo == null)
            //                throw new ArgumentException("Could not find a property with the name 'Result'");

            //            var memoizer = propertyInfo.GetValue(keyValuePair.Value, null);

            //            MethodInfo method = memoizer.GetType().GetMethod("Dispose");//, BindingFlags.Public);
            //            if (method == null)
            //                throw new ArgumentException("Could not find a method with the name 'Dispose'");

            //            method.Invoke(memoizer, null);

            //            // Remove the memoizer from the memoizer^2 registry
            //            LAZY_MEMOIZER_MEMOIZER.Value.cache.Remove(keyValuePair.Key);

            //            ++numberOfMemoizersRemoved;
            //        }
            //    }
            //}
            return numberOfMemoizersRemoved;
        }

        readonly Func<TParam1, TResult> function;

        internal ExpirationType ExpirationType { get; set; }
        internal int ExpirationValue { get; set; }
        internal TimeUnit ExpirationTimeUnit { get; set; }

        Action<String> loggerMethod;


        internal MemoizerFactory(Func<TParam1, TResult> functionToBeMemoized)
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

        MemoizerConfiguration MemoizerConfiguration
        {
            get { return new MemoizerConfiguration(this.function, this.ExpirationType, this.ExpirationValue, this.ExpirationTimeUnit, this.loggerMethod); }
        }

        public MemoizerFactory_AwaitingExpirationUnit<TParam1, TResult> KeepItemsCachedFor(int cacheExpirationValue)
        {
            return new MemoizerFactory_AwaitingExpirationUnit<TParam1, TResult>(cacheExpirationValue, this);
        }

        public MemoizerFactory<TParam1, TResult> InstrumentWith(Action<String> loggingAction)
        {
            this.loggerMethod = loggingAction;
            return this;
        }

        /// <summary>
        /// Force creation and <i>not</i> caching/sharing (via memoizer^2 registry) of created memoizer instance.
        /// </summary>
        public IMemoizer<TParam1, TResult> CreateMemoizer()
        {
            return GetMemoizer(cachedAndSharedMemoizerInstance: false);
        }

        public IMemoizer<TParam1, TResult> GetMemoizer(bool cachedAndSharedMemoizerInstance = true)
        {
            return cachedAndSharedMemoizerInstance ?
                LAZY_MEMOIZER_MEMOIZER.Value.InvokeWith(this.MemoizerConfiguration) :
                new Memoizer<TParam1, TResult>(this.MemoizerConfiguration, shared: cachedAndSharedMemoizerInstance);
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
        static readonly Lazy<IMemoizer<MemoizerConfiguration, IMemoizer<TParam1, TParam2, TResult>>> LAZY_MEMOIZER_MEMOIZER =
            new Lazy<IMemoizer<MemoizerConfiguration, IMemoizer<TParam1, TParam2, TResult>>>(
                () => new Memoizer<MemoizerConfiguration, Memoizer<TParam1, TParam2, TResult>>(
                    memoizerConfig => new Memoizer<TParam1, TParam2, TResult>(memoizerConfig)),
                isThreadSafe: typeof(IMemoizer<MemoizerConfiguration, IMemoizer<TParam1, TParam2, TResult>>) is IThreadSafe);

        readonly Func<TParam1, TParam2, TResult> function;
        internal Func<TParam1, TParam2, TResult> Function { get { return this.function; } }

        internal ExpirationType ExpirationType { get; set; }
        internal int ExpirationValue { get; set; }
        internal TimeUnit ExpirationTimeUnit { get; set; }

        Action<String> loggerMethod;
        internal Action<String> LoggerAction { get { return this.loggerMethod; } }

        MemoizerConfiguration MemoizerConfiguration
        {
            get { return new MemoizerConfiguration(this.function, this.ExpirationType, this.ExpirationValue, this.ExpirationTimeUnit, this.loggerMethod); }
        }

        internal MemoizerFactory(Func<TParam1, TParam2, TResult> functionToBeMemoized)
        {
            this.function = functionToBeMemoized;
        }

        public MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TResult> KeepItemsCachedFor(int cacheExpirationValue)
        {
            return new MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TResult>(cacheExpirationValue, this);
        }

        public MemoizerFactory<TParam1, TParam2, TResult> InstrumentWith(Action<String> loggerMethod)
        {
            this.loggerMethod = loggerMethod;
            return this;
        }

        public IMemoizer<TParam1, TParam2, TResult> CreateMemoizer() { return GetMemoizer(false); }

        public IMemoizer<TParam1, TParam2, TResult> GetMemoizer(bool cacheAndShareMemoizerInstance = true)
        {
            return cacheAndShareMemoizerInstance ? LAZY_MEMOIZER_MEMOIZER.Value.InvokeWith(this.MemoizerConfiguration) : new Memoizer<TParam1, TParam2, TResult>(this.MemoizerConfiguration);
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
        static readonly Lazy<IMemoizer<MemoizerConfiguration, IMemoizer<TParam1, TParam2, TParam3, TResult>>> LAZY_MEMOIZER_MEMOIZER =
            new Lazy<IMemoizer<MemoizerConfiguration, IMemoizer<TParam1, TParam2, TParam3, TResult>>>(
                () => new Memoizer<MemoizerConfiguration, IMemoizer<TParam1, TParam2, TParam3, TResult>>(
                    memoizerConfig => new Memoizer<TParam1, TParam2, TParam3, TResult>(memoizerConfig)),
                isThreadSafe: typeof(IMemoizer<MemoizerConfiguration, IMemoizer<TParam1, TParam2, TParam3, TResult>>) is IThreadSafe);

        readonly Func<TParam1, TParam2, TParam3, TResult> function;
        internal Func<TParam1, TParam2, TParam3, TResult> Function { get { return this.function; } }

        internal ExpirationType ExpirationType { get; set; }
        internal int ExpirationValue { get; set; }
        internal TimeUnit ExpirationTimeUnit { get; set; }

        Action<String> loggerMethod;
        internal Action<String> LoggerAction { get { return this.loggerMethod; } }

        MemoizerConfiguration MemoizerConfiguration
        {
            get { return new MemoizerConfiguration(this.function, this.ExpirationType, this.ExpirationValue, this.ExpirationTimeUnit, this.loggerMethod); }
        }

        internal MemoizerFactory(Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized)
        {
            this.function = functionToBeMemoized;
        }

        public MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TResult> KeepItemsCachedFor(int cacheExpirationValue)
        {
            return new MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TResult>(cacheExpirationValue, this);
        }

        public MemoizerFactory<TParam1, TParam2, TParam3, TResult> InstrumentWith(Action<String> loggerMethod)
        {
            this.loggerMethod = loggerMethod;
            return this;
        }

        public IMemoizer<TParam1, TParam2, TParam3, TResult> CreateMemoizer() { return GetMemoizer(false); }

        public IMemoizer<TParam1, TParam2, TParam3, TResult> GetMemoizer(bool cacheAndShareMemoizerInstance = true)
        {
            return cacheAndShareMemoizerInstance ? LAZY_MEMOIZER_MEMOIZER.Value.InvokeWith(this.MemoizerConfiguration) : new Memoizer<TParam1, TParam2, TParam3, TResult>(this.MemoizerConfiguration);
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
        static readonly Lazy<IMemoizer<MemoizerConfiguration, IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult>>> LAZY_MEMOIZER_MEMOIZER =
            new Lazy<IMemoizer<MemoizerConfiguration, IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult>>>(
                () => new Memoizer<MemoizerConfiguration, IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult>>(
                    memoizerFactory => new Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>(memoizerFactory)),
                isThreadSafe: typeof(IMemoizer<MemoizerConfiguration, IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult>>) is IThreadSafe);

        readonly Func<TParam1, TParam2, TParam3, TParam4, TResult> function;
        internal Func<TParam1, TParam2, TParam3, TParam4, TResult> Function { get { return this.function; } }

        internal ExpirationType ExpirationType { get; set; }
        internal int ExpirationValue { get; set; }
        internal TimeUnit ExpirationTimeUnit { get; set; }

        Action<String> loggerMethod;
        internal Action<String> LoggerAction { get { return this.loggerMethod; } }

        MemoizerConfiguration MemoizerConfiguration
        {
            get { return new MemoizerConfiguration(this.function, this.ExpirationType, this.ExpirationValue, this.ExpirationTimeUnit, this.loggerMethod); }
        }

        internal MemoizerFactory(Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized)
        {
            this.function = functionToBeMemoized;
        }

        public MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TParam4, TResult> KeepItemsCachedFor(int cacheExpirationValue)
        {
            return new MemoizerFactory_AwaitingExpirationUnit<TParam1, TParam2, TParam3, TParam4, TResult>(cacheExpirationValue, this);
        }

        public MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult> InstrumentWith(Action<String> loggerMethod)
        {
            this.loggerMethod = loggerMethod;
            return this;
        }

        public IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult> CreateMemoizer() { return GetMemoizer(false); }

        public IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult> GetMemoizer(bool cacheAndShareMemoizerInstance = true)
        {
            return cacheAndShareMemoizerInstance ? LAZY_MEMOIZER_MEMOIZER.Value.InvokeWith(this.MemoizerConfiguration) : new Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>(this.MemoizerConfiguration);
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
