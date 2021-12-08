using System.Collections.Generic;

namespace Extensions
{
    public static class DictionaryExtensions
    {
        public static void Update<T>(this Dictionary<CellCoordinates, List<T>> dict, Dictionary<CellCoordinates, List<T>> updatedDict)
        {
            foreach (var key in updatedDict.Keys)
                dict[key] = updatedDict[key];
        }
    }
}