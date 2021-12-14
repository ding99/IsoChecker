namespace IsoParser.Lib.Models {
	public class Item {
		public string Name { get; set; }
		public ItemType Type { get; set; }
		public object Value { get; set; }
	}
	public enum ItemType {
		None = 0,
		Bool,
		Byte,
		Short,
		UShort,
		Int,
		Long,
		Double,
		String,
		Matrix
	}
}
