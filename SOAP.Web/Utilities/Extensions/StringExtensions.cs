namespace SOAP.Web.Utilities.Extensions
{
    public static class StringExtensions
    {
        public static bool IsValidPhoneNumber(this string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Remove spaces and common separators
            var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            // Check if it's a valid Kenyan phone number
            if (cleaned.StartsWith("254") && cleaned.Length == 12)
                return cleaned.All(char.IsDigit);

            if (cleaned.StartsWith("0") && cleaned.Length == 10)
                return cleaned.All(char.IsDigit);

            return false;
        }

        public static string FormatPhoneNumber(this string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return phoneNumber;

            var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            if (cleaned.StartsWith("0") && cleaned.Length == 10)
            {
                return "254" + cleaned.Substring(1);
            }

            return cleaned;
        }

        public static bool IsValidKcpeNumber(this string kcpeNumber)
        {
            if (string.IsNullOrWhiteSpace(kcpeNumber))
                return false;

            // KCPE numbers are typically 11 digits
            return kcpeNumber.Length == 11 && kcpeNumber.All(char.IsDigit);
        }

        public static string ToTitleCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }

        public static string Truncate(this string input, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length <= maxLength)
                return input;

            return input.Substring(0, maxLength) + "...";
        }
    }
}