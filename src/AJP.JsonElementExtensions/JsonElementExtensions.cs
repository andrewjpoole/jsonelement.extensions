using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.IO;
using System.Text;
using System.Reflection;

namespace AJP
{
	public static class JsonElementExtensions
	{
		public static JsonElement AddProperty(this JsonElement jElement, string name, string value)
		{
			return ParseAsJsonStringAndMutate(jElement, utf8JsonWriter1 => utf8JsonWriter1.WriteString(name, value));
		}
		public static JsonElement AddProperty(this JsonElement jElement, string name, int value)
		{
			return ParseAsJsonStringAndMutate(jElement, utf8JsonWriter1 => utf8JsonWriter1.WriteNumber(name, value));
		}
		public static JsonElement AddProperty(this JsonElement jElement, string name, double value)
		{
			return ParseAsJsonStringAndMutate(jElement, utf8JsonWriter1 => utf8JsonWriter1.WriteNumber(name, value));
		}
		public static JsonElement AddProperty(this JsonElement jElement, string name, DateTime value)
		{
			return ParseAsJsonStringAndMutate(jElement, utf8JsonWriter1 => utf8JsonWriter1.WriteString(name, value));
		}
		public static JsonElement AddProperty(this JsonElement jElement, string name, bool value)
		{
			return ParseAsJsonStringAndMutate(jElement, utf8JsonWriter1 => utf8JsonWriter1.WriteBoolean(name, value));
		}
		public static JsonElement AddNullProperty(this JsonElement jElement, string name)
		{
			return ParseAsJsonStringAndMutate(jElement, utf8JsonWriter1 => utf8JsonWriter1.WriteNull(name));
		}
		public static JsonElement AddProperty(this JsonElement jElement, string name, string[] value)
		{
			return ParseAsJsonStringAndMutate(jElement, utf8JsonWriter1 =>
			{
				utf8JsonWriter1.WritePropertyName(name);
				utf8JsonWriter1.WriteStartArray();
				foreach (var element in value)
				{
					utf8JsonWriter1.WriteStringValue(element);
				}
				utf8JsonWriter1.WriteEndArray();
			});
		}
		//public static JsonElement AddProperty(this JsonElement jElement, string name, JsonElement value)
		//{
		//	return ParseAsJsonStringAndMutate(jElement, utf8JsonWriter1 =>
		//	{
		//		utf8JsonWriter1.WritePropertyName(name);
		//		utf8JsonWriter1.WriteStartObject();

		//		foreach (var prop in value.EnumerateObject())
		//		{
		//			utf8JsonWriter1.WritePropertyName(prop.Name);
		//			switch (prop.Value.ValueKind)
		//			{
		//				case JsonValueKind.Array:
		//					break;
		//				case JsonValueKind.Undefined:
		//					break;
		//				case JsonValueKind.Object:
		//					break;
		//				case JsonValueKind.String:
		//					break;
		//				case JsonValueKind.Number:
		//					break;
		//				case JsonValueKind.True:
		//					break;
		//				case JsonValueKind.False:
		//					break;
		//				case JsonValueKind.Null:
		//					break;
		//			}
		//		}
		//		utf8JsonWriter1.WriteEndObject();
		//	});
		//}

		public static JsonElement AddProperty(this JsonElement jElement, string name, object value)
		{
			return ParseAsJsonStringAndMutate(jElement, utf8JsonWriter1 =>
			{
				utf8JsonWriter1.WritePropertyName(name);
				utf8JsonWriter1.WriteStartObject();

				foreach (var prop in value.GetProperties())
				{
					utf8JsonWriter1.WritePropertyName(prop.Name);

					switch (prop.Value)
					{
						case string v:
							utf8JsonWriter1.WriteStringValue(v);
							break;
						case int v:
							utf8JsonWriter1.WriteNumberValue(v);
							break;
						case double v:
							utf8JsonWriter1.WriteNumberValue(v);
							break;
						case DateTime v:
							utf8JsonWriter1.WriteStringValue(v);
							break;
						case Guid v:
							utf8JsonWriter1.WriteStringValue(v);
							break;
						case bool v:
							utf8JsonWriter1.WriteBooleanValue(v);
							break;
						default:
							utf8JsonWriter1.WriteStringValue(prop.Value.ToString());
							break;
					}
				}
				utf8JsonWriter1.WriteEndObject();
			});
		}

		private static JsonElement ParseAsJsonStringAndMutate(JsonElement jElement, Action<Utf8JsonWriter> mutate)
		{
			if (jElement.ValueKind != JsonValueKind.Object)
				throw new Exception("Only able to add properties to json objects (i.e. jElement.ValueKind == JsonValueKind.Object)");

			using (MemoryStream memoryStream1 = new MemoryStream())
			{
				using (Utf8JsonWriter utf8JsonWriter1 = new Utf8JsonWriter(memoryStream1))
				{
					utf8JsonWriter1.WriteStartObject();

					mutate(utf8JsonWriter1);

					foreach (var jProp in jElement.EnumerateObject())
					{
						jProp.WriteTo(utf8JsonWriter1);
					}
					utf8JsonWriter1.WriteEndObject();
				}
				var resultJson = Encoding.UTF8.GetString(memoryStream1.ToArray());
				return JsonDocument.Parse(resultJson).RootElement;
			}
		}

		public static IEnumerable<(string Name, object Value)> GetProperties(this object src)
		{
			if (src is IDictionary<string, object> dictionary)
			{
				return dictionary.Select(x => (x.Key, x.Value));
			}
			return src.GetObjectProperties().Select(x => (x.Name, x.GetValue(src)));
		}

		public static IEnumerable<PropertyInfo> GetObjectProperties(this object src)
		{
			return src.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => !p.GetGetMethod().GetParameters().Any());
		}
	}
}
