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
            GeneratedPassword = TemplateText;
            return this;
        }
    }
}
