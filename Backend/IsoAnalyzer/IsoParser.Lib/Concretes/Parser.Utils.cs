using System;
using System.Text;

using IsoParser.Lib.Models;
using IsoParser.Lib.Tools;

namespace IsoParser.Lib.Concretes
{
    public partial class Parser
	{
		private bool isContainer (AtomType type)
		{
			return this.containers.Contains (type);
		}

		private string ParseTime (byte[] data, int offset, long position)
		{
			DateTime t = new DateTime (1904, 1, 1).AddSeconds (DataType.ByteUInt (data, offset));
			return $"{t.Year}-{t.Month:D2}-{t.Day:D2} {t.Hour:D2}:{t.Minute:D2}:{t.Second:D2} ({position + offset:x10}-{position + offset + 3:x10})";
		}

		private string ParseDuration (byte[] data, int offset, int? timeScale)
		{
			int value = DataType.ByteInt (data, offset);
			StringBuilder b = new ($"{value:x}h");
			if (timeScale.HasValue && timeScale > 0)
			{
				double s = value * 1.0 / (int)timeScale;
				b.Append ($" ({s} seconds, {(int)s / 3600:D2}:{(int)s / 60 % 60:D2}:{(int)s % 60:D2}.{((int)(s * 1000)) % 1000:D3})");
			}
			return b.ToString ();
		}

		private double[] ParseMatrix (byte[] data, int offset)
		{
			double[] values = new double[9];
			double fraction;
			int integerPortion;

			for (int i = 0; i < 9; i++)
			{
				if (i == 2 || i == 5 || i == 8)
				{
					integerPortion = (int)(data[offset + i * 4] >> 6);
					fraction = ((uint)DataType.ByteInt (data, offset + i * 4) & 0x3fffffff) * 1.0 / 1_073_741_824;
				}
				else
				{
					integerPortion = DataType.ByteShort (data, offset + i * 4);
					fraction = DataType.ByteUShort (data, offset + i * 4 + 2) * 1.0 / 65536;
				}
				if (integerPortion >= 0)
					values[i] = (double)integerPortion + fraction;
				else
					values[i] = (double)integerPortion - fraction;
			}

			return values;
		}
		private double ParseDimension (byte[] data, int offset)
		{
			int integerPortion = DataType.ByteShort (data, offset);
			double fraction = DataType.ByteUShort (data, offset + 2) * 1.0 / 65_536;

			return integerPortion >= 0 ? (double)integerPortion + fraction : (double)integerPortion - fraction;
		}
	}
}
