using System.Text;

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


        
        
        /// <summary>
        /// Generates a password based on <see cref="TemplateText"/>.
        /// </summary>
        /// <returns>An instance of the generated password.</returns>
        public Password GeneratePassword()
        {
            StringBuilder str = new StringBuilder();
            PWParser parser = new PWParser(TemplateText);
            if (parser.hasSpecial)
                foreach (char c in parser.specials)
                    str = str.Append(c);
            GeneratedPassword = str.ToString();
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

        private struct PWParser
        {
            public readonly string _defaultSpecials = "!@#$%^&*()_+-=`~[];:'\",<.>?";
            public readonly int _defaultNumberCount = 2;
            public readonly List<int> _defaultNumberRange = Enumerable.Range(0, 10).ToList();

            public bool hasSpecial;
            public List<char> specials;

            public PWParser(string template)
            {
                //List<int> openingCurlys = new List<int>();
                //List<int> closingCurlys = new List<int>();
                int indexOfSpecialKeyword = template.IndexOf("special");
                int indexOfNumberKeyword = template.IndexOf("number");
                int indexOfNameKeyword = template.IndexOf("name");

                //for (int i = template.IndexOf('{'); i > -1; i = template.IndexOf('{', i + 1))
                //    openingCurlys.Add(i);

                //for (int i = template.IndexOf('}'); i > -1; i = template.IndexOf('}', i + 1))
                //    closingCurlys.Add(i);

                string specialParams = string.Empty;
                string numberParams = string.Empty;
                string nameParams = string.Empty;

                if (indexOfSpecialKeyword != -1)
                {
                    int paramsStart = indexOfSpecialKeyword;
                    int paramsEnd = template.IndexOf('}', paramsStart);
                    specialParams = template[paramsStart..paramsEnd];
                    if (specialParams.Equals("special"))
                    {
                        specials = GetSpecials(_defaultSpecials);
                    }
                    else
                    {
                        specialParams = specialParams.Replace("special{", "");
                        List<string> _ = specialParams.Split('s').ToList();
                        StringBuilder str = new StringBuilder();
                        _.ForEach(s => str = str.Append(s));
                        specials = GetSpecials(str.ToString());
                    }
                }
                else { specials = new List<char>(); }

                hasSpecial = specials.Count > 0;
            }
        }
    }
}
