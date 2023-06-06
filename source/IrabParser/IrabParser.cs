using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Irab
{
    public class IrabParser
    {
        const string LineBreak = "\n"; //<br>
        

        int surahId = 0;
        
        public List<AyahIrab> Parse(string rootPath)
        {
            List<AyahIrab> irabAyahs = new List<AyahIrab>();

            for (int vi = 1; vi <= 12; vi++)
            {
                string vname = rootPath + vi.ToString("000") + ".htm";
                string volume = File.ReadAllText(vname);
                var pages = Utils.ExtractPages(volume);
                AyahIrab? irabAyah = null;
                foreach (var page in pages)
                {
                    var lines = Utils.ExtractLines(page);
                   
                    foreach (var line in lines)
                    {
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
                        if(Regex.IsMatch(rawLine, "\\[.*?(سورة).*?(آية)")) 
                        {
                            irabAyah = new AyahIrab() {  WordGroups = new List<WordGroupIrab>() };
                            irabAyahs.Add(irabAyah);
                            var saText = rawLine.Replace("]", "").Replace("[", "").Replace("سورة ", "").Replace(" آية ", "").Replace("(", "").Replace(")", "");
                            var saSplit = saText.Split(" ");
                            var snumeral = saSplit[saSplit.Length - 1].Split(":");
                            irabAyah.Location = new[] { Utils.ConvertArabicNumeral(snumeral[0]), Utils.ConvertArabicNumeral(snumeral[1]) };
                            irabAyah.Id = $"{irabAyah.Location[0]}:{irabAyah.Location[1]}";
                            surahId = irabAyah.Location[0];
                            continue;
                        }
                        if (Regex.IsMatch(rawLine, "(\\()?[٠١٢٣٤٥٦٧٨٩]+(\\))?\\s+{.+?}"))
                        {
                            irabAyah = new AyahIrab() { WordGroups = new List<WordGroupIrab>() };
                            irabAyahs.Add(irabAyah);
                            var ayah = Regex.Replace(rawLine, "{.+?}", "").Replace("(", "").Replace(")", "").Replace(".", "").Replace("*", "").Trim();
                            var aid = Utils.ConvertArabicNumeral(ayah);
                            if (aid == 1)
                            {
                                surahId++;
                            }
                            irabAyah.Id = $"{surahId}:{aid}";
                            irabAyah.Location = new[] { surahId, aid };
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
                                irabAyah.Grammar = rawLine;
                            else
                                irabAyah.Grammar += LineBreak + rawLine;
                        }
                        if (rawLine.StartsWith("•"))
                        {
                            if (!rawLine.Contains(":"))
                            {
                                continue;
                            }
                            var wordGroup = Utils.ExtractWordGroup( rawLine, true);
                            if (wordGroup != null)
                                irabAyah.WordGroups.Add(wordGroup);
                            continue;
                        }
                        if (irabAyah.WordGroups?.Count > 0)
                        {
                            var currentWord = irabAyah.WordGroups.Last();
                            currentWord.Grammar += LineBreak + rawLine;
                        }
                        else
                        {
                            if (!rawLine.Contains(":"))
                            {
                                continue;
                            }
                            var wordGroup = Utils.ExtractWordGroup(rawLine, false);
                            if (wordGroup != null)
                                irabAyah.WordGroups.Add(wordGroup);
                        }
                    }
                }
                irabAyah = null;
            }
            return irabAyahs;
        }

        
        
    }
}


