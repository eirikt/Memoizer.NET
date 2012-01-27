using System.Runtime.Serialization;

namespace Memoizer.NET
{
    public static class HashHelper
    {
        static internal readonly int[] PRIMES = new[] { 31, 37, 43, 47, 59, 61, 71, 73, 89, 97, 101, 103, 113, 127, 131, 137 };

        static readonly ObjectIDGenerator OBJECT_ID_GENERATOR = new ObjectIDGenerator();


        internal static long GetObjectId(object arg, ref bool firstTime)
        {
            // Unattended failure experienced: ArrayOutOfBoundException
            return OBJECT_ID_GENERATOR.GetId(arg, out firstTime);
        }


        //public static string CreateParameterHash(params object[] args)
        ////public static string CreateParameterHash(bool possiblySharedMemoizerConfig = true, params object[] args)
        //{
        //    if (args == null)
        //        return "NULLARGARRAY";

        //    if (args.Length == 1)
        //        return args[0] == null ? "NULLARG" : args[0].GetHashCode().ToString();

        //    int retVal;
        //    if (args[0] == null)
        //        retVal = Int32.MinValue;
        //    else
        //        retVal = args[0].GetHashCode() * PRIMES[0];

        //    for (int i = 1; i < args.Length; ++i)
        //    {
        //        if (args[i] == null)
        //            retVal = retVal * PRIMES[i] + Int32.MaxValue;
        //        else
        //            retVal = retVal * PRIMES[i] + args[i].GetHashCode();
        //    }

        //    return retVal.ToString();
        //}


        public static long CreateFunctionHash(object func)
        {
            bool firstTime = false;
            return GetObjectId(func, ref firstTime);
        }
    }
}
