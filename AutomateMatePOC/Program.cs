using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomateMatePOC
{
    public static class Program
    {
        static AutomationWindowCollection Windows;
        static AutomationActionCollection Actions;

        public static void Run()
        {
            // Define All Windows
            Windows = new AutomationWindowCollection
            {
                new MainWindow("NotepadWindow",  "notepad.exe", "WriteText", "OpenFile"),
                new AutomationWindow("SaveWindow", "Save", "DontSave", "Cancel"),
                new AutomationWindow("OpenFileWindow", "OpenFile", "Cancel")
            };

            // Define All Actions
            Actions = new AutomationActionCollection
            {
                new AutomationAction("WriteText", "NotepadWindow", false, "Text")
                {
                    Source = @"
                        UIADocument textEditor = window.GetControl<UIADocument>(new Identifier() { Name = ""Text Editor"" });
                        textEditor.Write(args[0]);
                    "
                },
                new AutomationAction("OpenFile", "NotepadWindow", false)
                {
                    Source = "Console.WriteLine(\"In OpenFile\");",
                    Windows = new []
                    {
                        "OpenFileWindow",
                        "SaveWindow"
                    }
                },
                new AutomationAction("Save", "SaveWindow", true)
                {
                    Source = "Console.WriteLine(\"In Save\");",
                    Windows = new []
                    {
                        "OpenFileWindow"
                    }
                },
                new AutomationAction("DontSave", "SaveWindow", true)
                {
                    Source = "Console.WriteLine(\"In DontSave\");",
                    Windows = new []
                    {
                        "OpenFileWindow"
                    }
                },
                new AutomationAction("Cancel", "SaveWindow", true)
                {
                    Source = "Console.WriteLine(\"In Cancel\");"
                },
                new AutomationAction("OpenFile", "OpenFileWindow", true, "File Path")
                {
                    Source = "Console.WriteLine(\"In OpenFile\");"
                },
                new AutomationAction("Cancel", "OpenFileWindow", true)
                {
                    Source = "Console.WriteLine(\"In OpenFile\");"
                }
            };

            // Verify Windows and Actions
            if (!VerifyRelationships()) return;

            // Create and build Automation Tree
            AutomationTree tree = new AutomationTree(Windows["NotepadWindow"] as MainWindow);
            BuildTree(tree, new Stack<string>());

            Console.WriteLine();

            // Print Automation Tree
            Console.WriteLine(tree);

            Console.WriteLine();

            // Build Compilation Set
            CompilerResults compilerResults = Compile(Actions);
            PrintStatus("Compiling", compilerResults.Errors.Count == 0, null);
            foreach(CompilerError ce in compilerResults.Errors)
            {
                Console.WriteLine(ce.ErrorNumber + ": " + ce.ErrorText);
            }

            Console.WriteLine("Run Tree");
            tree.Run(compilerResults.CompiledAssembly);
        }

        public static bool VerifyRelationships()
        {
            bool actionVerified = true;
            string errStr = string.Empty;
            foreach (AutomationAction action in Actions)
            {
                actionVerified = actionVerified && Windows[action.Window] != null;
                if (!actionVerified)
                {
                    errStr += string.Format("{0}\"{1}\" FAILED: No window \"{2}\" exists for action.", "\n\t", action.Name, action.Window);
                }
            }
            PrintStatus("Verifying Action -> Window Relationships", actionVerified, errStr);

            if (!actionVerified) return false;

            bool windowVerified = true;
            errStr = string.Empty;
            foreach (AutomationWindow window in Windows)
            {
                foreach(string action in window.Actions)
                {
                    windowVerified = windowVerified && Actions[action, window.Name] != null;
                    if (!windowVerified)
                    {
                        errStr += string.Format("{0}\"{1}\" FAILED: No action \"{2}\" exists for window.", "\n\t", window.Name, action);
                    }
                }
            }
            PrintStatus("Verifying Window -> Action Relationships", windowVerified, errStr);

            return windowVerified ? true : false;
        }

        public static void BuildTree(AutomationTree tree, Stack<string> windowStack, int depth = 0)
        {
            if(tree.CurrentNode is AutomationWindowNode)
            {
                AutomationWindowNode windowNode = tree.CurrentNode as AutomationWindowNode;
                AutomationWindow window = windowNode.Window;
                windowStack.Push(windowNode.Window.Name);
                depth++;
                while (!windowNode.Terminated)
                {
                    Console.WriteLine(string.Format("\nDepth: {0}", string.Join(" -> ", windowStack.Reverse())));
                    window.PrintActions(windowNode.Terminatable);
                    //List<AutomationWindow> previousWindows = FindPrevious(window);
                    int index = GetChoice(windowNode.Terminatable);
                    if (index == -1)
                    {
                        windowNode.Terminated = true;
                        return;
                    }
                    AutomationAction action = Actions[window.Actions[index], window.Name];
                    action.Compile = true;
                    AutomationActionNode actionNode = new AutomationActionNode(action, windowNode)
                    {
                        Depth = depth
                    };
                    actionNode.GetArguments();
                    windowNode.Add(actionNode);
                    windowNode.Terminated = action.Terminating;
                    tree.CurrentNode = actionNode;
                    BuildTree(tree, windowStack, depth);
                }
                depth--;
                windowStack.Pop();
            }
            else if (tree.CurrentNode is AutomationActionNode)
            {
                AutomationActionNode actionNode = tree.CurrentNode as AutomationActionNode;
                AutomationWindowNode parentWindowNode = actionNode.Parent as AutomationWindowNode;
                AutomationAction action = actionNode.Action;
                depth++;
                foreach(string windowName in action.Windows)
                {
                    AutomationWindow window = Windows[windowName];
                    AutomationWindowNode windowNode = new AutomationWindowNode(window, actionNode)
                    {
                        Depth = depth
                    };
                    actionNode.Add(windowNode);
                    tree.CurrentNode = windowNode;
                    BuildTree(tree, windowStack, depth);
                }
                depth--;
            }
        }

        public static void PrintStatus(string name, bool status, string errStr)
        {
            Console.WriteLine(string.Format("{0}{1}{2}", name.PadRight(45, '.'), status ? "Pass" : "Fail", status ? "" : errStr));
        }

        public static int GetChoice(bool terminatable)
        {
            Console.Write("Enter choice: ");
            string choice = Console.ReadLine();
            if (Int32.TryParse(choice, out int index))
            {
                return index - 1;
            }
            else if (terminatable && choice.ToUpper() == "T")
            {
                return -1;
            }
            throw new StatusException("Invalid input", true);
        }

        public static void GetArguments(this AutomationActionNode actionNode)
        {
            foreach(string argKey in actionNode.Action.Arguments)
            {
                Console.Write("  " + argKey + ": ");
                string argVal = Console.ReadLine();
                actionNode.Arguments[argKey] = argVal;
            }
        }

        public static CompilerResults Compile(AutomationActionCollection Actions)
        {
            string compileString = string.Empty;
            Dictionary<string, AutomationActionCollection> compileSet = new Dictionary<string, AutomationActionCollection>();
            foreach(AutomationAction action in Actions)
            {
                if (!action.Compile)
                {
                    continue;
                }
                if(compileSet.ContainsKey(action.Window))
                {
                    compileSet[action.Window].Add(action);
                }
                else
                {
                    compileSet.Add(action.Window, new AutomationActionCollection { action });
                }
            }
            foreach(KeyValuePair<string, AutomationActionCollection> pair in compileSet) {
                string classBody = string.Empty;
                foreach(AutomationAction action in pair.Value)
                {
                    classBody += MethodString(action.Name, action.Source);
                }
                compileString += ClassString(pair.Key, classBody);
            }

            using (CSharpCodeProvider cscp = new CSharpCodeProvider())
            {
                CompilerParameters compilerParams = new CompilerParameters()
                {
                    GenerateInMemory = true
                };

                foreach (String dependency in Constants.DEPENDENCIES)
                {
                    compilerParams.ReferencedAssemblies.Add(dependency);
                }

                //compileString = NamespaceString("AutomateMatePOC", compileString);
                compileString = Constants.USING + compileString;

                //Console.WriteLine("Source:\n" + compileString);

                return cscp.CompileAssemblyFromSource(compilerParams, compileString);
            }


            string MethodString(string name, string body)
            {
                return Constants.METHOD.Replace(Constants.METHOD_NAME, name).Replace(Constants.METHOD_BODY, body);
            }

            string ClassString(string name, string body)
            {
                return Constants.CLASS.Replace(Constants.CLASS_NAME, name).Replace(Constants.CLASS_BODY, body);
            }

            string NamespaceString(string name, string body)
            {
                return Constants.NAMESPACE.Replace(Constants.NAMESPACE_NAME, name).Replace(Constants.NAMESPACE_BODY, body);
            }
        }
    }
}
