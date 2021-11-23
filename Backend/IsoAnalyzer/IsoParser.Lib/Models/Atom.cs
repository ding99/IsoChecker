using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsoParser.Lib.Models {
	public class Atom {
		public string Id { get; set; }
		public long Size { get; set; }
		public long Offset { get; set; }

		public List<Item> Items { get; set; }
		public List<Atom> Atoms { get; set; }
	}

	public enum AtomType {
		CLEF = 0x636c6566,  //clef
		CLIP = 0x636c6970,  //clip
		CMOV = 0x636d6f76,  //cmov
		COLR = 0x636f6c72,  //colr
		CRGN = 0x6372676e,  //crgn
		CSLG = 0x63736c67,  //cslg
		CTAB = 0x63746162,  //ctab
		CTTS = 0x63747473,  //ctts
		DINF = 0x64696e66,  //dinf
		DREF = 0x64726566,  //dref
		EDTS = 0x65647473,  //edts
		ELNG = 0x656c6e67,  //elng
		ELST = 0x656c7374,  //elst
		ENOF = 0x656e6f66,  //enof
		FREE = 0x66726565,  //free
		FTYP = 0x66747970,  //ftyp
		GMHD = 0x676d6864,  //gmhd
		GMIN = 0x676d696e,  //gmin

		HDLR = 0x68646c72,  //hdlr
		IDAT = 0x69646174,  //idat
		IDSC = 0x69647363,  //idsc
		IMAP = 0x696d6170,  //imap
		KMAT = 0x6b6d6174,  //kmat
		LOAD = 0x6c6f6166,  //load
		MATT = 0x6d617474,  //matt
		MDAT = 0x6d646174,  //mdat
		MDIA = 0x6d646961,  //mdia
		MDHD = 0x6d646864,  //mdhd
		META = 0x6d657461,  //meta
		MINF = 0x6d696e66,  //minf
		MOOV = 0x6d6f6f76,  //moov
		MVHD = 0x6d766864,  //mvhd

		OBID = 0x6f626964,  //obid
		PNOT = 0x706e6f74,  //pnot
		PRFL = 0x7072666c,  //prfl
		PROF = 0x70726f66,  //prof
		RMDA = 0x726d6461,  //rmda
		RMRA = 0x726d7261,  //rmra
		SBGP = 0x73626770,  //sbgp
		SDTP = 0x73647470,  //sdtp
		SGPD = 0x73677064,  //sgpd
		SKIP = 0x736b6970,  //skip
		SMHD = 0x736d6864,  //smhd
		STBL = 0x7374626c,  //stbl
		STCO = 0x7374636f,  //stco
		STPS = 0x73747073,  //stps
		STSC = 0x73747363,  //stsc
		STSD = 0x73747364,  //stsd
		STSS = 0x73747373,  //stss
		STSZ = 0x7374737a,  //stsz
		STTS = 0x73747473,  //stts
		TAPT = 0x74617074,  //tapt
		TKHD = 0x746b6864,  //tkhd
		TRAK = 0x7472616b,  //trak
		TREF = 0x74726566,  //tref
		TXAS = 0x74786173,  //txas

		UDTA = 0x75647461,  //udta
		VMHD = 0x766d6864,  //vmhd
		WIDE = 0x77696465,  //wide

		IN = 0x2020696e,  // '  in'
		TY = 0x20207479,  // '  ty'
	}
}
