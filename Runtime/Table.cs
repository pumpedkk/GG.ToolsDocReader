using System;
using System.Collections.Generic;

namespace GGTools.FileReaders
{
    [Serializable]
    public class Table
    {
        public List<TableRow> Rows = new List<TableRow>();

        /// <summary>
        /// Creates and adds a new row.
        /// </summary>
        /// <param name="columns">Optional initial columns for the row.</param>
        /// <returns>The created row.</returns>
        public TableRow AddRow(params string[] columns)
        {
            TableRow row = new TableRow();

            if (columns != null && columns.Length > 0)
            {
                row.Columns.AddRange(columns);
            }

            Rows.Add(row);
            return row;
        }

        /// <summary>
        /// Removes all rows.
        /// </summary>
        public void Clear()
        {
            Rows.Clear();
        }

        /// <summary>
        /// Converts this table to a structure compatible with DocWriter.
        /// </summary>
        /// <returns>Rows and columns as IList of IList.</returns>
        public IList<IList<string>> ToIList()
        {
            List<IList<string>> data = new List<IList<string>>(Rows.Count);

            for (int i = 0; i < Rows.Count; i++)
            {
                TableRow row = Rows[i];
                if (row == null)
                {
                    data.Add(new List<string>());
                    continue;
                }

                data.Add(new List<string>(row.Columns));
            }

            return data;
        }

        /// <summary>
        /// Replaces current content from IList data.
        /// </summary>
        /// <param name="data">Source rows and columns.</param>
        public void FromIList(IList<IList<string>> data)
        {
            Rows.Clear();

            if (data == null)
            {
                return;
            }

            for (int i = 0; i < data.Count; i++)
            {
                IList<string> sourceRow = data[i];
                TableRow newRow = new TableRow();

                if (sourceRow != null)
                {
                    for (int c = 0; c < sourceRow.Count; c++)
                    {
                        newRow.Columns.Add(sourceRow[c] ?? string.Empty);
                    }
                }

                Rows.Add(newRow);
            }
        }
    }

    [Serializable]
    public class TableRow
    {
        public List<string> Columns = new List<string>();
    }
}
