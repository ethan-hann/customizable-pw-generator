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

            private readonly string specialRegEx = "\\{special\\{?[!@#$%^&*()_+-=`~[\\];:'\\\",<\\.>\\?s]*\\}?:?\\d*\\}";
            private readonly string nameRegEx = "\\{name\\{{1}\\w*(%20|\\s)*\\w*\\}:?(upperFirst|lowerFirst|)?,?(upperSecond|lowerSecond)?\\}";
            private readonly string numberRegEx = "\\{number\\{?(date|count)?:?([0-9]+|[0-9]{4}-[0-9]{2}-[0-9]{2}|[0-9]{4})?\\}?\\}";
            private readonly Random random = new Random();

            public bool hasSpecial = false;

            /// <summary>
            /// The specials dictionary that governs where special characters appear and the number of the special characters.
            /// The dictionary is in this format:
            /// <para>Dictionary: (index in original string, Dictionary: (number of specials, list of special characters))</para>
            /// </summary>
            public Dictionary<int, Dictionary<int, List<char>>> specials = new Dictionary<int, Dictionary<int, List<char>>>();

            /// <summary>
            /// The names dictionary that governs where and how names appear.
            /// The dictionary is in this format:
            /// <para>Dictionary: (index in original string, final name string)</para>
            /// </summary>
            public Dictionary<int, string> names = new Dictionary<int, string>();

            /// <summary>
            /// The numbers dictionary that governs where and how numbers appear.
            /// The dictionary is in this format:
            /// <para>Dictionary: (index in original string, final number string)</para>
            /// </summary>
            public Dictionary<int, List<int>> numbers = new Dictionary<int, List<int>>();

            public PWParser(string template)
            {
                ParseSpecials(template);
                ParseName(template);
                ParseNumbers(template);
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

            void ParseName(string template)
            {
                Regex matcher = new(nameRegEx);
                MatchCollection mc = matcher.Matches(template);
                if (mc.Count > 0)
                {
                    foreach (Match m in mc)
                    {
                        StringBuilder bldr = new StringBuilder();
                        string nameSubString = template.Substring(m.Index, m.Length);
                        nameSubString = nameSubString.Replace("{name{", ""); //remove extra information
                        string name = nameSubString[0..nameSubString.IndexOf('}')]; //parse out the name
                        bldr.Append(name);

                        //Modify name string based on modifiers (if they exist)
                        int lastColonIndex = nameSubString.LastIndexOf(':') + 1;
                        int lastCurlyIndex = nameSubString.LastIndexOf('}') + 1;
                        int lastNameIndex = -1;
                        if (nameSubString.Contains(':'))
                        {
                            string modifierString = nameSubString[lastColonIndex..(lastCurlyIndex-1)];
                            string[] modifiers;
                            if (modifierString.Contains(','))
                                modifiers = modifierString.Split(',');
                            else
                                modifiers = new string[] { modifierString };

                            foreach (string s in modifiers)
                            {
                                bldr[0] = s switch
                                {
                                    "upperFirst" => char.Parse(bldr[0].ToString().ToUpper()),
                                    "lowerFirst" => char.Parse(bldr[0].ToString().ToLower()),
                                    _ => bldr[0]
                                };

                                lastNameIndex = bldr.ToString().IndexOf(' ') + 1;
                                bldr[lastNameIndex] = s switch
                                {
                                    "upperSecond" => char.Parse(bldr[lastNameIndex].ToString().ToUpper()),
                                    "lowerSecond" => char.Parse(bldr[lastNameIndex].ToString().ToLower()),
                                    _ => bldr[lastNameIndex]
                                };
                            }
                        }

                        if (!names.ContainsKey(m.Index))
                            names.Add(m.Index, $"{bldr[0]}{(bldr[lastNameIndex] == -1 ? "" : bldr[lastNameIndex])}");
                    }
                }
            }

            void ParseNumbers(string template)
            {
                Regex matcher = new(numberRegEx);
                MatchCollection mc = matcher.Matches(template);
                if (mc.Count > 0)
                {
                    foreach (Match m in mc)
                    {
                        StringBuilder bldr = new StringBuilder();
                        string numberSubString = template.Substring(m.Index, m.Length);
                        numberSubString = numberSubString.Replace("{number{", ""); //remove extra information
                        string arguments = numberSubString[0..numberSubString.IndexOf('}')]; //parse out the arguments

                        int lastColonIndex = arguments.LastIndexOf(':') + 1;
                        int lastCurlyIndex = arguments.LastIndexOf('}') + 1;

                        if (arguments.Contains(':'))
                        {
                            switch (arguments)
                            {
                                default:
                                    bldr = new StringBuilder(arguments);
                                    break;
                            }
                        }
                        else
                        {
                            Random r = new Random();
                            //bldr = arguments;
                            switch (arguments)
                            {
                                case "date":
                                    {
                                        bldr = new StringBuilder(DateTime.Now.Year);
                                        break;
                                    }
                                case "count":
                                    {
                                        IEnumerable<int> nums = _defaultNumberRange.OrderBy(x => r.Next()).Take(_defaultNumberCount);
                                        if (!numbers.ContainsKey(m.Index))
                                            numbers.Add(m.Index, nums.ToList());
                                        else
                                            numbers[m.Index] = nums.ToList();
                                        break;
                                    }
                                default:
                                    bldr = new StringBuilder();
                                    break;
                            }

                            //    str.Append(string.Join(", ", Parser.specials.Select(pair => $"{pair.Key} => " +
                            //    $"{{{pair.Value.Select(innerPair => $"{innerPair.Key} => {innerPair.Value}")}}}")));
                        }

                        //if (!names.ContainsKey(m.Index))
                           // names.Add(m.Index, $"{bldr[0]}{(bldr[lastNameIndex] == -1 ? "" : bldr[lastNameIndex])}");
                    }
                }
            }
        }
    }
}
