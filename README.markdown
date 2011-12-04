## Memoizer.NET
This project is an implementation of a function-level/fine-grained cache (a.k.a. _memoizer_). It is based on an implementation from the book ["Java Concurrency in Practice"](http://jcip.net "http://jcip.net") by Brian Goetz et. al. - ported to C# 4.0. The noble thing about this implementation is that the _values_ are not cached, but rather _asynchronous tasks_ for retrieving those values. These tasks are guaranteed not to be executed more than once in case of concurrent first-time invocations.

A [`System.Runtime.Caching.MemoryCache`](http://msdn.microsoft.com/en-us/library/system.runtime.caching.memorycache.aspx "http://msdn.microsoft.com/en-us/library/system.runtime.caching.memorycache.aspx") instance is used as cache, enabling configuration via the [`System.Runtime.Caching.CacheItemPolicy`](http://msdn.microsoft.com/en-us/library/system.runtime.caching.cacheitempolicy.aspx "http://msdn.microsoft.com/en-us/library/system.runtime.caching.cacheitempolicy.aspx"). Default cache configuration is: items to be held as long as the CLR is alive, or until the memoizer is disposed/cleared. 

### API
Memoizer.NET adds a set of extension methods to your `Func` references.
	.CachedInvoke([args]*)
	.Memoize()
	.CacheFor([expiration value]1)
	.RemoveFromCache([args]*)
	.UnMemoize()

The first one, `CachedInvoke`, memoizes the given argument combination, using the default cache configuration. (It is named mimicking the regular `Func` methods `Invoke()` and `DynamicInvoke()`.)

The second method, `Memoize`, is the one that gets you into "memoizer config mode".

The third method, `CacheFor`, is a shortcut for `Memoize` and gets you straight into cache expiration configuration.

The two last extension methods deals with explicit/forced expiration, see below.

### Usage
#### Example 1 - default caching policy:
	Func<long, string> myExpensiveFunction = ...
	string val = myExpensiveFunction.CachedInvoke(someId);

#### Example 2 - implicit expiration via policy: keep items cached for 30 minutes:
	string val = myExpensiveFunction.Memoize().KeepItemsCachedFor(30).Minutes.GetMemoizer().InvokeWith(someId);
Or
	string val = myExpensiveFunction.CacheFor(30).Minutes.GetMemoizer().InvokeWith(someId);

#### Example 3 - explicit expiration:
	myExpensiveFunction.RemoveFromCache(someId);

#### Example 4 - clearing all cached values using this function:
	myExpensiveFunction.UnMemoize();

The "inlined" style, where the memoizer is configured and created/retrieved multiple times at runtime, works - because the memoized method handles are themselves memoized (behind the curtain), using a _memoizer registry_. More on that below.

#### Example 5 - recursive functions:
Memoizer.NET does not support memoization of recursive functions out of the box, as it does not do any kind of IL manipulation.

E.g the ubiquitous Fibonacci sequence example will not work just by memoizing the root function. Instead, the recursion points have to be memoized, like this:
	Func<int, long> fibonacci =
	    (arg =>
		    {
                if (arg <= 1) return arg;
                return fibonacci.CachedInvoke(arg - 1) + fibonacci.CachedInvoke(arg - 2);
            });
Now, the `fibonacci` function can be invoked as a regular C# function, but orders of magnitude faster.

### Working with an `IMemoizer`

Obtaining the `IMemoizer` object;
	IMemoizer memoizedFunc = myExpensiveFunction.GetMemoizer();
Or with a expiration policy:
	IMemoizer memoizedFunc = myExpensiveFunction.CacheFor(30).Minutes.GetMemoizer();

The _memoizer registry_ is shared memoization of these `IMemoizer` objects using the Memoizer.NET itself. A combined hash consisting of the `Func` reference and the expiration policy, is used as key. This means that the same `Func` reference with different expiration policies are treated as two different `IMemoizer` instances by the memoizer registry. The `GetMemoizer` method consults the memoizer registry when creating the `IMemoizer` instance.

By working directly against an `IMemoizer` instance, you get a performance benefit. This is because only one cache invocation is needed instead of two, as being the case when working directly with `Func` references. (One for retrieving the memoizer from the registry, and a second one for looking up the value in the memoizer.)

You can also bypass the memoizer registry entirely by using `CreateMemoizer()` instead of `GetMemoizer()`. Now the `IMemoizer` instance is created and handed directly to you without being put into the memoizer registry. 

`IMemoizer<TParam, TResult>` instances have methods like:
    TResult InvokeWith(TParam param)
	void Remove(TParam param)
	void Clear()

...in addition to some methods for instrumentation.


## Memoizer.NET.TwoPhaseExecutor
A class for synchronized execution of an arbitrary number of worker/task threads. All participating worker/task threads must derive from the `Memoizer.Net.AbstractTwoPhaseExecutorThread` class.

### Usage
See the `Memoizer.NET.Test.MemoizerTests` class for usage examples. In v0.7 a mini DSL/builder for easy `Memoizer.Net.TwoPhaseExecutor` usage will be included. Right now the API is rather cumbersome/sucks...


## Building the project *
	%DOTNET_FRAMEWORK_4_HOME%\MSBuild %MEMOIZER_NET_HOME%\Memoizer.NET.csproj /p:Configuration=Release

...

*) Prerequisites are:

#### 1) .NET Framework 4 Extended:
[http://www.microsoft.com/net/](http://www.microsoft.com/net/ "http://www.microsoft.com/net/")
=> "Developers" => "Install .NET Framework 4"

#### 2) Microsoft Windows SDK for Windows 7 and .NET Framework 4:
[http://www.microsoft.com/download/en/details.aspx?displayLang=en&id=8279
](http://www.microsoft.com/download/en/details.aspx?displayLang=en&id=8279
 "http://www.microsoft.com/download/en/details.aspx?displayLang=en&id=8279
")

#### 3) A command-line window with administrator rights:
    WinKey -> 'cmd' -> CTRL+SHIFT+ENTER


## Roadmap

#### v0.7 [jan 2012]

- A mini DSL/builder for easy `Memoizer.Net.TwoPhaseExecutor` usage
- Optimizations...


## Help

- Can all this be accomplished using C# attributes? _#lazyweb_
- How do I set up NuGet properly so I can remove the silly "packages"/"lib" folders in Git? _#lazyweb_
