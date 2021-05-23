using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AJP.JsonElementExtensions.UnitTests
{
	[TestFixture]
    public class JsonElementExtensionsTests
	{       
        [Test]
        public void Various_AddProperty_methods_should_add_properties_that_can_be_asserted_in_the_output()
        {
			// get a JsonElement to start with...
			const string jsonString = "{ \"Name\": \"Andrew\", \"EmailAddress\": \"a@b.com\" }";
			var jElement = JsonDocument.Parse(jsonString).RootElement;

			jElement = jElement
				.AddProperty("Age", 38)
				.AddProperty("Male", true)
				.AddProperty("Female", false)
				.AddNullProperty("Alien")
				.AddProperty("Roles", new[] {"admin", "user"})
				.AddProperty("Foods", new List<string> {"mac and cheese", "lasagna"})
				.AddProperty("LastUpdated", new DateTime(2020, 2, 27, 22, 09, 00))
				.AddProperty("crazyNewObject", new {
					Name = "Hobbies",
					Value = "bass guitar and writing c# code"
				})
				.AddProperty("nestedObject", new {
					Object = new {
						Name = "Hobbies"
					}
				});

			Assert.That(jElement.GetProperty("Age").ToString(), Is.EqualTo("38"));
			Assert.That(jElement.GetProperty("Male").GetBoolean(), Is.EqualTo(true));
			Assert.That(jElement.GetProperty("Female").GetBoolean(), Is.EqualTo(false));
			Assert.That(jElement.GetProperty("Alien").GetString(), Is.EqualTo(null));
			Assert.That(jElement.GetProperty("Roles").GetArrayLength(), Is.EqualTo(2));
			Assert.That(jElement.GetProperty("Roles").EnumerateArray().FirstOrDefault().GetString(), Is.EqualTo("admin"));
			Assert.That(jElement.GetProperty("Foods").GetArrayLength(), Is.EqualTo(2));
			Assert.That(jElement.GetProperty("Foods").EnumerateArray().Select(x => x.GetString()).ToArray(), Is.EqualTo(new[] {"mac and cheese", "lasagna"}));
			Assert.That(jElement.GetProperty("LastUpdated").GetString(), Is.EqualTo(new DateTime(2020, 2, 27, 22, 09, 00).ToString("s")));
			Assert.That(jElement.GetProperty("crazyNewObject").EnumerateObject().FirstOrDefault().Value.ToString(), Is.EqualTo("Hobbies"));
			Assert.That(jElement.GetProperty("nestedObject").GetProperty("Object").GetProperty("Name").GetString(), Is.EqualTo("Hobbies"));
        }

        [Test]
        public void AddProperty_method_respects_json_options() 
        {
	        var options = new JsonSerializerOptions {
		        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		        IgnoreNullValues = true
	        };
		        
	        // get a JsonElement to start with...
	        const string jsonString = "{ \"Name\": \"Andrew\", \"EmailAddress\": \"a@b.com\" }";
	        var jElement = JsonDocument.Parse(jsonString).RootElement;

	        var obj = new {
		        Hello = "hello",
		        World = (string)null
	        };

	        jElement = jElement
		        .AddProperty("withOptions", obj, options)
		        .AddProperty("withoutOptions", obj);

	        var withOptions = jElement.GetProperty("withOptions");
	        var withoutOptions = jElement.GetProperty("withoutOptions"); 
	        
	        Assert.That(withOptions.TryGetProperty("hello", out var _), Is.True);
	        Assert.That(withOptions.TryGetProperty("Hello", out var _), Is.Not.True);
	        Assert.That(withOptions.TryGetProperty("world", out var _), Is.Not.True);
	       
	        Assert.That(withoutOptions.TryGetProperty("Hello", out var _), Is.True);
	        Assert.That(withoutOptions.TryGetProperty("hello", out var _), Is.Not.True);
	        Assert.That(withoutOptions.TryGetProperty("World", out var _), Is.True);
        }

		[Test]
		public void ParseAsJsonStringAndMutate_method_should_add_properties_that_can_be_asserted_in_the_output()
		{
			// get a JsonElement to start with...
			const string jsonString = "{ \"Name\": \"Andrew\", \"EmailAddress\": \"a@b.com\" }";
			var jElement = JsonDocument.Parse(jsonString).RootElement;

			jElement = jElement.ParseAsJsonStringAndMutate((utf8JsonWriter1, namesOfPropertiesToRemove) => 
			{
				namesOfPropertiesToRemove.Add("EmailAddress");
				utf8JsonWriter1.WriteBoolean("IsAdmin", true);
			});

			Assert.That(jElement.GetProperty("IsAdmin").ToString(), Is.EqualTo(true.ToString()));
			Assert.Throws<KeyNotFoundException>(() => jElement.GetProperty("EmailAddress"));
		}		

		[Test]
		public void RemoveProperty_methods_should_remove_properties_from_the_output()
		{
			// get a JsonElement to start with...
			const string jsonString = "{ \"Name\": \"Andrew\", \"EmailAddress\": \"a@b.com\" }";
			var jElement = JsonDocument.Parse(jsonString).RootElement;

			jElement = jElement
				.RemoveProperty("EmailAddress");

			Assert.Throws<KeyNotFoundException>(() => jElement.GetProperty("EmailAddress"));
		}

		[Test]
		public void RemoveProperties_methods_should_remove_properties_from_the_output()
		{
			// get a JsonElement to start with...
			const string jsonString = "{ \"Name\": \"Andrew\", \"EmailAddress\": \"a@b.com\", \"Age\": 38 }";
			var jElement = JsonDocument.Parse(jsonString).RootElement;

			jElement = jElement.RemoveProperties(new List<string> { "EmailAddress", "Age" });

			Assert.Throws<KeyNotFoundException>(() => jElement.GetProperty("EmailAddress"));
			Assert.Throws<KeyNotFoundException>(() => jElement.GetProperty("Age"));
		}

        [Test] public void AddProperty_method_should_handle_primitive_style_objects_cast_as_objects()
        {
            // get a JsonElement to start with...
            const string jsonString = "{ \"Name\": \"Andrew\", \"EmailAddress\": \"a@b.com\", \"Age\": 38 }";
            var jElement = JsonDocument.Parse(jsonString).RootElement;

            var stringCastToObject = (object) "3jk4h5gkj3hg45kjh4g5";
			
            jElement = jElement.AddProperty("Id", stringCastToObject);

			Assert.That(jElement.GetProperty("Id").ToString(), Is.EqualTo("3jk4h5gkj3hg45kjh4g5"));
		}
        
        [Test] public void JsonElement_ConvertToObject_method_should_return_the_specified_object()
        {
	        // get a JsonElement to start with...
	        const string jsonString = "{ \"Name\": \"Andrew\", \"EmailAddress\": \"a@b.com\", \"Age\": 38 }";
	        var jElement = JsonDocument.Parse(jsonString).RootElement;

	        var result = jElement.ConvertToObject<TestClass>();
	        Assert.That(result, Is.Not.Null);
	        Assert.That(result.Name, Is.EqualTo("Andrew"));
	        Assert.That(result.EmailAddress, Is.EqualTo("a@b.com"));
        }
        
        [Test] public void JsonDocument_ConvertToObject_method_should_return_the_specified_object()
        {
	        // get a JsonElement to start with...
	        const string jsonString = "{ \"Name\": \"Andrew\", \"EmailAddress\": \"a@b.com\", \"Age\": 38 }";
	        var jDoc = JsonDocument.Parse(jsonString);
	        
	        var result = jDoc.ConvertToObject<TestClass>();
	        Assert.That(result, Is.Not.Null);
	        Assert.That(result.Name, Is.EqualTo("Andrew"));
	        Assert.That(result.EmailAddress, Is.EqualTo("a@b.com"));
        }
	}

    public class TestClass
    {
	    public string Name { get; set; }
	    public string EmailAddress { get; set; }
    }
}