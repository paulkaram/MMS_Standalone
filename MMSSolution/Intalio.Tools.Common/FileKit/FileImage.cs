using Aspose.BarCode.Generation;
using Microsoft.AspNetCore.Http;

namespace Intalio.Tools.Common.FileKit
{
	public static class FileImage
	{
		public static byte[]? GenerateQRcode(string text)
		{
			if (!string.IsNullOrWhiteSpace(text))
			{
				using (MemoryStream outstream = new())
				{
					BarcodeGenerator generator = new BarcodeGenerator(EncodeTypes.QR, text);
					generator.Parameters.Barcode.CodeTextParameters.Location = CodeLocation.None;
					generator.Save(outstream, BarCodeImageFormat.Png);
					return outstream.ToArray();
				}
			}
			return null;
		}

		public static byte[]? ConvertImageToPDF(byte[] bytes)
		{
			if (bytes == null)
			{
				return null;
			}

			using (MemoryStream outstream = new())
			{
				using (MemoryStream ms = new(bytes))
				{
					System.Drawing.Image srcImage = System.Drawing.Image.FromStream(ms);
					int h = srcImage.Height;
					int w = srcImage.Width;

					Aspose.Pdf.Document doc = new();
					Aspose.Pdf.Page page = doc.Pages.Add();
					Aspose.Pdf.Image image = new();
					image.ImageStream = ms;

					// Set page dimensions
					page.PageInfo.Height = h;
					page.PageInfo.Width = w;
					page.PageInfo.Margin.Bottom = 0;
					page.PageInfo.Margin.Top = 0;
					page.PageInfo.Margin.Right = 0;
					page.PageInfo.Margin.Left = 0;
					page.Paragraphs.Add(image);

					doc.Save(outstream, Aspose.Pdf.SaveFormat.Pdf);
					return outstream.ToArray();
				}
			}
		}

		//source: https://stackoverflow.com/questions/772388/c-sharp-how-can-i-test-a-file-is-a-jpeg
		public static bool IsJpeg(IFormFile file)
		{
			using (BinaryReader br = new BinaryReader(file.OpenReadStream()))
			{
				ushort soi = br.ReadUInt16();  // Start of Image (SOI) marker (FFD8)
				ushort marker = br.ReadUInt16(); // JFIF marker (FFE0) or EXIF marker(FFE1)

				return soi == 0xd8ff && (marker & 0xe0ff) == 0xe0ff;
			}
		}

		//source: https://asecuritysite.com/forensics/magic
		public static bool IsPNG(IFormFile file)
		{
			using (BinaryReader br = new BinaryReader(file.OpenReadStream()))
			{
				ushort soi = br.ReadUInt16();  // Start of Image (SOI) marker (FFD8)
				ushort marker = br.ReadUInt16(); // JFIF marker (FFE0) or EXIF marker(FFE1)

				return soi == 0x5089 && (marker & 0x474e) == 0x474e;
			}
		}

		public static bool IsPNG(byte[] bytes)
		{
			using (MemoryStream ms = new MemoryStream(bytes))
			{
				using (BinaryReader br = new BinaryReader(ms))
				{
					ushort soi = br.ReadUInt16();  // Start of Image (SOI) marker (FFD8)
					ushort marker = br.ReadUInt16(); // JFIF marker (FFE0) or EXIF marker(FFE1)

					return soi == 0x5089 && (marker & 0x474e) == 0x474e;
				}
			}
		}
	}
}
