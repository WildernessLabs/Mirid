using System.Text;

namespace ActionGen
{
    internal partial class WorkflowWriter
    {
        public void WriteWorkflow(Dictionary<string, FileInfo> projects, string filename = "nuget-level1.yml")
        {
            var text = new StringBuilder();

            foreach(var project in projects) 
            {
                WriteWorkflowEntry(project.Value, project.Key, text);
            }

            File.WriteAllText(filename, text.ToString());
        }

        void WriteWorkflowEntry(FileInfo file, string packageId, StringBuilder text)
        {
            string path = GetPath(file);

            text.AppendLine( "    - uses: ./.github/actions/build-package");
            text.AppendLine( "      with:");
            text.AppendLine($"        packageId: {packageId}");
            text.AppendLine($"        path: {path}");
            text.AppendLine( "        version: ${{ env.version }}");
            text.AppendLine( "        token: ${{ env.token }}");
            text.AppendLine();
        }

        string GetPath(FileInfo file)
        {
            var path = file.FullName;

            //remove root folders
            int index = path.IndexOf("Source");
            path = path.Substring(index).Replace("\\", "/");

            return path;
        }
    }
}
