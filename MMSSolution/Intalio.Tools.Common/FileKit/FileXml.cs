using Microsoft.AspNetCore.Http;
using System.Xml.Linq;

namespace Intalio.Tools.Common.FileKit
{
	public static class FileXml
	{

		public static bool IsXml(IFormFile file)
		{
			try
			{
				using (Stream memoryStream = file.OpenReadStream())
				{
					XDocument doc = XDocument.Load(memoryStream);
					return true;
				}
			}
			catch
			{
				return false;
			}
		}
	}
}
