using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomateMatePOC
{
    public class AutomationAction : IEqualityComparer<AutomationAction>
    {
        public string Name;
        public string Window;
        public string Source;
        public string[] Windows;
        public bool Terminating;
        public bool Compile;
        public string[] Arguments;

        public AutomationAction(string name, string window, bool terminating, params string[] arguments)
        {
            Name = name;
            Window = window;
            Terminating = terminating;
            Arguments = arguments;
            Windows = new string[] { };
            Compile = false;
        }

        public bool Equals(AutomationAction x, AutomationAction y)
        {
            return x.Window == y.Window && x.Name == y.Name;
        }

        public int GetHashCode(AutomationAction obj)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
            => string.Format("{0}.{1}", Window, Name);
    }

    public class AutomationActionCollection : List<AutomationAction>
    {
        public AutomationAction this[string actionName, string windowName]
            => this.FirstOrDefault(action => action.Name == actionName && action.Window == windowName);

        public void Compile()
        {
            Dictionary<String, AutomationActionCollection> compileSet = new Dictionary<string, AutomationActionCollection>();
            foreach(AutomationAction action in this) {
                if(action.Compile)
                {
                    if (compileSet.ContainsKey(action.Window) && !compileSet[action.Window].Contains(action))
                    {
                        compileSet[action.Window].Add(action);
                    }
                    else
                    {
                        compileSet.Add(action.Window, new AutomationActionCollection() { action });
                    }
                }
            }
        }
    }
}
