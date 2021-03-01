///////////////////////////////////////////////////////////////////////////
// TokenHelper.cs  -  Helper function to further parse tokens            //
// ver 1.0                                                               //
// Language:    C#, Visual Studio 2017, .Net Framework 4.5               //
// Platform:    Sandy Bridge / i7-2600k PC (custome build), Win 10 Pro   //
// Application: Pr#2 Code Maintainbility Analyzer, CSE681, Sum 2018      //
// Author:      Mudit Vats, Syracuse University                          //
//              mpvats@syr.edu                                           //
///////////////////////////////////////////////////////////////////////////
/*
 * Package Operations
 * ==================
 * This package contains functions to parse tokens. For the identification
 * of classes, user defined and system types, we need a way to break up
 * an expression into various ways and process (remove) unneeded elements.
 * The Toker and Semi provide some capability, but this class helps with
 * some additional processing so tokens can be more easily identified.
 * 
 * Public Interface
 * ================
 * RemoveNewLinesFromString(string) // removew newlines from string
 * RemoveNewLines(semi)             // remove new lines
 * RemoveKeyworkd(semi)             // remove C# keyworkds
 * RemoveAccess(semi)               // remove private, protected etc
 * RemoveGenerics(semi)             // remove generics <>
 * RemoveIndices(semi)              // remove indicies []
 * GetLeftOfEqual(semi)             // get left side of equal
 * GetRightOfEqual(semi)            // get right side of equal
 * CombineNamespace(semi)           // combine split namespace into one value
 * GetInheritance(semi)             // get class inheritance value
 * GetFunctionParameters(semi)      // get function parameters
 * GetNumberOfParameters(semi)      // get number of parameters
 * GetFunctionParameterAtIndex(semi, index) // get paramter at index
 * GetNewUsages(semi)              // get "new" uses
 * ...
 * 
 */
/*
 * Build Process
 * =============
 * Required Files:
 *   TokenHelper.cs
 *   
 * Compiler Command:
 *   devenv Analyzer.csproj /rebuild release
 * 
 * Maintenance History
 * ===================
 * ver 1.0 : 8/11/18
 *   - first release
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
    class TokenHelper
    {
        //----< remove new line characters from string >------------------------------
        static public string RemoveNewLinesFromString(string s)
        {
            string newString = s.Replace("\n", "");
            return newString;
        }

        //----< remove new lines from semi expression >------------------------------
        static public CSsemi.CSemiExp RemoveNewLines(CSsemi.CSemiExp s)
        {
            CSsemi.CSemiExp newSemi = new CSsemi.CSemiExp();

            for (int i = 0; i < s.count; i++)
            {
                string cleanString = RemoveNewLinesFromString(s[i]);
                if (cleanString.Length>0)
                {
                    newSemi.Add(cleanString);
                }
            }

            return newSemi;
        }

        //----< remove c# keywords from semi expression >------------------------------
        static public CSsemi.CSemiExp RemoveKeywords(CSsemi.CSemiExp s)
        {
            CSsemi.CSemiExp newSemi = new CSsemi.CSemiExp();

            for (int i = 0; i < s.count; i++)
            {
                if (s[i] != "static")
                {
                    newSemi.Add(s[i]);
                }
            }

            return newSemi;
        }

        //----< remove public/private/protected from semi expression >------------------------------
        static public CSsemi.CSemiExp RemoveAccess(CSsemi.CSemiExp s)
        {
            CSsemi.CSemiExp newSemi = new CSsemi.CSemiExp();

            for (int i = 0; i < s.count; i++)
            {
                if ((s[i] != "public") && (s[i] != "private") && (s[i] != "protected"))
                {
                    newSemi.Add(s[i]);
                }
            }

            return newSemi;
        }

        //----< remove generic contents from semi expression >------------------------------
        static public CSsemi.CSemiExp RemoveGenerics(CSsemi.CSemiExp s)
        {
            CSsemi.CSemiExp newSemi = new CSsemi.CSemiExp();

            int skip = 0;
            for (int i = 0; i < s.count; i++)
            {
                if (s[i] == "<")
                {
                    skip++;
                }
                if (skip == 0)
                {
                    newSemi.Add(s[i]);
                }
                if (s[i] == ">")
                {
                    --skip;
                }
            }

            return newSemi;
        }

        //----< remove bracket indicies from semi expression >------------------------------
        static public CSsemi.CSemiExp RemoveIndicies(CSsemi.CSemiExp s)
        {
            CSsemi.CSemiExp newSemi = new CSsemi.CSemiExp();

            int skip = 0;
            for (int i = 0; i < s.count; i++)
            {
                if (s[i] == "[")
                {
                    skip++;
                }
                if (skip == 0)
                {
                    newSemi.Add(s[i]);
                }
                if (s[i] == "]")
                {
                    --skip;
                }
            }

            return newSemi;
        }

        //----< get left of equal from semi expression >------------------------------
        static public CSsemi.CSemiExp GetLeftOfEqual(CSsemi.CSemiExp s)
        {
            CSsemi.CSemiExp newSemi = new CSsemi.CSemiExp();

            int index = s.FindFirst("=");
            if (index != -1)
            {
                for (int i = 0; i < index; i++)
                {
                    newSemi.Add(s[i]);
                }
            }
            else
            {
                for (int i = 0; i < s.count; i++)
                {
                    newSemi.Add(s[i]);
                }
            }

            return newSemi;
        }

        //----< get right of equal from semi expression >------------------------------
        static public CSsemi.CSemiExp GetRightOfEqual(CSsemi.CSemiExp s)
        {
            CSsemi.CSemiExp newSemi = new CSsemi.CSemiExp();

            int index = s.FindFirst("=");
            if (index != -1)
            {
                if (index + 1 < s.count)
                {
                    for (int i = index + 1; i < s.count; i++)
                    {
                        newSemi.Add(s[i]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < s.count; i++)
                {
                    newSemi.Add(s[i]);
                }
            }

            return newSemi;
        }

        //----< combine namespace tokens into full dotted notation for semi expression >------------
        static public CSsemi.CSemiExp CombineNamespace(CSsemi.CSemiExp s)
        {
            string combinedNamespace = "";
            CSsemi.CSemiExp newSemi = new CSsemi.CSemiExp();

            for (int i = 0; i < s.count; i++)
            {
                combinedNamespace = combinedNamespace + s[i];

                if (i + 1 < s.count)
                {
                    if (s[i + 1] == ".")
                    {
                        combinedNamespace = combinedNamespace + s[i + 1];
                        i++;
                    }
                    else
                    {
                        newSemi.Add(combinedNamespace);
                        combinedNamespace = "";
                    }
                }
                else
                {
                    newSemi.Add(combinedNamespace);
                    combinedNamespace = "";
                }
            }

            return newSemi;
        }

        //----< get class inhertitance from semi expression >------------
        static public CSsemi.CSemiExp GetInheritance(CSsemi.CSemiExp s)
        {
            CSsemi.CSemiExp newSemi = new CSsemi.CSemiExp();

            int index = s.FindFirst(":");
            if (index != -1)
            {
                if (index + 1 < s.count)
                {
                    for (int i = index + 1; i < s.count; i++)
                    {
                        if (s[i] != "{")
                        {
                            newSemi.Add(s[i]);
                        }
                    }
                }
            }

            return newSemi;
        }

        //----< get function parameters from semi expression >------------
        static public CSsemi.CSemiExp GetFunctionParameters(CSsemi.CSemiExp s)
        {
            CSsemi.CSemiExp newSemi = new CSsemi.CSemiExp();

            int index = s.FindFirst("(");
            if (index != -1)
            {
                for (int i = index; i < s.count; i++)
                {
                    if (s[i] == ")")
                    {
                        break;
                    }

                    if (s[i] != "(")
                    {
                        newSemi.Add(s[i]);
                    }
                }
            }

            //newSemi = CombineNamespace(newSemi);

            return newSemi;
        }

        //----< get number of parameters from function semi expression >------------
        static public int GetNumberOfParameters(CSsemi.CSemiExp s)
        {
            int n = 0;

            if (s.count > 0)
            {
                n = 1;
            }

            for (int i = 0; i < s.count; i++)
            {
                if (s[i] == ",")
                {
                    n++;
                }
            }

            return n;
        }

        //----< get function parameters at index from function semi expression >------------
        static public List<string> GetFunctionParameterAtIndex(CSsemi.CSemiExp s, int index)
        {
            int parameterNumber = 0;
            List<string> parameter = new List<string>();

            for (int i = 0; i < s.count; i++)
            {
                if (s[i] == ",")
                {
                    parameterNumber++;
                }

                if (parameterNumber == index)
                {
                    parameter.Add(s[i]);
                }
            }

            return parameter;
        }

        //----< get "new" usage from function semi expression >------------
        static public List<string> GetNewUsages(CSsemi.CSemiExp s)
        {
            List<string> newUsage = new List<string>();
            CSsemi.CSemiExp newSemi;

            newSemi = RemoveNewLines(s);
            newSemi = GetRightOfEqual(newSemi);
            newSemi = CombineNamespace(newSemi);

            int index = newSemi.FindFirst("new");
            if (index != -1)
            {
                if (index + 1 < newSemi.count)
                {
                    newUsage.Add(newSemi[index]);
                    newUsage.Add(newSemi[index + 1]);
                }
            }

            return newUsage;
        }

#if (TEST_TOKEN_HELPER)
        static void Main(string[] args)
        {
          CSsemi.CSemiExp newSemi = new CSsemi.CSemiExp();

            newSemi.Add("\n\n");
            newSemi.Add("\nusing");
            newSemi.Add("System;");
            newSemi.Add("");
            newSemi.Add("List");
            newSemi.Add("<");
            newSemi.Add("string");
            newSemi.Add(">");
            newSemi.Add("newList");
            newSemi.Add("=");
            newSemi.Add("new");
            newSemi.Add("List");
            newSemi.Add("<");
            newSemi.Add("string");
            newSemi.Add(">");
            newSemi.Add(";");

            CSsemi.CSemiExp a = GetLeftOfEqual(newSemi);
            CSsemi.CSemiExp b = GetRightOfEqual(newSemi);
            CSsemi.CSemiExp c = RemoveNewLines(newSemi);
            CSsemi.CSemiExp d = RemoveGenerics(newSemi);

            Console.WriteLine(a.displayStr());
            Console.WriteLine(b.displayStr());
            Console.WriteLine(c.displayStr());

            a = GetLeftOfEqual(newSemi);
            a = RemoveNewLines(a);
            a = RemoveGenerics(a);

            Console.WriteLine(a.displayStr());

            b = GetRightOfEqual(newSemi);
            b = RemoveNewLines(b);
            b = RemoveGenerics(b);

            Console.WriteLine(b.displayStr());

            CSsemi.CSemiExp newSemiCombine = new CSsemi.CSemiExp();
            newSemiCombine.Add("Namespace");
            newSemiCombine.Add(".");
            newSemiCombine.Add("Class");
            newSemiCombine.Add(".");
            newSemiCombine.Add("Function");
            newSemiCombine.Add("testFunc");
            newSemiCombine.Add("()");
            newSemiCombine.Add("=");
            newSemiCombine.Add("SuperNameSpace");
            newSemiCombine.Add(".");
            newSemiCombine.Add("SuperClass");
            newSemiCombine.Add(".");
            newSemiCombine.Add("SuperFunc");
            newSemiCombine.Add("superTestFunc");
            newSemiCombine.Add("(");
            newSemiCombine.Add(")");
            newSemiCombine.Add(";");

            c = CombineNamespace(newSemiCombine);
            Console.WriteLine(c.displayStr());

            newSemi.flush();
            newSemi.Add("pubilc");
            newSemi.Add("class");
            newSemi.Add("myClass");
            newSemi.Add(":");
            newSemi.Add("BaseClass");
            newSemi.Add(":");
            newSemi.Add("IInterface");
            newSemi.Add(":");
            newSemi.Add("IInterface");
            newSemi.Add("{");

            a = GetInheritance(newSemi);
            Console.WriteLine(a.displayStr());

            newSemi.flush();
            newSemi.Add("pubilc");
            newSemi.Add("void");
            newSemi.Add("parse");
            newSemi.Add("(");
            newSemi.Add("SuperNamespace");
            newSemi.Add(".");
            newSemi.Add("SuperClass");
            newSemi.Add(".");
            newSemi.Add("Parser");
            newSemi.Add("p");
            newSemi.Add(",");
            newSemi.Add("int");
            newSemi.Add("value");
            newSemi.Add(")");
            newSemi.Add("{");

            a = GetFunctionParameters(newSemi);
            Console.WriteLine(a.displayStr());

            int n = GetNumberOfParameters(a);
            Console.WriteLine("num parameters: {0}", n);
            List<string> parmList = GetFunctionParameterAtIndex(a, 0);
            Console.WriteLine("type: {0} value: {1}", parmList[0], parmList[1]);
            parmList = GetFunctionParameterAtIndex(a, 1);
            Console.WriteLine("type: {0} value: {1}", parmList[0], parmList[1]);

            newSemi.flush();
            newSemi.Add("pubilc");
            newSemi.Add("void");
            newSemi.Add("parse");
            newSemi.Add("=");
            newSemi.Add("new");
            newSemi.Add("BlockHead");
            newSemi.Add("(");
            newSemi.Add(")");
            newSemi.Add(";");

            List<string> usageList = GetNewUsages(newSemi);
            Console.WriteLine("type: {0} value: {1}", usageList[0], usageList[1]);
        }
#endif
    }
}
