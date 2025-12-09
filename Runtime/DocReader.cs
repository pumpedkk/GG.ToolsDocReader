using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;


namespace GGTools.FileReaders
{
    public static class CSV
    {
        /// <summary>
        /// Reads the content of a TextAsset and returns all lines as a string array,
        /// ignoring empty lines and line breaks.
        /// </summary>
        /// <param name="csvFile">The TextAsset file to read.</param>
        /// <returns>Array of lines from the file.</returns>
        public static string[] ReadLines(this TextAsset csvFile)
        {
            string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            return lines;
        }

        /// <summary>
        /// Reads the content of a TextAsset and returns all lines as a List of strings,
        /// ignoring empty lines and line breaks.
        /// </summary>
        /// <param name="csvFile">The TextAsset file to read.</param>
        /// <returns>List of lines from the file.</returns>
        public static List<string> ReadLinesList(this TextAsset csvFile)
        {
            string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            return new List<string>(lines);
        }

        /// <summary>
        /// 
        /// Reads a text file from a file path or file name and returns all lines as a List of strings.
        /// Search order:
        /// 1) Full absolute path
        /// 2) Application.persistentDataPath
        /// 3) Application.streamingAssetsPath
        /// 4) Resources folder (as TextAsset)
        /// </summary>
        /// <param name="file">File path or file name.</param>
        /// <returns>List of lines from the file.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file is not found in any location.</exception>
        public static List<string> ReadFilePathLinesList(this string file)
        {

            if (File.Exists(file))
            {
                string fileContent = File.ReadAllText(file);
                string[] lines = fileContent.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                return new List<string>(lines);
            }
            else if (File.Exists(Path.Combine(Application.persistentDataPath, file)))
            {
                string fileContent = File.ReadAllText(Path.Combine(Application.persistentDataPath, file));
                string[] lines = fileContent.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                return new List<string>(lines);
            }
            else if (File.Exists(Path.Combine(Application.streamingAssetsPath, file)))
            {
                string fileContent = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, file));
                string[] lines = fileContent.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                return new List<string>(lines);
            }
            else if (Resources.Load<TextAsset>(file))
            {
                TextAsset txt = Resources.Load<TextAsset>(file);
                string fileContent = txt.text;
                string[] lines = fileContent.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                return new List<string>(lines);
            }
            else
            {
                throw new FileNotFoundException("Arquivo não encontrado", file);
            }
        }


        /// <summary>
        /// Splits a raw string into an array of lines, ignoring empty lines.
        /// </summary>
        /// <param name="text">The text to split.</param>
        /// <returns>Array of lines.</returns>
        public static string[] ReadLines(this string text)
        {
            //if (text == "") throw new FileNotFoundException("Texto vazio fornecido");
            string[] lines = text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            return lines;
        }

        /// <summary>
        /// Reads a text file from a file path or file name and returns all lines as a string array.
        /// Searches the same locations as ReadFilePathLinesList.
        /// </summary>
        /// <param name="file">File path or file name.</param>
        /// <returns>Array of lines from the file.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file is not found in any location.</exception>
        public static string[] ReadFilePathLines(this string file)
        {
            if (File.Exists(file))
            {
                string fileContent = File.ReadAllText(file);
                string[] lines = fileContent.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                return lines;
            }
            else if (File.Exists(Path.Combine(Application.persistentDataPath, file)))
            {
                string fileContent = File.ReadAllText(Path.Combine(Application.persistentDataPath, file));
                string[] lines = fileContent.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                return lines;
            }
            else if (File.Exists(Path.Combine(Application.streamingAssetsPath, file)))
            {
                string fileContent = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, file));
                string[] lines = fileContent.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                return lines;
            }
            else if (Resources.Load<TextAsset>(file))
            {
                TextAsset txt = Resources.Load<TextAsset>(file);
                string fileContent = txt.text;
                string[] lines = fileContent.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                return lines;
            }
            else
            {
                throw new FileNotFoundException("Arquivo não encontrado", file);
            }
        }

        /// <summary>
        /// Splits each line of a CSV-formatted string array using the specified delimiter
        /// and returns a jagged array where each element represents the fields of that line.
        /// </summary>
        /// <param name="file">Array of strings representing each line of the CSV file.</param>
        /// <param name="splitCharacter">The character used to split each line (e.g., ',').</param>
        /// <returns>
        /// A jagged string array where each row contains the split fields of the corresponding line.
        /// </returns>
        public static string[][] SplitCSV(this string[] file, char splitCharacter)
        {
            string[][] aux = new string[file.Length][];
            if (file.Length < 2)
            {
                aux[0] = file[0].Split(splitCharacter);
            }
            else
            {
                for (int i = 0; i <= file.Length - 1; i++)
                {
                    aux[i] = file[i].Split(splitCharacter);
                }
            }

            return aux;
        }

    }


    public static class Txt
    {
        /// <summary>
        /// Reads the full text of a TextAsset and returns it as a single string.
        /// </summary>
        /// <param name="txtFile">The TextAsset file to read.</param>
        /// <returns>Full text content.</returns>
        public static string ReadLine(this TextAsset txtFile)
        {
            return txtFile.text;
        }

        /// <summary>
        /// Reads the full content of a text file from a file path or name,
        /// searching in the same locations as ReadFilePathLinesList.
        /// Returns the entire content as one single string.
        /// </summary>
        /// <param name="file">File path or file name.</param>
        /// <returns>Full text content.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file is not found.</exception>
        public static string ReadFilePathLine(this string file)
        {
            if (File.Exists(file))
            {
                string fileContent = File.ReadAllText(file);
                return fileContent;
            }
            else if (File.Exists(Path.Combine(Application.persistentDataPath, file)))
            {
                string fileContent = File.ReadAllText(Path.Combine(Application.persistentDataPath, file));
                return fileContent;
            }
            else if (File.Exists(Path.Combine(Application.streamingAssetsPath, file)))
            {
                string fileContent = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, file));
                return fileContent;
            }
            else if (Resources.Load<TextAsset>(file))
            {
                TextAsset txt = Resources.Load<TextAsset>(file);
                string fileContent = txt.text;
                return fileContent;
            }
            else
            {
                throw new FileNotFoundException("Arquivo não encontrado", file);
            }
        }

        public static string[] CreatePage(this string text, int maxChar)
        {
            List<string> returnString = new List<string>();
            if (string.IsNullOrEmpty(text) || maxChar <= 0)
            {
                returnString.Add(text);
                return returnString.ToArray();
            }

            int lastPageInsert = 0;

            while (lastPageInsert + maxChar <= text.Length)
            {
                int pageBreakIndex = lastPageInsert + maxChar;

                // Volta até o último espaço antes do limite
                while (pageBreakIndex > lastPageInsert && text[pageBreakIndex] != ' ')
                {
                    pageBreakIndex--;
                }

                // Se não achou espaço, quebra mesmo assim (melhor que travar)
                if (pageBreakIndex <= lastPageInsert)
                    pageBreakIndex = lastPageInsert + maxChar;
                if(lastPageInsert + pageBreakIndex > text.Length) 
                {
                    if (text.Substring(lastPageInsert) != "" && text.Substring(lastPageInsert) != " " && text.Substring(lastPageInsert) != "    ")
                    {
                        returnString.Add(text.Substring(lastPageInsert));
                    }
                    lastPageInsert = text.Length;
                }
                else 
                {
                    if (text.Substring(lastPageInsert, pageBreakIndex) != "" && text.Substring(lastPageInsert, pageBreakIndex) != " " && text.Substring(lastPageInsert, pageBreakIndex) != "    ")
                    {
                        returnString.Add(text.Substring(lastPageInsert, pageBreakIndex));
                    }
                    lastPageInsert = pageBreakIndex;
                }
                
            }
            if (text.Substring(lastPageInsert) != "" && text.Substring(lastPageInsert) != " " && text.Substring(lastPageInsert) != "    ")
            {
                returnString.Add(text.Substring(lastPageInsert));
            }
            return returnString.ToArray();
        }
    }

}



