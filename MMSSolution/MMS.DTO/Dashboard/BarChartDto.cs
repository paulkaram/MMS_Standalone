namespace MMS.DTO.Dashboard
{
	public class BarChartDto
	{
		public List<string> Labels { get; set; } = new List<string>();
		public List<BarChartSeries> Series { get; set; }= new List<BarChartSeries>();
	}
}
