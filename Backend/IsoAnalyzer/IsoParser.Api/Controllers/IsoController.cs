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
using IsoParser.Api.Models;

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
            if(tree != null)
            this._logger.LogInformation ($"id: {tree.Id}");

            //return JsonConvert.SerializeObject (tree);
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
            DisplayItem dItem = new () { Name = item.Name };
            switch (item.Type)
            {
            case ItemType.Byte:
            case ItemType.Int:
            case ItemType.Short:
            case ItemType.Long:
                dItem.Value = $"{item.Value} ({item.Value:x}h)";
                break;
            case ItemType.Matrix:
                dItem.Value = string.Join(",", ((double[])item.Value).ToArray());
                break;
            default:
                dItem.Value = item.Value.ToString ();
                break;
            }
            return dItem;
        }

        private string GetDisplayType(AtomType? type)
        {
            return type.HasValue ? BitConverter.GetBytes( (int)type).ToArray().Aggregate ("", (x, y) => Convert.ToChar (y) + x) : "none";
        }
    }
}
