///////////////////////////////////////////////////////////////////////////
// CalculateMetrics.cs  -  Calculate cohesion and coupling metrics       //
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
 * This package calculates cohesion based on the LCOM1 algorithm and also
 * calculated coupling.
 * 
 * Public Interface
 * ================
 * calculateCoupling(classInfo) // calculates coupling for a given class
 * calculateCohesion(classInfo) // calculates cohesion for a given class
 * ...
 * 
 */
/*
 * Build Process
 * =============
 * Required Files:
 *   CalculateMetrics.cs
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
    class CCalculateMetrics
    {
        //----< compute coupling using class and function dependencies >------------------------------------
        public static int calculateCoupling(CClassInfo classInfo)
        {
            List<string> uniqueDependencies = new List<string>(classInfo.dependencies);

            foreach (CFunctionInfo functionInfo in classInfo.functionInfoList)
            {
                foreach (string s in functionInfo.dependencies)
                {
                    if (!uniqueDependencies.Contains(s))
                    {
                        uniqueDependencies.Add(s);
                    }
                }
            }

            return uniqueDependencies.Count;
        }

        //----< calculate cohesion using class data members and function data member references >------------------------------------
        public static int calculateCohesion(CClassInfo classInfo)
        {
            int cohesionValue = 0;
            bool found;

            foreach (CFunctionInfo functionInfoA in classInfo.functionInfoList)
            {
                found = false;
                foreach (CFunctionInfo functionInfoB in classInfo.functionInfoList)
                {
                    if (functionInfoA.name == functionInfoB.name)
                    {
                        found = true;
                        continue;
                    }

                    if (found)
                    {
                        //Console.WriteLine("comparing {0} {1}", functionInfoA.name, functionInfoB.name);

                        if (!doMembersShareValues(functionInfoA.dataMemberReferences, functionInfoB.dataMemberReferences))
                        {
                            //Console.WriteLine("--- don't share {0} {1}", functionInfoA.name, functionInfoB.name);
                            cohesionValue++;
                        }
                    }
                }
            }

            return cohesionValue;
        }

        //----< determine if data members are shared >------------------------------------
        static bool doMembersShareValues(List<CMemberInfo> a, List<CMemberInfo> b)
        {
            foreach (CMemberInfo memberA in a)
            {
                foreach (CMemberInfo memberB in b)
                {
                    if ((memberA.type==memberB.type) && (memberA.name == memberB.name))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

#if (TEST_METRICS)
        static void Main(string[] args)
        {
            CClassList classList = new CClassList();

            CClassInfo classInfo = new CClassInfo();

            // CLASS
            classInfo.className = "CTestClass";
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

            int cohesion = calculateCohesion(classInfo);

            int coupling = calculateCoupling(classInfo);

            Console.WriteLine("cohesion {0}, coupling {1}", cohesion, coupling);

        }
#endif
    }

}
