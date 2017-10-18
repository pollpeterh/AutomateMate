using System.Collections.Generic;

namespace AutomateMatePOC
{
    public class Identifier
    {
        public string ID;
        public string Name;
        public string Class;

        public string[] GetProperties()
        {
            List<string> retVal = new List<string>();
            if (ID != null)
            {
                retVal.Add("ID");
            }
            if (Name != null)
            {
                retVal.Add("Name");
            }
            if (Class != null)
            {
                retVal.Add("ClassName");
            }
            return retVal.ToArray();
        }

        public string[] GetValues()
        {
            List<string> retVal = new List<string>();
            if (ID != null)
            {
                retVal.Add(ID);
            }
            if (Name != null)
            {
                retVal.Add(Name);
            }
            if (Class != null)
            {
                retVal.Add(Class);
            }
            return retVal.ToArray();
        }
    }
}