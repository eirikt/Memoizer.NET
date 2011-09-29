### Memoizer.Net
This project is an implementation of a method-level/fine-grained cache (a.k.a. _memoizer_). 
It is based on an implementation from the book ["Java Concurrency in Practice"](http://jcip.net "http://jcip.net") by Brian Goetz et. al. - ported to C# 4.0 using goodness like method handles/delegates, lambda expressions, and extension methods.

The noble thing about this implementation is that the _values_ are not cached, but rather _asynchronous functions_ for retrieving those values.
These functions are guarantied to be executed not more than once in case of concurrent first-time invocations.
The more expensive the memoized functions are, and the more concurrent the environment is - the better suited this memoizer implementation will be compared to others.

A [`System.Runtime.Caching.MemoryCache`](http://msdn.microsoft.com/en-us/library/system.runtime.caching.memorycache.aspx "http://msdn.microsoft.com/en-us/library/system.runtime.caching.memorycache.aspx") instance is used as cache, 
enabling configuration via the [`System.Runtime.Caching.CacheItemPolicy`](http://msdn.microsoft.com/en-us/library/system.runtime.caching.cacheitempolicy.aspx "http://msdn.microsoft.com/en-us/library/system.runtime.caching.cacheitempolicy.aspx").
Default cache configuration is: items to be held as long as the CLR is alive or the memoizer is disposed/cleared. 

#### Usage
Example, default caching policy:

    static readonly Func<long, string> MyExpensiveFunction = ...

    public static string ExpensiveFunction(long someId) { 
        return MyExpensiveFunction.MemoizedInvoke<long, string>(someId);
    }

Example, expiration policy:

	readonly CacheItemPolicy cacheItemEvictionPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(30) };

    public static string ExpensiveFunction(long someId) { 
        return MyExpensiveFunction.Memoize().CachePolicy(cacheItemEvictionPolicy).Get().InvokeWith(someId);
    }

#### ToDo

- Can all this be accomplished using C# attributes? _#lazyweb_

### Memoizer.Net.TwoPhaseExecutor

A class for synchronized execution of an arbitrary number of worker/task threads. All participating worker/task threads must derive from the `Memoizer.Net.AbstractTwoPhaseExecutorThread` class.

#### Usage
See the `Memoizer.NET.Test.MemoizerTests` class for usage examples. In v0.6 a mini DSL/builder for easy `Memoizer.Net.TwoPhaseExecutor` usage will be included. Right now the API kind of sucks...

---  

### Building the project *
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

---

### HELP
- How do I set up NuGet properly so I can remove the silly "packages"/"lib" folders in Git? _#lazyweb_
