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

                    Console.WriteLine ($"subtitles is null {this.iso.Subtitle.Subtitles == null}");
                    Console.WriteLine ($"subtitles count {this.iso.Subtitle.Subtitles.Count}");
					this.AnalyzeCaption (track);
				}
        }

		private void AnalyzeCaption(Track track)
        {

        }

	}
}
