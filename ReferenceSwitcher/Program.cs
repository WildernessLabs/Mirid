using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReferenceSwitcher
{
    class Program
    {
        // zero dependancy nugets
        static FileInfo[]? MeadowMQTTProjects;
        static FileInfo[]? MeadowUnitsProjects;
        static FileInfo[]? MeadowLoggingProjects;
        
        static FileInfo[]? MeadowContractsProjects;

        static FileInfo[]? MeadowModbusProjects;
        static FileInfo[]? MeadowCoreProjects;

        static FileInfo[]? MeadowFoundationDriverProjects;
        static FileInfo[]? MeadowFoundationSampleProjects;
        static FileInfo[]? MeadowFoundationCoreProjects;
        static FileInfo[]? MeadowFoundationGroveProjects;
        static FileInfo[]? MeadowFoundationMikroBusProjects;
        static FileInfo[]? MeadowFoundationFeatherwingsProjects;

        static FileInfo[]? MeadowProjectLabProjects;

        static RefSwitcher refSwitcher = new RefSwitcher();
        static RepoLoader repoLoader = new RepoLoader();
        static RepoData repoData = new RepoData();

        static Dictionary<string, Repo> repos = new Dictionary<string, Repo>();

        static void Main(string[] args)
        {
            Console.WriteLine("Hello Meadow developers!");

            LoadProjects();


            //toggle methods below for various repos

            //SwitchMeadowContracts(publish: true);

            //SwitchMeadowModbus(publish: true);

            //SwitchMeadowCore(publish: true);
            

            //SwitchMeadowFoundationCore(publish: true);

            //SwitchMeadowFoundation(publish: false);

            //SwitchMeadowFoundationGrove(publish: true);

            //SwitchMeadowFoundationFeatherwings(publish: true);

            //SwitchMeadowFoundationMikroBus(publish: true);

            SwitchMeadowProjectLab(publish: false);
        }

        static void LoadProjects()
        {
            foreach(var data in repoData.Repos)
            {
                repos.Add(data.Key, repoLoader.LoadRepo(data.Key, data.Value));
            }

            MeadowMQTTProjects = repos["MQTTnet"].ProjectFiles.ToArray();

            MeadowUnitsProjects = repos["Meadow.Units"].ProjectFiles.ToArray();
            MeadowLoggingProjects = repos["Meadow.Logging"].ProjectFiles.ToArray();
            MeadowContractsProjects = repos["Meadow.Contracts"].ProjectFiles.ToArray();
            MeadowModbusProjects = repos["Meadow.Modbus"].ProjectFiles.ToArray();

            MeadowCoreProjects = repos["Meadow.Core"].ProjectFiles.ToArray();

            MeadowFoundationDriverProjects = repos["Meadow.Foundation"].ProjectFiles.ToArray();
            MeadowFoundationSampleProjects = repos["Meadow.Foundation"].ProjectFiles.ToArray();

            MeadowFoundationCoreProjects = repos["Meadow.Foundation.Core"].ProjectFiles.ToArray();

            MeadowFoundationGroveProjects = repos["Meadow.Foundation.Grove"].ProjectFiles.ToArray();
            MeadowFoundationMikroBusProjects = repos["Meadow.Foundation.mikroBus"].ProjectFiles.ToArray();
            MeadowFoundationFeatherwingsProjects = repos["Meadow.Foundation.Featherwings"].ProjectFiles.ToArray();

            MeadowProjectLabProjects = repos["Meadow.ProjectLab"].ProjectFiles.ToArray();
        }

        static void SwitchRepo(IEnumerable<FileInfo> projectsToUpdate, IEnumerable<FileInfo>[] projectsToReference, bool publish)
        {
            foreach(var collection in projectsToReference)
            {
                if (publish)
                {
                    refSwitcher.SwitchToPublishingMode(projectsToUpdate, collection);
                }
                else
                {
                    refSwitcher.SwitchToDeveloperMode(projectsToUpdate, collection);
                }
            }
        }

        static void SwitchMeadowModbus(bool publish)
        {
            SwitchRepo(MeadowModbusProjects,
                new IEnumerable<FileInfo>[] { MeadowLoggingProjects, MeadowContractsProjects },
                publish);
        }

        static void SwitchMeadowContracts(bool publish)
        {
            SwitchRepo(MeadowModbusProjects,
                new IEnumerable<FileInfo>[] { MeadowLoggingProjects, MeadowUnitsProjects },
                publish);
        }

        static void SwitchMeadowCore(bool publish)
        {
            SwitchRepo(MeadowCoreProjects,
                new IEnumerable<FileInfo>[] { MeadowMQTTProjects, MeadowContractsProjects },
                publish);
        }

        static void SwitchMeadowFoundationCore(bool publish)
        {
            SwitchRepo(MeadowFoundationCoreProjects,
                new IEnumerable<FileInfo>[] { MeadowCoreProjects },
                publish);
        }

        static void SwitchMeadowFoundation(bool publish)
        {
            SwitchRepo(MeadowFoundationDriverProjects,
                new IEnumerable<FileInfo>[] { MeadowFoundationDriverProjects, 
                                              MeadowFoundationCoreProjects, 
                                              MeadowModbusProjects },
                publish);

            SwitchRepo(MeadowFoundationSampleProjects,
                new IEnumerable<FileInfo>[] { MeadowCoreProjects },
                publish);
        }

        static void SwitchMeadowFoundationFeatherwings(bool publish)
        {
            SwitchRepo(MeadowFoundationFeatherwingsProjects,
                new IEnumerable<FileInfo>[] { MeadowFoundationDriverProjects, MeadowCoreProjects },
                publish);
        }

        static void SwitchMeadowFoundationMikroBus(bool publish)
        {
            SwitchRepo(MeadowFoundationMikroBusProjects,
                new IEnumerable<FileInfo>[] { MeadowFoundationDriverProjects, MeadowCoreProjects },
                publish);
        }

        static void SwitchMeadowFoundationGrove(bool publish)
        {
            SwitchRepo(MeadowFoundationGroveProjects,
                new IEnumerable<FileInfo>[] { MeadowFoundationDriverProjects, MeadowCoreProjects }, 
                publish);
        }

        static void SwitchMeadowProjectLab(bool publish)
        {
            SwitchRepo(MeadowProjectLabProjects,
                new IEnumerable<FileInfo>[] { MeadowFoundationDriverProjects, 
                                              MeadowCoreProjects, 
                                              MeadowModbusProjects},
                publish);
        }
    }
}