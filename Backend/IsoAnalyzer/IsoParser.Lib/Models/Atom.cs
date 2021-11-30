/***
 * Referance
 * QuickTime File Format Spectification, 2012-08-14
 ***/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsoParser.Lib.Models {

	public class FullAtom : Atom {
		public string Version { get; set; }
		public int Flags { get; set; }
	}

	//Box: in ISO specifications for MPEG-4, JPEG-2000
	public class Atom {
		public int Id { get; set; }
		public int Index { get; set; }
		public AtomType? Type { get; set; }
		public long Offset { get; set; }
		public long Size { get; set; }
		public int Head { get; set; }

		public List<Item> Items { get; set; }
		public List<Atom> Atoms { get; set; }

		public Atom () { this.Head = 0; }

		public Atom (int id, long size, long offset, int head = 0) {
			this.Id = id;
			this.Size = size;
			this.Offset = offset;
			this.Head = head;
        }
	}

	public enum AtomType {
		BEHA = 0x62656861,  //beha, sprite behavior
		CHAP = 0x63686170,  //chap, chapter or scene list
		CLCP = 0x636c6370,  //clcp, closed caption
		CLEF = 0x636c6566,  //clef
		CLIP = 0x636c6970,  //clip, clipping
		CMOV = 0x636d6f76,  //cmov, compressed movie
		COLR = 0x636f6c72,  //colr
		CRGN = 0x6372676e,  //crgn
		CRSR = 0x63727372,  //crsr, sprite cursor behavior
		CSLG = 0x63736c67,  //cslg
		CTAB = 0x63746162,  //ctab, color table
		CTRY = 0x63747279,  //ctry
		CTTS = 0x63747473,  //ctts
		DEFI = 0x64656669,  //defi, sprite image defualt image index
		DFLT = 0x64666c74,  //dflt, sprite shared data
		DINF = 0x64696e66,  //dinf
		DREF = 0x64726566,  //dref
		DRPO = 0x6472706f,  //drpo, text sample
		DRPT = 0x64727074,  //drpt, text sample
		EDTS = 0x65647473,  //edts
		ELNG = 0x656c6e67,  //elng
		ELST = 0x656c7374,  //elst
		ENOF = 0x656e6f66,  //enof
		FALL = 0x66616c6c,  //fall, identical content audio trck
		FLOV = 0x666c6f76,  //flov, sprite floating point variable
		FOLW = 0x666f6c77,  //folw, ref subtitle track from audio
		FORC = 0x666f7263,  //forc, forced subtitle
		FREE = 0x66726565,  //free
		FTAB = 0x67746162,  //ftab, text sample
		FTYP = 0x66747970,  //ftyp
		GMHD = 0x676d6864,  //gmhd
		GMIN = 0x676d696e,  //gmin

		HCLR = 0x68636c72,  //hclr, text sample
		HDLR = 0x68646c72,  //hdlr
		HINT = 0x68696e74,  //hint, ref original media
		HLIT = 0x686c6974,  //hlit, text sample
		HTXT = 0x68747874,  //htxt, hypertext
		IDAT = 0x69646174,  //idat
		IDSC = 0x69647363,  //idsc
		ILST = 0x696c7374,  //ilst
		IMDA = 0x696d6461,  //imda, sprite imaged data
		IMAG = 0x696d6167,  //imag, text sample, sprite image
		IMAP = 0x696d6170,  //imap
		IMCT = 0x696d6374,  //imct, sprite images container
		IMGR = 0x696d6772,  //imgr, sprite image group id
		IMRE = 0x696d7265,  //imre, sprite image data ref
		IMRG = 0x696d7267,  //imrg, sprite image registration
		IMRT = 0x696d7274,  //imrt, sprite image data ref type
		KEYS = 0x6b657973,  //keys
		KMAT = 0x6b6d6174,  //kmat
		LANG = 0x6c616e67,  //lang
		LOAD = 0x6c6f6164,  //load
		MATT = 0x6d617474,  //matt
		MDAT = 0x6d646174,  //mdat
		MDIA = 0x6d646961,  //mdia
		MDHD = 0x6d646864,  //mdhd
		META = 0x6d657461,  //meta
		METR = 0x6d657472,  //metr, text sample
		MHDR = 0x6d686472,  //mhdr
		MINF = 0x6d696e66,  //minf
		MOOV = 0x6d6f6f76,  //moov
		MVHD = 0x6d766864,  //mvhd
		NAME = 0x6e616d65,  //name, sprite name, image name

		OBID = 0x6f626964,  //obid
		PNOT = 0x706e6f74,  //pnot, preview atom
		PRFL = 0x7072666c,  //prfl
		PROF = 0x70726f66,  //prof
		RMDA = 0x726d6461,  //rmda
		RMRA = 0x726d7261,  //rmra, ref movie
		SBGP = 0x73626770,  //sbgp
		SCPT = 0x73637074,  //scpt, transcript (ref a text track)
		SDTP = 0x73647470,  //sdtp
		SGPD = 0x73677064,  //sgpd
		SKIP = 0x736b6970,  //skip
		SMHD = 0x736d6864,  //smhd
		SPRT = 0x73707274,  //sprt, sprite
		SSRC = 0x73737263,  //ssrc, none primary source
		SSTR = 0x73737472,  //sstr, sprite status string behavior
		STBL = 0x7374626c,  //stbl
		STCO = 0x7374636f,  //stco
		STPS = 0x73747073,  //stps
		STRV = 0x73747276,  //strv, sprite string variable
		STSC = 0x73747363,  //stsc
		STSD = 0x73747364,  //stsd
		STSS = 0x73747373,  //stss
		STSZ = 0x7374737a,  //stsz
		STTS = 0x73747473,  //stts
		STYL = 0x7374796c,  //styl, text sample
		SYNC = 0x73796e63,  //sync, synchronization
		TAPT = 0x74617074,  //tapt
		TKHD = 0x746b6864,  //tkhd
		TMCD = 0x746d6364,  //tmcd, time code
		TRAK = 0x7472616b,  //trak
		TREF = 0x74726566,  //tref
		TXAS = 0x74786173,  //txas

		UDTA = 0x75647461,  //udta, user data
		USES = 0x75736573,  //uses, spite uses image id
		VARS = 0x76617273,  //vars, sprite variable container
		VMHD = 0x766d6864,  //vmhd
		WIDE = 0x77696465,  //wide
		WTXT = 0x77747874,  //wtxt, wired text

		URL = 0x75726c20,  //'url ', sprite url link

		IN = 0x2020696e,  // '  in'
		TY = 0x20207479,  // '  ty'
	}
}
