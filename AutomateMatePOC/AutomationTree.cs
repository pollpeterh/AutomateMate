using QAliber.Engine.Controls.UIA;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutomateMatePOC
{
    public class AutomationTree
    {
        AutomationRootNode RootNode;
        public AutomationTreeNode CurrentNode;

        public AutomationTree(MainWindow window)
        {
            RootNode = new AutomationRootNode(window);
            CurrentNode = RootNode;
        }

        public void Run(Assembly assembly)
        {
            AutomationTools.EnableQALiber();
            RootNode.Run(assembly);
        }

        public override string ToString()
        {
            return "Automation Tree:" + RootNode.ToString();
        }
    }

    public class AutomationTreeNode
    {
        public AutomationTreeNode Parent { get; set; }
        public int Depth;

        public void Add(AutomationTreeNode node)
        {
            if (node is AutomationWindowNode)
            {
                (this as AutomationActionNode).Windows.Add(node as AutomationWindowNode);
            }
            else
            {
                (this as AutomationWindowNode).Actions.Add(node as AutomationActionNode);
            }
        }

        public override string ToString() 
            => (this is AutomationWindowNode) ? (this as AutomationWindowNode).Window.ToString() : (this as AutomationActionNode).Action.ToString();
    }

    public class AutomationWindowNode : AutomationTreeNode
    {
        public AutomationWindow Window;
        public List<AutomationActionNode> Actions;
        public bool Terminated = false;
        public bool Terminatable => Actions.Count > 0;

        public AutomationWindowNode(AutomationWindow window, AutomationActionNode parent)
        {
            Window = window;
            Parent = parent;
            Actions = new List<AutomationActionNode>();
        }

        public void Run(Assembly assembly)
        {
            UIAWindow window = Window.GetWindow();
            foreach(AutomationActionNode action in Actions)
            {
                action.Run(assembly, window);
            }
        }

        public override string ToString()
        {
            string retVal = string.Format("\n{0}{1}", string.Concat(Enumerable.Repeat("| ", Depth)).TrimEnd(' ') + (Depth > 0 ? "-" : ""), Window.ToString());
            foreach (AutomationActionNode action in Actions)
            {
                retVal += action.ToString();
            }
            return retVal;
        }
    }

    public class AutomationActionNode : AutomationTreeNode
    {
        public AutomationAction Action;
        public List<AutomationWindowNode> Windows;
        public Dictionary<string, string> Arguments;

        public AutomationActionNode(AutomationAction action, AutomationWindowNode parent)
        {
            Action = action;
            Arguments = Action.Arguments.ToDictionary(key => key);
            Windows = new List<AutomationWindowNode>();
        }

        public void Run(Assembly assembly, UIAWindow window)
        {
            assembly.GetType(Action.Window).GetMethod(Action.Name).Invoke(null, new object[] { window, Arguments.Values.ToArray() });
            foreach (AutomationWindowNode windowNode in Windows)
            {
                windowNode.Run(assembly);
            }
        }

        public override string ToString()
        {
            string retVal = string.Format("\n{0}{1}({2})", string.Concat(Enumerable.Repeat("| ", Depth)).TrimEnd(' ') + "-", Action.ToString(), string.Join(", ", Arguments.Values));
            foreach (AutomationWindowNode window in Windows)
            {
                retVal += window.ToString();
            }
            return retVal;
        }
    }

    class AutomationRootNode : AutomationWindowNode
    {
        public new MainWindow Window;

        public AutomationRootNode(MainWindow mainWindow) : base(mainWindow, null)
        {
            Window = mainWindow;
        }

        public new void Run(Assembly assembly)
        {
            UIAWindow window = Window.LaunchApp();
            foreach (AutomationActionNode action in Actions)
            {
                action.Run(assembly, window);
            }
        }
    }
}
