using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using IsoParser.Lib.Concretes;
using IsoParser.Lib.Models;
using IsoParser.Lib.Services;

namespace IsoParser.Api.Controllers
{
    [ApiController]
    [Route ("[controller]")]
    public class IsoController : ControllerBase
    {
        private readonly ILogger<Parser> _logger;
        private readonly IParser _parser;

        public IsoController (ILogger<Parser> logger, IParser parser)
        {
            this._logger = logger;
            this._parser = parser;
        }

        [HttpGet]
        public string Get ()
        {
            this._logger.LogInformation ("Getting...");
            return "Returning a parsing result";
        }
    }
}
