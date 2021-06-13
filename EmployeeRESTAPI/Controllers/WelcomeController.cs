using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace EmployeeRESTAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WelcomeController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("********************************************************************");
            sb.AppendLine("");
            sb.AppendLine("THIS IS A SAMPLE RUNNING REST API, by Tom Osejo");
            sb.AppendLine("");
            sb.AppendLine("********************************************************************");
            return sb.ToString();
        }
    }
}
