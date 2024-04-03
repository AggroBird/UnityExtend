using System.Collections.Generic;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    public class RandomStream : System.Random
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
                (min, max) = (max, min);
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
                (min, max) = (max, min);
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

        public float Range(FloatRange range)
        {
            return (float)(range.Min + (NextDouble() * range.Range));
        }
        public double Range(DoubleRange range)
        {
            return range.Min + (NextDouble() * range.Range);
        }
        public int Range(IntRange range)
        {
            return Next(range.Min, range.Max);
        }

        public float NextFloat()
        {
            return (float)NextDouble();
        }

        public bool NextBool()
        {
            return (Next() & 1) == 0;
        }

        public T Select<T>(IReadOnlyList<T> list)
        {
            if (list == null)
            {
                throw new System.ArgumentNullException(nameof(list));
            }
            if (list.Count == 0)
            {
                throw new System.IndexOutOfRangeException();
            }
            return list[Range(0, list.Count)];
        }

        public void Shuffle<T>(IList<T> list)
        {
            if (list == null)
            {
                throw new System.ArgumentNullException(nameof(list));
            }
            int n = list.Count;
            if (n > 1)
            {
                for (int i = 0; i < n - 1; i++)
                {
                    int j = Next(i, n);

                    if (j != i)
                    {
                        T temp = list[i];
                        list[i] = list[j];
                        list[j] = temp;
                    }
                }
            }
        }

        public Vector2 InsideUnitCircle()
        {
            float angle = NextFloat() * 2 * Mathf.PI;
            float hyp = Mathf.Sqrt(NextFloat());
            return new Vector2(Mathf.Sin(angle) * hyp, Mathf.Cos(angle) * hyp);
        }
    }
}