namespace Intalio.Tools.Common.Objects
{
	public class FileStamp
	{
		public int ImageHeight { get; set; }
		public int ImageWidth { get; set; }
		public int ImageX { get; set; }
		public int ImageY { get; set; }
		public int PageNumber { get; set; }
		public int PageHeight { get; set; }
		public int PageWidth { get; set; }
		public string StampBase64 { get; set; } = null!;
	}
}
