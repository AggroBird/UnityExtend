using System;
using System.Collections.Generic;

namespace AggroBird.UnityEngineExtend
{
    public class RandomStream : Random
    {
        public RandomStream()
        {

        }
        public RandomStream(int seed) : base(seed)
        {

        }


        public float Range(float min, float max)
        {
            if (min > max)
            {
                float tmp = max;
                max = min;
                min = tmp;
            }
            else if (min == max)
            {
                return min;
            }

            return (float)(min + (NextDouble() * (max - min)));
        }
        public double Range(double min, double max)
        {
            if (min > max)
            {
                double tmp = max;
                max = min;
                min = tmp;
            }
            else if (min == max)
            {
                return min;
            }

            return min + (NextDouble() * (max - min));
        }
        public int Range(int min, int max)
        {
            return Next(min, max);
        }

        public float NextFloat()
        {
            return (float)NextDouble();
        }

        public bool NextBool()
        {
            return (Next() & 1) == 0;
        }

        public T Select<T>(IReadOnlyList<T> arr)
        {
            if (arr != null && arr.Count > 0)
            {
                return arr[Range(0, arr.Count)];
            }
            return default;
        }
    }
}