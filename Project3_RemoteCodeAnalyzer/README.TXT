Remote Code Analyzer
Mudit Vats, mpvats@syr.edu
8/26/2018

===============================================================================
DESCRIPTION
===============================================================================
The Remote Code Analyzer (RCA) application is an application which can 
be used to understand to remotely access code analyzer data for uploaded
sets of source code files.

===============================================================================
TEST FILES
===============================================================================
The *.cs files in this project and the previous Code Analyzer project
were used to test this application.

 Directory of .\Analyzer

08/12/2018  09:33 AM            21,815 Analyzer.cs
08/12/2018  09:30 AM             6,855 CalculateMetrics.cs
08/10/2018  08:28 PM             4,673 Display.cs
08/10/2018  08:48 PM             4,223 IRuleandAction.cs
08/12/2018  06:51 AM            16,252 ParsedData.cs
08/10/2018  08:59 PM             9,134 Parser.cs
08/11/2018  08:49 PM            31,175 RulesAndActions.cs
07/24/2018  05:49 PM             5,973 ScopeStack.cs
07/31/2018  06:25 PM            16,874 Semi.cs
07/15/2018  11:04 AM             5,494 TestCase-eligibleCheck.cs
08/11/2018  04:49 PM            15,995 TokenHelper.cs
08/11/2018  08:13 PM            24,866 Toker.cs
              12 File(s)        163,329 bytes

The recursive directories of *.cs files were used to test
upload, download and access grants.

 Directory of .\RemoteCodeAnalyzer\*.cs

===============================================================================
KNOWN ISSUES
===============================================================================
Server/Client
- Server requires restarting after client connects to it. The server save
  connection state for client connection. Currently, disconnections do 
  not reset that state. After a user is done with the client, both client and
  server should be closed. Basically, restart both client and server each
  time a new login session is required.

Future Enhancements:
- Support files with the same name. This is a Project #4 requirement so
  handling this scenario will be addressed in the next project.
- Support client reconnections without server restarts.

===============================================================================
BUILD
===============================================================================
Run the compile.bat to compile.

===============================================================================
EXECUTION
===============================================================================
Open two Administrator command promts for client and server.
- Run the server using run_server.bat.
- Run the client using run_client.bat.

Client downloaded files are located in:
- RemoteCodeAnalyzer\Client\Storage

Server files and metadata are located in:
- RemoteCodeAnalyzer\Server\Storage
