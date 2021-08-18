using System.Text.Json;

namespace SingleInstanceCore
{
	//For inline serializing and deserializing
	internal static class SerializationExtensions
	{
		private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions()
		{
			PropertyNamingPolicy = null,
			AllowTrailingCommas = true
		};

		internal static byte[] Serialize<T>(this T obj)
		{
			return JsonSerializer.SerializeToUtf8Bytes(obj, serializerOptions);
		}

		internal static T Deserialize<T>(this byte[] data)
		{
			return JsonSerializer.Deserialize<T>(data, serializerOptions);
		}
	}
}
