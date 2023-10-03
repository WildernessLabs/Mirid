using System.IO;

namespace Mirid.Models
{
    public class MFDriverCode
    {
        public int LineCount => lines?.Length ?? 0;
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

        public string Path { get; protected set; }

        public string Namespace
        {
            get
            {
                if (string.IsNullOrEmpty(_namespace))
                {
                    _namespace = GetNamespace();
                }
                return _namespace;
            }
        }
        string _namespace;


        string[] lines;

        public MFDriverCode(FileInfo driverFile)
        {
            ReadCodeFile(Path = driverFile.FullName);
        }

        public MFDriverCode(string filePath)
        {
            ReadCodeFile(Path = filePath);
        }

        void ReadCodeFile(string filePath)
        {
            if (File.Exists(filePath) == false)
            {
                //throw new FileNotFoundException($"Couldn't find driver file {filePath}");
                lines = new string[0];
                return;
            }

            lines = File.ReadAllLines(filePath);
        }

        string GetNamespace()
        {
            foreach (var line in lines)
            {
                if (line.Contains("namespace"))
                {
                    return line.Substring("namespace ".Length).TrimEnd(';');
                }
            }

            return string.Empty;
        }
    }
}