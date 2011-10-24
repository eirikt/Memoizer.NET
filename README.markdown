## Memoizer.NET
This project is an implementation of a method-level/fine-grained cache (a.k.a. _memoizer_). It is based on an implementation from the book ["Java Concurrency in Practice"](http://jcip.net "http://jcip.net") by Brian Goetz et. al. - ported to C# 4.0 using goodness like method handles/delegates, lambda expressions, and extension methods.

The noble thing about this implementation is that the _values_ are not cached, but rather _asynchronous functions_ for retrieving those values. These functions are guarantied to be executed not more than once in case of concurrent first-time invocations.

A [`System.Runtime.Caching.MemoryCache`](http://msdn.microsoft.com/en-us/library/system.runtime.caching.memorycache.aspx "http://msdn.microsoft.com/en-us/library/system.runtime.caching.memorycache.aspx") instance is used as cache, enabling configuration via the [`System.Runtime.Caching.CacheItemPolicy`](http://msdn.microsoft.com/en-us/library/system.runtime.caching.cacheitempolicy.aspx "http://msdn.microsoft.com/en-us/library/system.runtime.caching.cacheitempolicy.aspx"). Default cache configuration is: items to be held as long as the CLR is alive, or until the memoizer is disposed/cleared. 

### Usage

#### Example 1 - default caching policy

	Func<long, string> myExpensiveFunction = ...

	string ExpensiveFunction(long someId)
    {
        return myExpensiveFunction.CachedInvoke(someId);
    }

#### Example 2 - expiration policy: keep items cached for 30 minutes

	string ExpensiveFunctionWithExpiration(long someId)
	{
        return myExpensiveFunction.Memoize().KeepItemsCachedFor(30).Minutes.GetMemoizer().InvokeWith(someId);
		// Or
		//return myExpensiveFunction.CacheFor(30).Minutes.GetMemoizer().InvokeWith(someId);
    }

There are three `Func` extension methods at work here...

The first one, `CachedInvoke()`, just gives you the default cache configuration. (It is named mimicking the regular `Func` methods `Invoke()` and `DynamicInvoke()`.)

The second method, `Memoize()`, is the one that gets you into "memoizer config mode". The third method, `CacheFor()`, is a shortcut for `Memoize()` and gets you right into cache expiration configuration. The two last extension methods must be ended by `GetMemoizer()` to get hold of the `IMemoizer` object - ready for invocation.

#### Example 3 - clearing the cache

Removal of all the items the cache is also performed via the `IMemoizer` object. So, for removing the cached items from the `ExpensiveFunction` method, you may define a method for it like:

	void ExpensiveFunctionCacheClearing()
	{
		myExpensiveFunction.GetMemoizer().Clear();
	}

Or declare an `Action` for it:

	Action expensiveFunctionCacheClearingAction = myExpensiveFunction.GetMemoizer().Clear;

When doing multiple operations on a memoizer, it's maybe just as well declaring it upfront. Something like:

	IMemoizer<long, string> myExpensiveFunctionMemoizer = myExpensiveFunction.CacheFor(30).Minutes.InstrumentWith(Console.WriteLine).GetMemoizer();
	
	string ExpensiveFunctionWithExpiration(long someId)	{ return myExpensiveFunctionMemoizer.InvokeWith(someId); }
 	void ExpensiveFunctionCacheClearing() { myExpensiveFunctionMemoizer.Clear(); }

#### Memoizer.NET eats its own dog food...

The "inlined" style, where the memoizer is configured and created/retrieved at runtime, works because the memoized function handles are themselves memoized (behind the curtain).

#### Recursive functions

Memoization of recursive functions are not supported - so e.g. the Fibonacci sequence function will not work this way.

## Memoizer.NET.TwoPhaseExecutor

A class for synchronized execution of an arbitrary number of worker/task threads. All participating worker/task threads must derive from the `Memoizer.Net.AbstractTwoPhaseExecutorThread` class.

### Usage
See the `Memoizer.NET.Test.MemoizerTests` class for usage examples. In v0.7 a mini DSL/builder for easy `Memoizer.Net.TwoPhaseExecutor` usage will be included. Right now the API is rather cumbersome/sucks...

## Building the project *
    %DOTNET4_FRAMEWORK_HOME%\MSBuild Memoizer.NET.csproj /p:Configuration=Release

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

## Future plans

#### v0.6

- An immutable memoizer config class
- Invalidation of individual items
- Flexible administration of the memoized memoizer (get shared | create new | remove existing)
- ...

#### v0.7

- A mini DSL/builder for easy `Memoizer.Net.TwoPhaseExecutor` usage
- ...

## Help

- Can all this be accomplished using C# attributes? _#lazyweb_
- How do I set up NuGet properly so I can remove the silly "packages"/"lib" folders in Git? _#lazyweb_
- How to support memoization of recursive functions? _#lazyweb_
