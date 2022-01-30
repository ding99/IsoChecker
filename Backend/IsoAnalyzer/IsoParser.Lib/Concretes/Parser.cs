using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IsoParser.Lib.Services;
using IsoParser.Lib.Models;
using IsoParser.Lib.Tools;

namespace IsoParser.Lib.Concretes {
	public partial class Parser : IParser {
		private BinFile file;
		private long fileSize;

		private const int MaxSize = 1_048_576;
		private readonly HashSet<AtomType> containers;
		private readonly Dictionary<AtomType, int> references;

		private readonly List<Track> tracks;
		private Track track;

		#region movie variable
		private int? timeScale;
		#endregion

		#region public
		public Parser () {
			this.file = null;
			this.tracks = new ();
			this.track = new ();

			this.containers = new HashSet<AtomType> {
				AtomType.CLIP,
				AtomType.DINF,
				AtomType.EDTS,
				AtomType.GMHD,
				AtomType.IMAP,
				AtomType.IN,
				AtomType.MATT,
				AtomType.META,
				AtomType.MDIA,
				AtomType.MINF,
				AtomType.MOOV,
				AtomType.STBL,
				AtomType.TRAK,
				AtomType.UDTA
			};

            this.references = new Dictionary<AtomType, int> {
				[AtomType.DREF] = 8,
				[AtomType.STSD] = 8
			};
        }

		public void End () {
			if (this.file != null)
				this.file.End ();
        }

		public async Task<IsoInfo> GetTree (string path) {
			this.file = new (path);
			this.fileSize = this.file.FileSize ();

			Atom atom = new ();

			// Root atom id is always 1
			await Task.Run (() => atom = this.GetAtom (1, this.file.FileSize (), 0L, 0));

			if(this.track.Type != ComponentType.Unknown)
            {
				this.tracks.Add (this.track);
            }

			IsoInfo iso = new () { Atom = atom, Tracks = tracks };

			this.file.End ();
			return iso;
        }

		public async Task<byte[]> GetData (string path, long offset, int size) {
			if (size > MaxSize)
				size = MaxSize;

			return await Task.Run (() => new byte[size]);
		}
		#endregion public
	}
}
