﻿using System.Collections.Generic;
using System.Threading.Tasks;

using IsoParser.Lib.Models;
using IsoParser.Lib.Services;
using IsoParser.Lib.Tools;

namespace IsoParser.Lib.Concretes
{
    public partial class Parser : IParser {
		private BinFile file;
		private long fileSize;

		private const int MaxSize = 1_048_576;
		private readonly HashSet<AtomType> containers;
		private readonly Dictionary<AtomType, int> references;

		private readonly IsoInfo iso;

		private Track track;

		#region public
		public Parser () {
			this.file = null;

			this.containers = new HashSet<AtomType> {
				AtomType.clip,
				AtomType.dinf,
				AtomType.edts,
				AtomType.gmhd,
				AtomType.imap,
				AtomType.IN,
				AtomType.matt,
				AtomType.meta,
				AtomType.mdia,
				AtomType.minf,
				AtomType.moov,
				AtomType.stbl,
				AtomType.trak,
				AtomType.ilst,
				AtomType.udta
			};

            this.references = new Dictionary<AtomType, int> {
				[AtomType.dref] = 8,
				[AtomType.stsd] = 8,
				[AtomType.keys] = 8,
				[AtomType.avc1] = 78
			};

			this.iso = new ();

			this.track = new ();
		}

		public void End () {
			if (this.file != null)
				this.file.End ();
        }

		public async Task<IsoInfo> GetTree (string path) {
			this.file = new (path);
			this.fileSize = this.file.FileSize ();

			// Root atom id is always 1
			await Task.Run (() => this.iso.Atom = this.GetAtom (1, this.file.FileSize (), 0L, 0));

			if(this.track.Type != ComponentType.Unknown)
            {
				this.iso.Tracks.Add (this.track);
            }

			this.AnalyzeSubtitles ();

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
