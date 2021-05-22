using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Buffers;
using System.Collections;
using System.Diagnostics;

namespace AJP
{
	/// <summary>
	/// Methods which allow the addition or removal of properties on a JsonElement.
	/// JsonElement is immutable, so these methods work by enumerating the existing properties and writing them into a new jsonstring in memory.
	/// Additional properties can be added and existing properties can be removed and the resulting string is parsed into a new JsonElement which is returned.
	/// Please note this roundtrip process happens for every call, so if lots of changes are needed, please consider/test using ParseAsJsonStringAndMutate() 
	/// so that all changes can be done together, with only one roudtrip process.
	/// A new JsonElement is returned, the original is unchanged.
	/// </summary>
	public static class JsonElementExtensions
	{
        /// <summary>
		/// Method which recreates a new JsonElement from an existing one, with an extra property added along the way
		/// </summary>
		/// <param name="name">A string containing the name of the property to add</param>
		/// <returns>A new JsonElement containing the old properties plus the new property</returns>
		public static JsonElement AddNullProperty(this JsonElement jElement, string name) => 
	        jElement.ParseAsJsonStringAndMutate((utf8JsonWriter, namesOfPropertiesToRemove) => utf8JsonWriter.WriteNull(name));

        /// <summary>
		/// Method which recreates a new JsonElement from an existing one, with an extra property added along the way
		/// </summary>
		/// <param name="property">The property to add</param>
		/// <returns>A new JsonElement containing the old properties plus the new property</returns>
		public static JsonElement AddProperty(this JsonElement jElement, JsonProperty property) => 
	        jElement.ParseAsJsonStringAndMutate((utf8JsonWriter, namesOfPropertiesToRemove) => property.WriteTo(utf8JsonWriter));

        /// <summary>
		/// Method which recreates a new JsonElement from an existing one, with an extra property added along the way. 
		/// </summary>
		/// <param name="name">A string containing the name of the property to add</param>
		/// <param name="value">The value of the property to add, primitives and simple objects are supported.</param>
		/// <param name="options">The serializer options to respect when converting values.</param>
		/// <returns>A new JsonElement containing the old properties plus the new property</returns>
		public static JsonElement AddProperty(this JsonElement jElement, string name, object value, JsonSerializerOptions options = null) =>
            jElement.ParseAsJsonStringAndMutate((utf8JsonWriter, namesOfPropertiesToRemove) =>
            {
	            if(value is null) {
					HandleNull(utf8JsonWriter, name, options);
					return;
	            }
	            
                utf8JsonWriter.WritePropertyName(name);
                RenderValue(utf8JsonWriter, value, options);
            });

        private static void RenderValue(this Utf8JsonWriter writer, object value, JsonSerializerOptions options = null)
        {
	        // The value is not a primitive.
	        if(Convert.GetTypeCode(value) == TypeCode.Object && !(value is IEnumerable)) {
		        writer.WriteStartObject();
		        foreach (var (propName, propValue) in value.GetProperties()) {
			        if(propValue is null) {
						HandleNull(writer, propName, options);
						continue;
			        }
			        
			        writer.WritePropertyName(options?.PropertyNamingPolicy.ConvertName(propName) ?? propName);
			        writer.RenderValue(propValue);
		        }
		        writer.WriteEndObject();
		        return;
	        }
	        
            switch (value)
            {
                case string v:
                    writer.WriteStringValue(v);
                    break;
                case bool v:
                    writer.WriteBooleanValue(v);
                    break;
                case decimal v:
                    writer.WriteNumberValue(v);
                    break;
				case int v:
                    writer.WriteNumberValue(v);
                    break;
                case double v:
                    writer.WriteNumberValue(v);
                    break;
                case float v:
                    writer.WriteNumberValue(v);
                    break;
				case DateTime v:
                    writer.WriteStringValue(v);
                    break;
                case Guid v:
                    writer.WriteStringValue(v);
                    break;
                case IEnumerable<object> arr:
	                writer.WriteStartArray();
	                arr.ToList()
		                .ForEach(obj => RenderValue(writer, obj));
	                writer.WriteEndArray();
	                break;
                default:
                    writer.WriteStringValue(value.ToString());
                    break;
            }
        }

        private static void HandleNull(Utf8JsonWriter writer, string propName, JsonSerializerOptions options = null) {
	        if(options?.IgnoreNullValues == true)
		        return;
	        
	        writer.WriteNull(options?.PropertyNamingPolicy?.ConvertName(propName) ?? propName);
        }

		/// <summary>
		/// Method which recreates a new JsonElement from an existing one, but without one of the exiting properties
		/// </summary>
		/// <param name="nameOfPropertyToRemove">A string containing the name of the property to remove</param>
		/// <returns>A new JsonElement containing the old properties apart from the named property to remove</returns>
		public static JsonElement RemoveProperty(this JsonElement jElement, string nameOfPropertyToRemove) => 
			jElement.ParseAsJsonStringAndMutate((writer, namesOfPropertiesToRemove) => namesOfPropertiesToRemove.Add(nameOfPropertyToRemove));

		/// <summary>
		/// Method which recreates a new JsonElement from an existing one, but without some of the exiting properties
		/// </summary>
		/// <param name="propertyNamesToRemove">A list of names of the properties to remove</param>
		/// <returns>A new JsonElement without the properties listed</returns>
		public static JsonElement RemoveProperties(this JsonElement jElement, IEnumerable<string> propertyNamesToRemove) =>
			jElement.ParseAsJsonStringAndMutate((writer, namesOfPropertiesToRemove) =>
			{
				namesOfPropertiesToRemove.AddRange(propertyNamesToRemove);
			});

		/// <summary>
		/// Method which recreates a new JsonElement from an existing one, with the opportunity to add new and remove existing properties
		/// </summary>
		/// <param name="mutate">An Action of Utf8JsonWriter and List of strings. 
		/// The Utf8JsonWriter allows the calling code to write additional properties, its possible to add highly complex nested structures,
		/// the list of strings is a list names of any existing properties to be removed from the resulting JsonElement</param>
		/// <returns>A new JsonElement</returns>
		public static JsonElement ParseAsJsonStringAndMutate(this JsonElement jElement, Action<Utf8JsonWriter, List<string>> mutate)
		{
			if (jElement.ValueKind != JsonValueKind.Object)
				throw new Exception("Only able to add properties to json objects (i.e. jElement.ValueKind == JsonValueKind.Object)");
			
			var arrayBufferWriter = new ArrayBufferWriter<byte>();
            using (var utf8JsonWriter1 = new Utf8JsonWriter(arrayBufferWriter))
            {
                utf8JsonWriter1.WriteStartObject();

                var namesOfPropertiesToRemove = new List<string>();

                mutate?.Invoke(utf8JsonWriter1, namesOfPropertiesToRemove);

                foreach (var jProp in jElement.EnumerateObject())
                {
                    if (!(namesOfPropertiesToRemove.Contains(jProp.Name)))
                    {
                        jProp.WriteTo(utf8JsonWriter1);
                    }
                }
                utf8JsonWriter1.WriteEndObject();
            }
            var resultJson = Encoding.UTF8.GetString(arrayBufferWriter.WrittenSpan);
            return JsonDocument.Parse(resultJson).RootElement;
        }
		
		/// <summary>
		/// Method which returns a list of property name and value, from a given object
		/// </summary>
		public static IEnumerable<(string Name, object Value)> GetProperties(this object source)
        {
            if (source is IDictionary<string, object> dictionary)
			{
				return dictionary.Select(x => (x.Key, x.Value));
			}
			
			return source.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => !p.GetGetMethod().GetParameters().Any())
				.Select(x => (x.Name, x.GetValue(source)));
		}
		
		/// <summary>
		/// Method which attempts to convert a given JsonElement into the specified type
		/// </summary>
		/// <param name="jElement">The JsonElement to convert</param>
		/// <param name="options">JsonSerializerOptions to use</param>
		/// <typeparam name="T">The specified type</typeparam>
		/// <returns></returns>
		public static T ConvertToObject<T>(this JsonElement jElement, JsonSerializerOptions options = null)
		{
			var arrayBufferWriter = new ArrayBufferWriter<byte>();
			using (var writer = new Utf8JsonWriter(arrayBufferWriter))
				jElement.WriteTo(writer);
			
			return JsonSerializer.Deserialize<T>(arrayBufferWriter.WrittenSpan, options);
		}

		/// <summary>
		/// Method which attempts to convert a given JsonDocument into the specified type
		/// </summary>
		/// <param name="jDocument">The JsonDocument to convert</param>
		/// <param name="options">JsonSerializerOptions to use</param>
		/// <typeparam name="T">The specified type</typeparam>
		/// <returns>An instance of the specified type from the supplied JsonDocument</returns>
		/// <exception cref="ArgumentNullException">Thrown if the JsonDocument cannot be dserialised into the specified type</exception>
		public static T ConvertToObject<T>(this JsonDocument jDocument, JsonSerializerOptions options = null)
		{
			if (jDocument == null)
				throw new ArgumentNullException(nameof(jDocument));
			
			return jDocument.RootElement.ConvertToObject<T>(options);
		}
	}
}
