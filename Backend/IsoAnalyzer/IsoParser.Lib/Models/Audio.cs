﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsoParser.Lib.Models
{
    public class Audio
    {
        public string Codec { get; set; }
        public int Bitrate { get; set; }
        public int ChannelsCount { get; set; }
    }
}
