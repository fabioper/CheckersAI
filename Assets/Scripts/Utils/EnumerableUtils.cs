using System.Collections.Generic;

namespace Utils
{
    public static class EnumerableUtils
    {
        public static IEnumerable<int> Range(int start, int stop, int step)
        {
            var x = start;

            do
            {
                yield return x;
                x += step;
                if (step < 0 && x <= stop || 0 < step && stop <= x)
                    break;
            }
            while (true);
        }
    }
}