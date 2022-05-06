using System;

namespace FishNet.Utility.Extension
{
    public static class EnumFN
    {
        /// <summary>
        /// Returns the highest numeric value for T.
        /// </summary>
        internal static int GetHighestValue<T>()
        {
            var enumType = typeof(T);
            /* Brute force enum values. 
             * Linq Last/Max lookup throws for IL2CPP. */
            var highestValue = 0;
            var pidValues = Enum.GetValues(enumType);
            foreach (T pid in pidValues)
            {
                var obj = Enum.Parse(enumType, pid.ToString());
                var value = Convert.ToInt32(obj);
                highestValue = Math.Max(highestValue, value);
            }

            return highestValue;
        }
    }
}