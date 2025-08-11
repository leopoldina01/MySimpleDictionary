using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySimpleDictionary.Helper
{
    public static class PrimeNumbersHelper
    {
        public static int[] primeNumbers;

        static PrimeNumbersHelper()
        {
            //ovu listu prostih brojeva sam nasla na internetu
            //https://www.dotnetperls.com/prime
            primeNumbers = new int[]
            {
                3, 7, 11, 17, 23, 29, 37,
                47, 59, 71, 89, 107, 131,
                163, 197, 239, 293, 353,
                431, 521, 631, 761, 919,
                1103, 1327, 1597, 1931,
                2333, 2801, 3371, 4049,
                4861, 5839, 7013, 8419,
                10103, 12143, 14591, 17519,
                21023, 25229, 30293, 36353,
                43627, 52361, 62851, 75431,
                90523, 108631, 130363,
                156437, 187751, 225307,
                270371, 324449, 389357,
                467237, 560689, 672827,
                807403, 968897, 1162687,
                1395263, 1674319, 2009191,
                2411033, 2893249, 3471899,
                4166287, 4999559, 5999471,
                7199369
            };
        }

        public static bool IsNumberPrime(int number)
        {
            if (number == 2)
            {
                return true;
            }
            else if (number % 2 == 0)
            {
                return false;
            }

            for (int i = 3; i < number / 2; i++)
            {
                if (number % i == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static int GetFirstNextPrime(int number)
        {
            for (int i = 0; i < primeNumbers.Length; i++)
            {
                if (number < primeNumbers[i])
                {
                    return primeNumbers[i];
                }
            }

            //i ova ideja da se ide do poslednje int vrednosti je uzeta sa sajta
            //https://www.dotnetperls.com/prime
            int maxIntPossible = 2147483647;

            for (int i = number; i < maxIntPossible; i++)
            {
                if (IsNumberPrime(i))
                {
                    return i;
                }
            }

            return number * 2; //verovatno se nikad nece desiti, jer nam nece trebati toliki dictionary
        }
    }
}
