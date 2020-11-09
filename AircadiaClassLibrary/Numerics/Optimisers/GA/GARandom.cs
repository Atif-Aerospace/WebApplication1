using System;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA
{
	[Serializable()]
    public class GARandom
    {
        private static long seed = -100;

        static long idum2 = 123456789;
        static long iy = 0;
        static long[] iv = new long[32];


        public GARandom()
        {
        }

        //public GARandom(long seedValue)
        //{
        //    seed = seedValue;
        //    idum2 = 123456789;
        //    iy = 0;
        //    iv = new long[32];
        //}



        public static double random01()
        {
            long j;
            long k;
            double temp;

            if (seed <= 0)
            {
                if (-(seed) < 1) seed = 1;
                else seed = -(seed);
                idum2 = (seed);
                for (j = 32 + 7; j >= 0; j--)
                {
                    k = (seed) / 53668;
                    seed = 40014 * (seed - k * 53668) - k * 12211;
                    if (seed < 0) seed += 2147483563;
                    if (j < 32) iv[j] = seed;
                }
                iy = iv[0];
            }
            k = (seed) / 53668;
            seed = 40014 * (seed - k * 53668) - k * 12211;
            if (seed < 0) seed += 2147483563;
            k = idum2 / 52774;
            idum2 = 40692 * (idum2 - k * 52774) - k * 3791;
            if (idum2 < 0) idum2 += 2147483399;
            j = (long)(iy / (1 + (2147483563 - 1) / 32));
            iy = iv[j] - idum2;
            iv[j] = seed;
            if (iy < 1) iy += (2147483563 - 1);
            if ((temp = (1.0 / 2147483563) * iy) > (1.0 - 1.2e-7)) return (1.0 - 1.2e-7);
            else return temp;
        }


        //generates a random integer between lower bound and upper bound
        public static int BoundedRandomInteger(int lBound, int uBound)
        {
            return (int)((uBound - lBound) * random01() + lBound);
        }

        //generates a random double between lower bound and upper bound
        public static double BoundedRandomDouble(double lBound, double uBound)
        {
            return (uBound - lBound) * random01() + lBound;
        }

        //random flip for a given probability
        public static bool Flip(double prob)
        {
            double tempDouble = random01();
            //System.Console.WriteLine("tempDouble: " + tempDouble);
            return tempDouble <= prob;
        }


    }
}
