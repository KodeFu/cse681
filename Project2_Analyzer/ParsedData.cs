///////////////////////////////////////////////////////////////////////////
// ParsedData.cs  -  Data storage for parsed classes and functions       //
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
 * This package contains the data structure to store parsed data including
 * class information (dependencies and class members) and functions
 * (dependencies and references to class members).
 * 
 * Public Interface
 * ================
 * CFunctionInfo
 *   name                               // name of function
 *   dependency                         // list of class names
 *   dataMembers                        // type/name of class' data members
     findDependency(functionName)       // get dependency list for function
 *   findDataMember(type, name)         // find index of data member
 *   addDependency(className)           // add a new dependency to the list
 *   addDataMember(type, name)          // add new data member
 *   addDataMemberReference(type, name) // add new reference
 *   dumpFunctionInfoList()             // output functionInfo data
 * CClassInfo
 *   name                               // name of class
 *   dependency                         // list of class names
 *   dataMembers                        // type/name of data members
 *   findDependency(className)          // get dependency list
 *   findDataMember(type, name)         // find index of data member
 *   addDependency(className)           // add new dependency to the list
 *   addDataMember(type, name)          // add new data member
 *   dumpFunctionInfoList()             // output classInfo data
 * CClassList
 *   classList                          // the list of all classes
 *   getClassIndex(className)           // get index of class
 *   getClassInfo(className)            // get classInfo
 *   getClassFunctionInfo(className, functionName) // get function info for function
 *   dumpClassList()                    // dump all parsedData; i.e. classList
 * ...
 * 
 */
/*
 * Build Process
 * =============
 * Required Files:
 *   ParsedData.cs
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
    public class CMemberInfo
    {
        public string type { get; set; }
        public string name { get; set; }
    }

    public class CFunctionInfo
    {
        public string name { get; set; }

        public List<string> dependencies { get; set; }
        public List<CMemberInfo> dataMemberReferences { get; set; }

        public CFunctionInfo()
        {
            name = "";

            dependencies = new List<string>();
            dataMemberReferences = new List<CMemberInfo>();
        }

        //----< output dependecy info >------------------------------------
        public void dumpFunctioninfo()
        {
            Console.WriteLine("    {0}()", name);
            Console.WriteLine("        Dependencies:");
            foreach (string s in dependencies)
            {
                Console.WriteLine("        - {0}", s);
            }
            Console.WriteLine();

            Console.WriteLine("        Data Member References:");
            foreach (CMemberInfo m in dataMemberReferences)
            {
                Console.WriteLine("        - {0} {1}", m.type, m.name);
            }
        }

        //----< find dependency using function name >------------------------------------
        public int findDependency(string name)
        {
            int index = 0;
            foreach (string d in dependencies)
            {
                if (d == name)
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        //----< find data member using type and name >------------------------------------
        public int findDataMember(string type, string name)
        {
            int index = 0;
            foreach (CMemberInfo mi in dataMemberReferences)
            {
                if ((mi.type == type) && (mi.name == name))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        //----< add a dependency >------------------------------------
        public void addDependency(string name)
        {
            if (findDependency(name) == -1)
            {
                dependencies.Add(name);
            }
        }

        //----< add a data member reference >------------------------------------
        public void addDataMemberReference(string type, string name)
        {
            if (findDataMember(type, name) == -1)
            {
                CMemberInfo memberInfo = new CMemberInfo();

                memberInfo.type = type;
                memberInfo.name = name;

                dataMemberReferences.Add(memberInfo);
            }
        }
    }

    public class CClassInfo
    {
        public string className { get; set; }
        public List<string> dependencies { get; set; }
        public List<CMemberInfo> dataMembers { get; set; }
        public List<CFunctionInfo> functionInfoList { get; set; }

        public CClassInfo()
        {
            className = "";
            dataMembers = new List<CMemberInfo>();
            dependencies = new List<string>();
            functionInfoList = new List<CFunctionInfo>();
        }

        //----< output dependecy info >------------------------------------
        public void dumpClassInfo()
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Class: {0}()", className);
            Console.WriteLine("------------------------------------");
            Console.WriteLine();

            Console.WriteLine("Data Members:");
            foreach (CMemberInfo m in dataMembers)
            {
                Console.WriteLine("    - {0} {1}", m.type, m.name);
            }

            Console.WriteLine();
            Console.WriteLine("Dependencies:");
            foreach (string s in dependencies)
            {
                Console.WriteLine("    - {0}", s);
            }

            Console.WriteLine();
            Console.WriteLine("Function Info:");
            foreach (CFunctionInfo f in functionInfoList)
            {
                Console.WriteLine();
                f.dumpFunctioninfo();
            }
        }

        //----< find class dependency using class name >------------------------------------
        public int findDependency(string name)
        {
            int index = 0;
            foreach (string d in dependencies)
            {
                if (d == name)
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        //----< find data member using type and name >------------------------------------
        public int findDataMember(string type, string name)
        {
            int index = 0;
            foreach (CMemberInfo dm in dataMembers)
            {
                if ((dm.type == type) && (dm.name == name))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        //----< add a new class dependency >------------------------------------
        public void addDependency(string name)
        {
            if (findDependency(name) == -1)
            {
                dependencies.Add(name);
            }
        }

        //----< add a new data member >------------------------------------
        public void addDataMember(string type, string name)
        {
            if (findDataMember(type, name) == -1)
            {
                CMemberInfo memberInfo = new CMemberInfo();

                memberInfo.type = type;
                memberInfo.name = name;

                dataMembers.Add(memberInfo);
            }
        }
    }

    public class CClassList
    {
        public List<CClassInfo> classList { get; set; }

        public CClassList()
        {
            classList = new List<CClassInfo>();
        }

        public void dumpClassList()
        {
            foreach (CClassInfo c in classList)
            {
                c.dumpClassInfo();
                Console.WriteLine();
            }
        }

        //----< retrieve the class at a specified classList index >------------------------------------
        public int getClassIndex(string className)
        {
            int index = 0;
            foreach (CClassInfo ci in classList)
            {
                if (ci.className == className)
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        //----< find the class using the class name >------------------------------------
        public CClassInfo getClassInfo(string className)
        {
            foreach (CClassInfo ci in classList)
            {
                if (ci.className == className)
                {
                    return ci;
                }
            }

            // allocate a new one
            CClassInfo newClassInfo = new CClassInfo();

            newClassInfo.className = className;
            classList.Add(newClassInfo);

            return newClassInfo;
        }

        //----< get function info for a particular class >------------------------------------
        public CFunctionInfo getClassFunctionInfo(string className, string classFunction)
        {
            CClassInfo classInfo = getClassInfo(className);

            foreach (CFunctionInfo fi in classInfo.functionInfoList)
            {
                if (fi.name == classFunction)
                {
                    return fi;
                }
            }

            // allocate a new one
            CFunctionInfo newFunctionInfo = new CFunctionInfo();

            newFunctionInfo.name = classFunction;
            classInfo.functionInfoList.Add(newFunctionInfo);

            return newFunctionInfo;
        }

#if (TEST_PARSED_DATA)
        static void Main(string[] args)
        {
            CClassList classList = new CClassList();

            CClassInfo classInfo = new CClassInfo();

            classList.classList.Add(classInfo);

            // CLASS B
            classInfo.className = "CTestClassA";
            classInfo.dependencies.Add("cdCToken");
            classInfo.dependencies.Add("cdITem");
            classInfo.dependencies.Add("cdString");

            CMemberInfo memberInfo = new CMemberInfo();
            memberInfo.type = "cmA";
            memberInfo.name = "nameA";
            classInfo.dataMembers.Add(memberInfo);
            memberInfo = new CMemberInfo();
            memberInfo.type = "cmB";
            memberInfo.name = "nameB";
            classInfo.dataMembers.Add(memberInfo);

            // FUNCTION A
            CFunctionInfo functionInfo = new CFunctionInfo();
            functionInfo.name = "fnTestFunction1";

            memberInfo = new CMemberInfo();
            memberInfo.type = "cmA";
            memberInfo.name = "nameA";
            functionInfo.dataMemberReferences.Add(memberInfo);
            memberInfo = new CMemberInfo();
            memberInfo.type = "cmB";
            memberInfo.name = "nameB";
            functionInfo.dataMemberReferences.Add(memberInfo);

            classInfo.functionInfoList.Add(functionInfo);

            // FUNCTION B
            functionInfo = new CFunctionInfo();
            functionInfo.name = "fnTestFunction2";

            memberInfo = new CMemberInfo();
            memberInfo.type = "cmZ";
            memberInfo.name = "newItemA";
            functionInfo.dataMemberReferences.Add(memberInfo);
            memberInfo = new CMemberInfo();
            memberInfo.type = "cmB";
            memberInfo.name = "nameB";
            functionInfo.dataMemberReferences.Add(memberInfo);

            classInfo.functionInfoList.Add(functionInfo);

            // FUNCTION C
            functionInfo = new CFunctionInfo();
            functionInfo.name = "fnTestFunction3";

            memberInfo = new CMemberInfo();
            memberInfo.type = "cmB";
            memberInfo.name = "nameB";
            functionInfo.dataMemberReferences.Add(memberInfo);
            memberInfo = new CMemberInfo();
            memberInfo.type = "cmJ";
            memberInfo.name = "newItemB";
            functionInfo.dataMemberReferences.Add(memberInfo);

            classInfo.functionInfoList.Add(functionInfo);

            classInfo = new CClassInfo();
            classList.classList.Add(classInfo);

            // CLASS B
            classInfo.className = "CTestClassB";
            classInfo.dependencies.Add("yellow");
            classInfo.dependencies.Add("orange");
            classInfo.dependencies.Add("green");

            memberInfo = new CMemberInfo();
            memberInfo.type = "Monkey";
            memberInfo.name = "dave";
            classInfo.dataMembers.Add(memberInfo);
            memberInfo = new CMemberInfo();
            memberInfo.type = "Car";
            memberInfo.name = "Toyota";
            classInfo.dataMembers.Add(memberInfo);

            // FUNCTION A
            functionInfo = new CFunctionInfo();
            functionInfo.name = "fnTestFunction1B";

            memberInfo = new CMemberInfo();
            memberInfo.type = "Monkey";
            memberInfo.name = "dave";
            functionInfo.dataMemberReferences.Add(memberInfo);
            memberInfo = new CMemberInfo();
            memberInfo.type = "Snake";
            memberInfo.name = "Python";
            functionInfo.dataMemberReferences.Add(memberInfo);

            classInfo.functionInfoList.Add(functionInfo);

            // FUNCTION B
            functionInfo = new CFunctionInfo();
            functionInfo.name = "fnTestFunction2B";

            memberInfo = new CMemberInfo();
            memberInfo.type = "Bug";
            memberInfo.name = "Beatles";
            functionInfo.dataMemberReferences.Add(memberInfo);
            memberInfo = new CMemberInfo();
            memberInfo.type = "Snake";
            memberInfo.name = "Python";
            functionInfo.dataMemberReferences.Add(memberInfo);

            classInfo.functionInfoList.Add(functionInfo);

            // FUNCTION C
            functionInfo = new CFunctionInfo();
            functionInfo.name = "fnTestFunction3B";

            memberInfo = new CMemberInfo();
            memberInfo.type = "Computer";
            memberInfo.name = "Dell";
            functionInfo.dataMemberReferences.Add(memberInfo);
            memberInfo = new CMemberInfo();
            memberInfo.type = "Computer";
            memberInfo.name = "HP";
            functionInfo.dataMemberReferences.Add(memberInfo);

            classInfo.functionInfoList.Add(functionInfo);

            Console.WriteLine("Break here and observe ParsedData data structure.");
        }
#endif
    }
}