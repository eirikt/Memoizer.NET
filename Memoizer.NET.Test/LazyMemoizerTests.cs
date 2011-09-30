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
using System.Reflection;
using System.Threading;
using NUnit.Framework;

namespace Memoizer.NET.Test
{

    //#region LazyMemoizer extension methods
    //public static class LazyMemoizerExtensionMethods
    //{
    //    public static T GetFieldValue<T, TResult, TParam>(this LazyMemoizer<TResult, TParam> source, string fieldName)
    //    {
    //        FieldInfo field = source.GetType().GetField(fieldName, BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
    //        if (field != null)
    //        {
    //            if (field.FieldType != typeof(T)) { throw new ArgumentException("Field name '" + fieldName + "' is not of type " + typeof(T)); }
    //            return (T)field.GetValue(source);
    //        }
    //        throw new ArgumentException("Non-existing field name '" + fieldName + "' given to LazyMemoizerExtensionMethods.GetFieldValue()");
    //    }
    //}
    //#endregion

    //[TestFixture]
    //public class LazyMemoizerTests
    //{
    //    const int METHOD_RESPONSE_LATENCY_IN_MILLIS = 1000;
    //    const string METHOD_RESPONSE_ELEMENT = "VeryExpensiveMethodResponseFor";

    //    //static string VeryExpensiveStaticServiceCall(long longArg)
    //    //{
    //    //    //Console.WriteLine("VeryExpensiveServiceCall invoked...");
    //    //    //Console.WriteLine("Sleeping for " + METHOD_RESPONSE_LATENCY_IN_MILLIS + "ms... [" + DateTime.Now + "]");
    //    //    Thread.Sleep(METHOD_RESPONSE_LATENCY_IN_MILLIS);
    //    //    //Console.WriteLine("Sleeping for " + METHOD_RESPONSE_LATENCY_IN_MILLIS + "ms... [" + DateTime.Now + "]");
    //    //    //Console.WriteLine("VeryExpensiveServiceCall returns...");
    //    //    //Console.WriteLine();
    //    //    return METHOD_RESPONSE_ELEMENT + "(" + longArg + ")";
    //    //}


    //    [Test]
    //    public void LazyInitializerShouldLazyLoadMemoizer_UsingMethodHash()
    //    {
    //        IInvocable<string,  string,long> someLazyMemoizer =
    //            MemoizerTests.ReallySlowNetworkInvocation1c.Memoize().Get();

    //        Assert.That(someLazyMemoizer, Is.Not.Null);
    //        // TODO: replace with property assertions
    //        //Assert.That(someLazyMemoizer.GetFieldValue<bool, string, long>("doInstrumentInvocations"), Is.True);
    //        //Assert.That(someLazyMemoizer.GetFieldValue<string, string, long>("methodHash"), Is.EqualTo("Memoizer.NET.Test.LazyMemoizerTests.someLazyMemoizer"));
    //        //Assert.That(someLazyMemoizer.GetFieldValue<string, string, long>("nameOfMethodToBeMemoized"), Is.Null);
    //        //Assert.That(someLazyMemoizer.GetFieldValue<Type, string, long>("invokingType"), Is.Null);

    //        Lazy<IInvocable<string, long>> lazyInitializerField = someLazyMemoizer.GetFieldValue<Lazy<Memoizer<string, long>>, string, long>("lazyInitializer");
    //        Assert.That(lazyInitializerField, Is.Not.Null);
    //        Assert.That(lazyInitializerField.IsValueCreated, Is.False);

    //        Assert.That(someLazyMemoizer.InvokeWith(13L), Is.EqualTo(METHOD_RESPONSE_ELEMENT + "(13)"));
    //        Assert.That(lazyInitializerField.IsValueCreated, Is.True);
    //    }


    //    [Test]
    //    public void LazyInitializerShouldLazyLoadMemoizer_UsingTypeAndMethodName()
    //    {
    //        LazyMemoizer<string, long> someLazyMemoizer = new LazyMemoizer<string, long>(//typeof(LazyMemoizerTests),
    //            //"VeryExpensiveStaticServiceCall",
    //                                                                                     VeryExpensiveStaticServiceCall//,
    //            //doInstrumentInvocations: true
    //        );
    //        someLazyMemoizer.LoggingMethod = Console.WriteLine;
    //        //someLazyMemoizer.MethodName = "Memoizer.NET.Test.LazyMemoizerTests.VeryExpensiveStaticServiceCall"; // => <unknown...>

    //        Assert.That(someLazyMemoizer, Is.Not.Null);
    //        // TODO: replace with property assertions
    //        //Assert.That(someLazyMemoizer.GetFieldValue<bool, string, long>("doInstrumentInvocations"), Is.True);
    //        //Assert.That(someLazyMemoizer.GetFieldValue<string, string, long>("methodHash"), Is.Null);
    //        //Assert.That(someLazyMemoizer.GetFieldValue<string, string, long>("nameOfMethodToBeMemoized"), Is.EqualTo("VeryExpensiveStaticServiceCall"));
    //        //Assert.That(someLazyMemoizer.GetFieldValue<Type, string, long>("invokingType"), Is.EqualTo(typeof(LazyMemoizerTests)));

    //        Lazy<Memoizer<string, long>> lazyInitializerField = someLazyMemoizer.GetFieldValue<Lazy<Memoizer<string, long>>, string, long>("lazyInitializer");
    //        Assert.That(lazyInitializerField, Is.Not.Null);
    //        Assert.That(lazyInitializerField.IsValueCreated, Is.False);

    //        Assert.That(someLazyMemoizer.InvokeWith(13L), Is.EqualTo(METHOD_RESPONSE_ELEMENT + "(13)"));
    //        Assert.That(lazyInitializerField.IsValueCreated, Is.True);
    //    }


    //    // TODO: dispose method

    //}
}
