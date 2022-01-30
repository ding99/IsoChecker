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

		private void AnalyzeCaption(Track track)
        {
            Console.WriteLine ($"-- Caption : {track.DataFormats[0]}");
        }

	}
}
