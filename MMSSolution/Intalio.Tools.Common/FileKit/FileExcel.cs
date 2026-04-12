using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Intalio.Tools.Common.FileKit.Attributes;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Intalio.Tools.Common.FileKit
{
	public static class FileExcel
	{

		public static byte[] ExportToExcel<T>(List<T> list)
		{
			using MemoryStream ms = new();
			using (SpreadsheetDocument document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook))
			{
				WorkbookPart workbookPart = document.AddWorkbookPart();
				workbookPart.Workbook = new Workbook();

				Sheets excelSheets = workbookPart.Workbook.AppendChild(new Sheets());

				//generate the style for all sheets
				WorkbookStylesPart stylePart = workbookPart.AddNewPart<WorkbookStylesPart>();
				stylePart.Stylesheet = GenerateStyleSheet();
				stylePart.Stylesheet.Save();


				uint sheetId = 1;
				PropertyInfo[] properties = list.GetType().GetGenericArguments()[0].GetProperties();
				CreateSheet(workbookPart, excelSheets, sheetId++, "Data", properties);
				AddDataToSheet(workbookPart, "Data", list);

				workbookPart.Workbook.Save();
			}
			return ms.ToArray();
		}

		public static DataTable? ReadExcel(Stream dataStream)
		{
			using SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(dataStream, false);
			WorkbookPart? workbookPart = spreadSheetDocument.WorkbookPart;
			if (workbookPart != null)
			{
				string relationshipId = workbookPart.Workbook.GetFirstChild<Sheets>()?.Elements<Sheet>()?.First()?.Id?.Value ?? "";
				if (!string.IsNullOrWhiteSpace(relationshipId))
				{

					IEnumerable<Row>? rows = ((WorksheetPart)workbookPart.GetPartById(relationshipId)).Worksheet.GetFirstChild<SheetData>()?.Descendants<Row>();
					if (rows != null && rows.Any())
					{

						DataTable dt = new();
						// add the header to datatable
						foreach (Cell cell in rows.ElementAt(0).Cast<Cell>())
						{
							dt.Columns.Add(GetCellValue(spreadSheetDocument, cell));
						}

						//add the rows to datatable
						for (int rowId = 1; rowId < rows.Count(); rowId++)
						{
							DataRow tempRow = dt.NewRow();

							for (int colId = 0; colId < dt.Columns.Count; colId++)
							{
								tempRow[colId] = GetCellValue(spreadSheetDocument, rows.ElementAt(rowId).Descendants<Cell>().ElementAt(colId));
							}

							dt.Rows.Add(tempRow);
						}

						return dt;
					}

				}
			}

			return null;
		}

		public static object ReadExcel(byte[] bytes)
		{
			List<object> retval = new();
			using (Stream memoryStream = new MemoryStream(bytes))
			{
				using SpreadsheetDocument doc = SpreadsheetDocument.Open(memoryStream, false);
				WorksheetPart? worksheetPart = doc.WorkbookPart?.WorksheetParts?.FirstOrDefault();
				if (worksheetPart != null)
				{
					using OpenXmlReader reader = OpenXmlReader.Create(worksheetPart);
					while (reader.Read())
					{
						if (reader.ElementType == typeof(Row))
						{
							int index = 0;
							reader.ReadFirstChild();

							ExpandoObject row = new();
							do
							{
								if (reader.ElementType == typeof(Cell))
								{
									row.TryAdd("column" + index++, reader.LoadCurrentElement() is not Cell cell ? null : GetCellValue(doc, cell));
								}
							} while (reader.ReadNextSibling());

							retval.Add(row);
						}
					}
				}
			}

			return retval;
		}

		public static bool IsExcel(IFormFile file)
		{
			try
			{
				using Stream memoryStream = file.OpenReadStream();
				using SpreadsheetDocument doc = SpreadsheetDocument.Open(memoryStream, false);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool IsExcel(byte[] bytes)
		{
			try
			{
				using MemoryStream ms = new(bytes);
				using SpreadsheetDocument doc = SpreadsheetDocument.Open(ms, false);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static void AddDataToSheet<T>(WorkbookPart workbookPart, string sheetName, IEnumerable<T>? list, object[]? primaryKeyValues = null)
		{
			if (list != null)
			{
				PropertyInfo[] properties = list.GetType().GetGenericArguments()[0].GetProperties();

				string? sheetId = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault(s =>  s.Name != null && s.Name.Equals(sheetName))?.Id;
				if (!string.IsNullOrWhiteSpace(sheetId))
				{
					SheetData? sheetData = ((WorksheetPart)workbookPart.GetPartById(sheetId)).Worksheet.Elements<SheetData>().FirstOrDefault();
					if (sheetData != null)
					{
						uint rowIndex = sheetData.Elements<Row>().LastOrDefault()?.RowIndex ?? 0;

						foreach (T item in list)
						{
							var columnASCII = 65;
							var row = new Row { RowIndex = ++rowIndex };
							foreach (PropertyInfo property in properties)
							{
								if (property.PropertyType is { IsGenericType: true, IsValueType: false })
								{
									if (workbookPart.Workbook.Descendants<Sheet>().Any(s => s.Name != null && s.Name.Equals(property.Name)))
									{
										PropertyInfo[] propertiesOfList = property.PropertyType.GetGenericArguments()[0].GetProperties();
										PropertyInfo[] primaryProperties = properties.Where(x => Attribute.IsDefined(x, typeof(PrimaryKeyForSheet))).ToArray();
										List<object> values = new();
										foreach (PropertyInfo primary in primaryProperties)
										{
											var val = primary.GetValue(item, null);
											values.Add(val ?? "");
										}

										AddDataToSheet(workbookPart, property.Name, property.GetValue(item, null) as IEnumerable<object>, values.ToArray());
									}
								}
								else
								{
									row.Append(new Cell { CellValue = new CellValue(Convert.ToString(property.GetValue(item, null)) ?? string.Empty), DataType = CellValues.String, StyleIndex = 2, CellReference = ((char)columnASCII).ToString() });
									columnASCII++;
								}
							}

							if (primaryKeyValues != null && primaryKeyValues.Length > 0)
							{
								foreach (object primaryKey in primaryKeyValues)
								{
									row.Append(new Cell { CellValue = new CellValue(Convert.ToString(primaryKey) ?? string.Empty), DataType = CellValues.String, StyleIndex = 2, CellReference = ((char)columnASCII).ToString() });
									columnASCII++;
								}
							}
							sheetData.AppendChild(row);
						}
					}
				}

				
			}
		}

		private static void CreateSheet(WorkbookPart workbookPart, Sheets excelSheets, uint sheetId, string sheetName, PropertyInfo[] properties, PropertyInfo[]? primaryProperties = null)
		{
			WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
			worksheetPart.Worksheet = new();

			//create sheet columns
			Columns columns = new();
			for (int i = 0; i < properties.Length; i++)
			{
				columns.Append(new Column { Min = 1U, Max = 1U, Width = 24D, CustomWidth = true });
			}
			//add columns for primary key if exists
			if (primaryProperties != null)
			{
				for (int i = 0; i < primaryProperties.Length; i++)
				{
					columns.Append(new Column { Min = 1U, Max = 1U, Width = 24D, CustomWidth = true });
				}
			}
			worksheetPart.Worksheet.AppendChild(columns);

			//create new sheet
			Sheet addSheet = new()
			{
				Id = workbookPart.GetIdOfPart(worksheetPart),//relation between sheet and worksheetPart
				SheetId = sheetId,
				Name = sheetName
			};
			excelSheets.Append(addSheet);

			//add new sheetdata to add data to excel
			SheetData sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());

			//add header

			Row row = new() { RowIndex = 1 };
			int columnASCII = 65;
			foreach (PropertyInfo item in properties)
			{
				if (item.PropertyType.IsGenericType && Nullable.GetUnderlyingType(item.PropertyType) == null)
				{
					if (!workbookPart.Workbook.Descendants<Sheet>().Any(s => s.Name != null && s.Name.Equals(item.Name)))
					{
						PropertyInfo[] propertiesOfList = item.PropertyType.GetGenericArguments()[0].GetProperties();
						CreateSheet(workbookPart, excelSheets, ++sheetId, item.Name, propertiesOfList, properties.Where(x => Attribute.IsDefined(x, typeof(PrimaryKeyForSheet))).ToArray());
					}
				}
				else
				{
					row.Append(new Cell { CellValue = new(item.Name), DataType = CellValues.String, StyleIndex = 1, CellReference = ((char)columnASCII).ToString() });
					columnASCII++;
				}
			}
			if (primaryProperties != null)
			{
				foreach (PropertyInfo item in primaryProperties)
				{
					if (Attribute.GetCustomAttribute(item, typeof(PrimaryKeyForSheet)) is PrimaryKeyForSheet attribute)
					{
						row.Append(new Cell { CellValue = new(attribute.Name), DataType = CellValues.String, StyleIndex = 1, CellReference = ((char)columnASCII).ToString() });
						columnASCII++;
					}
				}
			}
			sheetData.AppendChild(row);
		}

		private static Stylesheet GenerateStyleSheet()
		{
			Fonts fonts = new(
				new Font(new FontSize() { Val = 11 }),// Index 0 - default
				new Font( // Index 1 - header
					new FontSize() { Val = 11 },
					new Bold(),
					new Color() { Rgb = "FFFFFF" }));

			Fills fills = new(
					new Fill(new PatternFill() { PatternType = PatternValues.None }), // Index 0 - default
					new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }), // Index 1 - default
					new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue() { Value = "009688" } })
					{ PatternType = PatternValues.Solid })); // Index 2 - header

			Borders borders = new(
					new Border(), // index 0 default
					new Border( // index 1 black border
						new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
						new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
						new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
						new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
						new DiagonalBorder())
				);

			CellFormats cellFormats = new(
					new CellFormat(), // default
					new CellFormat { FontId = 1, FillId = 2, BorderId = 1, ApplyFill = true }, // header
					new CellFormat { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true });// body

			return new Stylesheet(fonts, fills, borders, cellFormats);
		}

		private static string GetCellValue(SpreadsheetDocument document, Cell cell)
		{
			if(cell.CellValue == null)
			{
				return string.Empty;
			}

			string value = cell.CellValue.InnerXml;
			if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
			{
				SharedStringTablePart? stringTablePart = document?.WorkbookPart?.SharedStringTablePart;
				return stringTablePart == null 
					? string.Empty 
					: stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
			}
			else
			{
				return value;
			}
		}
	}
}
