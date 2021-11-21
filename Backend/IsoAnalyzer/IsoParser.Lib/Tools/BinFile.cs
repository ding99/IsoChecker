using System;
using System.IO;

namespace IsoParser.Lib.Tools {
	public class FInfo {
		public long fsize { get; set; }
		public int psize { get; set; }
		public int npack { get; set; }
		public int phead { get; set; }
	}

	public class BinFile {
		private readonly BinaryReader reader;
		private bool use;

		private FInfo info;

		public BinFile(string tsid) {
			this.use = true;

			this.info = new FInfo();

			if (!File.Exists(tsid)) {
				this.use = false;
				return;
			}

			try {
				this.reader = new BinaryReader(File.Open(tsid, FileMode.Open, FileAccess.Read, FileShare.Read));
			}
			catch (Exception) {
				this.use = false;
			}

			this.reader.BaseStream.Seek(0, SeekOrigin.End);
			this.info.fsize = this.reader.BaseStream.Position;
			this.reader.BaseStream.Seek(0, SeekOrigin.Begin);
		}

		public void End() {
			if (this.use)
				this.reader.Close();
		}

		public bool IsValid() {
			return this.use;
		}

		public long FileSize() {
			return this.info.fsize;
		}

		public bool NextPack(ref byte[] buffer) {
			try {
				buffer = this.reader.ReadBytes(this.info.psize);
				return true;
			}
			catch (Exception) {
				return this.use = false;
			}
		}

		public bool SomePack(ref byte[] buffer, int packn) {
			if (packn > this.info.npack)
				return this.use = false;

			try {
				this.reader.BaseStream.Seek(packn * this.info.psize, SeekOrigin.Begin);
				buffer = this.reader.ReadBytes(this.info.psize);
				return true;
			}
			catch (Exception) {
				return this.use = false;
			}
		}

		public bool GotoPack(int packn) {
			if (packn > this.info.npack)
				return this.use = false;

			try {
				this.reader.BaseStream.Seek((long)packn * this.info.psize, SeekOrigin.Begin);
				return true;
			}
			catch (Exception) {
				return this.use = false;
			}
		}

		public bool BackPack(int packn) {
			try {
				this.reader.BaseStream.Seek(-(long)packn * this.info.psize, SeekOrigin.Current);
				return true;
			}
			catch (Exception) {
				return this.use = false;
			}
		}

		public bool GotoByte(long offset) {
			if (offset > this.info.fsize)
				return this.use = false;

			try {
				this.reader.BaseStream.Seek(offset, SeekOrigin.Begin);
				return true;
			}
			catch (Exception) {
				return this.use = false;
			}
		}

		public bool ReadData(ref byte[] buffer, int size) {
			try {
				buffer = this.reader.ReadBytes(size);
				return true;
			}
			catch (Exception) {
				return this.use = false;
			}
		}

		public bool SetPack(int size) {
			this.info.psize = size;
			if (size > 0) this.info.npack = (int)(this.info.fsize / size);
			return this.info.fsize % size != 0 ? this.use = false : true;
		}
	}
}
