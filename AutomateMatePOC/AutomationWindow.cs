using QAliber.Engine.Controls;
using QAliber.Engine.Controls.UIA;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace AutomateMatePOC
{
    public class AutomationWindow
    {
        public string Name;
        public string[] Actions;


        public AutomationWindow(string name, params string[] actions)
        {
            Name = name;
            Actions = actions;
        }

        public void PrintActions(bool terminatable)
        {
            const string SPACING = "  ";
            Console.WriteLine(string.Format("{0} Actions:", Name));
            for(int i = 0; i < Actions.Length; i++)
            {
                Console.WriteLine(string.Format("{0}{1}: {2}", SPACING, i + 1, Actions[i]));
            }
            if (terminatable)
            {
                Console.WriteLine(string.Format("{0}T: Terminate", SPACING));
            }
        }

        public UIAWindow GetWindow()
        {
            Console.WriteLine("Get Window " + Name);
            return null;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class MainWindow : AutomationWindow
    {
        string ExecutablePath;

        public MainWindow(string name, string executablePath, params string[] actions) : base(name, actions)
        {
            ExecutablePath = executablePath;
        }

        public UIAWindow LaunchApp()
        {
            Process process = Process.Start(ExecutablePath);
            process.WaitForInputIdle();
            return Desktop.UIA.Find(TreeScope.Children, "Handle", process.MainWindowHandle.ToInt32()) as UIAWindow;
        }
    }

    public class AutomationWindowCollection : List<AutomationWindow>
    {
        public AutomationWindow this[string windowName]
            => this.FirstOrDefault(window => window.Name == windowName);
    }
}
