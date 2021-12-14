using System;
using System.Linq;

namespace IsoParser.Lib.Tools
{
    public class DataType
    {
		public static int StringInt (string type)
		{
			return type.Select (c => (int)c).Aggregate (0, (x, y) => (x << 8) + y);
		}
		public static int ByteInt (byte[] data, int offset)
		{
			return data.Skip (offset).Take (4).ToArray ().Aggregate (0, (x, y) => (x << 8) + y);
		}

		public static uint ByteUInt (byte[] data, int offset)
		{
			return data.Skip (offset).Take (4).ToArray ().Aggregate (0U, (x, y) => (x << 8) + y);
		}

		public static short ByteShort (byte[] data, int offset)
		{
			return (short)(data[offset] << 8 | data[offset + 1]);
		}
		public static ushort ByteUShort (byte[] data, int offset)
		{
			return (ushort)(data[offset] << 8 | data[offset + 1]);
		}

		public static long ByteLong (byte[] data, int offset)
		{
			return (long)ByteInt (data, offset) << 32 + ByteInt (data, offset + 4);
		}

		public static string ByteString (byte[] data, int offset)
		{
			return data.Skip (offset).Take (4).ToArray ().Aggregate ("", (x, y) => x + Convert.ToChar (y));
		}

		public static string ByteString (byte[] data, int offset, int size = 4)
		{
			return data.Skip (offset).Take (size).ToArray ().Aggregate ("", (x, y) => x + Convert.ToChar (y));
		}

		public static bool ValidId (int id)
		{
			return ValidByte (id >> 24)
				&& ValidByte (id >> 16)
				&& ValidByte (id >> 8)
				&& ValidByte (id);
		}

		private static bool ValidByte (int id)
		{
			return (id & 255) > 0x1f;
		}

	}
}
