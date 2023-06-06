using System.Text.RegularExpressions;

namespace Irab
{
    public class Utils
    {
        static string[] ArabicNumerals = new string[]
        {
            "٠","١","٢","٣","٤","٥","٦","٧","٨","٩"
        };
        
        public static int ConvertArabicNumeral(string arabic)
        {
            var input = arabic.Reverse().ToArray();
            int output = 0;
            int power = 0;
            for (int i = 0; i < input.Length; i++)
            {
                int digit = Array.IndexOf<string>(ArabicNumerals, input[i].ToString());
                if (digit == -1)
                {
                    throw new ArgumentException("error.arabicnumeral");
                }
                output += digit * (int)Math.Pow(10, power);
                power++;
            }
            return output;
        }

        public static string[] ExtractPages(string volumText)
        {
            var cleanText = volumText.Trim();
            cleanText = Regex.Replace(cleanText, "<!DOCTYPE(.|\\n)+?<body>", "");
            cleanText = Regex.Replace(cleanText, "<div class='Main'>", "");
            cleanText = Regex.Replace(cleanText, "</div></body></html>", "");
            var pages = cleanText.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return pages;
        }
        public static string[] ExtractLines(string pageText)
        {
            var cleanText = pageText.Replace("\n", "").Trim();
            cleanText = Regex.Replace(cleanText, "<div class='PageHead'>.+?<\\/div>", "");
            cleanText = Regex.Replace(cleanText, "<span class=\"title\">.+?<\\/span>", "");
            cleanText = Regex.Replace(cleanText, "<font.+?>|<\\/font>", "");
            cleanText = Regex.Replace(cleanText, "<div class='PageText'>", "");
            cleanText = Regex.Replace(cleanText, "</div>", "");
            cleanText = Regex.Replace(cleanText, "</p>", "\n");
            cleanText = Regex.Replace(cleanText, "]", "]\n");
            var lines = cleanText.Trim().Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return lines;
        }
        public static WordGroupIrab ExtractWordGroup(string rawLine, bool dotted)
        {
            var colon = rawLine.IndexOf(":");
            string words = dotted ? rawLine.Substring(1, colon - 1).Trim() : rawLine.Substring(0, colon).Trim();
            words = words.Replace("{", "").Replace("}", "").Replace("\u200D", "").Trim();
            return new WordGroupIrab() { Words = words, Grammar = rawLine };
        }
        
    }
}