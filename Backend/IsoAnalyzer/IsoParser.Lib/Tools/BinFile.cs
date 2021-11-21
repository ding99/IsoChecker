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

		private bool use;

		public BinFile(string tsid) {
			this.use = true;

			this.info = new FileInfo();

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
			this.info.FileSize = this.reader.BaseStream.Position;
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
			return this.info.FileSize;
		}

		public bool NextPack(ref byte[] buffer) {
			try {
				buffer = this.reader.ReadBytes(this.info.PackSize);
				return true;
			}
			catch (Exception) {
				return this.use = false;
			}
		}

		public bool SomePack(ref byte[] buffer, int packn) {
			if (packn > this.info.PackCount)
				return this.use = false;

			try {
				this.reader.BaseStream.Seek(packn * this.info.PackSize, SeekOrigin.Begin);
				buffer = this.reader.ReadBytes(this.info.PackSize);
				return true;
			}
			catch (Exception) {
				return this.use = false;
			}
		}

		public bool GotoPack(int packn) {
			if (packn > this.info.PackCount)
				return this.use = false;

			try {
				this.reader.BaseStream.Seek((long)packn * this.info.PackSize, SeekOrigin.Begin);
				return true;
			}
			catch (Exception) {
				return this.use = false;
			}
		}

		public bool BackPack(int packn) {
			try {
				this.reader.BaseStream.Seek(-(long)packn * this.info.PackSize, SeekOrigin.Current);
				return true;
			}
			catch (Exception) {
				return this.use = false;
			}
		}

		public bool GotoByte(long offset) {
			if (offset > this.info.FileSize)
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
			this.info.PackSize = size;
			if (size > 0) this.info.PackCount = (int)(this.info.FileSize / size);
			return this.info.FileSize % size != 0 ? this.use = false : true;
		}
	}
}
