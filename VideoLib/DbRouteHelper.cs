using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAapp04
{
    public static class DbRouteHelper
    {

        public static string CapitalizeAllWords(string s)
        {
            var sb = new StringBuilder(s.Length);
            bool inWord = false;
            foreach (char c in s)
            {

                if (char.IsLetter(c) == true && c != 'I')
                {


                    sb.Append(inWord ? char.ToLower(c) : char.ToUpper(c));
                    inWord = true;
                }
                else
                {
                    sb.Append(c);
                    inWord = false;
                }
            }
            return sb.ToString();
        }

        public static string ConvertEngToRus(string input)
        {
            const string English = "qwertyuiop[]asdfghjkl;'zxcvbnm,.";
            string English2 = English.ToUpper();
            const string Russian = "йцукенгшщзхъфывапролджэячсмитьбю";

            var result = new StringBuilder(input.Length);
            int index;
            foreach (var symbol in input)
                result.Append((index = English.IndexOf(symbol)) != -1 ? Russian[index] : symbol);

            input = "";
            input = result.ToString();

            var result2 = new StringBuilder(input.Length);
            foreach (var symbol in input)
                result2.Append((index = English2.IndexOf(symbol)) != -1 ? Russian[index] : symbol);

            return result2.ToString();
        }
    }
}
