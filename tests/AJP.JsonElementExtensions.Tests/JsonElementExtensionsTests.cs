using NUnit.Framework;
using System;
using System.Linq;
using System.Text.Json;

namespace AJP.JsonElementExtensions.Tests
{
	[TestFixture]
    public class JsonElementExtensionsTests
	{       
        [Test]
        public void Various_AddProperty_methods_should_add_properties_that_can_be_asserted_in_the_output()
        {
			// get a JsonElement to start with...
			var jsonString = "{ \"Name\": \"Andrew\", \"EmailAddress\": \"andrewjpoole@gmail.com\" }";
			var jElement = JsonDocument.Parse(jsonString).RootElement;

			jElement = jElement
				.AddProperty("Age", 38)
				.AddProperty("Male", true)
				.AddProperty("Female", false)
				.AddNullProperty("Alien")
				.AddProperty("Roles", new string[] { "admin", "user" })
				.AddProperty("LastUpdated", new DateTime(2020, 2, 27, 22, 09, 00))
				.AddProperty("crazyNewObject", new
					{
						Name = "Hobbies",
						Value = "bass guitar and writing c# code"
					});

			Assert.That(jElement.GetProperty("Age").ToString(), Is.EqualTo("38"));
			Assert.That(jElement.GetProperty("Male").GetBoolean(), Is.EqualTo(true));
			Assert.That(jElement.GetProperty("Female").GetBoolean(), Is.EqualTo(false));
			Assert.That(jElement.GetProperty("Alien").GetString(), Is.EqualTo(null));
			Assert.That(jElement.GetProperty("Roles").GetArrayLength(), Is.EqualTo(2));
			Assert.That(jElement.GetProperty("Roles").EnumerateArray().FirstOrDefault().GetString(), Is.EqualTo("admin"));
			Assert.That(jElement.GetProperty("LastUpdated").GetString(), Is.EqualTo(new DateTime(2020, 2, 27, 22, 09, 00).ToString("s")));
			Assert.That(jElement.GetProperty("crazyNewObject").EnumerateObject().FirstOrDefault().Value.ToString(), Is.EqualTo("Hobbies"));
        }
    }
}