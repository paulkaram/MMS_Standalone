namespace MMS.DTO.Dashboard
{
	public class DougnutChartDto
	{
		public List<string> Labels { get; set; }=new List<string>();
		public List<decimal> Values { get; set; } =new List<decimal>();
	}
}
