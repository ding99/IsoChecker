using System;
using System.IO;

namespace IsoParser.Lib.Tools {
	public class FileInfo {
		public long FileSize { get; set; }
		public int PackSize { get; set; }
		public int PackCount { get; set; }
		public int Head { get; set; }
	}

	public class BinFile {
		private readonly BinaryReader reader;
		private readonly FileInfo info;

		private bool valid;

		public BinFile (string path) {
			this.valid = true;

			this.info = new FileInfo ();

			if (!File.Exists (path)) {
				this.valid = false;
				return;
			}

			try {
				this.reader = new BinaryReader (File.Open (path, FileMode.Open, FileAccess.Read, FileShare.Read));

				this.reader.BaseStream.Seek (0, SeekOrigin.End);
				this.info.FileSize = this.reader.BaseStream.Position;
				this.reader.BaseStream.Seek (0, SeekOrigin.Begin);
			}
			catch (Exception) {
				if (this.reader != null) {
					this.reader.Close ();
					this.reader.Dispose ();
				}
				this.valid = false;
			}
		}

		public void End () {
			if (this.valid) {
				this.reader.Close ();
				this.reader.Dispose ();
			}
		}

		public bool IsValid () {
			return this.valid;
		}

		public long FileSize () {
			return this.info.FileSize;
		}

		public byte[] NextPack () {
			try {
				return this.reader.ReadBytes (this.info.PackSize);
			}
			catch (Exception) {
				this.valid = false;
				return Array.Empty<byte> ();
			}
		}

		public byte[] SomePack (int packNumber) {
			if (packNumber > this.info.PackCount)
				return Array.Empty<byte> ();

			try {
				this.reader.BaseStream.Seek (packNumber * this.info.PackSize, SeekOrigin.Begin);
				return this.reader.ReadBytes (this.info.PackSize);
			}
			catch (Exception) {
				this.valid = false;
				return Array.Empty<byte> ();
			}
		}

		public bool GotoPack (int packn) {
			if (packn > this.info.PackCount)
				return this.valid = false;

			try {
				this.reader.BaseStream.Seek ( (long) packn * this.info.PackSize, SeekOrigin.Begin);
				return true;
			}
			catch (Exception) {
				return this.valid = false;
			}
		}

		public bool BackPack (int packn) {
			try {
				this.reader.BaseStream.Seek (-(long)packn * this.info.PackSize, SeekOrigin.Current);
				return true;
			}
			catch (Exception) {
				return this.valid = false;
			}
		}

		public bool GotoByte (long offset) {
			if (offset > this.info.FileSize)
				return this.valid = false;

			try {
				this.reader.BaseStream.Seek (offset, SeekOrigin.Begin);
				return true;
			}
			catch (Exception) {
				return this.valid = false;
			}
		}

		public byte[] Read (int size) {
			try {
				return this.reader.ReadBytes (size);
			}
			catch (Exception) {
				this.valid = false;
				return Array.Empty<byte> ();
			}
		}

		public byte[] Read (int size, long offset) {
			return  this.GotoByte (offset) ? this.Read(size) : Array.Empty<byte> ();
		}

		public bool SetPack (int size) {
			this.info.PackSize = size;
			if (size > 0) this.info.PackCount = (int) (this.info.FileSize / size);
			return this.info.FileSize % size != 0 ? this.valid = false : true;
		}
	}
}
