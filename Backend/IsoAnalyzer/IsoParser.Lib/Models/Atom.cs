/***
 * Referance
 * QuickTime File Format Spectification, 2012-08-14
 ***/


using System;
using System.Collections.Generic;

namespace IsoParser.Lib.Models
{

    public class FullAtom : Atom {
		public string Version { get; set; }
		public int Flags { get; set; }
	}

	//Box: in ISO specifications for MPEG-4, JPEG-2000
	public class Atom {
		public int Id { get; set; }
		public AtomType? Type { get; set; }
		public long Offset { get; set; }
		public long Size { get; set; }

        #region QT atom
        public int Index { get; set; }
		public int QTId { get; set; }
		public int Children { get; set; }
        #endregion

        public List<Item> Items { get; set; }
		public List<Atom> Atoms { get; set; }

        public Atom () { }

        public Atom (int id, long size, long offset) {
			this.Id = id;
			this.Size = size;
			this.Offset = offset;

			if (Enum.IsDefined (typeof (AtomType), id))
				this.Type = (AtomType)id;
			else if (id == 1)
				this.Type = AtomType.TOP;
        }
	}

	public enum AtomType {
		alis = 0x616c6973,  //alis, Macintosh alias
		beha = 0x62656861,  //beha, sprite behavior
		btrt = 0x62747274,  //btrt, bit rate
		cdsc = 0x63647363,  //cdsc, reference in timed metaata track
		chan = 0x6368616e,  //chan, audio channel layout
		chap = 0x63686170,  //chap, chapter or scene list
		clap = 0x636c6170,  //clap, clean aperture
		clcp = 0x636c6370,  //clcp, closed caption
		clef = 0x636c6566,  //clef, track clean aperture dimension
		clip = 0x636c6970,  //clip, clipping
		cmov = 0x636d6f76,  //cmov, compressed movie
		colr = 0x636f6c72,  //colr, color parameters
		crgn = 0x6372676e,  //crgn, clipping region
		crsr = 0x63727372,  //crsr, sprite cursor behavior
		cslg = 0x63736c67,  //cslg, composition shift least greatest
		ctab = 0x63746162,  //ctab, color table
		ctry = 0x63747279,  //ctry, metadata contry list
		ctts = 0x63747473,  //ctts, composition offset
		data = 0x64617461,  //data, value atome
		defi = 0x64656669,  //defi, sprite image defualt image index
		dflt = 0x64666c74,  //dflt, sprite shared data
		dinf = 0x64696e66,  //dinf, data info
		dref = 0x64726566,  //dref, data ref
		drpo = 0x6472706f,  //drpo, text sample
		drpt = 0x64727074,  //drpt, text sample
		edts = 0x65647473,  //edts, edit
		elng = 0x656c6e67,  //elng, extended language tag
		elst = 0x656c7374,  //elst, edit list
		enof = 0x656e6f66,  //enof, track encoded pixels demension
		esds = 0x65736473,  //esds, elementary stream descriptor
		fall = 0x66616c6c,  //fall, identical content audio trck
		fiel = 0x6669656c,  //fiel, field handling, two 8-bit integers
		flov = 0x666c6f76,  //flov, sprite floating point variable
		folw = 0x666f6c77,  //folw, ref subtitle track from audio
		forc = 0x666f7263,  //forc, forced subtitle
		free = 0x66726565,  //free, free space
		frma = 0x66726d61,  //frma, format
		ftab = 0x67746162,  //ftab, text sample
		ftyp = 0x66747970,  //ftyp, file type compatibility
		gmhd = 0x676d6864,  //gmhd, base media info head
		gmin = 0x676d696e,  //gmin, base media info

		hclr = 0x68636c72,  //hclr, text sample
		hdlr = 0x68646c72,  //hdlr, handler ref
		hinf = 0x68696e66,  //hinf, hint track info
		hint = 0x68696e74,  //hint, ref original media
		hlit = 0x686c6974,  //hlit, text sample
		hnti = 0x686e7469,  //hnti, movie user data
		htxt = 0x68747874,  //htxt, hypertext
		idat = 0x69646174,  //idat, image data
		idsc = 0x69647363,  //idsc, image description
		ilst = 0x696c7374,  //ilst, metadata item list
		imda = 0x696d6461,  //imda, sprite imaged datatkhd
		imag = 0x696d6167,  //imag, text sample, sprite image
		imap = 0x696d6170,  //imap, track input map
		imct = 0x696d6374,  //imct, sprite images container
		imgr = 0x696d6772,  //imgr, sprite image group id
		imre = 0x696d7265,  //imre, sprite image data ref
		imrg = 0x696d7267,  //imrg, sprite image registration
		imrt = 0x696d7274,  //imrt, sprite image data ref type
		itif = 0x69746966,  //itif, item information
		keys = 0x6b657973,  //keys, metadata item keys
		kmat = 0x6b6d6174,  //kmat, compressed metta
		lang = 0x6c616e67,  //lang, metadata language list
		load = 0x6c6f6164,  //load, track load settings
		matt = 0x6d617474,  //matt, track metta
		mdat = 0x6d646174,  //mdat, media data
		mdia = 0x6d646961,  //mdia, media
		mdhd = 0x6d646864,  //mdhd, media header
		mdta = 0x6d647461,  //mdta, key namespace
		meta = 0x6d657461,  //meta, metadata
		metr = 0x6d657472,  //metr, text sample
		mhdr = 0x6d686472,  //mhdr, metadata header
		minf = 0x6d696e66,  //minf, media info
		moov = 0x6d6f6f76,  //moov, movie
		mvhd = 0x6d766864,  //mvhd, movie header
		name = 0x6e616d65,  //name, sprite name, image name

		obid = 0x6f626964,  //obid, object id
		pasp = 0x70617370,  //pasp, pixel aspect ratio
		pnot = 0x706e6f74,  //pnot, preview atom
		prfl = 0x7072666c,  //prfl, profile
		prof = 0x70726f66,  //prof, track production aperture dimensions
		rmda = 0x726d6461,  //rmda, reference movie descriptor
		rmra = 0x726d7261,  //rmra, ref movie
		rsrc = 0x72737363,  //rsrc, Macintosh alias
		sbgp = 0x73626770,  //sbgp, sample-to-group
		scpt = 0x73637074,  //scpt, transcript (ref a text track)
		sdtp = 0x73647470,  //sdtp, sample dependency flags
		sgpd = 0x73677064,  //sgpd, sample group description
		skip = 0x736b6970,  //skip, placeholder for unused space
		smhd = 0x736d6864,  //smhd, sound media info header
		sprt = 0x73707274,  //sprt, sprite
		ssrc = 0x73737263,  //ssrc, none primary source
		sstr = 0x73737472,  //sstr, sprite status string behavior
		stbl = 0x7374626c,  //stbl, sample table
		stco = 0x7374636f,  //stco, chunk offset
		stps = 0x73747073,  //stps, partial sync sample
		strv = 0x73747276,  //strv, sprite string variable
		stsc = 0x73747363,  //stsc, sample-to-chunk
		stsd = 0x73747364,  //stsd, sample description
		stsh = 0x73747368,  //stsh, shadow sync
		stss = 0x73747373,  //stss, sync sample
		stsz = 0x7374737a,  //stsz, sample size
		stts = 0x73747473,  //stts, time-to-sample
		styl = 0x7374796c,  //styl, text sample
		sync = 0x73796e63,  //sync, synchronization
		tapt = 0x74617074,  //tapt, track apeture mode dimensions
		tcmi = 0x74636d69,  //tcmi, timecode media info
		tkhd = 0x746b6864,  //tkhd, track header
		tmcd = 0x746d6364,  //tmcd, time code
		tnam = 0x746e616d,  //tnam, track name
		trak = 0x7472616b,  //trak, track
		tref = 0x74726566,  //tref, track reference
		txas = 0x74786173,  //txas, track exclude from autoselection

		udta = 0x75647461,  //udta, user data
		uses = 0x75736573,  //uses, spite uses image id
		vars = 0x76617273,  //vars, sprite variable container
		vmhd = 0x766d6864,  //vmhd, video media info header
		wave = 0x77617665,  //wave, decompression parameter
		wide = 0x77696465,  //wide, placeholder
		wtxt = 0x77747874,  //wtxt, wired text

		RTP = 0x72747020,  //'rtp ', real time protocal, hint sample data format
		URL = 0x75726c20,  //'url ', sprite url link

		IN = 0x2020696e,  // '  in'
		TY = 0x20207479,  // '  ty'

		#region self define
		TOP = 0x544f5020,   // 'top ', top level of mov file
		ITEM = 0x4954454d,  // item, metadata item correponding to key_index
		#endregion

		#region codec
		avc1 = 0x61766331,  //avc1, H.264 video
		avcC = 0x61766343,  //avcC, H.264 Configuration Box, 14496-15

		mp4a = 0x6d703461,  //mp4a, MPEG-4, advanced audio coding (AAC)

		c608 = 0x63363038,  //c608, closed caption format
		c708 = 0x63373038,  //c708, closed caption format
		#endregion
	}
	public enum GraphicsMode
    {
		Copy = 0,
		Blend = 0x20,  // Use Opcolor
		Transparent = 0x24,  // Use Opcolor
		DitherCopy = 0x40,
		StraightAlpha = 0x100,
		PremulWhiteAlpha = 0x101,
		PremulBlackAlpha = 0x102,
		Commposition = 0x103,
		StraightAlphaBlend = 0x104  // Use Opcolor
	}
}
