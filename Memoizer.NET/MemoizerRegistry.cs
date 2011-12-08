using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Memoizer.NET
{
    #region MemoizerRegistryHelper
    static class MemoizerRegistryHelper
    {
        /// <summary>
        /// Coupled with the <code>MemoizerConfiguration.GetHashCode()</code> method.
        /// </summary>
        /// <param name="function">The memoized function to look for</param>
        /// <param name="memoizerRegistry">The memoizer^2 registry instance to look into</param>
        /// <returns>An enumeration of keys pointing to memoizer instances in the memoizer^2 registry, having the given function</returns>
        internal static IEnumerable<string> FindMemoizerKeysInRegistryHavingFunction<T>(object function, Memoizer<MemoizerConfiguration, T> memoizerRegistry)
        {
            IList<string> memoizerKeyList = new List<string>();
            foreach (KeyValuePair<string, object> keyValuePair in memoizerRegistry.cache)
            {
                string funcIdPartOfMemoizerConfigurationHashCode = keyValuePair.Key.PadLeft(10, '0').Substring(0, 5);

                bool firstTime = false;
                long funcId_long = MemoizerHelper.GetObjectId(function, ref firstTime);
                if (funcId_long > 21474) { throw new InvalidOperationException("Memoizer.NET only supports 21474 different Func references at the moment..."); }
                string funcIdPartOfFunctionToLookFor = funcId_long.ToString().PadLeft(5, '0');

                if (funcIdPartOfFunctionToLookFor == funcIdPartOfMemoizerConfigurationHashCode)
                    memoizerKeyList.Add(keyValuePair.Key);
            }
            return memoizerKeyList;
        }


        /// <summary>
        /// Remove all memoizer instances having the given Func, from the memoizer registry
        /// </summary>
        /// <param name="functionToUnMemoize">The memoized function to remove from the memoizer registry</param>
        /// <param name="memoizerRegistry">The memoizer registry instance</param>
        /// <returns>Number of memoizer instances removed from memoizer registry</returns>
        //internal static int RemoveRegistryMemoizersHavingFunction(Func<TParam1, TResult> functionToUnMemoize)
        internal static int RemoveRegistryMemoizersHavingFunction<T>(object functionToUnMemoize, Memoizer<MemoizerConfiguration, T> memoizerRegistry) where T : IDisposable
        {
            int numberOfMemoizersRemoved = 0;
            IEnumerable<string> memoizerKeyList = FindMemoizerKeysInRegistryHavingFunction(functionToUnMemoize, memoizerRegistry);
            foreach (var memoizerKey in memoizerKeyList)
            {
                //Task<Memoizer<TParam1, TResult>> cacheValueTask = (Task<Memoizer<TParam1, TResult>>)LAZY_MEMOIZER_MEMOIZER.Value.cache.Get(memoizerKey);
                Task<T> cacheValueTask = (Task<T>)memoizerRegistry.cache.Get(memoizerKey);
                IDisposable memoizer = cacheValueTask.Result;
                if (memoizerRegistry.cache.Contains(memoizerKey))
                {
                    memoizer.Dispose();
                    //long i = LAZY_MEMOIZER_MEMOIZER.Value.cache.GetCount();
                    memoizerRegistry.cache.Remove(memoizerKey);
                    ++numberOfMemoizersRemoved;
                    //Console.WriteLine(memoizerKey + "[" + numberOfMemoizersRemoved + "] memoizer removed");
                }
            }
            return numberOfMemoizersRemoved;
        }
    }
    #endregion

    #region MemoizerRegistry<TResult>
    static class MemoizerRegistry<TResult>
    {
        internal static readonly Lazy<Memoizer<MemoizerConfiguration, Memoizer<TResult>>> LAZY_MEMOIZER_REGISTRY =
           new Lazy<Memoizer<MemoizerConfiguration, Memoizer<TResult>>>(
               () => new Memoizer<MemoizerConfiguration, Memoizer<TResult>>(
                   memoizerConfig => new Memoizer<TResult>(memoizerConfig)),
               isThreadSafe: typeof(IMemoizer<MemoizerConfiguration, IMemoizer<TResult>>) is IThreadSafe);
    }
    #endregion

    #region MemoizerRegistry<TParam1, TResult>
    static class MemoizerRegistry<TParam1, TResult>
    {
        #region CLR-wide shared memoizer of memoizers (a.k.a. memoizer registry)
        // Static delegate for creating a memoizer with TParam1 as key type, and TResult as item type, from a MemoizerRegistry instance
        static readonly Func<MemoizerConfiguration, Memoizer<TParam1, TResult>> CREATE_MEMOIZER_FROM_CONFIG =
            new Func<MemoizerConfiguration, Memoizer<TParam1, TResult>>(
                delegate(MemoizerConfiguration memoizerConfig)
                {
                    //Console.WriteLine("Creating Memoizer<TParam1, TResult> from MemoizerRegistry<TParam1, TResult> [hash=" + MemoizerHelper.CreateMemoizerRegistryHash(memoizerFactory) + "]...");
                    return new Memoizer<TParam1, TResult>(memoizerConfig);
                });

        // Static delegate for creating a memoizer with MemoizerRegistry as key type, and Memoizer as item type, from function CREATE_MEMOIZER_FROM_FACTORY above
        static readonly Func<Memoizer<MemoizerConfiguration, Memoizer<TParam1, TResult>>> CREATE_MEMOIZER_REGISTRY =
            new Func<Memoizer<MemoizerConfiguration, Memoizer<TParam1, TResult>>>(
                delegate()
                {
                    //Console.WriteLine("Creating Memoizer for Memoizer items with key MemoizerRegistry<TParam1, TResult>...");
                    return new Memoizer<MemoizerConfiguration, Memoizer<TParam1, TResult>>(CREATE_MEMOIZER_FROM_CONFIG);
                });

        // Static lazy-loaded memoizer of memoizers, from function above
        internal static readonly Lazy<Memoizer<MemoizerConfiguration, Memoizer<TParam1, TResult>>> LAZY_MEMOIZER_REGISTRY =
           new Lazy<Memoizer<MemoizerConfiguration, Memoizer<TParam1, TResult>>>(
               CREATE_MEMOIZER_REGISTRY,
               isThreadSafe: typeof(IMemoizer<MemoizerConfiguration, IMemoizer<TParam1, TResult>>) is IThreadSafe);
        #endregion

        internal static void RemoveCachedElementsFromRegistryMemoizersHavingFunction(Func<TParam1, TResult> memoizedFunction, TParam1 arg1)
        {
            IEnumerable<string> memoizerKeyList = MemoizerRegistryHelper.FindMemoizerKeysInRegistryHavingFunction(memoizedFunction, LAZY_MEMOIZER_REGISTRY.Value);
            foreach (var memoizerKey in memoizerKeyList)
            {
                Task<Memoizer<TParam1, TResult>> cacheValueTask = (Task<Memoizer<TParam1, TResult>>)LAZY_MEMOIZER_REGISTRY.Value.cache.Get(memoizerKey);
                IMemoizer<TParam1, TResult> memoizer = cacheValueTask.Result;
                memoizer.Remove(arg1);
            }
        }
    }
    #endregion

    #region MemoizerRegistry<TParam1, TParam2, TResult>
    static class MemoizerRegistry<TParam1, TParam2, TResult>
    {
        internal static readonly Lazy<Memoizer<MemoizerConfiguration, Memoizer<TParam1, TParam2, TResult>>> LAZY_MEMOIZER_REGISTRY =
           new Lazy<Memoizer<MemoizerConfiguration, Memoizer<TParam1, TParam2, TResult>>>(
               () => new Memoizer<MemoizerConfiguration, Memoizer<TParam1, TParam2, TResult>>(
                   memoizerConfig => new Memoizer<TParam1, TParam2, TResult>(memoizerConfig)),
               isThreadSafe: typeof(IMemoizer<MemoizerConfiguration, IMemoizer<TParam1, TParam2, TResult>>) is IThreadSafe);

        internal static void RemoveCachedElementsFromRegistryMemoizersHavingFunction(Func<TParam1, TParam2, TResult> memoizedFunction, TParam1 arg1, TParam2 arg2)
        {
            IEnumerable<string> memoizerKeyList = MemoizerRegistryHelper.FindMemoizerKeysInRegistryHavingFunction(memoizedFunction, LAZY_MEMOIZER_REGISTRY.Value);
            foreach (var memoizerKey in memoizerKeyList)
            {
                Task<Memoizer<TParam1, TParam2, TResult>> cacheValueTask = (Task<Memoizer<TParam1, TParam2, TResult>>)LAZY_MEMOIZER_REGISTRY.Value.cache.Get(memoizerKey);
                IMemoizer<TParam1, TParam2, TResult> memoizer = cacheValueTask.Result;
                memoizer.Remove(arg1, arg2);
            }
        }
    }
    #endregion

    #region MemoizerRegistry<TParam1, TParam2, TParam3, TResult>
    static class MemoizerRegistry<TParam1, TParam2, TParam3, TResult>
    {
        internal static readonly Lazy<Memoizer<MemoizerConfiguration, Memoizer<TParam1, TParam2, TParam3, TResult>>> LAZY_MEMOIZER_REGISTRY =
           new Lazy<Memoizer<MemoizerConfiguration, Memoizer<TParam1, TParam2, TParam3, TResult>>>(
               () => new Memoizer<MemoizerConfiguration, Memoizer<TParam1, TParam2, TParam3, TResult>>(
                   memoizerConfig => new Memoizer<TParam1, TParam2, TParam3, TResult>(memoizerConfig)),
               isThreadSafe: typeof(IMemoizer<MemoizerConfiguration, IMemoizer<TParam1, TParam2, TParam3, TResult>>) is IThreadSafe);

        internal static void RemoveCachedElementsFromRegistryMemoizersHavingFunction(Func<TParam1, TParam2, TParam3, TResult> memoizedFunction, TParam1 arg1, TParam2 arg2, TParam3 arg3)
        {
            IEnumerable<string> memoizerKeyList = MemoizerRegistryHelper.FindMemoizerKeysInRegistryHavingFunction(memoizedFunction, LAZY_MEMOIZER_REGISTRY.Value);
            foreach (var memoizerKey in memoizerKeyList)
            {
                Task<Memoizer<TParam1, TParam2, TParam3, TResult>> cacheValueTask = (Task<Memoizer<TParam1, TParam2, TParam3, TResult>>)LAZY_MEMOIZER_REGISTRY.Value.cache.Get(memoizerKey);
                IMemoizer<TParam1, TParam2, TParam3, TResult> memoizer = cacheValueTask.Result;
                memoizer.Remove(arg1, arg2, arg3);
            }
        }
    }
    #endregion

    #region MemoizerRegistry<TParam1, TParam2,TParam3, TParam3,TParam4, TResult>
    static class MemoizerRegistry<TParam1, TParam2, TParam3, TParam4, TResult>
    {
        internal static readonly Lazy<Memoizer<MemoizerConfiguration, Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>>> LAZY_MEMOIZER_REGISTRY =
            new Lazy<Memoizer<MemoizerConfiguration, Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>>>(
                () => new Memoizer<MemoizerConfiguration, Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>>(
                    memoizerFactory => new Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>(memoizerFactory)),
                isThreadSafe: typeof(IMemoizer<MemoizerConfiguration, IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult>>) is IThreadSafe);

        internal static void RemoveCachedElementsFromRegistryMemoizersHavingFunction(Func<TParam1, TParam2, TParam3, TParam4, TResult> memoizedFunction, TParam1 arg1, TParam2 arg2, TParam3 arg3, TParam4 arg4)
        {
            IEnumerable<string> memoizerKeyList = MemoizerRegistryHelper.FindMemoizerKeysInRegistryHavingFunction(memoizedFunction, LAZY_MEMOIZER_REGISTRY.Value);
            foreach (var memoizerKey in memoizerKeyList)
            {
                Task<Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>> cacheValueTask = (Task<Memoizer<TParam1, TParam2, TParam3, TParam4, TResult>>)LAZY_MEMOIZER_REGISTRY.Value.cache.Get(memoizerKey);
                IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult> memoizer = cacheValueTask.Result;
                memoizer.Remove(arg1, arg2, arg3, arg4);
            }
        }
    }
    #endregion
}
