﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.IO;
using System.Text;
using System.Reflection;

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
		/// <param name="value">The value of the property to add</param>
		/// <returns>A new JsonElement containing the old properties plus the new property</returns>
		public static JsonElement AddProperty(this JsonElement jElement, string name, string value)
		{
			return jElement.ParseAsJsonStringAndMutate((utf8JsonWriter1, namesOfPropertiesToRemove) => utf8JsonWriter1.WriteString(name, value));
		}
		/// <summary>
		/// Method which recreates a new JsonElement from an existing one, with an extra property added along the way
		/// </summary>
		/// <param name="name">A string containing the name of the property to add</param>
		/// <param name="value">The value of the property to add</param>
		/// <returns>A new JsonElement containing the old properties plus the new property</returns>
		public static JsonElement AddProperty(this JsonElement jElement, string name, int value)
		{
			return jElement.ParseAsJsonStringAndMutate((utf8JsonWriter1, namesOfPropertiesToRemove) => utf8JsonWriter1.WriteNumber(name, value));
		}
		/// <summary>
		/// Method which recreates a new JsonElement from an existing one, with an extra property added along the way
		/// </summary>
		/// <param name="name">A string containing the name of the property to add</param>
		/// <param name="value">The value of the property to add</param>
		/// <returns>A new JsonElement containing the old properties plus the new property</returns>
		public static JsonElement AddProperty(this JsonElement jElement, string name, double value)
		{
			return jElement.ParseAsJsonStringAndMutate((utf8JsonWriter1, namesOfPropertiesToRemove) => utf8JsonWriter1.WriteNumber(name, value));
		}
		/// <summary>
		/// Method which recreates a new JsonElement from an existing one, with an extra property added along the way
		/// </summary>
		/// <param name="name">A string containing the name of the property to add</param>
		/// <param name="value">The value of the property to add</param>
		/// <returns>A new JsonElement containing the old properties plus the new property</returns>
		public static JsonElement AddProperty(this JsonElement jElement, string name, DateTime value)
		{
			return jElement.ParseAsJsonStringAndMutate((utf8JsonWriter1, namesOfPropertiesToRemove) => utf8JsonWriter1.WriteString(name, value));
		}
		/// <summary>
		/// Method which recreates a new JsonElement from an existing one, with an extra property added along the way
		/// </summary>
		/// <param name="name">A string containing the name of the property to add</param>
		/// <param name="value">The value of the property to add</param>
		/// <returns>A new JsonElement containing the old properties plus the new property</returns>
		public static JsonElement AddProperty(this JsonElement jElement, string name, bool value)
		{
			return jElement.ParseAsJsonStringAndMutate((utf8JsonWriter1, namesOfPropertiesToRemove) => utf8JsonWriter1.WriteBoolean(name, value));
		}
		/// <summary>
		/// Method which recreates a new JsonElement from an existing one, with an extra property added along the way
		/// </summary>
		/// <param name="name">A string containing the name of the property to add</param>
		/// <returns>A new JsonElement containing the old properties plus the new property</returns>
		public static JsonElement AddNullProperty(this JsonElement jElement, string name)
		{
			return jElement.ParseAsJsonStringAndMutate((utf8JsonWriter1, namesOfPropertiesToRemove) => utf8JsonWriter1.WriteNull(name));
		}
		/// <summary>
		/// Method which recreates a new JsonElement from an existing one, with an extra property added along the way
		/// </summary>
		/// <param name="property">The property to add</param>
		/// <returns>A new JsonElement containing the old properties plus the new property</returns>
		public static JsonElement AddProperty(this JsonElement jElement, JsonProperty property)
		{
			return jElement.ParseAsJsonStringAndMutate((utf8JsonWriter1, namesOfPropertiesToRemove) => property.WriteTo(utf8JsonWriter1));
		}
		/// <summary>
		/// Method which recreates a new JsonElement from an existing one, with an extra property added along the way
		/// </summary>
		/// <param name="name">A string containing the name of the property to add</param>
		/// <param name="value">The value of the property to add</param>
		/// <returns>A new JsonElement containing the old properties plus the new property</returns>
		public static JsonElement AddProperty(this JsonElement jElement, string name, string[] value)
		{
			return jElement.ParseAsJsonStringAndMutate((utf8JsonWriter1, namesOfPropertiesToRemove) =>
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
		/// <summary>
		/// Method which recreates a new JsonElement from an existing one, with an extra property added along the way
		/// </summary>
		/// <param name="name">A string containing the name of the property to add</param>
		/// <param name="value">The value of the property to add</param>
		/// <returns>A new JsonElement containing the old properties plus the new property</returns>
		public static JsonElement AddProperty(this JsonElement jElement, string name, object value)
		{
			return jElement.ParseAsJsonStringAndMutate((utf8JsonWriter1, namesOfPropertiesToRemove) =>
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
		/// <summary>
		/// Method which recreates a new JsonElement from an existing one, but without one of the exiting properties
		/// </summary>
		/// <param name="nameOfPropertyToRemove">A string containing the name of the property to remove</param>
		/// <returns>A new JsonElement containing the old properties apart from the named property to remove</returns>
		public static JsonElement RemoveProperty(this JsonElement jElement, string nameOfPropertyToRemove)
		{
			return jElement.ParseAsJsonStringAndMutate((writer, namesOfPropertiesToRemove) => namesOfPropertiesToRemove.Add(nameOfPropertyToRemove));
		}
		/// <summary>
		/// Method which recreates a new JsonElement from an existing one, but without some of the exiting properties
		/// </summary>
		/// <param name="propertyNamesToRemove">A list of names of the properties to remove</param>
		/// <returns>A new JsonElement without the properties listed</returns>
		public static JsonElement RemoveProperties(this JsonElement jElement, List<string> propertyNamesToRemove)
		{
			return jElement.ParseAsJsonStringAndMutate((writer, namesOfPropertiesToRemove) => 
			{
				foreach (var name in propertyNamesToRemove) 
				{
					namesOfPropertiesToRemove.Add(name);
				}
			});
		}
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

			using (MemoryStream memoryStream1 = new MemoryStream())
			{
				using (Utf8JsonWriter utf8JsonWriter1 = new Utf8JsonWriter(memoryStream1))
				{
					utf8JsonWriter1.WriteStartObject();

					var namesOfPropertiesToRemove = new List<string>();

					mutate?.Invoke(utf8JsonWriter1, namesOfPropertiesToRemove);

					foreach (var jProp in jElement.EnumerateObject())
					{
						if (!(namesOfPropertiesToRemove != null && namesOfPropertiesToRemove.Contains(jProp.Name)))
						{
							jProp.WriteTo(utf8JsonWriter1);
						}
					}
					utf8JsonWriter1.WriteEndObject();
				}
				var resultJson = Encoding.UTF8.GetString(memoryStream1.ToArray());
				return JsonDocument.Parse(resultJson).RootElement;
			}
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
	}
}