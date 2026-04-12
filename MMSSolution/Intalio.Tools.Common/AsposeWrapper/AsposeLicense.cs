namespace Intalio.Tools.Common.AsposeWrapper
{
	public class AsposeLicense
	{
		public static void SetLicense()
		{
			new Aspose.Words.License().SetLicense("Aspose.Total.Product.Family.lic");
			new Aspose.Pdf.License().SetLicense("Aspose.Total.Product.Family.lic");
			new Aspose.BarCode.License().SetLicense("Aspose.Total.Product.Family.lic");
			new Aspose.Slides.License().SetLicense("Aspose.Total.Product.Family.lic");
		}
	}
}
