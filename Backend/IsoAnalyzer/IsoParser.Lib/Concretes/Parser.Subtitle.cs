using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using IsoParser.Lib.Models;
using IsoParser.Lib.Tools;

namespace IsoParser.Lib.Concretes
{
    partial class Parser
	{
		private void AnalyzeSubtitles ()
		{
            Console.WriteLine ($"-- tracks count {this.iso.Tracks.Count}");
			foreach (var track in this.iso.Tracks)
				if (track.Type == ComponentType.Media && track.SubType == ComponentSubType.Caption)
				{
					if (this.iso.Subtitle == null)
						this.iso.Subtitle = new ();

					this.AnalyzeCaption (track);
				}
        }

		private int GainCount (Track track, int chunk)
		{
			int n = track.SampleToChunks.Count;
			for (int i = 0; i < n; i++)
				if (chunk == track.SampleToChunks[i].FirstChunk || i + 1 == n || chunk < track.SampleToChunks[i + 1].FirstChunk)
					return track.SampleToChunks[i].SamplesPerChunk;

			return track.SampleSizeCount;
		}

		private void AnalyzeCaption (Track track)
		{
			Subtitle sub = new () { Type = track.DataFormats.Count > 0 ? track.DataFormats[0] : "Unknow" };
            //int temp = 0, pre = 0;
            Console.WriteLine ($"   sub {sub.Type}, ChunkOffsets count {track.ChunkOffsets.Count}");
			for (int i = 0; i < track.ChunkOffsets.Count; i++)
			{
				int count = this.GainCount (track, i + 1) * 2;
				this.file.GotoByte (track.ChunkOffsets[i]);
				Console.WriteLine ($"  count {count}");
				for (int k = 0; k < count; k++)
				{
					byte[] head = this.file.Read (8);
					//if (i < 2)
					//{
					//	sub.Frames.Add (this.file.Read (DataType.ByteInt (head, 0) - 8, ref temp));
					//	System.Console.WriteLine ($"  <  {k} : {temp:x}  > {(temp != pre + 10 ? "WWWWarning" : "")}");
					//	pre = temp;
					//}
					//else
					bool finish = false;
					var wt = (DataType.ByteInt (head, 4));
					switch (DataType.ByteInt (head, 4))
                    {
					case 0x63646174:  //cdat
						sub.CC1.Frames.Add (this.file.Read (DataType.ByteInt (head, 0) - 8));
						break;
					case 0x63647432:  //cdt2
						sub.CC1.Frames.Add (this.file.Read (DataType.ByteInt (head, 0) - 8));  // cdt2 as CC1 temporarily
						break;
					case 0x63647433:  //cdt3
						sub.CC3.Frames.Add (this.file.Read (DataType.ByteInt (head, 0) - 8));
						break;
					case 0x63647434:  //cdt4
						sub.CC4.Frames.Add (this.file.Read (DataType.ByteInt (head, 0) - 8));
						break;
					default:
                        Console.WriteLine ($"<><><> {wt}");
						finish = true;
						break;
					}
					if(finish)
						break;
				}
			}

			//this.file.GotoByte (0x1896c1);
			//byte[] head1 = this.file.Read (8);
			//byte[] read = this.file.Read (DataType.ByteInt (head1, 0) - 8);
			//string reads = string.Join (" ", Array.ConvertAll(read, x => x.ToString("x2")));
   //         System.Console.WriteLine ($"  <<  {reads}  >>");

			this.iso.Subtitle.Subtitles.Add (sub);
		}

	}
}
