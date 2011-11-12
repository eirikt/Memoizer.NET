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
using NUnit.Framework;

namespace Memoizer.NET.Test
{

    [TestFixture]
    class MemoizerConfigurationTests
    {

        [Test]
        public void MemoizerConfigurationHashcodeTests()
        {
            // TODO: use reflection
            //MemoizerConfiguration conf1 = new MemoizerConfiguration(
            //    MemoizerFactoryTests.FIBONACCI,
            //    ExpirationType.Relative,
            //    30,
            //    TimeUnit.Minutes,
            //    null);

            //MemoizerConfiguration conf2 = new MemoizerConfiguration(
            //    MemoizerFactoryTests.FIBONACCI,
            //    ExpirationType.Relative,
            //    30,
            //    TimeUnit.Seconds,
            //    null);

            //MemoizerConfiguration conf3 = new MemoizerConfiguration(
            //    MemoizerFactoryTests.FIBONACCI3,
            //    ExpirationType.Relative,
            //    30,
            //    TimeUnit.Minutes,
            //    null);

            ////Console.WriteLine(MemoizerHelper.CreateParameterHash(conf1));
            ////Console.WriteLine(MemoizerHelper.CreateParameterHash(conf2));
            ////Console.WriteLine(MemoizerHelper.CreateParameterHash(conf3));

            //Assert.That(MemoizerHelper.CreateParameterHash(conf1), Is.EqualTo(conf1.GetHashCode().ToString()));

            //Assert.That(MemoizerHelper.CreateParameterHash(conf1), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(conf2)));
            //Assert.That(MemoizerHelper.CreateParameterHash(conf2), Is.Not.EqualTo(MemoizerHelper.CreateParameterHash(conf3)));
        }
    }
}
