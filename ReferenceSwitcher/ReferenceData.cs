using System.Collections.Generic;

namespace ReferenceSwitcher
{
    public class ReferenceData
    {
        public Dictionary<string, Reference> References { get; set; }

        public ReferenceData() 
        {
            References = new Dictionary<string, Reference>();
        }

        public void AddReference(string name, Reference refrenceDefintion)
        {
            References.Add(name, refrenceDefintion);
        }

    }

}
