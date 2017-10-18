using QAliber.Engine;
using QAliber.Engine.Controls.UIA;
using QAliber.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomateMatePOC
{
    public static class AutomationTools
    {
        public static void EnableQALiber()
        {
            Log.Default.Enabled = false;
            PlayerConfig.Default.AnimateMouseCursor = false;
            PlayerConfig.Default.DelayAfterAction = 0;
        }

        public static T GetControl<T>(this UIAControl parent, Identifier identifier)
        {
            return (T)(object)parent.Find(System.Windows.Automation.TreeScope.Children, identifier.GetProperties(), identifier.GetValues());
        }
    }
}
