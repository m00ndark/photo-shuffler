using Newtonsoft.Json;

namespace PhotoShuffler
{
	internal static class Json
	{
		public static string Serialize(this object obj) => JsonConvert.SerializeObject(obj, Formatting.Indented);

		public static T Deserialize<T>(this string json) => JsonConvert.DeserializeObject<T>(json);
	}
}
