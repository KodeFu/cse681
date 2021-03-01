Code Maintainabilty Analyzer
Mudit Vats, mpvats@syr.edu
8/11/2018

===============================================================================
DESCRIPTION
===============================================================================
The Code Maintainability Analyzer (CMA) application is an application which can 
be used to understand the health of a set of source code. It does this by 
calculating metrics on the source code and sharing the results with the 
end-user. The results can be one of several metrics and a final overall index
that can be used to quickly gauge the quality (i.e. maintainability) of the 
software.

===============================================================================
TEST FILES
===============================================================================
The files in this project were used to test the analyzer executable for
file parsing stability, data collection and all metrics.

 Directory of .\Analyzer
 
08/11/2018  03:43 PM            18,892 Analyzer.cs
08/11/2018  11:07 AM             1,685 CalculateMetrics.cs
08/10/2018  08:28 PM             4,673 Display.cs
08/10/2018  08:48 PM             4,223 IRuleandAction.cs
08/10/2018  08:56 PM             5,588 ParsedData.cs
08/10/2018  08:59 PM             9,134 Parser.cs
08/11/2018  06:41 AM            30,668 RulesAndActions.cs
07/24/2018  05:49 PM             5,973 ScopeStack.cs
07/31/2018  06:25 PM            16,874 Semi.cs
07/15/2018  11:04 AM             5,494 TestCase-eligibleCheck.cs
08/10/2018  08:56 PM            12,381 TokenHelper.cs
07/24/2018  05:49 PM            24,842 Toker.cs

The files below were used to test cohesion and coupling.

 Directory of .\Analyzer\TestFiles\TestFiles

08/12/2018  09:21 AM               752 TestClass1.cs
08/12/2018  09:31 AM             1,024 TestClass2.cs
08/12/2018  09:36 AM             1,078 TestClass3.cs

===============================================================================
KNOWN ISSUES
===============================================================================
Parser:
- Getter / setter in same line unhandled by parser, which can causing missing 
  properties.
- Anonymous type may cause scope stack to pop class info which can cause 
  incorrect detection.
- Types within generics are ignored.

Future Enhancements:
- Support classes within classes.
- Support namespaces.

===============================================================================
USAGE
===============================================================================
Usage: analyzer FILEPATH FILE... <OPTIONS>
Code Maintainability Analyzer computes code metrics given a set of source code.

Metrics:
  -c  METRIC    compute only one metric where METRIC can be:
                    loc     for lines of code
                    cmplx   for code complexity
                    cohsn   for cohesion using LCOM1
                    coupl   for coupling value
                    main    for maintainability index

Coefficients:
  -cl VALUE     coefficient for LOC metric
  -cx VALUE     coefficient for the complexity metric
  -ch VALUE     coefficient for the cohesion metric
  -cp VALUE     coefficient for the coupling metric

  * Default value for all coefficients is 1

Miscellaneous:
  -o FILEPATH   redirect console outpu to FILEPATH; ex. "\myLogs\out.txt"
  -r            show relationships
  -h            show help / application usage

===============================================================================
EXECUTION
===============================================================================

Analyze all files in \my\test\directory
  analyzer \my\test\directory *.cs

Analyze and compute only LOC metric
  analyzer \my\test\directory *.cs -c loc

Analyze and redirect output to file
  analyzer \my\test\directory *.cs -o \myLogs\outputfile.txt

Analyze and specifify coefficients
  analyzer \my\test\directory *.cs -cl 1 -cx 8 -ch 2 -cp 10
