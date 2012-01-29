using System;

namespace Memoizer.NET
{
    #region Func extension methods
    /// <summary>
    /// Primary Memoizer.NET API entrance points
    /// </summary>
    public static partial class FuncExtensionMethods
    {
        // TODO ?
        //#region IsMemoized()
        //public static bool IsMemoized<TResult>(this Func<TResult> functionToBeMemoized) { }
        //public static bool IsMemoized<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized) { }
        //public static bool IsMemoized<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized) { }
        //public static bool IsMemoized<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized) { }
        //public static bool IsMemoized<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized) { }
        //#endregion

        #region CreateMemoizer()
        public static IMemoizer<TResult> CreateMemoizer<TResult>(this Func<TResult> functionToBeMemoized)
        {
            return new MemoizerFactory<TResult>(functionToBeMemoized).CreateMemoizer();
        }
        public static IMemoizer<TParam1, TResult> CreateMemoizer<TParam1, TResult>(this Func<TParam1, TResult> functionToBeMemoized)
        {
            return new MemoizerFactory<TParam1, TResult>(functionToBeMemoized).CreateMemoizer();
        }
        public static IMemoizer<TParam1, TParam2, TResult> CreateMemoizer<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToBeMemoized)
        {
            return new MemoizerFactory<TParam1, TParam2, TResult>(functionToBeMemoized).CreateMemoizer();
        }
        public static IMemoizer<TParam1, TParam2, TParam3, TResult> CreateMemoizer<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized)
        {
            return new MemoizerFactory<TParam1, TParam2, TParam3, TResult>(functionToBeMemoized).CreateMemoizer();
        }
        public static IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult> CreateMemoizer<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized)
        {
            return new MemoizerFactory<TParam1, TParam2, TParam3, TParam4, TResult>(functionToBeMemoized).GetMemoizer();
        }
        #endregion

        #region GetMemoizer()
        public static IMemoizer<TResult> GetMemoizer<TResult>(this Func<TResult> functionToBeMemoized)
        {
            return new MemoizerFactory<TResult>(functionToBeMemoized).GetMemoizer();
        }
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
        public static MemoizerFactory<TResult> Memoize<TResult>(this Func<TResult> functionToBeMemoized)
        {
            return new MemoizerFactory<TResult>(functionToBeMemoized);
        }
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
        public static void DynamicUnMemoize(this object functionToUnMemoize)
        {
            MemoizerRegistryHelper.RemoveRegistryMemoizersHavingFunction(functionToUnMemoize, MemoizerRegistry.LAZY_MEMOIZER_REGISTRY.Value);
        }
        public static void UnMemoize<TResult>(this Func<TResult> functionToUnMemoize)
        {
            MemoizerRegistryHelper.RemoveRegistryMemoizersHavingFunction(functionToUnMemoize, MemoizerRegistry<TResult>.LAZY_MEMOIZER_REGISTRY.Value);
        }
        public static void UnMemoize<TParam1, TResult>(this Func<TParam1, TResult> functionToUnMemoize)
        {
            MemoizerRegistryHelper.RemoveRegistryMemoizersHavingFunction(functionToUnMemoize, MemoizerRegistry<TParam1, TResult>.LAZY_MEMOIZER_REGISTRY.Value);
        }
        public static void UnMemoize<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> functionToUnMemoize)
        {
            MemoizerRegistryHelper.RemoveRegistryMemoizersHavingFunction(functionToUnMemoize, MemoizerRegistry<TParam1, TParam2, TResult>.LAZY_MEMOIZER_REGISTRY.Value);
            DynamicUnMemoize(functionToUnMemoize);
        }
        public static void UnMemoize<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> functionToUnMemoize)
        {
            MemoizerRegistryHelper.RemoveRegistryMemoizersHavingFunction(functionToUnMemoize, MemoizerRegistry<TParam1, TParam2, TParam3, TResult>.LAZY_MEMOIZER_REGISTRY.Value);
        }
        public static void UnMemoize<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToUnMemoize)
        {
            MemoizerRegistryHelper.RemoveRegistryMemoizersHavingFunction(functionToUnMemoize, MemoizerRegistry<TParam1, TParam2, TParam3, TParam4, TResult>.LAZY_MEMOIZER_REGISTRY.Value);
        }
        #endregion

        #region CacheFor(int expirationValue)
        public static MemoizerFactory_AwaitingExpirationUnit<TResult> CacheFor<TResult>(this Func<TResult> functionToBeMemoized, int expirationValue)
        {
            return new MemoizerFactory<TResult>(functionToBeMemoized).KeepItemsCachedFor(expirationValue);
        }
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
        public static dynamic DynamicCachedInvoke(this object functionToBeMemoized, dynamic[] args)
        {
            return new MemoizerFactory(functionToBeMemoized).GetMemoizer().InvokeWith(args);
        }
        public static TResult CachedInvoke<TResult>(this Func<TResult> functionToBeMemoized)
        {
            return new MemoizerFactory<TResult>(functionToBeMemoized).GetMemoizer().Invoke();
        }
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
        public static void RemoveFromCache<TResult>(this Func<TResult> memoizedFunction)
        {
            memoizedFunction.UnMemoize();
        }
        public static void RemoveFromCache<TParam1, TResult>(this Func<TParam1, TResult> memoizedFunction, TParam1 arg1ForRemoval)
        {
            MemoizerRegistry<TParam1, TResult>.RemoveCachedElementsFromRegistryMemoizersHavingFunction(memoizedFunction, arg1ForRemoval);
        }
        public static void RemoveFromCache<TParam1, TParam2, TResult>(this Func<TParam1, TParam2, TResult> memoizedFunction, TParam1 arg1ForRemoval, TParam2 arg2ForRemoval)
        {
            MemoizerRegistry<TParam1, TParam2, TResult>.RemoveCachedElementsFromRegistryMemoizersHavingFunction(memoizedFunction, arg1ForRemoval, arg2ForRemoval);
        }
        public static void RemoveFromCache<TParam1, TParam2, TParam3, TResult>(this Func<TParam1, TParam2, TParam3, TResult> memoizedFunction, TParam1 arg1ForRemoval, TParam2 arg2ForRemoval, TParam3 arg3ForRemoval)
        {
            MemoizerRegistry<TParam1, TParam2, TParam3, TResult>.RemoveCachedElementsFromRegistryMemoizersHavingFunction(memoizedFunction, arg1ForRemoval, arg2ForRemoval, arg3ForRemoval);
        }
        public static void RemoveFromCache<TParam1, TParam2, TParam3, TParam4, TResult>(this Func<TParam1, TParam2, TParam3, TParam4, TResult> memoizedFunction, TParam1 arg1ForRemoval, TParam2 arg2ForRemoval, TParam3 arg3ForRemoval, TParam4 arg4ForRemoval)
        {
            MemoizerRegistry<TParam1, TParam2, TParam3, TParam4, TResult>.RemoveCachedElementsFromRegistryMemoizersHavingFunction(memoizedFunction, arg1ForRemoval, arg2ForRemoval, arg3ForRemoval, arg4ForRemoval);
        }
        #endregion
    }
    #endregion
}
