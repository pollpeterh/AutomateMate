using System;

namespace AutomateMatePOC
{
    public static class Constants
    {
        public const string NAMESPACE_NAME  = "NAMESPACE_NAME";
        public const string NAMESPACE_BODY  = "NAMESPACE_BODY";
        public const string CLASS_NAME      = "CLASS_NAME";
        public const string CLASS_BODY      = "CLASS_BODY";
        public const string METHOD_NAME     = "METHOD_NAME";
        public const string METHOD_BODY     = "METHOD_BODY";

        public static readonly string[] DEPENDENCIES = new string[]
        {
            "QAliber.Engine.dll",
            "AutomateMatePOC.dll",
            "System.dll"
        };

        public const string NAMESPACE = 
            "namespace " + NAMESPACE_NAME + "\n" +  
            "{\n" +
                NAMESPACE_BODY + "\n" +
            "}\n";

        public const string USING = 
            "using QAliber.Engine.Controls.UIA;\n" +
            "using QAliber.Engine.Controls;\n" +
            "using AutomateMatePOC;\n" + 
            "using System;\n";

        public const string CLASS = 
            "\tpublic static class " + CLASS_NAME + "\n" +
            "\t{\n" +
                 CLASS_BODY + "\n" +
            "\t}\n";

        public const string METHOD = 
            "\t\tpublic static void " + METHOD_NAME + "(UIAWindow window, string[] args)\n" +
            "\t\t{\n" +
                "\t\t\t" + METHOD_BODY + "\n" +                     
            "\t\t}\n";
    }
}
