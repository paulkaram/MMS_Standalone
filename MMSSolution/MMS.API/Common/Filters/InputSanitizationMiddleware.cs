using Microsoft.Extensions.Primitives;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace MMS.API.Common.Filters
{
	public class InputSanitizationMiddleware
	{
		private readonly RequestDelegate _next;

		public InputSanitizationMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			// 1. Sanitize Query Parameters
			if (context.Request.Query.Any())
			{
				var sanitizedQuery = new Dictionary<string, StringValues>();

				foreach (var param in context.Request.Query)
				{
					sanitizedQuery[param.Key] = SanitizeInput(param.Key, param.Value);
				}

				context.Request.Query = new QueryCollection(sanitizedQuery);
			}


			// 3. Handle Form Content
			if (context.Request.HasFormContentType)
			{
				var form = await context.Request.ReadFormAsync();
				var sanitizedForm = new Dictionary<string, StringValues>();

				foreach (var field in form)
				{
					// Sanitize each form field
					sanitizedForm[field.Key] = SanitizeInput(field.Key,field.Value);
				}

				context.Request.Body = new MemoryStream();
				var writer = new StreamWriter(context.Request.Body);
				await writer.WriteAsync(string.Join("&", sanitizedForm.Select(x => $"{x.Key}={x.Value}")));
				await writer.FlushAsync();
				context.Request.Body.Seek(0, SeekOrigin.Begin);
			}
            // 4. Sanitize JSON Content
            else if (context.Request.ContentType?.Contains("application/json") == true && context.Request.Body.CanRead)
            {
                context.Request.EnableBuffering();
                var originalBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Seek(0, SeekOrigin.Begin);

                if (!string.IsNullOrEmpty(originalBody))
                {
                    try
                    {
                        var jsonElement = JsonSerializer.Deserialize<JsonElement>(originalBody);

                        if (jsonElement.ValueKind == JsonValueKind.Object)
                        {
                            // Handle JSON object
                            var jsonObject = JsonSerializer.Deserialize<Dictionary<string, object>>(originalBody);
                            var sanitizedJson = SanitizeJson(jsonObject);
                            WriteSanitizedJsonToRequest(context, sanitizedJson);
                        }
                        else if (jsonElement.ValueKind == JsonValueKind.String)
                        {
                            // Handle JSON string
                            var sanitizedString = SanitizeString(jsonElement.GetString());
                            WriteSanitizedJsonToRequest(context, sanitizedString);
                        }
                        else
                        {
                            // Handle other JSON types if needed
                        }
                    }
                    catch (JsonException)
                    {
                        // Handle invalid JSON gracefully
                    }
                }
            }

            await _next(context);
		}
        void WriteSanitizedJsonToRequest(HttpContext context, object sanitizedContent)
        {
            var sanitizedJsonString = JsonSerializer.Serialize(sanitizedContent);
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(sanitizedJsonString));
            context.Request.ContentLength = context.Request.Body.Length;
            context.Request.Body.Seek(0, SeekOrigin.Begin);
        }
        private Dictionary<string, object> SanitizeJson(Dictionary<string, object> jsonObject)
		{
			var sanitizedJson = new Dictionary<string, object>();

			foreach (var key in jsonObject.Keys)
			{
				if (jsonObject[key] is JsonElement element)
				{
					switch (element.ValueKind)
					{
						case JsonValueKind.String:
							sanitizedJson[key] = SanitizeInput(key,element.GetString());
							break;
						case JsonValueKind.Object:
							var nestedObject = JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText());
							sanitizedJson[key] = SanitizeJson(nestedObject);
							break;
						case JsonValueKind.Array:
							var arrayElements = JsonSerializer.Deserialize<List<object>>(element.GetRawText());
							sanitizedJson[key] = arrayElements?.Select(item =>
								item is JsonElement nestedElement && nestedElement.ValueKind == JsonValueKind.Object
									? SanitizeJson(JsonSerializer.Deserialize<Dictionary<string, object>>(nestedElement.GetRawText()))
									: item is string strItem ? SanitizeInput(key,strItem) : item).ToList();
							break;
						default:
							sanitizedJson[key] = jsonObject[key];
							break;
					}
				}
				else if (jsonObject[key] is string stringValue)
				{
					sanitizedJson[key] = SanitizeInput(key, stringValue);
				}
				else
				{
					sanitizedJson[key] = jsonObject[key];
				}
			}

			return sanitizedJson;
		}


        string SanitizeString(string input)
        {
            string pattern = @"[^\p{L}\p{N}\s_\-:@.]";  // Allow letters, numbers, whitespace, and underscore , Allow valid email and URLs



            string sanitizedInput = Regex.Replace(input, pattern, string.Empty);
            string htmlEncodedInput = System.Net.WebUtility.HtmlEncode(sanitizedInput);
            return htmlEncodedInput;
        }

        private string SanitizeInput(string key,string input)
		{
			if (key.Contains("password", StringComparison.OrdinalIgnoreCase)|| key.Contains("token", StringComparison.OrdinalIgnoreCase))
			{
				return input;
			}
			string pattern = @"[^\p{L}\p{N}\s_\-:@.]";  // Allow letters, numbers, whitespace, and underscore , Allow valid email and URLs
			if (key.Contains("date", StringComparison.OrdinalIgnoreCase))
			{
				 pattern = @"[^\p{L}\p{N}\s_\-:.TzZ/]"; // Dates Patterns allow dot and : and -
			}
			

			string sanitizedInput = Regex.Replace(input, pattern, string.Empty);
			string htmlEncodedInput = System.Net.WebUtility.HtmlEncode(sanitizedInput);
			return htmlEncodedInput;
		}
	}

}
