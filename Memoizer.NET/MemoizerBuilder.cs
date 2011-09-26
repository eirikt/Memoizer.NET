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

        //public static Func<Memoizer<TResult, TParam>, long> newMemoizer =
        //    new Func<Memoizer<TResult, TParam>, long> {};

        //public static LazyMemoizer<Memoizer<TResult, TParam>, long> memoizerMemoizer = new LazyMemoizer<Memoizer<TResult, TParam>, long>();

        Action<String> LoggingMethod;
        public MemoizerBuilder(Func<TParam, TResult> functionToBeMemoized) { Function = functionToBeMemoized; }
        public Func<TParam, TResult> Function { get; private set; }
        public Memoizer<TResult, TParam> Get()
        {
            Memoizer<TResult, TParam> memoizer = new Memoizer<TResult, TParam>(Function);
            
            //Memoizer<TResult, TParam> memoizer = memoizerMemoizer.InvokeWith(Memoizer<TResult, TParam>(Function));
            
            if (this.LoggingMethod != null) { memoizer.InstrumentWith(this.LoggingMethod); }
            return memoizer;

        }
        public void InstrumentWith(Action<String> loggingAction) { this.LoggingMethod = loggingAction; }
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
