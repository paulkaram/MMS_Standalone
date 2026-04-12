using DocumentFormat.OpenXml.Presentation;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Intalio.Tools.Common.Extensions.StringExtensions
{
    public static class StringExtensions
    {
        public static string PrefixSlash(this string input)
        {
            char slash = input.Contains('\\') ? '\\' : '/';

            return !string.IsNullOrEmpty(input)
                ? input.StartsWith(slash) ? input : $"{slash}{input}"
                : $"{slash}";
        }

        public static string SuffixSlash(this string input)
        {
            char slash = input.Contains('\\') ? '\\' : '/';

            return !string.IsNullOrEmpty(input)
                ? input.EndsWith(slash) ? input : $"{input}{slash}"
                : $"{slash}";
        }

        public static bool IsPasswordComplex(this string password)
        {
            return password.Length >= 8
                && !new Regex(@"\s+").Match(password).Success
                && new Regex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[~!@#$%^*\-_=+[{\]};:,.?]).*$").Match(password).Success;
        }

        public static string SuffixQuestionMark(this string input)
        {
            return !string.IsNullOrEmpty(input)
                ? input.EndsWith("?") ? input : $"{input}?"
                : "?";
        }

        public static bool IsInteger(this string input)
        {
            return int.TryParse(input, out int n);
        }

        public static string ToArabicDigits(this string input)
        {
            return input.Replace("1", "١")
                       .Replace("2", "٢")
                       .Replace("3", "٣")
                       .Replace("4", "٤")
                       .Replace("5", "٥")
                       .Replace("6", "٦")
                       .Replace("7", "٧")
                       .Replace("8", "٨")
                       .Replace("9", "٩")
                       .Replace("0", "٠");
        }

        public static string FormatTo12Hour(this string time)
        {
            if (string.IsNullOrWhiteSpace(time))
                return string.Empty;

            var parts = time.Split(':').Select(int.Parse).ToArray();
            int hour = parts[0];
            int minute = parts[1];
            string period = hour >= 12 ? "PM" : "AM";

            int formattedHour = hour % 12 == 0 ? 12 : hour % 12;
            return $"{formattedHour}:{minute:D2} {period}";
        }
        /// <summary>
        /// This method is applicable for Saudi Arabia mobile numbers only
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="clean"></param>
        /// <returns></returns>
        public static bool TryCleanMobileSA(this string mobile, out string clean)
        {
            clean = string.Empty;
            if (!string.IsNullOrEmpty(mobile))
            {
                mobile = Regex.Replace(mobile, "[^0-9]+", "");
                //Remove NAN characters "[^0-9]+"
                //search the first occurence of 5
                if (Regex.Match(mobile, "^0*(966)?0*5").Success)
                {
                    int firstFive = Regex.Match(mobile, "5").Index;
                    mobile = "966" + mobile.Substring(firstFive, mobile.Length - firstFive);
                    if (Regex.Match(mobile, @"9665\d{8}$").Success)
                    {
                        clean = mobile;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// A string is considered complex if it meets  all the following rules:
        /// <para>1. must have 8 or more characters</para>
        /// <para>2. must contain at least 1 uppercase letter</para>
        /// <para>3. must contain at least 1 lowercase letter</para>
        /// <para>4. must contain at least 1 special character: ~!@#$%^*\-_=+[{\]};:,.?</para>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsComplexString(this string input)
        {
            string pattern = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[~!@#$%^*\-_=+[{\]};:,.?]).*$";

            return input.Length >= 8
                && !new Regex(@"\s+").Match(input).Success
                && new Regex(pattern).Match(input).Success;
        }

        public static byte[]? GetFromCleanBase64(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }
            string val = input.Contains("base64,") ? input.Split("base64,")[1] : input;
            try
            {
                return Convert.FromBase64String(val);
            }
            catch (Exception)
            {
                return null;
            }

        }

        public static string GenerateSalt(this Int32 nSalt)
        {
            var saltBytes = new byte[nSalt];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }


        public static List<int> GetIntegersFromString(this string input)
        {
            List<int> retVal = new List<int>();
            string[] values = input.Split(',');
            for (int i = 0; i < values.Length; i++)
            {
                retVal.Add(Convert.ToInt32(values[i].Trim()));
            }
            return retVal;
        }

    }
}
