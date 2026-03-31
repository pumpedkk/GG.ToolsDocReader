using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GGTools.FileReaders
{
    public static class DocWriter
    {
        /// <summary>
        /// Exports a Table object (Inspector-friendly) to CSV with configurable delimiter.
        /// </summary>
        /// <param name="filePath">Output CSV file path.</param>
        /// <param name="table">Table object created in Unity Inspector.</param>
        /// <param name="delimiter">CSV delimiter (e.g. ',', ';').</param>
        public static void ExportCsv(string filePath, Table table, char delimiter = ',')
        {
            ValidateUnityTable(table);
            ExportCsv(filePath, table.ToIList(), delimiter);
        }

        /// <summary>
        /// Exports tabular data to CSV with configurable delimiter.
        /// </summary>
        /// <param name="filePath">Output CSV file path.</param>
        /// <param name="table">Table data (rows and columns).</param>
        /// <param name="delimiter">CSV delimiter (e.g. ',', ';').</param>
        public static void ExportCsv(string filePath, IList<IList<string>> table, char delimiter = ',')
        {
            string resolvedPath = PrepareOutputPath(filePath, ".csv", "export");
            ValidateTable(table);

            StringBuilder builder = new StringBuilder();

            for (int row = 0; row < table.Count; row++)
            {
                IList<string> columns = table[row] ?? Array.Empty<string>();
                for (int col = 0; col < columns.Count; col++)
                {
                    if (col > 0)
                    {
                        builder.Append(delimiter);
                    }

                    builder.Append(EscapeCsv(columns[col], delimiter));
                }

                if (row < table.Count - 1)
                {
                    builder.AppendLine();
                }
            }

            File.WriteAllText(resolvedPath, builder.ToString(), new UTF8Encoding(true));
        }

        /// <summary>
        /// Exports tabular data to XLSX (single worksheet).
        /// </summary>
        /// <param name="filePath">Output XLSX file path.</param>
        /// <param name="table">Table data (rows and columns).</param>
        /// <param name="sheetName">Worksheet name.</param>
        public static void ExportXlsx(string filePath, IList<IList<string>> table, string sheetName = "Sheet1")
        {
            string resolvedPath = PrepareOutputPath(filePath, ".xlsx", "export");
            ValidateTable(table);
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                sheetName = "Sheet1";
            }

            if (File.Exists(resolvedPath))
            {
                File.Delete(resolvedPath);
            }

            using (FileStream stream = new FileStream(resolvedPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create, false))
            {
                WriteEntry(archive, "[Content_Types].xml", BuildContentTypesXml());
                WriteEntry(archive, "_rels/.rels", BuildRootRelsXml());
                WriteEntry(archive, "xl/workbook.xml", BuildWorkbookXml(sheetName));
                WriteEntry(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRelsXml());
                WriteEntry(archive, "xl/styles.xml", BuildStylesXml());
                WriteEntry(archive, "xl/worksheets/sheet1.xml", BuildWorksheetXml(table));
            }
        }

        /// <summary>
        /// Exports a Table object (Inspector-friendly) to XLSX.
        /// </summary>
        /// <param name="filePath">Output XLSX file path.</param>
        /// <param name="table">Table object created in Unity Inspector.</param>
        /// <param name="sheetName">Worksheet name.</param>
        public static void ExportXlsx(string filePath, Table table, string sheetName = "Sheet1")
        {
            ValidateUnityTable(table);
            ExportXlsx(filePath, table.ToIList(), sheetName);
        }

        /// <summary>
        /// Converts a CSV file to XLSX.
        /// </summary>
        /// <param name="csvFilePath">Source CSV path.</param>
        /// <param name="xlsxFilePath">Output XLSX path.</param>
        /// <param name="delimiter">CSV delimiter used in source file.</param>
        /// <param name="sheetName">Worksheet name.</param>
        public static void ConvertCsvToXlsx(string csvFilePath, string xlsxFilePath, char delimiter = ',', string sheetName = "Sheet1")
        {
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException("Arquivo CSV não encontrado", csvFilePath);
            }

            List<List<string>> table = ParseCsv(File.ReadAllText(csvFilePath), delimiter);
            ExportXlsx(xlsxFilePath, table.Select(row => (IList<string>)row).ToList(), sheetName);
        }

        /// <summary>
        /// Converts a XLSX file (first worksheet) to CSV.
        /// </summary>
        /// <param name="xlsxFilePath">Source XLSX path.</param>
        /// <param name="csvFilePath">Output CSV path.</param>
        /// <param name="delimiter">Delimiter for output CSV.</param>
        public static void ConvertXlsxToCsv(string xlsxFilePath, string csvFilePath, char delimiter = ',')
        {
            if (!File.Exists(xlsxFilePath))
            {
                throw new FileNotFoundException("Arquivo XLSX não encontrado", xlsxFilePath);
            }

            List<List<string>> table = ReadFirstWorksheet(xlsxFilePath);
            ExportCsv(csvFilePath, table.Select(row => (IList<string>)row).ToList(), delimiter);
        }

        private static void ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("O caminho do arquivo não pode ser vazio.", nameof(filePath));
            }
        }

        private static string PrepareOutputPath(string filePath, string extension, string defaultFileName)
        {
            ValidateFilePath(filePath);

            string resolved = filePath.Trim();

            // If a directory is provided, create a default file inside it.
            if (Directory.Exists(resolved))
            {
                resolved = Path.Combine(resolved, defaultFileName + extension);
            }
            else if (!Path.HasExtension(resolved))
            {
                resolved += extension;
            }

            string directory = Path.GetDirectoryName(resolved);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return resolved;
        }

        private static void ValidateTable(IList<IList<string>> table)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table), "A tabela não pode ser nula.");
            }
        }

        private static void ValidateUnityTable(Table table)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table), "A Table não pode ser nula.");
            }
        }

        private static string EscapeCsv(string value, char delimiter)
        {
            if (value == null)
            {
                return string.Empty;
            }

            bool mustQuote = value.IndexOfAny(new[] { '"', '\r', '\n', delimiter }) >= 0;
            if (!mustQuote)
            {
                return value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static List<List<string>> ParseCsv(string csvText, char delimiter)
        {
            List<List<string>> rows = new List<List<string>>();
            List<string> currentRow = new List<string>();
            StringBuilder currentCell = new StringBuilder();

            bool inQuotes = false;

            for (int i = 0; i < csvText.Length; i++)
            {
                char c = csvText[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        bool isEscapedQuote = i + 1 < csvText.Length && csvText[i + 1] == '"';
                        if (isEscapedQuote)
                        {
                            currentCell.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        currentCell.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == delimiter)
                    {
                        currentRow.Add(currentCell.ToString());
                        currentCell.Length = 0;
                    }
                    else if (c == '\r')
                    {
                        if (i + 1 < csvText.Length && csvText[i + 1] == '\n')
                        {
                            i++;
                        }

                        currentRow.Add(currentCell.ToString());
                        currentCell.Length = 0;
                        rows.Add(currentRow);
                        currentRow = new List<string>();
                    }
                    else if (c == '\n')
                    {
                        currentRow.Add(currentCell.ToString());
                        currentCell.Length = 0;
                        rows.Add(currentRow);
                        currentRow = new List<string>();
                    }
                    else
                    {
                        currentCell.Append(c);
                    }
                }
            }

            if (currentCell.Length > 0 || currentRow.Count > 0 || csvText.EndsWith(delimiter.ToString(), StringComparison.Ordinal))
            {
                currentRow.Add(currentCell.ToString());
                rows.Add(currentRow);
            }

            return rows;
        }

        private static void WriteEntry(ZipArchive archive, string path, string content)
        {
            ZipArchiveEntry entry = archive.CreateEntry(path, CompressionLevel.Optimal);
            using (StreamWriter writer = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
            {
                writer.Write(content);
            }
        }

        private static string BuildContentTypesXml()
        {
            return new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(XName.Get("Types", "http://schemas.openxmlformats.org/package/2006/content-types"),
                    new XElement(XName.Get("Default", "http://schemas.openxmlformats.org/package/2006/content-types"),
                        new XAttribute("Extension", "rels"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                    new XElement(XName.Get("Default", "http://schemas.openxmlformats.org/package/2006/content-types"),
                        new XAttribute("Extension", "xml"),
                        new XAttribute("ContentType", "application/xml")),
                    new XElement(XName.Get("Override", "http://schemas.openxmlformats.org/package/2006/content-types"),
                        new XAttribute("PartName", "/xl/workbook.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),
                    new XElement(XName.Get("Override", "http://schemas.openxmlformats.org/package/2006/content-types"),
                        new XAttribute("PartName", "/xl/worksheets/sheet1.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml")),
                    new XElement(XName.Get("Override", "http://schemas.openxmlformats.org/package/2006/content-types"),
                        new XAttribute("PartName", "/xl/styles.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"))
                )
            ).ToString(SaveOptions.DisableFormatting);
        }

        private static string BuildRootRelsXml()
        {
            return new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(XName.Get("Relationships", "http://schemas.openxmlformats.org/package/2006/relationships"),
                    new XElement(XName.Get("Relationship", "http://schemas.openxmlformats.org/package/2006/relationships"),
                        new XAttribute("Id", "rId1"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                        new XAttribute("Target", "xl/workbook.xml"))
                )
            ).ToString(SaveOptions.DisableFormatting);
        }

        private static string BuildWorkbookXml(string sheetName)
        {
            string safeSheetName = new string(sheetName.Where(ch => ch != ':' && ch != '\\' && ch != '/' && ch != '?' && ch != '*' && ch != '[' && ch != ']').ToArray());
            if (string.IsNullOrWhiteSpace(safeSheetName))
            {
                safeSheetName = "Sheet1";
            }

            return new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(XName.Get("workbook", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"),
                    new XAttribute(XNamespace.Xmlns + "r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"),
                    new XElement(XName.Get("sheets", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"),
                        new XElement(XName.Get("sheet", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"),
                            new XAttribute("name", safeSheetName),
                            new XAttribute("sheetId", "1"),
                            new XAttribute(XName.Get("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"), "rId1"))
                    )
                )
            ).ToString(SaveOptions.DisableFormatting);
        }

        private static string BuildWorkbookRelsXml()
        {
            return new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(XName.Get("Relationships", "http://schemas.openxmlformats.org/package/2006/relationships"),
                    new XElement(XName.Get("Relationship", "http://schemas.openxmlformats.org/package/2006/relationships"),
                        new XAttribute("Id", "rId1"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"),
                        new XAttribute("Target", "worksheets/sheet1.xml")),
                    new XElement(XName.Get("Relationship", "http://schemas.openxmlformats.org/package/2006/relationships"),
                        new XAttribute("Id", "rId2"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"),
                        new XAttribute("Target", "styles.xml"))
                )
            ).ToString(SaveOptions.DisableFormatting);
        }

        private static string BuildStylesXml()
        {
            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

            return new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(ns + "styleSheet",
                    new XElement(ns + "fonts", new XAttribute("count", "1"),
                        new XElement(ns + "font",
                            new XElement(ns + "sz", new XAttribute("val", "11")),
                            new XElement(ns + "name", new XAttribute("val", "Calibri")))),
                    new XElement(ns + "fills", new XAttribute("count", "1"),
                        new XElement(ns + "fill",
                            new XElement(ns + "patternFill", new XAttribute("patternType", "none")))),
                    new XElement(ns + "borders", new XAttribute("count", "1"),
                        new XElement(ns + "border",
                            new XElement(ns + "left"),
                            new XElement(ns + "right"),
                            new XElement(ns + "top"),
                            new XElement(ns + "bottom"),
                            new XElement(ns + "diagonal"))),
                    new XElement(ns + "cellStyleXfs", new XAttribute("count", "1"),
                        new XElement(ns + "xf",
                            new XAttribute("numFmtId", "0"),
                            new XAttribute("fontId", "0"),
                            new XAttribute("fillId", "0"),
                            new XAttribute("borderId", "0"))),
                    new XElement(ns + "cellXfs", new XAttribute("count", "1"),
                        new XElement(ns + "xf",
                            new XAttribute("numFmtId", "0"),
                            new XAttribute("fontId", "0"),
                            new XAttribute("fillId", "0"),
                            new XAttribute("borderId", "0"),
                            new XAttribute("xfId", "0"))),
                    new XElement(ns + "cellStyles", new XAttribute("count", "1"),
                        new XElement(ns + "cellStyle",
                            new XAttribute("name", "Normal"),
                            new XAttribute("xfId", "0"),
                            new XAttribute("builtinId", "0")))
                )
            ).ToString(SaveOptions.DisableFormatting);
        }

        private static string BuildWorksheetXml(IList<IList<string>> table)
        {
            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            XElement sheetData = new XElement(ns + "sheetData");

            for (int rowIndex = 0; rowIndex < table.Count; rowIndex++)
            {
                IList<string> row = table[rowIndex] ?? Array.Empty<string>();
                XElement rowElement = new XElement(ns + "row", new XAttribute("r", rowIndex + 1));

                for (int colIndex = 0; colIndex < row.Count; colIndex++)
                {
                    string value = row[colIndex] ?? string.Empty;
                    string cellRef = GetCellReference(colIndex, rowIndex);

                    XElement cell = new XElement(ns + "c",
                        new XAttribute("r", cellRef),
                        new XAttribute("t", "inlineStr"),
                        new XElement(ns + "is",
                            new XElement(ns + "t", value)));

                    rowElement.Add(cell);
                }

                sheetData.Add(rowElement);
            }

            XDocument sheet = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(ns + "worksheet", sheetData));

            return sheet.ToString(SaveOptions.DisableFormatting);
        }

        private static string GetCellReference(int zeroBasedColumn, int zeroBasedRow)
        {
            return GetColumnName(zeroBasedColumn) + (zeroBasedRow + 1).ToString(CultureInfo.InvariantCulture);
        }

        private static string GetColumnName(int zeroBasedColumn)
        {
            int n = zeroBasedColumn + 1;
            StringBuilder name = new StringBuilder();

            while (n > 0)
            {
                int remainder = (n - 1) % 26;
                name.Insert(0, (char)('A' + remainder));
                n = (n - 1) / 26;
            }

            return name.ToString();
        }

        private static List<List<string>> ReadFirstWorksheet(string xlsxPath)
        {
            using (FileStream stream = new FileStream(xlsxPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                Dictionary<int, string> sharedStrings = ReadSharedStrings(archive);
                ZipArchiveEntry worksheetEntry = FindFirstWorksheetEntry(archive);

                if (worksheetEntry == null)
                {
                    throw new InvalidDataException("Nenhuma planilha foi encontrada no arquivo XLSX.");
                }

                using (Stream worksheetStream = worksheetEntry.Open())
                {
                    XDocument worksheetDoc = XDocument.Load(worksheetStream);
                    XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

                    List<List<string>> table = new List<List<string>>();
                    IEnumerable<XElement> rows = worksheetDoc.Descendants(ns + "row");

                    foreach (XElement row in rows)
                    {
                        List<string> line = new List<string>();
                        int currentColumn = 0;

                        foreach (XElement cell in row.Elements(ns + "c"))
                        {
                            string cellRef = (string)cell.Attribute("r") ?? string.Empty;
                            int targetColumn = GetColumnIndexFromCellReference(cellRef);

                            while (currentColumn < targetColumn)
                            {
                                line.Add(string.Empty);
                                currentColumn++;
                            }

                            line.Add(ReadCellValue(cell, ns, sharedStrings));
                            currentColumn++;
                        }

                        table.Add(line);
                    }

                    return table;
                }
            }
        }

        private static ZipArchiveEntry FindFirstWorksheetEntry(ZipArchive archive)
        {
            ZipArchiveEntry sheet1 = archive.GetEntry("xl/worksheets/sheet1.xml");
            if (sheet1 != null)
            {
                return sheet1;
            }

            return archive.Entries
                .Where(e => e.FullName.StartsWith("xl/worksheets/", StringComparison.OrdinalIgnoreCase) &&
                            e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        }

        private static Dictionary<int, string> ReadSharedStrings(ZipArchive archive)
        {
            Dictionary<int, string> map = new Dictionary<int, string>();
            ZipArchiveEntry sharedStringsEntry = archive.GetEntry("xl/sharedStrings.xml");
            if (sharedStringsEntry == null)
            {
                return map;
            }

            using (Stream sharedStream = sharedStringsEntry.Open())
            {
                XDocument sharedDoc = XDocument.Load(sharedStream);
                XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
                List<XElement> items = sharedDoc.Descendants(ns + "si").ToList();

                for (int i = 0; i < items.Count; i++)
                {
                    string value = string.Concat(items[i].Descendants(ns + "t").Select(t => t.Value));
                    map[i] = value;
                }
            }

            return map;
        }

        private static string ReadCellValue(XElement cell, XNamespace ns, Dictionary<int, string> sharedStrings)
        {
            string cellType = (string)cell.Attribute("t");

            if (cellType == "inlineStr")
            {
                XElement textElement = cell.Descendants(ns + "t").FirstOrDefault();
                return textElement != null ? textElement.Value : string.Empty;
            }

            if (cellType == "s")
            {
                string indexText = (string)cell.Element(ns + "v");
                if (int.TryParse(indexText, out int sharedIndex) && sharedStrings.TryGetValue(sharedIndex, out string sharedValue))
                {
                    return sharedValue;
                }

                return string.Empty;
            }

            XElement valueElement = cell.Element(ns + "v");
            return valueElement != null ? valueElement.Value : string.Empty;
        }

        private static int GetColumnIndexFromCellReference(string cellReference)
        {
            if (string.IsNullOrEmpty(cellReference))
            {
                return 0;
            }

            int index = 0;
            while (index < cellReference.Length && char.IsLetter(cellReference[index]))
            {
                index++;
            }

            string columnPart = cellReference.Substring(0, index).ToUpperInvariant();
            if (columnPart.Length == 0)
            {
                return 0;
            }

            int column = 0;
            for (int i = 0; i < columnPart.Length; i++)
            {
                column = (column * 26) + (columnPart[i] - 'A' + 1);
            }

            return Math.Max(0, column - 1);
        }
    }
}
