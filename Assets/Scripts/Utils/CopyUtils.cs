using Newtonsoft.Json;

namespace Utils
{
    public static class CopyUtils
    {
        public static T DeepCopy<T>(this T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}