using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irab
{
    public class AyahIrab
    {
        public string Id { get; set; }
        public int[] Location { get; set; }
        public string Arabic { get; set; }
        public string Grammar { get; set; }
        public List<WordGroupIrab> WordGroups { get; set; }
    }

    public class WordGroupIrab
    {
        public int[] Location { get; set; }
        public string Words { get; set; }
        public string Grammar { get; set; }
    }
}
