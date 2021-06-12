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
            var jElement = GetJsonElement();

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
				})
				.AddProperty("nullProperty", null);

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
			Assert.That(jElement.GetProperty("nullProperty").ValueKind, Is.EqualTo(JsonValueKind.Null));
        }

        [Test]
        public void AddProperty_method_respects_json_options() 
        {
	        var options = new JsonSerializerOptions {
		        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		        IgnoreNullValues = true
	        };

			var jElement = GetJsonElement();

			var obj = new {
		        Hello = "hello",
		        World = (string)null
	        };

	        jElement = jElement
		        .AddProperty("withOptions", obj, options)
		        .AddProperty("withoutOptions", obj)
		        .AddNullProperty("NullWithOptions", options)
		        .AddNullProperty("NullWithoutOptions")
		        .AddNullProperty("NullWithoutIgnoreNull",
			        new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});

	        var withOptions = jElement.GetProperty("withOptions");
	        var withoutOptions = jElement.GetProperty("withoutOptions"); 
	        
	        Assert.That(withOptions.TryGetProperty("hello", out var _), Is.True);
	        Assert.That(withOptions.TryGetProperty("Hello", out var _), Is.Not.True);
	        Assert.That(withOptions.TryGetProperty("world", out var _), Is.Not.True);
	       
	        Assert.That(withoutOptions.TryGetProperty("Hello", out var _), Is.True);
	        Assert.That(withoutOptions.TryGetProperty("hello", out var _), Is.Not.True);
	        Assert.That(withoutOptions.TryGetProperty("World", out var _), Is.True);

	        Assert.That(jElement.TryGetProperty("nullWithOptions", out var _), Is.Not.True);
	        Assert.That(jElement.TryGetProperty("NullWithoutOptions", out var _), Is.True);
	        Assert.That(jElement.TryGetProperty("nullWithoutIgnoreNull", out var _), Is.True);
        }
		
        [TestCase(2, @"{""Name"":""Andrew"",""EmailAddress"":""a@b.com"",""IsAdmin"":true}")]
        [TestCase(1, @"{""Name"":""Andrew"",""IsAdmin"":true,""EmailAddress"":""a@b.com""}")]
        [TestCase(0, @"{""IsAdmin"":true,""Name"":""Andrew"",""EmailAddress"":""a@b.com""}")]
        public void InsertProperty_adds_property_at_correct_index(int insertAt, string expected)
        {
            var jElement = GetJsonElement();
            jElement = jElement.InsertProperty("IsAdmin", true, insertAt);
			Assert.That(jElement.ToString(), Is.EqualTo(expected));
        }

		[TestCase(3)]
		[TestCase(99)]
		[TestCase(-1)]
        public void InsertProperty_throws_if_insertAt_is_larger_than_the_length_of_the_list(int index)
        {
            var jElement = GetJsonElement();
            Assert.Throws<ArgumentOutOfRangeException>(() => jElement.InsertProperty("IsAdmin", true, index));
        }
		
		[TestCase(0, "{\"ExtraProp\":true,\"Name\":\"Andrew\",\"EmailAddress\":\"a@b.com\"}")]
		[TestCase(1, "{\"Name\":\"Andrew\",\"ExtraProp\":true,\"EmailAddress\":\"a@b.com\"}")]
		[TestCase(2, "{\"Name\":\"Andrew\",\"EmailAddress\":\"a@b.com\",\"ExtraProp\":true}")]
        public void InsertProperty_should_add_properties_that_can_be_asserted_in_the_output(int index, string expectedJson)
        {
            var jElement = GetJsonElement();

            jElement = jElement.InsertProperty("ExtraProp", true, index);

            Assert.That(jElement.ToString(), Is.EqualTo(expectedJson));
		}
		
        [Test]
        public void InsertProperty_should_respect_json_options()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true
            };

            var jElement = GetJsonElement();

            var obj = new
            {
                Hello = "hello",
                World = (string)null
            };

            jElement = jElement
                .InsertProperty("withOptions", obj, 0, options)
                .InsertProperty("withoutOptions", obj, 0)
                .InsertNullProperty("NullWithOptions", 0, options)
                .InsertNullProperty("NullWithoutOptions", 0)
                .InsertNullProperty("NullWithoutIgnoreNull", 0,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var withOptions = jElement.GetProperty("withOptions");
            var withoutOptions = jElement.GetProperty("withoutOptions");

            Assert.That(withOptions.TryGetProperty("hello", out var _), Is.True);
            Assert.That(withOptions.TryGetProperty("Hello", out var _), Is.Not.True);
            Assert.That(withOptions.TryGetProperty("world", out var _), Is.Not.True);

            Assert.That(withoutOptions.TryGetProperty("Hello", out var _), Is.True);
            Assert.That(withoutOptions.TryGetProperty("hello", out var _), Is.Not.True);
            Assert.That(withoutOptions.TryGetProperty("World", out var _), Is.True);

            Assert.That(jElement.TryGetProperty("nullWithOptions", out var _), Is.Not.True);
            Assert.That(jElement.TryGetProperty("nullWithoutOptions", out var _), Is.True);
            Assert.That(jElement.TryGetProperty("nullWithoutIgnoreNull", out var _), Is.True);
		}
		
		[TestCase("Name", "Bob", "{\"Name\":\"Bob\",\"EmailAddress\":\"a@b.com\"}")]
		[TestCase("Name", 1234, "{\"Name\":1234,\"EmailAddress\":\"a@b.com\"}")]
		[TestCase("Name", true, "{\"Name\":true,\"EmailAddress\":\"a@b.com\"}")]
		[TestCase("Name", null, "{\"Name\":null,\"EmailAddress\":\"a@b.com\"}")]
		public void UpdateProperty_should_update_properties(string name, object updatedValue, string expectedJson)
        {
            var jElement = GetJsonElement();
            jElement = jElement.UpdateProperty(name, updatedValue);
            Assert.That(jElement.ToString(), Is.EqualTo(expectedJson));
		}

		[Test]
        public void UpdateProperty_should_respect_json_options_ignore_null_values()
        {
            var jElement = GetJsonElement();
            jElement = jElement.UpdateProperty("Name", null, new JsonSerializerOptions { IgnoreNullValues = true});
            Assert.That(jElement.ToString(), Is.EqualTo("{\"EmailAddress\":\"a@b.com\"}"));
        }

        [Test]
        public void UpdateProperty_should_respect_json_options_dont_ignore_null_values()
        {
            var jElement = GetJsonElement();
            jElement = jElement.UpdateProperty("Name", null, new JsonSerializerOptions { IgnoreNullValues = false });
            Assert.That(jElement.ToString(), Is.EqualTo("{\"Name\":null,\"EmailAddress\":\"a@b.com\"}"));
        }

		[Test]
        public void ParseAsJsonStringAndMutatePreservingOrder_method_should_be_able_to_reorder_properties()
        {
            var jElement = GetJsonElement();
            jElement = jElement.ParseAsJsonStringAndMutatePreservingOrder(props =>
            {
				props.Remove("Name");
				props.Insert("Name", "Andrew", 1);
            }, new JsonSerializerOptions());
            Assert.That(jElement.ToString(), Is.EqualTo("{\"EmailAddress\":\"a@b.com\",\"Name\":\"Andrew\"}"));
		}

        [Test]
		public void ParseAsJsonStringAndMutate_method_should_add_properties_that_can_be_asserted_in_the_output()
		{
            var jElement = GetJsonElement();

            jElement = jElement.ParseAsJsonStringAndMutate((utf8JsonWriter1, namesOfPropertiesToRemove) => 
			{
				namesOfPropertiesToRemove.Add("EmailAddress");
				utf8JsonWriter1.WriteBoolean("IsAdmin", true);
			});

			Assert.That(jElement.GetProperty("IsAdmin").ToString(), Is.EqualTo(true.ToString()));
			Assert.Throws<KeyNotFoundException>(() => jElement.GetProperty("EmailAddress"));
		}
		
        [Test]
        public void ParseAsJsonStringAndMutatePreservingOrder_method_should_add_properties_that_can_be_asserted_in_the_output()
        {
			var jElement = GetJsonElement();

			jElement = jElement.ParseAsJsonStringAndMutatePreservingOrder(props =>
            {
                props.Remove("EmailAddress");
				props.Insert("IsAdmin", true, 0);
			}, new JsonSerializerOptions());

            Assert.That(jElement.GetProperty("IsAdmin").ToString(), Is.EqualTo(true.ToString()));
            Assert.Throws<KeyNotFoundException>(() => jElement.GetProperty("EmailAddress"));
        }

		[Test]
		public void RemoveProperty_methods_should_remove_properties_from_the_output()
		{
            var jElement = GetJsonElement();

			jElement = jElement
				.RemoveProperty("EmailAddress");

			Assert.Throws<KeyNotFoundException>(() => jElement.GetProperty("EmailAddress"));
		}

		[Test]
		public void RemoveProperties_methods_should_remove_properties_from_the_output()
		{
            var jElement = GetJsonElementWithThreeProperties();

			jElement = jElement.RemoveProperties(new List<string> { "EmailAddress", "Age" });

			Assert.Throws<KeyNotFoundException>(() => jElement.GetProperty("EmailAddress"));
			Assert.Throws<KeyNotFoundException>(() => jElement.GetProperty("Age"));
		}

        [Test] public void AddProperty_method_should_handle_primitive_style_objects_cast_as_objects()
        {
			var jElement = GetJsonElementWithThreeProperties();

			var stringCastToObject = (object) "3jk4h5gkj3hg45kjh4g5";
			
            jElement = jElement.AddProperty("Id", stringCastToObject);

			Assert.That(jElement.GetProperty("Id").ToString(), Is.EqualTo("3jk4h5gkj3hg45kjh4g5"));
		}
        
        [Test] public void JsonElement_ConvertToObject_method_should_return_the_specified_object()
        {
			var jElement = GetJsonElementWithThreeProperties();

			var result = jElement.ConvertToObject<TestClass>();
	        Assert.That(result, Is.Not.Null);
	        Assert.That(result.Name, Is.EqualTo("Andrew"));
	        Assert.That(result.EmailAddress, Is.EqualTo("a@b.com"));
        }
        
        [Test] public void JsonDocument_ConvertToObject_method_should_return_the_specified_object()
        {
	        const string jsonString = "{ \"Name\": \"Andrew\", \"EmailAddress\": \"a@b.com\", \"Age\": 38 }";
	        var jDoc = JsonDocument.Parse(jsonString);
	        
	        var result = jDoc.ConvertToObject<TestClass>();
	        Assert.That(result, Is.Not.Null);
	        Assert.That(result.Name, Is.EqualTo("Andrew"));
	        Assert.That(result.EmailAddress, Is.EqualTo("a@b.com"));
        }

        private static JsonElement GetJsonElement()
        {
            const string jsonString = "{ \"Name\": \"Andrew\", \"EmailAddress\": \"a@b.com\" }";
            var jElement = JsonDocument.Parse(jsonString).RootElement;
            return jElement;
        }

        private static JsonElement GetJsonElementWithThreeProperties()
        {
			const string jsonString = "{ \"Name\": \"Andrew\", \"EmailAddress\": \"a@b.com\", \"Age\": 38 }";
			var jElement = JsonDocument.Parse(jsonString).RootElement;
            return jElement;
        }
	}

    public class TestClass
    {
	    public string Name { get; set; }
	    public string EmailAddress { get; set; }
    }
}