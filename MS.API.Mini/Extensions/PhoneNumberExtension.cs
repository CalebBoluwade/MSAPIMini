using System.Text.RegularExpressions;

namespace MS.API.Mini.Extensions;

public static partial class PhoneNumberExtension
{
    [GeneratedRegex(@"[^\d]")]
    private static partial Regex NonDigitRegex();

    public static string SanitizePhoneNumber(this string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
        {
            return string.Empty;
        }

        // Remove non-digit characters
        phoneNumber = NonDigitRegex().Replace(phoneNumber, "");

        if (phoneNumber.StartsWith("234") && phoneNumber.Length == 13)
        {
            // Already in correct format
            return phoneNumber;
        }

        if (phoneNumber.StartsWith('0') && phoneNumber.Length == 11)
        {
            // Remove leading 0, then add 234
            return "234" + phoneNumber[1..];
        }

        if (phoneNumber.Length == 10)
        {
            // Assume already missing leading 0, just add 234
            return "234" + phoneNumber;
        }

        return string.Empty;
    }
}