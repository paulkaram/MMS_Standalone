using System.Globalization;

namespace Intalio.Tools.Common.Extensions.DateTimeExtensions
{
	public static class DateTimeExtensions
	{
		public static string ToHijri(this DateTime dateGregorian, string format = "dd/MM/yyyy")
		{
			DateTimeFormatInfo formatInfo = new CultureInfo("ar-sa", true).DateTimeFormat;
			formatInfo.Calendar = new UmAlQuraCalendar();

			return dateGregorian.Date.ToString(format, formatInfo);
		}

		public static int GetHijriYear(this DateTime dateGregorian)
		{
			return new UmAlQuraCalendar().GetYear(dateGregorian);
		}

		public static int GetHijriMonth(this DateTime dateGregorian)
		{
			return new UmAlQuraCalendar().GetMonth(dateGregorian);
		}

		public static DateTime ResolveDateAsGregorian(this DateTime dateHijri)
		{
			if(dateHijri.Year < 1700)
			{
				DateTime dateInHijriFormat = new DateTime(dateHijri.Year, dateHijri.Month, dateHijri.Day, new UmAlQuraCalendar());

				Calendar gregorian = new GregorianCalendar();
				return new DateTime(gregorian.GetYear(dateInHijriFormat), gregorian.GetMonth(dateInHijriFormat), gregorian.GetDayOfMonth(dateInHijriFormat));
			}

			return dateHijri;
		}

		public static string ToStringGregorian(this DateTime dateGregorian, string format = "yyyy-MM-dd")
		{
			return dateGregorian.ToString(format);
		}

		public static DateTime ToGregorianDate(this string hijri, string format = "dd/MM/yyyy")
		{
			return DateTime.ParseExact(hijri, format, new CultureInfo("ar-sa"));
		}

		public static DateTime ToDateTime(this string gregorian, string format = "dd/MM/yyyy")
		{
			return DateTime.ParseExact(gregorian, format, new CultureInfo("en-us"));
		}
	}
}
