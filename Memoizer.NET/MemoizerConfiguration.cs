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


namespace Memoizer.NET
{

    /// <remarks>
    /// Immutable class for configuration of memoizers - used for creating memoizer instances.
    /// </remarks>
    public class MemoizerConfiguration
    {
        public MemoizerConfiguration(object function,
                                     ExpirationType expirationType,
                                     int expirationValue,
                                     TimeUnit expirationTimeUnit,
                                     Action<string> loggerMethod)
        {
            Function = function;
            ExpirationType = expirationType;
            ExpirationValue = expirationValue;
            ExpirationTimeUnit = expirationTimeUnit;
            LoggerAction = loggerMethod;
        }

        public object Function { get; private set; }
        public ExpirationType ExpirationType { get; private set; }
        public int ExpirationValue { get; private set; }
        public TimeUnit ExpirationTimeUnit { get; private set; }
        public Action<string> LoggerAction { get; private set; }

        public override int GetHashCode()
        {
            bool firstTime = false;
            int funcId = (Int32)MemoizerHelper.GetObjectId(Function, ref firstTime);

            int retVal = 0;
            retVal = retVal * MemoizerHelper.PRIMES[7] + funcId;
            retVal = retVal * MemoizerHelper.PRIMES[6] + ExpirationType.GetHashCode();
            retVal = retVal * MemoizerHelper.PRIMES[5] + ExpirationValue.GetHashCode();
            retVal = retVal * MemoizerHelper.PRIMES[4] + ExpirationTimeUnit.GetHashCode();

            return retVal;
        }

        public override bool Equals(object otherObject)
        {
            if (ReferenceEquals(null, otherObject)) { return false; }
            if (ReferenceEquals(this, otherObject)) { return true; }
            if (!(otherObject is MemoizerConfiguration)) { return false; }
            MemoizerConfiguration otherMemoizerConfiguration = otherObject as MemoizerConfiguration;
            return this.GetHashCode().Equals(otherMemoizerConfiguration.GetHashCode());
        }
    }
}
