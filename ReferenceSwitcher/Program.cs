using System;

namespace ReferenceSwitcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Meadow developers!");

            var switcher = new MeadowReferenceSwitcher();

            switcher.LoadProjects();

            //toggle methods below for various repos

            //switcher.SwitchMeadowContracts(publish: true);
            //switcher.SwitchMeadowModbus(publish: false);
            //switcher.SwitchMeadowCore(publish: false);
            //switcher.SwitchMeadowFoundationCore(publish: false);
            //switcher.SwitchMeadowFoundation(publish: false);
            switcher.SwitchMeadowFoundationGrove(publish: false);
            switcher.SwitchMeadowFoundationFeatherwings(publish: false);
            switcher.SwitchMeadowFoundationMikroBus(publish: false);

            //switcher.SwitchMeadowCoreSamples(publish: false);
            //switcher.SwitchMeadowProjectSamples(publish: false);

            //switcher.SwitchMeadowProjectLabSamples(publish: false);

            //switcher.SwitchMeadowProjectLab(publish: false);
            //switcher.SwitchGPS_Tracker(publish: false);
            //switcher.SwitchClima(publish: false);
            //switcher.SwitchJuego(publish: false);
        }
    }
}