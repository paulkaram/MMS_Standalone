namespace Intalio.Tools.Common.Objects
{
	public class FileBookmark
	{
		private const double WIDTH_VALUE = 150;
		private const double HEIGHT_VALUE = 30;

		public string Name { get; set; }
		public string Value { get; set; }
		public bool IsImage { get; set; }
		public double Height { get; set; }
		public double Width { get; set; }

		public FileBookmark(string name, string? value, bool isImage = false)
		{
			Name = name;
			Value = string.IsNullOrEmpty(value) ? "" : value;
			IsImage = isImage;
			Width = WIDTH_VALUE;
			Height = HEIGHT_VALUE;
		}

		public FileBookmark(string name, bool isImage = false)
		{
			Name = name;
			Value = "";
			IsImage = isImage;
			Width = WIDTH_VALUE;
			Height = HEIGHT_VALUE;
		}

		public FileBookmark(string name, string value, double height, double width)
		{
			Name = name;
			Value = string.IsNullOrEmpty(value) ? "" : value;
			IsImage = true;
			Height = height;
			Width = width;
		}

	}
}
