using System.IO;
using System.Linq;

namespace Mirid.Models
{
    //custom logic for MeadowFoundation Core peripheral drivers
    public class MFCoreDriverSet : MFDriverSet
    {
        
        public MFCoreDriverSet(string name, string MFSourcePath, string driverSourcePath, string docsOverridePath, string githubUrl) 
            : base(name, MFSourcePath, driverSourcePath, docsOverridePath, githubUrl)
        {
        }
                
        protected override void ReadPackageData(string peripheralsPath, string docsOverridePath)
        {
            //Drivers
            var projectFiles = FileCrawler.GetAllProjectsInFolders(peripheralsPath, true);
            var driverProjectFiles = FileCrawler.GetDriverProjects(projectFiles);

            //should only be one
            foreach (var file in driverProjectFiles)
            {
                DriverPackages.Add(new MFCorePackage(file, docsOverridePath));
            }
        }
    }
}
