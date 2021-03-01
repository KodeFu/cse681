///////////////////////////////////////////////////////////////////////////
// Analyzer.cs  -  The main application                                  //
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
 * The main executabile for the Code Maintainability Analyzer 
 * application. Code Maintainability Analyzer computes code metrics given 
 * a set of source code.
 * 
 * Public Interface
 * ================
 * None
 * ...
 * 
 */
/*
 * Build Process
 * =============
 * Required Files:
 *   Analyzer.cs
 *   CalculateMetrics.cs
 *   Display.cs
 *   IRuleandAction.cs
 *   ParsedData.cs
 *   Parser.cs
 *   RulesAndActions.cs
 *   ScopeStack.cs
 *   Semi.cs
 *   TokenHelper.cs
 *   Toker.cs
 *   
 * Compiler Command:
 *   devenv Analyzer.csproj /rebuild release
 * 
 *  * Usage
 * ================
 * Usage: analyzer FILEPATH FILE... <OPTIONS>
 * Code Maintainability Analyzer computes code metrics given a set of source code.
 * 
 * Metrics:
 *   -c  METRIC    compute only one metric where METRIC can be:
 *                   loc     for lines of code
 *                   cmplx   for code complexity
 *                   cohsn   for cohesion using LCOM1
 *                   coupl   for coupling value
 *                   main    for maintainability index
 * 
 * Coefficients:
 *   -cl VALUE     coefficient for LOC metric
 *   -cx VALUE     coefficient for the complexity metric
 *   -ch VALUE     coefficient for the cohesion metric
 *   -cp VALUE     coefficient for the coupling metric
 * 
 * *Default value for all coefficients is 1
 * 
 * Miscellaneous:
 *   -o FILEPATH   redirect console outpu to FILEPATH; ex. "\myLogs\out.txt"
 *   -h            show help / application usage
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
using System.IO;

namespace CodeAnalysis
{
    class Analyzer
    {
        static bool optOutputToFile;
        static string optOutputToFileFilepath;
        static bool optLOC;
        static bool optComplexity;
        static bool optCohesion;
        static bool optCoupling;
        static bool optShowRelationships;
        static int optLOCCoefficient;
        static int optComplexityCoefficient;
        static int optCohesionCoefficient;
        static int optCouplingCoefficient;
        static int totalLOC;
        static int totalComplexity;
        static int totalCohesion;
        static int totalCoupling;
        static int totalMainIndex;

        //----< initialize options and control vars >-----------------
        static Analyzer()
        {
            optLOC = true;
            optComplexity = true;
            optCohesion = true;
            optCoupling = true;
            optShowRelationships = false;
            optLOCCoefficient = 1;
            optComplexityCoefficient = 1;
            optCohesionCoefficient = 1;
            optCouplingCoefficient = 1;
            optOutputToFile = false;
            optOutputToFileFilepath = string.Empty;

            totalComplexity = 0;
            totalCohesion = 0;
            totalCoupling = 0;
            totalMainIndex = 0;
        }

        //----< handle arguments with an associated integer value >-----------------
        static bool getIntOptionParameter(string[] args, string optionName, out int optionParameter)
        {
            optionParameter = 0;

            List<string> options = args.ToList<string>();
            int optionIndex = options.IndexOf(optionName);

            if ((optionIndex != -1) && (optionIndex + 1 < options.Count))
            {
                if (Int32.TryParse(options[optionIndex + 1], out optionParameter))
                {
                    return true;
                }

            }

            return false;
        }

        //----< handle arguments with an associated string value >-----------------
        static bool getStringOptionParameter(string[] args, string optionName, out string optionParameter)
        {
            optionParameter = string.Empty;

            List<string> options = args.ToList<string>();
            int optionIndex = options.IndexOf(optionName);

            if ((optionIndex != -1) && (optionIndex + 1 < options.Count))
            {
                optionParameter = options[optionIndex + 1];
                return true;
            }

            return false;
        }

        //----< save metric coeffienct arguments >-----------------
        static bool processMetricCoeffiencts(string[] args)
        {
            int optionValue;
            if (getIntOptionParameter(args, "-cl", out optionValue))
            {
                optLOCCoefficient = optionValue;
            }

            if (getIntOptionParameter(args, "-cx", out optionValue))
            {
                optComplexityCoefficient = optionValue;
            }

            if (getIntOptionParameter(args, "-ch", out optionValue))
            {
                optCohesionCoefficient = optionValue;
            }

            if (getIntOptionParameter(args, "-cp", out optionValue))
            {
                optCouplingCoefficient = optionValue;
            }

            if (getIntOptionParameter(args, "-cl", out optionValue))
            {
                optLOCCoefficient = optionValue;
            }

            return true;
        }

        //----< handle file redirection option >-----------------
        static bool processFileRedirection(string[] args)
        {
            string optionString;

            if (getStringOptionParameter(args, "-o", out optionString))
            {
                DirectoryInfo dir;
                string fileName = Path.GetFileName(optionString);
                string dirName = Path.GetDirectoryName(optionString);
                try
                {
                    dir = new DirectoryInfo(Path.GetFullPath(dirName));
                }
                catch
                {
                    Console.WriteLine("error: path not in proper form.");
                    return false;
                }

                if (!dir.Exists)
                {
                    Console.WriteLine("error: directory does not exist.");
                    return false;
                }

                if (fileName == string.Empty)
                {
                    Console.WriteLine("error: filename in FILEPATH empty.");
                    return false;
                }

                optOutputToFile = true;
                optOutputToFileFilepath = optionString;

                return true;
            }

            return true;
        }

        //----< handle showing output for only one metric >-----------------
        static bool processSingleMetricOption(string[] args)
        {
            string optionString;
            if (getStringOptionParameter(args, "-c", out optionString))
            {
                optLOC = false;
                optComplexity = false;
                optCohesion = false;
                optCoupling = false;

                switch (optionString)
                {
                    case "loc":
                        optLOC = true;
                        break;
                    case "cmplx":
                        optComplexity = true;
                        break;
                    case "cohsn":
                        optCohesion = true;
                        break;
                    case "coupl":
                        optCoupling = true;
                        break;
                    default:
                        Console.WriteLine("error: unrecognized METRIC value.");
                        return false;
                }
            }

            return true;
        }

        //----< handle files to parse; i.e. get the list of file to parse >-----------------
        static List<string> processFilesToParse(string[] args)
        {
            List<string> files = new List<string>();
            string path = args[0];
            path = Path.GetFullPath(path);

            for (int i = 1; i < args.Length; ++i)
            {
                string filename = Path.GetFileName(args[i]);
                try
                {
                    files.AddRange(Directory.GetFiles(path, filename));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error: {0}", ex.Message);
                    return null;
                }
            }

            if (files.Count == 0)
            {
                Console.WriteLine("error: No files to parse.");
                return null;
            }

            return files;
        }

        //----< handle all command line options >-----------------
        static List<string> processCommandline(string[] args)
        {

            if (args.Contains("-h"))
            {
                showUsage();
                return null;
            }

            if (args.Length < 2)
            {
                Console.WriteLine("error: Not enough parameters specified.");
                return null;
            }

            if (!processSingleMetricOption(args))
            {
                return null;
            }

            if (!processMetricCoeffiencts(args))
            {
                return null;
            }

            if (!processFileRedirection(args))
            {
                return null;
            }
            
            if (args.Contains("-r"))
            {
                optShowRelationships = true;
            }

            return processFilesToParse(args);
        }

        //----< show help text >-----------------
        static void showUsage()
        {
            Console.WriteLine("Usage: analyzer FILEPATH FILE... <OPTIONS>");
            Console.WriteLine("Code Maintainability Analyzer computes code metrics given a set of source code.");
            Console.WriteLine();
            Console.WriteLine("Metrics:");
            Console.WriteLine("  -c  METRIC    compute only one metric where METRIC can be:");
            Console.WriteLine("                    loc     for lines of code");
            Console.WriteLine("                    cmplx   for code complexity");
            Console.WriteLine("                    cohsn   for cohesion using LCOM1");
            Console.WriteLine("                    coupl   for coupling value");
            Console.WriteLine("                    main    for maintainability index");
            Console.WriteLine();
            Console.WriteLine("Coefficients:");
            Console.WriteLine("  -cl VALUE     coefficient for LOC metric");
            Console.WriteLine("  -cx VALUE     coefficient for the complexity metric");
            Console.WriteLine("  -ch VALUE     coefficient for the cohesion metric");
            Console.WriteLine("  -cp VALUE     coefficient for the coupling metric");
            Console.WriteLine();
            Console.WriteLine("  * Default value for all coefficients is 1");
            Console.WriteLine("");
            Console.WriteLine("Miscellaneous:");
            Console.WriteLine("  -o FILEPATH   redirect console outpu to FILEPATH; ex. \"\\myLogs\\out.txt\"");
            Console.WriteLine("  -r            show relationships");
            Console.WriteLine("  -h            show help / application usage");
        }

        //----< show analyzer final summary / stats >-----------------
        static void showSummary(int numFiles)
        {
            Repository rep = Repository.getInstance();

            int totalClassCount = rep.parsedData.classList.Count;

            Console.WriteLine();
            Console.WriteLine("===============================================================================");
            Console.WriteLine(" Final Summary");
            Console.WriteLine("===============================================================================");
            Console.WriteLine();
            Console.WriteLine("  Total Number Files:                     {0}", numFiles);
            Console.WriteLine("  Total Number of Classes:                {0}", totalClassCount);
            Console.WriteLine();
            Console.WriteLine("  Total Lines of Code:                    {0}", totalLOC);
            Console.WriteLine("  Total Complexity:                       {0}", totalComplexity);
            Console.WriteLine("  Total Cohesion:                         {0}", totalCohesion);
            Console.WriteLine("  Total Coupling:                         {0}", totalCoupling);
            Console.WriteLine("  Total Maintability Index:               {0}", totalMainIndex);
            Console.WriteLine();
            Console.WriteLine("  Average Lines of Code per File:         {0}", (numFiles > 0) ? totalLOC / numFiles : 0);
            Console.WriteLine("  Average Complexity per File:            {0}", (numFiles > 0) ? totalComplexity / numFiles : 0);
            Console.WriteLine("  Average Cohesion per Class:             {0}", (totalClassCount > 0) ? totalCohesion / totalClassCount : 0);
            Console.WriteLine("  Average Coupling per Class:             {0}", (totalClassCount > 0) ? totalCoupling / totalClassCount : 0);
            Console.WriteLine("  Average Maintability Index per Class:   {0}", (totalClassCount > 0) ? totalMainIndex / totalClassCount : 0);
            Console.WriteLine();
            Console.WriteLine("  * Average values rounded down to nearest whole number.");
        }

        //----< show parsed data; helpful to show relationships >-----------------
        static void showRelationships()
        {
            if (optShowRelationships)
            {
                Repository rep = Repository.getInstance();
                Console.WriteLine();
                rep.parsedData.dumpClassList();
            }
        }

        //----< show formatted analysis output >-----------------
        static void showOutput()
        {
            Repository rep = Repository.getInstance();
            List<Elem> table = rep.locations;

            Console.WriteLine();

            Console.WriteLine(
                    "  {0,13}  {1,24}  {2,5}  {3,5}  {4,5}  {5,5}  {6,5}",
                    "category", "name", "loc", "cmplx", "cohsn", "coupl", "main"
                );
            Console.WriteLine(
                "  {0,13}  {1,24}  {2,5}  {3,5}  {4,5}  {5,5}  {6,5}",
                "--------", "----", "---", "-----", "-----", "-----", "-----"
            );

            foreach (Elem e in table)
            {
                bool isClass = false;
                int locValue = optLOC ? optLOCCoefficient * (e.endLine - e.beginLine + 1) : 0;
                int complexityValue = optComplexity ? optComplexityCoefficient * (e.endScopeCount - e.beginScopeCount + 1) : 0;
                int couplingValue = 0;
                int cohesionValue = 0;
                int mainIndex = 0;

                if (e.type == "class" || e.type == "struct")
                {
                    isClass = true;
                    CClassInfo classInfo = rep.parsedData.getClassInfo(e.name);
                    couplingValue = optCoupling ? optCouplingCoefficient * CCalculateMetrics.calculateCoupling(classInfo) : 0;
                    cohesionValue = optCohesion ? optCohesionCoefficient * CCalculateMetrics.calculateCohesion(classInfo) : 0;
                    mainIndex = locValue + complexityValue + couplingValue + cohesionValue;

                    Console.WriteLine();
                }

                Console.WriteLine(
                  "  {0,13}  {1,24}  {2,5}  {3,5}  {4,5}  {5,5}  {6,5}",
                  e.type,
                  e.name,
                  optLOC ? locValue.ToString() : string.Empty,
                  optComplexity ? complexityValue.ToString() : string.Empty,
                  optCohesion ? (isClass ? cohesionValue.ToString() : string.Empty) : string.Empty,
                  optCoupling ? (isClass ? couplingValue.ToString() : string.Empty) : string.Empty,
                  isClass ? mainIndex.ToString() : string.Empty
                );

                totalLOC += locValue;
                totalComplexity += complexityValue;
                totalCohesion += cohesionValue;
                totalCoupling += couplingValue;
                totalMainIndex += mainIndex;
            }
        }

        //----< setup and execute the parser >-----------------
        static void parse(string file)
        {
            CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
            semi.displayNewLines = false;
            if (!semi.open(file as string))
            {
                Console.WriteLine("  Can't open {0}", file);
                return;
            }

            BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
            Parser parser = builder.build(true);

            try
            {
                while (semi.getSemi())
                {
                    parser.parse(semi);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("  {0}", ex.Message);
            }

            semi.close();
        }

        //----< analyzer execution start here! >-----------------
        static int Main(string[] args)
        {
            List<string> files = processCommandline(args);
            FileStream outputFileStream = null;
            StreamWriter streamWriter = null;

            if (files == null)
            {
                return 0;
            }

            if (optOutputToFile)
            {
                try
                {
                    outputFileStream = new FileStream(optOutputToFileFilepath, FileMode.Create);
                }
                catch
                {
                    Console.WriteLine("error: can create output file {0}.", optOutputToFileFilepath);
                }

                streamWriter = new StreamWriter(outputFileStream);
                Console.SetOut(streamWriter);
            }

            Console.WriteLine();
            Console.WriteLine("===============================================================================");
            Console.WriteLine("  Code Maintainability Analyzer");
            Console.WriteLine("===============================================================================");

            Parser parser;
            BuildCodeAnalyzer builder = null;
            foreach (string file in files)
            {
                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                    Console.WriteLine("  Can't open {0}", args[0]);
                    return -1;
                }

                builder = new BuildCodeAnalyzer(semi);
                parser = builder.build(true);

                try
                {
                    while (semi.getSemi())
                    {
                        parser.parse(semi);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("  {0}", ex.Message);
                }
                semi.close();
            }



            foreach (string file in files)
            {
                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                    Console.Write("  Can't open {0}", args[0]);
                    return -1;
                }

                Console.WriteLine();
                Console.WriteLine("-------------------------------------------------------------------------------");
                Console.WriteLine("  Type and Function Analysis for {0}", System.IO.Path.GetFileName(file));
                Console.WriteLine("-------------------------------------------------------------------------------");

                builder = new BuildCodeAnalyzer(semi);
                parser = builder.build(false);

                try
                {
                    while (semi.getSemi())
                    {
                        parser.parse(semi);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("  {0}", ex.Message);
                }

                semi.close();

                showOutput();

            }

            showSummary(files.Count);

            showRelationships();


            if (optOutputToFile)
            {
                streamWriter.Close();
            }

            return 0;
        }
    }
}
