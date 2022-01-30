using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using IsoParser.Lib.Models;
using IsoParser.Lib.Tools;

namespace IsoParser.Lib.Concretes
{
	partial class Parser
	{
		private void AnalyzeSubtitles ()
		{
			foreach (var track in this.iso.Tracks)
				if (track.Type == ComponentType.Media && track.SubType == ComponentSubType.Caption)
				{
					if(this.iso.Subtitle == null)
						this.iso.Subtitle = new ();

					this.AnalyzeCaption (track);
				}
        }

		private int GainCount (Track track, int chunk)
        {
			int n = track.SampleToChunks.Count;
			for (int i = 0; i < n; i++)
			{
				if (chunk < track.SampleToChunks[i].FirstChunk)
					continue;
				if (chunk == track.SampleToChunks[i].FirstChunk)
					return track.SampleToChunks[i].SamplesPerChunk;

				if(i + 1 < n)
                {
					if (chunk < track.SampleToChunks[i + 1].FirstChunk)
						return track.SampleToChunks[i].SamplesPerChunk;
					continue;
                }
                else
                {
					return track.SampleToChunks[i].SamplesPerChunk;
                }
			}

			return track.SampleSizeCount;
        }

		private void AnalyzeCaption (Track track)
        {
            Console.WriteLine ($"-- Caption : {track.DataFormats[0]}");
			Subtitle sub = new () { Type = track.DataFormats.Count > 0 ? track.DataFormats[0] : "Unknow" };

			for(int i = 0; i < track.ChunkOffsets.Count; i++)
            {
				int samplesCount = this.GainCount (track, i + 1);
				for(int k = 0; k < samplesCount; k++)
                {
					sub.Frames.Add (new byte[] { 0x96, 0x69, 3 });
                }
            }

			this.iso.Subtitle.Subtitles.Add (sub);
        }

	}
}
