using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace VariableConverter
{
    internal class Converter
    {
        public static string ToTarget(string[] texts)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i] = texts[i].ToLower();
                if (texts.Length == 1)
                {
                    break;
                }
                if (i != 0)
                {
                    char c2 = texts[i][0];
                    texts[i] = $"{c2.ToString().ToUpper()}{texts[i].Substring(1, texts[i].Length - 1)}";
                }
            }
            return string.Join("", texts);
        }
    }
}
