using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

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
        public async Task<string> Get (string path)
        {
            this._logger.LogInformation ($"path: {path}");

            var movs = await this._parser.GetTree (path);
            if(movs != null)
            this._logger.LogInformation ($"id: {movs.Id}");

            return JsonConvert.SerializeObject (movs);
        }
    }
}
