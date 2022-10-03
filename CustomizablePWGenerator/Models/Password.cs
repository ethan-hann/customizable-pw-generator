using System.Text;
using System.Text.RegularExpressions;

namespace CustomizablePWGenerator.Models
{
    public class Password
    {
        /// <summary>
        /// The template text that defines this password.
        /// </summary>
        public string TemplateText { get; set; } = string.Empty;

        ///// <summary>
        ///// The regular expression that defines this password output.
        ///// </summary>
        //public string Regex { get; set; } = string.Empty;

        public string GeneratedPassword { get; set; } = string.Empty;

        public PWParser Parser { get; set; }
        
        
        /// <summary>
        /// Generates a password based on <see cref="TemplateText"/>.
        /// </summary>
        /// <returns>An instance of the generated password.</returns>
        public Password GeneratePassword()
        {
            StringBuilder str = new StringBuilder();
            Parser = new PWParser(TemplateText);
            //if (Parser.hasSpecial)
            //{
            //    str.Append(string.Join(", ", Parser.specials.Select(pair => $"{pair.Key} => " +
            //    $"{{{pair.Value.Select(innerPair => $"{innerPair.Key} => {innerPair.Value}")}}}")));
            //}
            ////str = str.Append($"number of specials: {parser.numOfSpecials}");
            //GeneratedPassword = str.ToString();
            return this;
        }



        /// <summary>
        /// Select a random subset of special characters based on the specials list specified./>
        /// </summary>
        /// <param name="speciaList">The list of specials as a string.</param>
        /// <param name="count">The number of specials to pick. Default is 2.</param>
        /// <returns>A list of special characters.</returns>
        public static List<char> GetSpecials(string speciaList, int count = 2)
        {
            Random r = new Random();
            int listCount = speciaList.Length;
            List<char> specials = new List<char>();

            for (int i = 0; i < count; i++)
            {
                int index = r.Next(listCount);
                specials.Add(speciaList[index]);
            }

            return specials;
        }

        public struct PWParser
        {
            public readonly string _defaultSpecials = "!@#$%^&*()_+-=`~[];:'\",<.>?";
            public readonly int _defaultNumberCount = 2;
            public readonly List<int> _defaultNumberRange = Enumerable.Range(0, 10).ToList();

            public readonly string specialRegEx = "\\{special\\{?[!@#$%^&*()_+-=`~[\\];:'\\\",<\\.>\\?s]*\\}?:?\\d*\\}";

            public bool hasSpecial = false;

            /// <summary>
            /// The specials dictionary that governs where special characters appear and the number of the special characters.
            /// The dictionary is in this format:
            /// <para>Dictionary: (index in original string, Dictionary: (number of specials, list of special characters))</para>
            /// </summary>
            public Dictionary<int, Dictionary<int, List<char>>> specials = new Dictionary<int, Dictionary<int, List<char>>>();

            public PWParser(string template)
            {
                ParseSpecials(template);
                
                ////List<int> openingCurlys = new List<int>();
                ////List<int> closingCurlys = new List<int>();
                //int indexOfSpecialKeyword = template.IndexOf("special");
                //int indexOfNumberKeyword = template.IndexOf("number");
                //int indexOfNameKeyword = template.IndexOf("name");

                ////for (int i = template.IndexOf('{'); i > -1; i = template.IndexOf('{', i + 1))
                ////    openingCurlys.Add(i);

                ////for (int i = template.IndexOf('}'); i > -1; i = template.IndexOf('}', i + 1))
                ////    closingCurlys.Add(i);

                //string specialParams = string.Empty;
                //string numberParams = string.Empty;
                //string nameParams = string.Empty;

                //if (indexOfSpecialKeyword != -1)
                //{
                //    int paramsStart = indexOfSpecialKeyword;
                //    int paramsEnd = template.IndexOf('}', paramsStart);
                //    specialParams = template[paramsStart..paramsEnd];
                //    if (specialParams.Equals("special"))
                //    {
                //        specials = GetSpecials(_defaultSpecials);
                //    }
                //    else
                //    {
                //        specialParams = specialParams.Replace("special{", "");
                //        List<string> _ = specialParams.Split('s').ToList();
                //        StringBuilder str = new StringBuilder();
                //        _.ForEach(s => str = str.Append(s));
                //        specials = GetSpecials(str.ToString());
                //    }
                //}
                //else { specials = new List<char>(); }

                //hasSpecial = specials.Count > 0;
            }

            void ParseSpecials(string template)
            {
                Regex matcher = new(specialRegEx);
                MatchCollection mc = matcher.Matches(template);
                if (mc.Count > 0)
                {

                    foreach (Match m in mc)
                    {
                        string specialSubString = template.Substring(m.Index, m.Length);
                        if (specialSubString.Equals("{special}"))
                        {
                            if (!specials.ContainsKey(m.Index))
                            {
                                specials.Add(m.Index, new Dictionary<int, List<char>>()
                                {
                                    {_defaultNumberCount, GetSpecials(_defaultSpecials) }
                                });
                            }
                            //specials.AddRange(GetSpecials(_defaultSpecials));
                        }
                        else
                        {
                            specialSubString = specialSubString.Replace("{special{", "");
                            string[] tokens = specialSubString.Split('s'); //split special string on 's'
                            char[] chars = tokens.Aggregate((s1, s2) => s1 + s2).ToCharArray();

                            //Get special character count
                            int lastColonIndex = specialSubString.LastIndexOf(':') + 1;
                            int lastCurlyIndex = specialSubString.LastIndexOf('}') + 1;
                            int numberOfSpecials = -1;
                            if (lastColonIndex != -1 && lastCurlyIndex != -1)
                                int.TryParse(specialSubString[lastColonIndex..lastCurlyIndex], out numberOfSpecials);

                            if (!specials.ContainsKey(m.Index))
                            {
                                specials.Add(m.Index, new Dictionary<int, List<char>>()
                                {
                                    {(numberOfSpecials == -1 || numberOfSpecials == 0) ? 2 : numberOfSpecials, GetSpecials(tokens.Aggregate((s1, s2) => s1 + s2)) }
                                });
                            }
                        }
                    }
                }
                hasSpecial = specials.Count > 0;
            }
        }
    }
}
