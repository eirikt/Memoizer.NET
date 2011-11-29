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
using System.Threading;
using System.Threading.Tasks;

namespace Memoizer.NET
{
    #region IThreadSafe
    /// <remarks>
    /// Marker interface for thread-safe classes.
    /// </remarks>
    public interface IThreadSafe { }
    #endregion

    #region IClearable
    /// <remarks>
    /// Interface for classes that can be cleared.
    /// </remarks>
    public interface IClearable
    {
        /// <summary>
        /// Clears the cache, removing all items.
        /// </summary>
        void Clear();
    }
    #endregion

    #region IMemoizer
    public interface IManageableMemoizer
    {
        int NumberOfTimesInvoked { get; }
        int NumberOfTimesNoCacheInvoked { get; }
        int NumberOfTimesCleared { get; }
        int NumberOfElementsCleared { get; }
    }
    public interface IMemoizer<out TResult> : IThreadSafe, IDisposable, IClearable, IManageableMemoizer
    {
        TResult Invoke();
    }
    public interface IMemoizer<in TParam1, out TResult> : IThreadSafe, IDisposable, IClearable, IManageableMemoizer
    {
        TResult InvokeWith(TParam1 param);
        void Remove(TParam1 param);
    }
    public interface IMemoizer<in TParam1, in TParam2, out TResult> : IThreadSafe, IDisposable, IClearable, IManageableMemoizer
    {
        TResult InvokeWith(TParam1 param1, TParam2 param2);
        void Remove(TParam1 param1, TParam2 param2);
    }
    public interface IMemoizer<in TParam1, in TParam2, in TParam3, out TResult> : IThreadSafe, IDisposable, IClearable, IManageableMemoizer
    {
        TResult InvokeWith(TParam1 param1, TParam2 param2, TParam3 param3);
        void Remove(TParam1 param1, TParam2 param2, TParam3 param3);
    }
    public interface IMemoizer<in TParam1, in TParam2, in TParam3, in TParam4, out TResult> : IThreadSafe, IDisposable, IClearable, IManageableMemoizer
    {
        TResult InvokeWith(TParam1 param1, TParam2 param2, TParam3 param3in, TParam4 param4);
        void Remove(TParam1 param1, TParam2 param2, TParam3 param3in, TParam4 param4);
    }
    #endregion

    #region Memoizer (using a MemoryCache instance and Goetz's algorithm)
    /// <remarks>
    /// This class is an implementation of a method-level/fine-grained cache (a.k.a. <i>memoizer</i>). 
    /// It is based on an implementation from the book "Java Concurrency in Practice" by Brian Goetz et. al. - ported to C# 4.0
    /// <p/>
    /// A <code>System.Runtime.Caching.MemoryCache</code> instance is used as cache, enabling configuration via the <code>System.Runtime.Caching.CacheItemPolicy</code>. 
    /// Default cache configuration is: items to be held as long as the CLR is alive or the memoizer is disposed/cleared.
    /// <p/>
    /// This class is thread-safe.
    /// </remarks>
    /// <see>http://jcip.net/</see>
    /// <see><code>Memoizer.Net.LazyMemoizer</code></see>
    /// <author>Eirik Torske</author>
    abstract class AbstractMemoizer<TResult> : IManageableMemoizer
    {
        protected internal MemoryCache cache;
        protected CacheItemPolicy cacheItemPolicy;
        protected Action<string> loggingMethod;

        /// <summary>
        /// Flag indicating if memoizer is in memoizer^2 registry
        /// </summary>
        internal bool IsShared { get; set; }

        int numberOfTimesInvoked;
        int numberOfTimesNoCacheInvoked;
        int numberOfTimesCleared;
        int numberOfElementsCleared;


        public int NumberOfTimesInvoked { get { return this.numberOfTimesInvoked; } }
        public int NumberOfTimesNoCacheInvoked { get { return this.numberOfTimesNoCacheInvoked; } }
        public int NumberOfTimesCleared { get { return this.numberOfTimesCleared; } }
        public int NumberOfElementsCleared { get { return this.numberOfElementsCleared; } }


        public void Dispose() { this.cache.Dispose(); }


        /// <summary>
        /// Lock object for removal of element and incrementing total element removal index.
        /// </summary>
        static readonly object @lock = new Object();

        public void Clear()
        {
            lock (@lock)
            {
                int i = 0;
                foreach (var element in this.cache.AsEnumerable())
                {
                    this.cache.Remove(element.Key);
                    Interlocked.Increment(ref this.numberOfElementsCleared);
                    ConditionalLogging("Removed cached element #" + ++i + ": " + element.Key + "=" + ((Task<string>)element.Value).Status + " [" + this.NumberOfElementsCleared + " elements removed in total]");
                }
                Interlocked.Increment(ref this.numberOfTimesCleared);
                ConditionalLogging("All " + i + " elements in memoizer removed [memoizer cleared " + NumberOfTimesCleared + " times]");
            }
        }


        protected void ConditionalLogging(string logMessage)
        {
            if (this.loggingMethod != null) { this.loggingMethod(this.GetType().Namespace + "." + this.GetType().Name + " [" + this.GetHashCode() + "] : " + logMessage); }
        }


        /// <summary>
        /// Gets the delegate of the function to be memoized, closed under given arguments.
        /// </summary>
        protected abstract Func<TResult> GetFunctionClosure(params object[] args);


        /// <summary>
        /// Invokes the function delegate - consulting the cache on the way.
        /// </summary>
        protected TResult Invoke(params object[] args)
        {
            long startTime = DateTime.Now.Ticks;
            string key = MemoizerHelper.CreateParameterHash(args);
            //string key = MemoizerHelper.CreateParameterHash(IsShared, args);
            CacheItem taskCacheItem = this.cache.GetCacheItem(key);
            if (taskCacheItem == null)
            {
                //Console.WriteLine("OS thread ID=" + AppDomain.GetCurrentThreadId() + ", " + "Managed thread ID=" + Thread.CurrentThread.GetHashCode() + "/" + Thread.CurrentThread.ManagedThreadId + ": cacheItem == null");

                //Func<TResult> func = new Func<TResult>(delegate() { return this.functionToBeMemoized((TParam)args[0]); });
                //Task<TResult> task = new Task<TResult>(func);
                //CacheItem newCacheItem = new CacheItem(key, task);
                // Or just:
                //CacheItem newCacheItem = new CacheItem(key, new Task<TResult>(() => this.functionToBeMemoized((TParam) args[0])));
                // And finally more subclass-friendly:
                CacheItem newTaskCacheItem = new CacheItem(key, new Task<TResult>(GetFunctionClosure(args)));

                // The 'AddOrGetExisting' method is atomic: If a cached value for the key exists, the existing cached value is returned; otherwise null is returned as value property
                taskCacheItem = this.cache.AddOrGetExisting(newTaskCacheItem, this.cacheItemPolicy);
                if (taskCacheItem.Value == null)
                {
                    //Console.WriteLine("OS thread ID=" + AppDomain.GetCurrentThreadId() + ", " + "Managed thread ID=" + Thread.CurrentThread.GetHashCode() + "/" + Thread.CurrentThread.ManagedThreadId + ": cacheItem.Value == null");
                    taskCacheItem = newTaskCacheItem;
                    // The 'Start' method is idempotent
                    ((Task<TResult>)taskCacheItem.Value).Start();
                    Interlocked.Increment(ref this.numberOfTimesNoCacheInvoked);
                    ConditionalLogging("(Possibly expensive) async caching function execution #" + this.numberOfTimesNoCacheInvoked);
                }
                //    else
                //    {
                //        Console.WriteLine("OS thread ID=" + AppDomain.GetCurrentThreadId() + ", " + "Managed thread ID=" + Thread.CurrentThread.GetHashCode() + "/" + Thread.CurrentThread.ManagedThreadId + ": cacheItem.Value == " + cacheItem.Value);
                //    }
                //}
                //else
                //{
                //    Console.WriteLine("OS thread ID=" + AppDomain.GetCurrentThreadId() + ", " + "Managed thread ID=" + Thread.CurrentThread.GetHashCode() + "/" + Thread.CurrentThread.ManagedThreadId + ": cacheItem == " + cacheItem);
            }

            // The 'Result' property blocks until a value is available
            //Console.WriteLine("OS thread ID=" + AppDomain.GetCurrentThreadId() + ", " + "Managed thread ID=" + Thread.CurrentThread.GetHashCode() + "/" + Thread.CurrentThread.ManagedThreadId + ": Status: " + ((Task<TResult>)cacheItem.Value).Status);
            var cachedValue = ((Task<TResult>)taskCacheItem.Value).Result;
            //Console.WriteLine("OS thread ID=" + AppDomain.GetCurrentThreadId() + ", " + "Managed thread ID=" + Thread.CurrentThread.GetHashCode() + "/" + Thread.CurrentThread.ManagedThreadId + ": Invoke(" + args + ") took " + (DateTime.Now.Ticks - startTime) + " ticks");

            Interlocked.Increment(ref this.numberOfTimesInvoked);
            ConditionalLogging("Invocation #" + this.numberOfTimesInvoked + " took " + (DateTime.Now.Ticks - startTime) + " ticks | " + (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond + " ms");

            return cachedValue;
        }
    }


    internal class Memoizer<TResult> : IMemoizer<TResult>
    {
        readonly Func<TResult> functionToBeMemoized;
        readonly CacheItemPolicy cacheItemPolicy;
        readonly string key;

        
        public Memoizer(MemoizerConfiguration memoizerConfig) : this(memoizerConfig, shared: true) { }

        public Memoizer(MemoizerConfiguration memoizerConfig, bool shared)
        {
            this.functionToBeMemoized = (Func<TResult>)memoizerConfig.Function;
            this.cacheItemPolicy = CacheItemPolicyFactory.CreateCacheItemPolicy(memoizerConfig.ExpirationType, memoizerConfig.ExpirationValue, memoizerConfig.ExpirationTimeUnit);
            this.key = memoizerConfig.GetHashCode().ToString();
        }
        

        static readonly object @NOARG_MEMOIZER_LOCK = new Object();

        public TResult Invoke()
        {
            // Works, but ugh...
            lock (@NOARG_MEMOIZER_LOCK)
            {
                TResult val = (TResult)MemoryCache.Default.Get(this.key);
                if (val == null)
                    MemoryCache.Default.Set(new CacheItem(this.key, this.functionToBeMemoized.Invoke()), this.cacheItemPolicy);
            }
            return (TResult)MemoryCache.Default.GetCacheItem(this.key).Value;

            // TODO: does not work... why?
            //CacheItem taskCacheItem = MemoryCache.Default.GetCacheItem(this.key);
            //if (taskCacheItem == null)
            //{
            //    CacheItem newCacheItem = new CacheItem(this.key, new Task<TResult>(() => this.functionToBeMemoized()));

            //    // The 'AddOrGetExisting' method is atomic: If a cached value for the key exists, the existing cached value is returned; otherwise null is returned as value property
            //    taskCacheItem = MemoryCache.Default.AddOrGetExisting(newCacheItem, this.cacheItemPolicy);
            //    if (taskCacheItem.Value == null)
            //    {
            //        taskCacheItem.Value = newCacheItem.Value;

            //        // The 'Start' method is idempotent
            //        ((Task<TResult>)taskCacheItem.Value).Start();
            //    }
            //}

            //// The 'Result' property blocks until a value is available
            //return ((Task<TResult>)taskCacheItem.Value).Result;
        }

        public void Dispose() { MemoryCache.Default.Remove(key); }

        public void Clear() { Dispose(); }

        public int NumberOfTimesInvoked { get { throw new NotImplementedException(); } }

        public int NumberOfTimesNoCacheInvoked { get { throw new NotImplementedException(); } }

        public int NumberOfTimesCleared { get { throw new NotImplementedException(); } }

        public int NumberOfElementsCleared { get { throw new NotImplementedException(); } }
    }




    internal class Memoizer<TParam1, TResult> : AbstractMemoizer<TResult>, IMemoizer<TParam1, TResult>
    {
        readonly Func<TParam1, TResult> functionToBeMemoized;

        internal Memoizer(MemoizerConfiguration memoizerConfig, bool shared = true)
        {
            this.cache = new MemoryCache(memoizerConfig.GetHashCode().ToString());
            this.functionToBeMemoized = (Func<TParam1, TResult>)memoizerConfig.Function;
            this.cacheItemPolicy = CacheItemPolicyFactory.CreateCacheItemPolicy(memoizerConfig.ExpirationType, memoizerConfig.ExpirationValue, memoizerConfig.ExpirationTimeUnit);
            this.loggingMethod = memoizerConfig.LoggerAction;
            this.IsShared = shared;
        }

        // Only for verbose lazy-loaded memoizer^2
        internal Memoizer(Func<TParam1, TResult> functionToBeMemoized)
        {
            this.functionToBeMemoized = functionToBeMemoized;
            this.cache = new MemoryCache(MemoizerHelper.CreateFunctionHash(this.functionToBeMemoized).ToString());
        }

        protected override Func<TResult> GetFunctionClosure(params object[] args)
        {
            //return new Func<TResult>(delegate() { return this.functionToBeMemoized((TParam1)args[0]); });
            // Or just:
            return () => this.functionToBeMemoized((TParam1)args[0]);
        }

        public TResult InvokeWith(TParam1 param)
        {
            return Invoke(param);
        }

        public void Remove(TParam1 param)
        {
            this.cache.Remove(MemoizerHelper.CreateParameterHash(param));
        }
    }


    internal class Memoizer<TParam1, TParam2, TResult> : AbstractMemoizer<TResult>, IMemoizer<TParam1, TParam2, TResult>
    {
        readonly Func<TParam1, TParam2, TResult> functionToBeMemoized;

        internal Memoizer(MemoizerConfiguration memoizerConfig)
        {
            this.cache = new MemoryCache(memoizerConfig.GetHashCode().ToString());
            this.functionToBeMemoized = (Func<TParam1, TParam2, TResult>)memoizerConfig.Function;
            this.cacheItemPolicy = CacheItemPolicyFactory.CreateCacheItemPolicy(memoizerConfig.ExpirationType, memoizerConfig.ExpirationValue, memoizerConfig.ExpirationTimeUnit);
            this.loggingMethod = memoizerConfig.LoggerAction;
        }

        protected override Func<TResult> GetFunctionClosure(params object[] args) { return () => this.functionToBeMemoized((TParam1)args[0], (TParam2)args[1]); }

        public TResult InvokeWith(TParam1 param1, TParam2 param2) { return Invoke(param1, param2); }

        public void Remove(TParam1 param1, TParam2 param2) { this.cache.Remove(MemoizerHelper.CreateParameterHash(param1, param2)); }
    }


    internal class Memoizer<TParam1, TParam2, TParam3, TResult> : AbstractMemoizer<TResult>, IMemoizer<TParam1, TParam2, TParam3, TResult>
    {
        readonly Func<TParam1, TParam2, TParam3, TResult> functionToBeMemoized;

        internal Memoizer(MemoizerConfiguration memoizerConfig)
        {
            this.cache = new MemoryCache(memoizerConfig.GetHashCode().ToString());
            this.functionToBeMemoized = (Func<TParam1, TParam2, TParam3, TResult>)memoizerConfig.Function;
            this.cacheItemPolicy = CacheItemPolicyFactory.CreateCacheItemPolicy(memoizerConfig.ExpirationType, memoizerConfig.ExpirationValue, memoizerConfig.ExpirationTimeUnit);
            this.loggingMethod = memoizerConfig.LoggerAction;
        }

        protected override Func<TResult> GetFunctionClosure(params object[] args) { return () => this.functionToBeMemoized((TParam1)args[0], (TParam2)args[1], (TParam3)args[2]); }

        public TResult InvokeWith(TParam1 param1, TParam2 param2, TParam3 param3) { return Invoke(param1, param2, param3); }

        public void Remove(TParam1 param1, TParam2 param2, TParam3 param3) { this.cache.Remove(MemoizerHelper.CreateParameterHash(param1, param2, param3)); }
    }


    internal class Memoizer<TParam1, TParam2, TParam3, TParam4, TResult> : AbstractMemoizer<TResult>, IMemoizer<TParam1, TParam2, TParam3, TParam4, TResult>
    {
        readonly Func<TParam1, TParam2, TParam3, TParam4, TResult> functionToBeMemoized;

        internal Memoizer(MemoizerConfiguration memoizerConfig)
        {
            this.cache = new MemoryCache(memoizerConfig.GetHashCode().ToString());
            this.functionToBeMemoized = (Func<TParam1, TParam2, TParam3, TParam4, TResult>)memoizerConfig.Function;
            this.cacheItemPolicy = CacheItemPolicyFactory.CreateCacheItemPolicy(memoizerConfig.ExpirationType, memoizerConfig.ExpirationValue, memoizerConfig.ExpirationTimeUnit);
            this.loggingMethod = memoizerConfig.LoggerAction;
        }

        protected override Func<TResult> GetFunctionClosure(params object[] args) { return () => this.functionToBeMemoized((TParam1)args[0], (TParam2)args[1], (TParam3)args[2], (TParam4)args[3]); }

        public TResult InvokeWith(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4) { return Invoke(param1, param2, param3, param4); }

        public void Remove(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4) { this.cache.Remove(MemoizerHelper.CreateParameterHash(param1, param2, param3, param4)); }
    }
    #endregion
}
