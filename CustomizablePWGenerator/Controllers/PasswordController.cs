using CustomizablePWGenerator.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomizablePWGenerator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        Password generator = new Password();

        [Route("getPassword")]
        [HttpGet]
        public Password GetPassword(string templateText)
        {
            generator.TemplateText = templateText;
            return generator.GeneratePassword();
        }
    }
}
