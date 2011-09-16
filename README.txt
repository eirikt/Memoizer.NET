Memoizer.Net.Memoizer / Memoizer.Net.LazyMemoizer
=================================================

This class is an implementation of a method-level/fine-grained cache (a.k.a. memoizer). It is based on an implementation from the book "Java Concurrency in Practice" by Brian Goetz et. al. (http://jcip.net/) - ported to C# 4.0 using goodness like method handles/delegates and lambda expressions.

A System.Runtime.Caching.MemoryCache instance is used as cache, enabling configuration via the System.Runtime.Caching.CacheItemPolicy. Default cache configuration is: items to be held as long as the CLR is alive or the memoizer is disposed/cleared. 




Memoizer.Net.LazyMemoizer
=========================
Every Memoizer.Net.Memoizer instance creates its own System.Runtime.Caching.MemoryCache instance. One could re-design this to utilize the ubiquitous default System.Runtime.Caching.MemoryCache instance to make this memoizer even faster. As a middle-way, the Memoizer.Net.Memoizer instance, with its System.Runtime.Caching.MemoryCache member instance, can be lazy-loaded by using the Memoizer.Net.LazyMemoizer.

Usage
=====

Example:

static readonly Func<long, string> MyExpensiveFunction = ...
static readonly IInvocable<string, long> MyExpensiveFunctionMemoizer = new LazyMemoizer<string, long>(methodId: "MyExpensiveFunction", methodToBeMemoized: MyExpensiveFunction);

public static string MyExpensiveFunction(long someId) { return MyExpensiveFunctionMemoizer.InvokeWith(someId); }


TODO:
// In v0.5: a mini DSL/builder for the memoizer:
static readonly IInvocable<string, long> MyExpensiveFunctionMemoizer = Memoize(MyExpensiveFunction).Instrumented.LazyLoaded.Build();

HELP: can this be even more shortened with a generic extension method?
HELP: can all this be accomplished using an attribute?




Memoizer.Net.TwoPhaseExecutor
=============================

A class for synchronized execution of an arbitrary number of worker/task threads. All participating worker/task threads must derive from the Memoizer.Net.AbstractTwoPhaseExecutorThread class.

Usage
=====

See the Memoizer.NET.Test.MemoizerTests class for usage examples. In v0.6 a mini DSL/builder for easy Memoizer.Net.TwoPhaseExecutor usage will be included.




Building the project *)
=======================

%DOTNET4_FRAMEWORK_HOME%\MSBuild Memoizer.NET.csproj /p:Configuration=Release


*) Prerequisites are:
---------------------

1) .NET Framework 4 Extended:
   http://www.microsoft.com/net/
      -> Developers
      -> Install .NET Framework 4

2) Microsoft Windows SDK for Windows 7 and .NET Framework 4:
   http://www.microsoft.com/download/en/details.aspx?displayLang=en&id=8279

3) A command-line window with administrator rights:
   WinKey -> 'cmd' -> CTRL+SHIFT+ENTER




Help
====

- How do I set up NuGet properly so I can remove the silly ‘packages’/’lib’ folders in Git?
