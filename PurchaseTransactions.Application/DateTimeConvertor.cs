using System.Text.Json;
using System.Text.Json.Serialization;

namespace PurchaseTransactions.Application
{
	public class DateTimeConvertor : JsonConverter<DateTime>
	{
		private readonly string _format = "yyyy-MM-dd HH:mm:ss";
		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var dateString = reader.GetString();
			if(DateTime.TryParseExact(dateString, _format, null, System.Globalization.DateTimeStyles.None, out DateTime date))
			{
				return date;
			}
			if(DateTime.TryParse(dateString, out date))
			{
				return date;
			}
			throw new JsonException($"Unable to convert \"{dateString}\" to DateTime.");
			
		}



	   public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString(_format));
		}
	}
}
