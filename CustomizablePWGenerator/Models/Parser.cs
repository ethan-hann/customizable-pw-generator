using System.Text.RegularExpressions;
using System.Text;

namespace CustomizablePWGenerator.Models
{
    public class Parser
    {
        public const string SPECIAL_REGEX = "{special:?\\d*}";
        public const string NAME_REGEX = "{name=\"\\w*\\s*\\w+\";?(lowerFirst|upperFirst)?,(lowerSecond|upperSecond)?}";
        public const string DATE_REGEX = "{date}";

        public readonly string _defaultSpecials = "!@#$%^&*()_+-=`~[];:'\",<.>?";
        public readonly List<int> _defaultNumberRange = Enumerable.Range(0, 10).ToList();

        /// <summary>
        /// A dictionary of regular expressions with the REGEX as the key and a (key,value) pair as the value.
        /// The <c>key</c> in the (key,value) pair means: the shortend replaced text in the original string.
        /// The <c>value</c> in the (key,value) pair means: the number of occurances of this key in the original string.
        /// </summary>
        readonly Dictionary<string, (string, int)> _regularExpressions;
        readonly List<string> regexes;
        
        private readonly string stringNoTemplate = string.Empty;
        private readonly List<int> replaceIndices = new List<int>();
        private static readonly Random random = new Random();

        public string ParsedString { get; private set; }

        public Parser(string template)
        {
            regexes = new List<string>() { SPECIAL_REGEX, NAME_REGEX, DATE_REGEX };
            _regularExpressions = new Dictionary<string, (string, int)>()
                {
                    {SPECIAL_REGEX, ("[s]", 0)},
                    {NAME_REGEX, ("[n]", 0)},
                    {DATE_REGEX, ("[d]", 0)}
                };

            stringNoTemplate = ReplaceTemplateText(template, regexes);
            FindReplacementIndices(stringNoTemplate);

            //Get the text for the name to replace.
            string nameText = ParseName(template);

            //Get the correct number of special characters to replace.
            List<char> specials = GetSpecials(_defaultSpecials, _regularExpressions[SPECIAL_REGEX].Item2);

            string date = ParseDate(template);

            ParsedString = nameText + specials + date;
        }

        /// <summary>
        /// Replaces the template text with their shorter equivalents based on the list of regular expressions.
        /// </summary>
        /// <param name="template">The template string to search.</param>
        /// <param name="regexes">A list of strings containing regular expression patterns.</param>
        /// <returns>A new string with all occurances of the regular expressions replaced with their shorter variants.</returns>
        string ReplaceTemplateText(string template, List<string> regexes)
        {
            StringBuilder bldr = new StringBuilder(template);
            foreach (string regex in regexes)
            {
                Regex matcher = new(regex);
                MatchCollection mc = matcher.Matches(template);
                if (mc.Count > 0)
                {
                    foreach (Match m in mc)
                    {
                        bldr.Replace(m.Value, _regularExpressions[regex].Item1);
                        var val = _regularExpressions[regex];
                        val.Item2++;
                        _regularExpressions[regex] = (_regularExpressions[regex].Item1, val.Item2);
                    }
                }
            }

            return bldr.ToString();
        }

        /// <summary>
        /// Finds in the template string the actual indices that need to be replaced with their template value and adds those indicies to the
        /// <see cref="replaceIndices"/> list.
        /// </summary>
        /// <param name="template">The string to search.</param>
        void FindReplacementIndices(string template)
        {
            for (int i = 0; i < template.Length; i++)
            {
                foreach ((string,int) val in _regularExpressions.Values)
                {
                    int index = template.ToList().FindIndex(i, c => c.Equals(val.Item1));
                    if (index > 0)
                        replaceIndices.Add(index);
                }
            }
            
        }

        string ParseName(string template)
        {
            Regex matcher = new(NAME_REGEX);
            MatchCollection mc = matcher.Matches(template);
            StringBuilder bldr = new StringBuilder();
            if (mc.Count > 0)
            {
                foreach (Match m in mc)
                {
                    //{name="John%20Smith";lowerFirst,lowerSecond}
                    bldr = new StringBuilder();
                    string nameString = template.Substring(m.Index, m.Length);
                    string options = string.Empty;

                    if (nameString.Contains(';'))
                    {
                        options = nameString[nameString.IndexOf(';')..nameString.IndexOf('}')]; //parse out the options if they exist
                        nameString = nameString.Remove(nameString.IndexOf(';')); //remove extra options from name string
                    }

                    nameString = nameString.Replace("{name=\"", ""); //remove extra information

                    bldr = bldr.Append(nameString);

                    //Modify name string based on modifiers (if they exist)
                    int lastNameIndex = -1;
                    if (!options.Equals(string.Empty))
                    {
                        string[] tokens = options.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (string token in tokens)
                        {
                            string t = token.Replace(";", "").Replace(",", ""); //remove extra characters from token, if they exist.

                            bldr[0] = t switch
                            {
                                "upperFirst" => char.Parse(bldr[0].ToString().ToUpper()),
                                "lowerFirst" => char.Parse(bldr[0].ToString().ToLower()),
                                _ => bldr[0]
                            };

                            lastNameIndex = bldr.ToString().IndexOf(' ') + 1;
                            if (lastNameIndex > 0)
                            {
                                bldr[lastNameIndex] = t switch
                                {
                                    "upperSecond" => char.Parse(bldr[lastNameIndex].ToString().ToUpper()),
                                    "lowerSecond" => char.Parse(bldr[lastNameIndex].ToString().ToLower()),
                                    _ => bldr[lastNameIndex]
                                };
                            }
                        }
                    }
                }
            }
            return bldr.ToString();
        }

        string ParseDate(string template)
        {
            Regex matcher = new(NAME_REGEX);
            MatchCollection mc = matcher.Matches(template);
            StringBuilder bldr = new StringBuilder();
            if (mc.Count > 0)
            {
                foreach (Match m in mc)
                {
                    bldr = bldr.Append(DateTime.Now.Year).Append(',');
                }
            }
            return bldr.ToString();
        }

        /// <summary>
        /// Get random special characters based on the specials list specified.
        /// </summary>
        /// <param name="speciaList">The list of specials as a string.</param>
        /// <param name="count">The number of special characters to retrieve.</param>
        /// <returns>A list of special characters.</returns>
        public static List<char> GetSpecials(string speciaList, int count = 1)
        {
            int listCount = speciaList.Length;
            List<char> specials = new List<char>();
            for (int i = 0; i < count; i++)
                specials.Add(speciaList[random.Next(listCount)]);
            return specials;
        }
    }
}
