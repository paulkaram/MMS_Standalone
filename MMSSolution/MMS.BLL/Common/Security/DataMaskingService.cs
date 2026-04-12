using System.Text.RegularExpressions;

namespace MMS.BLL.Common.Security
{
    /// <summary>
    /// Data masking service for protecting Personally Identifiable Information (PII).
    /// DCC Compliance (NCA DCC-1:2022 Section 2-4): Implements data masking for sensitive information in audit logs.
    /// </summary>
    public interface IDataMaskingService
    {
        /// <summary>
        /// Masks sensitive data in a string based on configured patterns.
        /// </summary>
        string MaskSensitiveData(string input);

        /// <summary>
        /// Masks an email address (e.g., "user@domain.com" -> "u***@d***.com")
        /// </summary>
        string MaskEmail(string email);

        /// <summary>
        /// Masks a phone number (e.g., "+966501234567" -> "+966*****4567")
        /// </summary>
        string MaskPhoneNumber(string phoneNumber);

        /// <summary>
        /// Masks a national ID (e.g., "1234567890" -> "1******890")
        /// </summary>
        string MaskNationalId(string nationalId);

        /// <summary>
        /// Masks a credit card number (e.g., "4111111111111111" -> "4111********1111")
        /// </summary>
        string MaskCreditCard(string cardNumber);

        /// <summary>
        /// Masks sensitive fields in a dictionary of parameters for audit logging.
        /// </summary>
        IDictionary<string, object?> MaskAuditParameters(IDictionary<string, object?> parameters);
    }

    /// <summary>
    /// Implementation of data masking service.
    /// DCC Compliance (NCA DCC-1:2022 Section 2-4): Masks PII in audit logs and sensitive operations.
    /// </summary>
    public class DataMaskingService : IDataMaskingService
    {
        // Sensitive field names that should be masked in audit logs
        private static readonly HashSet<string> SensitiveFieldNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "password", "pwd", "secret", "token", "apikey", "api_key",
            "creditcard", "credit_card", "cardnumber", "card_number",
            "cvv", "cvc", "ssn", "nationalid", "national_id", "idnumber", "id_number",
            "phone", "phonenumber", "phone_number", "mobile", "mobilenumber",
            "email", "emailaddress", "email_address",
            "bankaccount", "bank_account", "iban", "accountnumber", "account_number",
            "privatekey", "private_key", "secretkey", "secret_key",
            "authorization", "bearer", "accesstoken", "access_token", "refreshtoken", "refresh_token"
        };

        // Regex patterns for detecting sensitive data
        private static readonly Regex EmailPattern = new(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);
        private static readonly Regex PhonePattern = new(@"(\+?\d{1,4}[-.\s]?)?\(?\d{1,4}\)?[-.\s]?\d{1,4}[-.\s]?\d{1,9}", RegexOptions.Compiled);
        private static readonly Regex CreditCardPattern = new(@"\b\d{4}[-\s]?\d{4}[-\s]?\d{4}[-\s]?\d{4}\b", RegexOptions.Compiled);
        private static readonly Regex NationalIdPattern = new(@"\b\d{10}\b", RegexOptions.Compiled);

        public string MaskSensitiveData(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = input;

            // Mask emails
            result = EmailPattern.Replace(result, match => MaskEmail(match.Value));

            // Mask credit cards
            result = CreditCardPattern.Replace(result, match => MaskCreditCard(match.Value.Replace("-", "").Replace(" ", "")));

            return result;
        }

        public string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains('@'))
                return email;

            var parts = email.Split('@');
            if (parts.Length != 2)
                return email;

            var localPart = parts[0];
            var domainParts = parts[1].Split('.');

            // Mask local part: keep first char, mask middle, keep last char if long enough
            var maskedLocal = localPart.Length > 2
                ? localPart[0] + new string('*', Math.Min(localPart.Length - 2, 3)) + localPart[^1]
                : localPart[0] + "***";

            // Mask domain: keep first char of each part
            var maskedDomain = string.Join(".", domainParts.Select(p =>
                p.Length > 1 ? p[0] + new string('*', Math.Min(p.Length - 1, 3)) : p));

            return $"{maskedLocal}@{maskedDomain}";
        }

        public string MaskPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return phoneNumber;

            // Remove non-digit characters for processing
            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

            if (digits.Length < 4)
                return new string('*', phoneNumber.Length);

            // Keep country code (if present) and last 4 digits
            var hasPlus = phoneNumber.StartsWith('+');
            var prefix = hasPlus ? "+" : "";

            if (digits.Length > 8)
            {
                // Assume first 3-4 digits are country/area code
                var codeLength = digits.Length > 10 ? 3 : 2;
                return prefix + digits[..codeLength] + new string('*', digits.Length - codeLength - 4) + digits[^4..];
            }

            return prefix + new string('*', digits.Length - 4) + digits[^4..];
        }

        public string MaskNationalId(string nationalId)
        {
            if (string.IsNullOrEmpty(nationalId))
                return nationalId;

            var digits = new string(nationalId.Where(char.IsDigit).ToArray());

            if (digits.Length < 4)
                return new string('*', nationalId.Length);

            // Keep first digit (ID type indicator) and last 3 digits
            return digits[0] + new string('*', digits.Length - 4) + digits[^3..];
        }

        public string MaskCreditCard(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return cardNumber;

            var digits = new string(cardNumber.Where(char.IsDigit).ToArray());

            if (digits.Length < 8)
                return new string('*', cardNumber.Length);

            // Keep first 4 and last 4 digits (PCI DSS compliant)
            return digits[..4] + new string('*', digits.Length - 8) + digits[^4..];
        }

        public IDictionary<string, object?> MaskAuditParameters(IDictionary<string, object?> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return parameters;

            var maskedParams = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var param in parameters)
            {
                if (IsSensitiveField(param.Key))
                {
                    // Completely mask sensitive fields
                    maskedParams[param.Key] = "[REDACTED]";
                }
                else if (param.Value is string stringValue)
                {
                    // Check for embedded sensitive data in string values
                    maskedParams[param.Key] = MaskSensitiveData(stringValue);
                }
                else if (param.Value is IDictionary<string, object?> nestedDict)
                {
                    // Recursively mask nested dictionaries
                    maskedParams[param.Key] = MaskAuditParameters(nestedDict);
                }
                else
                {
                    maskedParams[param.Key] = param.Value;
                }
            }

            return maskedParams;
        }

        private static bool IsSensitiveField(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                return false;

            // Check if field name contains any sensitive keywords
            var normalizedName = fieldName.Replace("_", "").Replace("-", "");
            return SensitiveFieldNames.Any(sensitive =>
                normalizedName.Contains(sensitive, StringComparison.OrdinalIgnoreCase));
        }
    }
}
