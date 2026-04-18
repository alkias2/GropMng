using System.Text;

namespace GropMng.Core
{
    public class CommonHelper
    {
        /// <summary>
        /// Converts an enum value to a more human-readable string by inserting spaces before capital letters.
        /// For example, "MyEnumValue" becomes "My Enum Value".
        /// </summary>
        /// <param name="enumValue">The enum value to convert.</param>
        /// <returns>A human-readable string representation of the enum value.</returns>
        public static string ConvertEnum(string? enumValue)
        {
            if (string.IsNullOrWhiteSpace(enumValue))
                return string.Empty;
            var stringBuilder = new StringBuilder();
            foreach (var character in enumValue)
            {
                if (char.IsUpper(character) && stringBuilder.Length > 0)
                    stringBuilder.Append(' ');
                stringBuilder.Append(character);
            }
            return stringBuilder.ToString();
        }
    }
}
