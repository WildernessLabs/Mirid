using System.Linq;

namespace Mirid.Models
{
    public class MFLibraryDriverSet : MFDriverSet
    {
        public MFLibraryDriverSet(string name, string MFSourcePath, string driverSourcePath, string docsOverridePath, string githubUrl)
            : base(name, MFSourcePath, driverSourcePath, docsOverridePath, githubUrl)
        {
        }

        protected override void ReadPackageData(string peripheralsPath, string docsOverridePath)
        {
            var projectFiles = FileCrawler.GetAllProjectsInFolders(peripheralsPath, true);
            var driverProjectFiles = FileCrawler.GetDriverProjects(projectFiles);

            foreach (var file in driverProjectFiles)
            {
                DriverPackages.Add(new MFLibraryPackage(file, docsOverridePath));
            }

            DriverPackages = DriverPackages.OrderBy(x => x.PackageName).ToList();
        }
    }
}
