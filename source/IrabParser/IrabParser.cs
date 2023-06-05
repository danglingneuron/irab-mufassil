using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Irab
{
    public class IrabParser
    {
        const string LineBreak = "\n"; //<br>
        string[] ArabicNumerals = new string[]
        {
            "٠","١","٢","٣","٤","٥","٦","٧","٨","٩"
        };
        int ConvertArabicNumeral(char[] input)
        {
            int output = 0;
            int power = 0;
            for (int i = 0; i < input.Length; i++)
            {
                int digit = Array.IndexOf<string>(ArabicNumerals, input[i].ToString());
                if (digit == -1)
                {
                    continue;
                }
                output += digit * (int)Math.Pow(10, power);
                power++;
            }
            return output;
        }

        int surahId = 0;
        int ayahId = 0;
        public List<AyahIrab> Parse(string rootPath)
        {
            List<AyahIrab> irabAyahs = new List<AyahIrab>();

            for (int vi = 1; vi <= 12; vi++)
            {
                string vname = rootPath + vi.ToString("000") + ".htm";
                string volume = File.ReadAllText(vname);
                var pages = ExtractPages(volume);
                AyahIrab? irabAyah = null;
                foreach (var page in pages)
                {
                    var lines = ExtractLines(page);
                   
                    for (int il = 0; il < lines.Length; il++)
                    {
                        var line = lines[il];
                        var rawLine = line.Trim();
                        if (string.IsNullOrEmpty(rawLine) || rawLine == "\u200C")
                        {
                            continue;
                        }
                        if (rawLine == "* * *" || rawLine == "انتهى الجزء الرابع ويليه الجزء الخامس"
                                || rawLine == "انتهى بفضل الله إعراب آل عمران ويليه سورة النساء ..")
                        {
                            irabAyah = null;
                            continue;
                        }
                        if (rawLine.StartsWith("[") && rawLine.Contains("سورة") && rawLine.Contains("آية"))
                        {
                            ayahId++;
                            irabAyah = new AyahIrab() {  WordGroups = new List<WordGroupIrab>() };
                            irabAyahs.Add(irabAyah);
                            var saText = rawLine.Replace("]", "").Replace("[", "").Replace("سورة ", "").Replace(" آية ", "").Replace("(", "").Replace(")", "");
                            var saSplit = saText.Split(" ");
                            var snumeral = saSplit[saSplit.Length - 1].Split(":");
                            var sid = ConvertArabicNumeral(snumeral[0].Reverse().ToArray());
                            var aid = ConvertArabicNumeral(snumeral[1].Reverse().ToArray());
                            surahId = sid;
                            irabAyah.Id = $"{sid}:{aid}";
                            irabAyah.Location = new[] { sid, aid };
                            continue;
                        }
                        if (Regex.IsMatch(rawLine, "(\\()?[٠١٢٣٤٥٦٧٨٩]+(\\))?\\s+{.+?}"))
                        {
                            ayahId++;
                            irabAyah = new AyahIrab() { WordGroups = new List<WordGroupIrab>() };
                            irabAyahs.Add(irabAyah);
                            var ayah = Regex.Replace(rawLine, "{.+?}", "").Replace("(", "").Replace(")", "").Replace(".", "").Replace("*", "").Trim();
                            var aid = ConvertArabicNumeral(ayah.Reverse().ToArray());
                            if (aid == 1)
                            {
                                surahId++;
                            }
                            var sid = surahId;
                            irabAyah.Id = $"{sid}:{aid}";
                            irabAyah.Location = new[] { sid, aid };
                            irabAyah.Arabic = rawLine;
                            continue;
                        }

                        if (irabAyah == null)
                            continue;


                        if (irabAyah.Id.EndsWith(":1"))
                        {
                            if (rawLine == "بِسْمِ اللَّهِ الرَّحْمنِ الرَّحِيمِ" || rawLine == "بِسْمِ اللَّهِ الرَّحْمَنِ الرَّحِيمِ.")
                            {
                                continue;
                            }
                            else if (rawLine.Contains("(١)"))
                            {
                                //skip first bismillah
                                if (rawLine != "بِسْمِ اللَّهِ الرَّحْمنِ الرَّحِيمِ (١)")
                                    rawLine = rawLine.Replace("بِسْمِ اللَّهِ الرَّحْمَنِ الرَّحِيمِ.", "").Replace("بِسْمِ اللَّهِ الرَّحْمنِ الرَّحِيمِ", "").Trim();
                            }

                        }
                        if (rawLine.Contains("("))
                        {
                            if (string.IsNullOrEmpty(irabAyah.Arabic))
                            {
                                irabAyah.Arabic = rawLine;
                                continue;
                            }
                        }
                        if (!string.IsNullOrEmpty(irabAyah.Arabic))
                        {
                            if (string.IsNullOrEmpty(irabAyah.Grammar))
                                irabAyah.Grammar = ProcessLine(rawLine);
                            else
                                irabAyah.Grammar += LineBreak + ProcessLine(rawLine);
                        }
                        if (rawLine.StartsWith("•"))
                        {
                            if (!rawLine.Contains(":"))
                            {
                                continue;
                            }
                            var wordGroup = ExtractWordGroup( rawLine, true);
                            if (wordGroup != null)
                                irabAyah.WordGroups.Add(wordGroup);
                            continue;
                        }
                        if (irabAyah.WordGroups?.Count > 0)
                        {
                            var currentWord = irabAyah.WordGroups.Last();
                            currentWord.Grammar += LineBreak + ProcessLine(rawLine);
                        }
                        else
                        {
                            if (!rawLine.Contains(":"))
                            {
                                continue;
                            }
                            var wordGroup = ExtractWordGroup(rawLine, false);
                            if (wordGroup != null)
                                irabAyah.WordGroups.Add(wordGroup);
                        }
                    }
                }
                irabAyah = null;
            }
            return irabAyahs;
        }

        
        string[] ExtractPages(string volumText)
        {
            var cleanText = volumText.Trim();
            cleanText = Regex.Replace(cleanText, "<!DOCTYPE(.|\\n)+?<body>", "");
            cleanText = Regex.Replace(cleanText, "<div class='Main'>", "");
            cleanText = Regex.Replace(cleanText, "</div></body></html>", "");
            var pages = cleanText.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return pages;
        }
        string[] ExtractLines(string pageText)
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
        WordGroupIrab ExtractWordGroup(string rawLine, bool dotted)
        {
            var colon = rawLine.IndexOf(":");
            string words = dotted? rawLine.Substring(1, colon - 1).Trim(): rawLine.Substring(0, colon).Trim();
            words = words.Replace("{", "").Replace("}", "").Replace("\u200D", "").Trim();
            return new WordGroupIrab() { Words = words, Grammar = ProcessLine(rawLine) };

        }
        string ProcessLine(string lineText)
        {
            return lineText.Trim();
        }
    }
}


