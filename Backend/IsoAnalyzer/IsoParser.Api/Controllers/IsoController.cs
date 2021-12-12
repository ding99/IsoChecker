using IsoParser.Api.Models;
using IsoParser.Lib.Concretes;
using IsoParser.Lib.Models;
using IsoParser.Lib.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

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

            var tree = await this._parser.GetTree (path);

            return JsonConvert.SerializeObject (this.GetAtom (tree));
        }

        private DisplayAtom GetAtom (Atom atom)
        {
            DisplayAtom dAtom = new (atom.Id, this.GetDisplayType (atom.Type),  atom.Size, atom.Offset);

            if (atom.Items != null)
                dAtom.Items = atom.Items.Select (e => this.GetItem (e)).ToList ();

            if (atom.Atoms != null)
                dAtom.Atoms = atom.Atoms.Select (e => this.GetAtom (e)).ToList ();

            return dAtom;
        }

        private DisplayItem GetItem (Item item)
        {
            return new ()
            {
                Name = item.Name,
                Value = item.Type switch
                {
                    ItemType.Byte or ItemType.Int or ItemType.Short or ItemType.Long => $"{item.Value} ({item.Value:x}h)",
                    ItemType.Matrix => string.Join (",", ((double[])item.Value).ToArray ()),
                    _ => item.Value.ToString ()
                }
            };
        }

        private string GetDisplayType(AtomType? type)
        {
            return type.HasValue ? BitConverter.GetBytes( (int)type).ToArray().Aggregate ("", (x, y) => Convert.ToChar (y) + x) : "none";
        }
    }
}
