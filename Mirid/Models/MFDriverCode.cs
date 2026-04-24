using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            if (!File.Exists(Path)) return string.Empty;

            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(Path));
            var root = tree.GetRoot();

            var fileScopedNs = root.DescendantNodes()
                .OfType<FileScopedNamespaceDeclarationSyntax>()
                .FirstOrDefault();
            if (fileScopedNs != null)
                return fileScopedNs.Name.ToString();

            var blockNs = root.DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault();
            if (blockNs != null)
                return blockNs.Name.ToString();

            return string.Empty;
        }
    }
}