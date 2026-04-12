namespace MMS.DTO
{
	public class StampDto
	{
		public AnnotationTypeEnum AnnotationType { get; set; }
		public List<int>? Color { get; set; }
		public int FontSize { get; set; }
		public string? Value { get; set; }
		public int PageIndex { get; set; }
		public double[]? Rect { get; set; }
		public int Rotation { get; set; }
		public int? StampType { get; set; }
		public string? StampData { get; set; }
		public string? Language { get; set; }
		public int? Thickness { get; set; }
		public int? Opacity { get; set; }
		public List<Path>? Paths { get; set; }
	}

	public enum AnnotationTypeEnum
	{
		Stamp = 21,
		Signature = 23,
		Text = 3,
		Draw = 15
	}
	public class Path
	{
		public List<double>? bezier { get; set; }
		public List<double>? points { get; set; }
	}

}
