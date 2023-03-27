using System.Collections.Generic;
using System.IO;

namespace ReferenceSwitcher
{
    public enum MeadowRepo
    {
        Units,
        MQTTnet,
        Logging,

        Modbus,
        Contracts,
        Core,
        FoundationCore,
        Foundation,
        FoundationSamples,
        FoundationFeatherwings,
        FoundationGrove,
        FoundationMikroBus,

        ProjectLab,
        CoreSamples,
        ProjectSamples,
        ProjectLabSamples,

        GPS_Tracker,
        Clima,

        count
    }

    public class MeadowReferenceSwitcher
    {
        Dictionary<MeadowRepo, Repo>? Repos;
        readonly RefSwitcher refSwitcher = new RefSwitcher();
        readonly RepoLoader repoLoader = new RepoLoader();

        public void LoadProjects()
        {
            Repos = new Dictionary<MeadowRepo, Repo>
            {
                { MeadowRepo.Units, repoLoader.LoadRepo("Meadow.Units", "Meadow.Units/Source/") },
                { MeadowRepo.Modbus, repoLoader.LoadRepo("Meadow.Modbus", "Meadow.Modbus/src/") },
                { MeadowRepo.MQTTnet, repoLoader.LoadRepo("MQTTnet", "MQTTnet/Source/MQTTnet/") },
                { MeadowRepo.Logging, repoLoader.LoadRepo("Meadow.Logging", "Meadow.Logging/Source/") },
                { MeadowRepo.Contracts, repoLoader.LoadRepo("Meadow.Contracts", "Meadow.Contracts/Source/") },
                { MeadowRepo.Core, repoLoader.LoadRepo("Meadow.Core", "Meadow.Core/Source/") },
                { MeadowRepo.Foundation, repoLoader.LoadRepo("Meadow.Foundation", "Meadow.Foundation/Source/") },
                { MeadowRepo.FoundationSamples, repoLoader.LoadRepo("Meadow.Foundation", "Meadow.Foundation/Source/", RefSwitcher.Projects.Samples) },
                { MeadowRepo.FoundationCore, repoLoader.LoadRepo("Meadow.Foundation.Core", "Meadow.Foundation/Source/Meadow.Foundation.Core/") },
                { MeadowRepo.FoundationFeatherwings, repoLoader.LoadRepo("Meadow.Foundation.Featherwings", "Meadow.Foundation.Featherwings/Source/", RefSwitcher.Projects.All) },
                { MeadowRepo.FoundationGrove, repoLoader.LoadRepo("Meadow.Foundation.Grove", "Meadow.Foundation.Grove/Source/", RefSwitcher.Projects.All) },
                { MeadowRepo.FoundationMikroBus, repoLoader.LoadRepo("Meadow.Foundation.mikroBus", "Meadow.Foundation.mikroBus/Source/", RefSwitcher.Projects.All) },
                { MeadowRepo.ProjectLab, repoLoader.LoadRepo("Meadow.ProjectLab", "Meadow.ProjectLab/Source/") },
                { MeadowRepo.CoreSamples, repoLoader.LoadRepo("Meadow.Core.Samples", "Meadow.Core.Samples/Source/", RefSwitcher.Projects.All) },
                { MeadowRepo.ProjectSamples, repoLoader.LoadRepo("Meadow.Project.Samples", "Meadow.Project.Samples/Source/", RefSwitcher.Projects.All) },
                { MeadowRepo.ProjectLabSamples, repoLoader.LoadRepo("Meadow.ProjectLab.Samples", "Meadow.ProjectLab.Samples/Source/", RefSwitcher.Projects.All) },
                { MeadowRepo.GPS_Tracker, repoLoader.LoadRepo("GNSS_Tracker", "GPS_Tracker/Source/", RefSwitcher.Projects.All) },
                { MeadowRepo.Clima, repoLoader.LoadRepo("Clima", "ClimaDoingNz/Source/", RefSwitcher.Projects.All) }
            };
        }


        void SwitchRepo(IEnumerable<FileInfo> projectsToUpdate, IEnumerable<FileInfo>[] projectsToReference, bool publish)
        {
            foreach (var collection in projectsToReference)
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

        public void SwitchMeadowModbus(bool publish)
        {
            SwitchRepo(Repos[MeadowRepo.Modbus].ProjectFiles,
                new IEnumerable<FileInfo>[] {
                    Repos[MeadowRepo.Logging].ProjectFiles,
                    Repos[MeadowRepo.Contracts].ProjectFiles },
                publish);
        }

        public void SwitchMeadowContracts(bool publish)
        {
            SwitchRepo(Repos[MeadowRepo.Contracts].ProjectFiles,
                new IEnumerable<FileInfo>[] {
                    Repos[MeadowRepo.Logging].ProjectFiles,
                    Repos[MeadowRepo.Units].ProjectFiles },
                publish);
        }

        public void SwitchMeadowCore(bool publish)
        {
            SwitchRepo(Repos[MeadowRepo.Core].ProjectFiles,
                new IEnumerable<FileInfo>[] {
                    Repos[MeadowRepo.MQTTnet].ProjectFiles,
                    Repos[MeadowRepo.Contracts].ProjectFiles,
                    Repos[MeadowRepo.Core].ProjectFiles },
                publish);
        }

        public void SwitchMeadowFoundationCore(bool publish)
        {
            SwitchRepo(Repos[MeadowRepo.FoundationCore].ProjectFiles,
                new IEnumerable<FileInfo>[] {
                    Repos[MeadowRepo.Core].ProjectFiles },
                publish);
        }

        public void SwitchMeadowFoundation(bool publish)
        {
            SwitchRepo(Repos[MeadowRepo.Foundation].ProjectFiles,
                new IEnumerable<FileInfo>[] {
                    Repos[MeadowRepo.Foundation].ProjectFiles,
                    Repos[MeadowRepo.FoundationCore].ProjectFiles,
                    Repos[MeadowRepo.Modbus].ProjectFiles },
                publish);

            SwitchRepo(Repos[MeadowRepo.FoundationSamples].ProjectFiles,
                new IEnumerable<FileInfo>[] {
                    Repos[MeadowRepo.Core].ProjectFiles },
                publish);
        }

        public void SwitchMeadowFoundationFeatherwings(bool publish)
        {
            SwitchRepo(Repos[MeadowRepo.FoundationFeatherwings].ProjectFiles,
                new IEnumerable<FileInfo>[] {
                    Repos[MeadowRepo.Foundation].ProjectFiles,
                    Repos[MeadowRepo.Core].ProjectFiles },
                publish);
        }

        public void SwitchMeadowFoundationMikroBus(bool publish)
        {
            SwitchRepo(Repos[MeadowRepo.FoundationMikroBus].ProjectFiles,
                new IEnumerable<FileInfo>[] {
                    Repos[MeadowRepo.Foundation].ProjectFiles,
                    Repos[MeadowRepo.Core].ProjectFiles },
                publish);
        }

        public void SwitchMeadowFoundationGrove(bool publish)
        {
            SwitchRepo(Repos[MeadowRepo.FoundationGrove].ProjectFiles,
                new IEnumerable<FileInfo>[] {
                    Repos[MeadowRepo.Foundation].ProjectFiles,
                    Repos[MeadowRepo.Core].ProjectFiles },
                publish);
        }

        public void SwitchMeadowProjectLab(bool publish)
        {
            SwitchRepo(Repos[MeadowRepo.ProjectLab].ProjectFiles,
                new IEnumerable<FileInfo>[] {
                    Repos[MeadowRepo.Foundation].ProjectFiles,
                    Repos[MeadowRepo.Core].ProjectFiles,
                    Repos[MeadowRepo.Modbus].ProjectFiles },
                publish);
        }

        public void SwitchMeadowCoreSamples(bool publish)
        {
            SwitchRepo(Repos[MeadowRepo.CoreSamples].ProjectFiles,
                new IEnumerable<FileInfo>[] {
                    Repos[MeadowRepo.Foundation].ProjectFiles,
                    Repos[MeadowRepo.Core].ProjectFiles,
                    Repos[MeadowRepo.Modbus].ProjectFiles },
                publish);
        }

        public void SwitchMeadowProjectSamples(bool publish)
        {
            SwitchRepo(Repos[MeadowRepo.ProjectSamples].ProjectFiles,
                new IEnumerable<FileInfo>[] {
                    Repos[MeadowRepo.Foundation].ProjectFiles,
                    Repos[MeadowRepo.Core].ProjectFiles,
                    Repos[MeadowRepo.Modbus].ProjectFiles },
                publish);
        }

        public void SwitchGPS_Tracker(bool publish)
        {
            SwitchRepo(Repos[MeadowRepo.GPS_Tracker].ProjectFiles,
                new IEnumerable<FileInfo>[] { Repos[MeadowRepo.Foundation].ProjectFiles,
                                              Repos[MeadowRepo.Core].ProjectFiles,
                                              Repos[MeadowRepo.Modbus].ProjectFiles },
                publish);
        }

        public void SwitchClima(bool publish)
        {
            SwitchRepo(Repos[MeadowRepo.Clima].ProjectFiles,
                new IEnumerable<FileInfo>[] { Repos[MeadowRepo.Foundation].ProjectFiles,
                                              Repos[MeadowRepo.Core].ProjectFiles,
                                              Repos[MeadowRepo.Modbus].ProjectFiles },
                publish);
        }

        public void SwitchMeadowProjectLabSamples(bool publish)
        {
            SwitchRepo(Repos[MeadowRepo.ProjectLabSamples].ProjectFiles,
                new IEnumerable<FileInfo>[] {
                    Repos[MeadowRepo.ProjectLab].ProjectFiles,
                    Repos[MeadowRepo.Foundation].ProjectFiles,
                    Repos[MeadowRepo.FoundationGrove].ProjectFiles,
                    Repos[MeadowRepo.FoundationMikroBus].ProjectFiles },
                publish);
        }
    }
}
