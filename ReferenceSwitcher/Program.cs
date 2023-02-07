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
            //switcher.SwitchMeadowModbus(publish: true);
            switcher.SwitchMeadowCore(publish: false);
            //switcher.SwitchMeadowFoundationCore(publish: true);
            //switcher.SwitchMeadowFoundation(publish: false);
            //switcher.SwitchMeadowFoundationGrove(publish: true);
            //switcher.SwitchMeadowFoundationFeatherwings(publish: true);
            //switcher.SwitchMeadowFoundationMikroBus(publish: true);
            //switcher.SwitchMeadowProjectLab(publish: false);


            //switcher.SwitchMeadowCoreSamples(publish: true);
        }
    }
}