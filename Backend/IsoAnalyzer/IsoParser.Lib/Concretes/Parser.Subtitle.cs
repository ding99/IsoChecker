using IsoParser.Lib.Models;
using IsoParser.Lib.Tools;

namespace IsoParser.Lib.Concretes
{
    partial class Parser
	{
		private void AnalyzeSubtitles ()
		{
			System.Console.WriteLine ($"-- tracks ({this.iso.Tracks.Count})");
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
			System.Console.WriteLine ($"  sub {sub.Type}, ChunkOffsets count {track.ChunkOffsets.Count}");

			for (int i = 0; i < track.ChunkOffsets.Count; i++)
			{
				int count = this.GainCount (track, i + 1) * 2;
				this.file.GotoByte (track.ChunkOffsets[i]);
				System.Console.WriteLine ($"  count {count}");

				for (int k = 0; k < count; k++)
				{
					byte[] head = this.file.Read (8);
					bool finish = false;
					switch (DataType.ByteInt (head, 4))
                    {
					case 0x63646174:  //cdat
					case 0x63647432:  //cdt2
					case 0x63636470:  //ccdp
						sub.Frames.Add (this.file.Read (DataType.ByteInt (head, 0) - 8));  // cdt2 as CC1 temporarily
						break;
					default:
						finish = true;
						break;
					}
					if(finish)
						break;
				}
			}
            System.Console.WriteLine ($"  sub, frames count [{sub.Frames.Count}]");
			this.iso.Subtitle.Subtitles.Add (sub);
		}

	}
}
