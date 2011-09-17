### Memoizer.Net.Memoizer
This class is an implementation of a method-level/fine-grained cache (a.k.a. _memoizer_). 
It is based on an implementation from the book ["Java Concurrency in Practice"](http://jcip.net "http://jcip.net") by Brian Goetz et. al. - ported to C# 4.0 using goodness like method handles/delegates and lambda expressions.

The noble thing about this implementation is that it is not caching the _values_ - rather _asynchronous functions_ for retrieving those values are cached.
These functions are guarantied not to be executed more than once in case of concurrent first-time invocations.
The more expensive the memoized functions are, and the more concurrent the environment is - the better suited this memoizer implementation will be.

A [`System.Runtime.Caching.MemoryCache`](http://msdn.microsoft.com/en-us/library/system.runtime.caching.memorycache.aspx "http://msdn.microsoft.com/en-us/library/system.runtime.caching.memorycache.aspx") instance is used as cache, 
enabling configuration via the [`System.Runtime.Caching.CacheItemPolicy`](http://msdn.microsoft.com/en-us/library/system.runtime.caching.cacheitempolicy.aspx "http://msdn.microsoft.com/en-us/library/system.runtime.caching.cacheitempolicy.aspx").
Default cache configuration is: items to be held as long as the CLR is alive or the memoizer is disposed/cleared. 

### Memoizer.Net.LazyMemoizer
Every `Memoizer.Net.Memoizer` instance creates its own [`System.Runtime.Caching.MemoryCache`](http://msdn.microsoft.com/en-us/library/system.runtime.caching.memorycache.aspx "http://msdn.microsoft.com/en-us/library/system.runtime.caching.memorycache.aspx") instance. 
One could re-design this to utilize the ubiquitous _`default`_ [`System.Runtime.Caching.MemoryCache`](http://msdn.microsoft.com/en-us/library/system.runtime.caching.memorycache.aspx "http://msdn.microsoft.com/en-us/library/system.runtime.caching.memorycache.aspx") instance to make the intansiation of this memoizer faster. 
As a middle-way, the `Memoizer.Net.Memoizer` instance, with its [`System.Runtime.Caching.MemoryCache`](http://msdn.microsoft.com/en-us/library/system.runtime.caching.memorycache.aspx "http://msdn.microsoft.com/en-us/library/system.runtime.caching.memorycache.aspx") member instance, can be lazy-loaded by using the `Memoizer.Net.LazyMemoizer`.

#### Usage
Example:

    static readonly Func<long, string> MyExpensiveFunction = ...

    static readonly IInvocable<string, long> MyExpensiveFunctionMemoizer = 
        new LazyMemoizer<string, long>(methodId:           "MyExpensiveFunction",
                                       methodToBeMemoized: MyExpensiveFunction);

    public static string ExpensiveFunction(long someId) { 
        return MyExpensiveFunctionMemoizer.InvokeWith(someId);
    }

#### ToDo
In v0.5: some mini DSL/builder for the memoizer:
    static readonly IInvocable<string, long> MyExpensiveFunctionMemoizer =
        Memoize(MyExpensiveFunction).Instrumented.LazyLoaded.Build();

- Can this be even more shortened with a generic extension method? _#lazyweb_

- Can all this be accomplished using a C# attribute? _#lazyweb_

### Memoizer.Net.TwoPhaseExecutor

A class for synchronized execution of an arbitrary number of worker/task threads. All participating worker/task threads must derive from the `Memoizer.Net.AbstractTwoPhaseExecutorThread` class.

#### Usage
See the `Memoizer.NET.Test.MemoizerTests` class for usage examples. In v0.6 a mini DSL/builder for easy `Memoizer.Net.TwoPhaseExecutor` usage will be included.

### Building the project *
    %DOTNET4_FRAMEWORK_HOME%\MSBuild Memoizer.NET.csproj /p:Configuration=Release


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

### HELP
- How do I set up NuGet properly so I can remove the silly "packages"/"lib" folders in Git? _#lazyweb_
