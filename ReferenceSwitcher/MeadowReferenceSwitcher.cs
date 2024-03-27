using MeadowRepos;
using System.Collections.Generic;
using System.IO;

namespace ReferenceSwitcher
{
    public enum MeadowRepo
    {
        amqpnetlite,
        Units,
        MQTTnet,
        Logging,
        LogProviders,

        Modbus,
        Contracts,
        Core,
        FoundationCore,
        Foundation,
        FoundationSamples,
        FoundationFeatherwings,
        FoundationGrove,
        FoundationMikroBus,
        Maple,

        ProjectLab,
        GPS_Tracker,
        Clima,
        Juego,

        MeadowSamples,

        count
    }

    public class MeadowReferenceSwitcher
    {
        Dictionary<MeadowRepo, GitRepo>? Repos;
        readonly RepoLoader repoLoader = new RepoLoader();

        public void LoadProjects()
        {
            Repos = new Dictionary<MeadowRepo, GitRepo>
            {
                { MeadowRepo.amqpnetlite, repoLoader.LoadRepo("amqpnetlite", "amqpnetlite/src/") },
                { MeadowRepo.Units, repoLoader.LoadRepo("Meadow.Units", "Meadow.Units/Source/") },
                { MeadowRepo.Modbus, repoLoader.LoadRepo("Meadow.Modbus", "Meadow.Modbus/src/") },
                { MeadowRepo.MQTTnet, repoLoader.LoadRepo("MQTTnet", "MQTTnet/Source/MQTTnet/") },
                { MeadowRepo.Logging, repoLoader.LoadRepo("Meadow.Logging", "Meadow.Logging/Source/Meadow.Logging/") },
                { MeadowRepo.LogProviders, repoLoader.LoadRepo("Meadow.Logging", "Meadow.Logging/Source/Meadow.Logging.LogProviders/") },
                { MeadowRepo.Contracts, repoLoader.LoadRepo("Meadow.Contracts", "Meadow.Contracts/Source/") },
                { MeadowRepo.Core, repoLoader.LoadRepo("Meadow.Core", "Meadow.Core/source/") },
                { MeadowRepo.Foundation, repoLoader.LoadRepo("Meadow.Foundation", "Meadow.Foundation/Source/", ProjectType.Drivers) },
                { MeadowRepo.FoundationSamples, repoLoader.LoadRepo("Meadow.Foundation", "Meadow.Foundation/Source/", ProjectType.Samples) },
                { MeadowRepo.FoundationCore, repoLoader.LoadRepo("Meadow.Foundation.Core", "Meadow.Foundation/Source/Meadow.Foundation.Core/") },
                { MeadowRepo.FoundationFeatherwings, repoLoader.LoadRepo("Meadow.Foundation.Featherwings", "Meadow.Foundation.Featherwings/Source/", ProjectType.All) },
                { MeadowRepo.FoundationGrove, repoLoader.LoadRepo("Meadow.Foundation.Grove", "Meadow.Foundation.Grove/Source/", ProjectType.All) },
                { MeadowRepo.FoundationMikroBus, repoLoader.LoadRepo("Meadow.Foundation.mikroBus", "Meadow.Foundation.mikroBus/Source/", ProjectType.All) },
                { MeadowRepo.Maple, repoLoader.LoadRepo("Maple", "Maple/Source/", ProjectType.All) },
                { MeadowRepo.ProjectLab, repoLoader.LoadRepo("Meadow.ProjectLab", "Meadow.ProjectLab/Source/") },
                { MeadowRepo.GPS_Tracker, repoLoader.LoadRepo("GNSS_Tracker", "GNSS_Sensor_Tracker/Source/", ProjectType.All) },
                { MeadowRepo.Clima, repoLoader.LoadRepo("Clima", "Clima/Source/", ProjectType.All) },
                { MeadowRepo.Juego, repoLoader.LoadRepo("Juego", "Juego/Source/", ProjectType.All) },
                { MeadowRepo.MeadowSamples, repoLoader.LoadRepo("Meadow.Samples", "Meadow.Samples/Source/", ProjectType.All) }
            };
        }

        void SwitchRepo(IEnumerable<FileInfo> projectsToUpdate, IEnumerable<FileInfo>[] projectsToReference, bool publish)
        {
            foreach (var collection in projectsToReference)
            {
                if (publish)
                {
                    RefSwitcher.SwitchToPublishingMode(projectsToUpdate, collection, null);
                }
                else
                {
                    RefSwitcher.SwitchToDeveloperMode(projectsToUpdate, collection);
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
                    Repos[MeadowRepo.Contracts].ProjectFiles },
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

        public void SwitchJuego(bool publish)
        {
            SwitchRepo(Repos[MeadowRepo.Juego].ProjectFiles,
                new IEnumerable<FileInfo>[] { Repos[MeadowRepo.Foundation].ProjectFiles,
                                              Repos[MeadowRepo.Core].ProjectFiles,
                                              Repos[MeadowRepo.Modbus].ProjectFiles },
                publish);
        }

        public void SwitchMeadowSamples(bool publish)
        {
            SwitchRepo(Repos[MeadowRepo.MeadowSamples].ProjectFiles,
                new IEnumerable<FileInfo>[] {
                    Repos[MeadowRepo.Core].ProjectFiles,
                    Repos[MeadowRepo.LogProviders].ProjectFiles,
                    Repos[MeadowRepo.amqpnetlite].ProjectFiles,
                    Repos[MeadowRepo.Maple].ProjectFiles,
                    Repos[MeadowRepo.Modbus].ProjectFiles,
                    Repos[MeadowRepo.ProjectLab].ProjectFiles,
                    Repos[MeadowRepo.Juego].ProjectFiles,
                    Repos[MeadowRepo.MQTTnet].ProjectFiles,
                    Repos[MeadowRepo.Foundation].ProjectFiles,
                    Repos[MeadowRepo.FoundationGrove].ProjectFiles,
                    Repos[MeadowRepo.FoundationMikroBus].ProjectFiles },
                publish);
        }
    }
}