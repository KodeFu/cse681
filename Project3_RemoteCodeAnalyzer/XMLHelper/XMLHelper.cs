/////////////////////////////////////////////////////////////////////
// XMLHelper.cs - XML functions to access file and user xml files  //
// ver 1.0                                                         //
// Mudit Vats, CSE681-OnLine, Summer 2018                          //
/////////////////////////////////////////////////////////////////////
/*
 * Started this project with C# Console Project wizard
 * - Added references to:
 *   - System.Xml.
 *   - System.Xml.Linq
 *   
 * Package Operations:
 * -------------------
 * This package one class, XMLHelper, with static member functions:
 * 
 * getSingleValueFromString     : get value from xml value from an XML string; i.e. key/value
 * getSingleValueFromFile       : get value from xml value from an XML file; i.e. key/value
 * updateSingleValueFromFile    : update value in xml file; i.e. key/value
 * getElementFromFile           : get element from xml file
 * updateElementFromFile        : update element from xml file
 * dumpXMLFile                  : dump/output the xml file
 * filesAddNode                 : add files element to files.xml
 * batchesAddNode               : add batch node to batches.xml
 * getBatchesList               : get batch list from batches.xml
 * getBatchesFileList           : get file list associated with a batch
 * getUsersList                 : get users list from users.xml
 * getBatchesXMLList            : get XML'ized list of batches
 * getBatchesFileListXMLList    : get XML'ized list of files
 * getUsersXMLList              : get users XML'ized list
 * grantFileAccess              : update file list xml to grant file access
 * getUserRole                  : get user (Developer/Admin) for a user
 * 
 * 
 
 * Maintenance History:
 * --------------------
  * ver 1.0 : 25 Aug 2018
 * - first release
 * 
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PluggableRepository
{
    public class XMLHelper
    {
        static string fileStorage = @"..\..\Storage\";

        // From XML string which includes only unique keys, get the value
        static public string getSingleValueFromString(string xmlString, string subElement)
        {
            XElement xmlTree = XElement.Parse(xmlString);

            var element = (from e in xmlTree.Descendants(subElement)
                           select e).SingleOrDefault();

            return element.Value;
        }

        // From XML file which includes only unique keys, get the value
        static public string getSingleValueFromFile(string xmlFile, string subElement)
        {
            XElement xmlTree = XElement.Load(xmlFile);

            var element = (from e in xmlTree.Descendants(subElement)
                           select e).SingleOrDefault();

            return element.Value;
        }

        // From XML file which includes only unique keys, update the value
        static public bool updateSingleValueFromFile(string xmlFile, string subElement, string value)
        {
            XElement xmlTree = XElement.Load(xmlFile);

            var element = (from e in xmlTree.Descendants(subElement)
                           select e).SingleOrDefault();

            if (element != null)
            {
                element.Value = value;
                xmlTree.Save(xmlFile);

                return true;
            }

            return false;
        }

        // From XML file which includes one or many elements (with sub-elements), get the element
        static public XElement getElementFromFile(string xmlFile, string subElement, string subSubElement, string index)
        {
            XElement xmlTree = XElement.Load(xmlFile);

            var element = (from e in xmlTree.Descendants(subElement)
                           where e.Element(subSubElement).Value == index
                           select e).SingleOrDefault();

            return element;
        }

        // From XML file which includes one or many elements (with sub-elements), update the element
        static public bool updateElementFromFile(string xmlFile, string subElement, string subSubElement, string index, XElement updatedElement)
        {
            XElement xmlTree = XElement.Load(xmlFile);

            var element = (from e in xmlTree.Descendants(subElement)
                           where e.Element(subSubElement).Value == index
                           select e).SingleOrDefault();

            if (element != null)
            {
                element.ReplaceWith(updatedElement);
                xmlTree.Save(xmlFile);

                return true;
            }

            return false;
        }

        static public void dumpXMLFile(string xmlFile)
        {
            XElement xmlTree = XElement.Load(xmlFile);

            Console.WriteLine(xmlTree);
        }


        static public void filesAddNode(string filename, string batchname, string owner)
        {
            string filesXML = fileStorage + "Files.xml";

            if (!File.Exists(filesXML))
            {

                var files =
                    new XElement("Files",
                      new XElement("File",
                      new XElement("Filename", filename),
                      new XElement("Batch", batchname),
                      new XElement("Owner", owner)
                      )
                    );

                files.Save(filesXML);
            }
            else
            {
                XElement xmlTree = XElement.Load(filesXML);

                var files =
                        new XElement("File",
                        new XElement("Filename", filename),
                        new XElement("Batch", batchname),
                        new XElement("Owner", owner)
                    );

                xmlTree.Add(files);
                xmlTree.Save(filesXML);
            }
        }

        static public void batchesAddNode(string batchname, string owner)
        {
            string batchesXML = fileStorage + "Batches.xml";

            if (!File.Exists(batchesXML))
            {
                var batch =
                   new XElement("Batches",
                      new XElement("Batch",
                      new XElement("Batchname", batchname),
                      new XElement("Owner", owner)
                      )
                );
                batch.Save(batchesXML);
            }
            else
            {
                XElement xmlTree = XElement.Load(batchesXML);

                var batch =
                      new XElement("Batch",
                      new XElement("Batchname", batchname),
                      new XElement("Owner", owner)
                );

                xmlTree.Add(batch);
                xmlTree.Save(batchesXML);
            }
        }

        static public List<string> getBatchesList(string owner)
        {
            List<string> batchesList = new List<string>();

            string filesXML = fileStorage + "Files.xml";

            XElement xmlTree = XElement.Load(filesXML);

            if (getUserRole(owner)=="Administrator")
            {
                // get all batches for Admin
                var element = (from e in xmlTree.Descendants("File")
                               select e.Element("Batch").Value);


                foreach (string batch in element)
                {
                    if (!batchesList.Contains(batch))
                    {
                        batchesList.Add(batch);
                    }
                }
            }
            else
            {
                var element = (from e in xmlTree.Descendants("File")
                               where e.Element("Owner").Value == owner
                               select e.Element("Batch").Value);


                foreach (string batch in element)
                {
                    if (!batchesList.Contains(batch))
                    {
                        batchesList.Add(batch);
                    }
                }
            }

            return batchesList;
        }

        static public List<string> getBatchesFileList(string batchname, string owner)
        {
            List<string> filesList = new List<string>();

            string filesXML = fileStorage + "Files.xml";

            XElement xmlTree = XElement.Load(filesXML);

            if (getUserRole(owner)=="Administrator")
            {
                var element = (from e in xmlTree.Descendants("File")
                               where e.Element("Batch").Value == batchname
                               select e.Element("Filename").Value);

                foreach (string file in element)
                {
                    if (!filesList.Contains(file))
                    {
                        filesList.Add(file);
                    }
                }
            }
            else
            {
                var element = (from e in xmlTree.Descendants("File")
                               where e.Element("Batch").Value == batchname
                               where e.Element("Owner").Value == owner
                               select e.Element("Filename").Value);

                foreach (string file in element)
                {
                    if (!filesList.Contains(file))
                    {
                        filesList.Add(file);
                    }
                }

            }

            return filesList;
        }

        static public List<string> getUsersList()
        {
            List<string> filesList = new List<string>();

            string filesXML = fileStorage + "Users.xml";

            XElement xmlTree = XElement.Load(filesXML);

            var element = (from e in xmlTree.Descendants("User")
                            select e.Element("Username").Value);

            foreach (string file in element)
            {
                if (!filesList.Contains(file))
                {
                    filesList.Add(file);
                }
            }
            
            return filesList;
        }

        static public string getBatchesXMLList(string owner)
        {
            string finalString = "";
            List<string> batchesList = getBatchesList(owner);

            finalString = "<StringBody>";
            foreach (string batch in batchesList)
            {
                finalString += "<Batch><Name>" + batch + "</Name></Batch>";
            }
            finalString += "</StringBody>";

            return finalString;
        }

        static public string getBatchesFileListXMLList(string batchname, string owner)
        {
            string finalString = "";
            List<string> batchesList = getBatchesFileList(batchname, owner);

            finalString = "<StringBody>";
            foreach (string batch in batchesList)
            {
                finalString += "<File><Name>" + batch + "</Name></File>";
            }
            finalString += "</StringBody>";

            return finalString;
        }

        static public string getUsersXMLList()
        {
            string finalString = "";
            List<string> usersList = getUsersList();

            finalString = "<StringBody>";
            foreach (string user in usersList)
            {
                finalString += "<User><Name>" + user + "</Name></User>";
            }
            finalString += "</StringBody>";

            return finalString;
        }
        // Create BATCHES.XML

        static public void grantFileAccess(string filename, string owner, string grantUsername)
        {
            string filesXML = fileStorage + "Files.xml";

            XElement xmlTree = XElement.Load(filesXML);

            var files =
                       new XElement("File",
                       new XElement("Filename", filename),
                       new XElement("Batch", "Granted Access Batch " + grantUsername),
                       new XElement("Owner", grantUsername)
                   );

            xmlTree.Add(files);
            xmlTree.Save(filesXML);

        }

        static public string getUserRole(string user)
        {
            List<string> filesList = new List<string>();

            string filesXML = fileStorage + "Users.xml";

            XElement xmlTree = XElement.Load(filesXML);

            var element = (from e in xmlTree.Descendants("User")
                           where e.Element("Username").Value == user
                           select e.Element("Role").Value);

            string role = "";
            foreach (string s in element)
            {
                role = s;
            }

            return role;
        }

        static public string getFileOwner(string filename, string batch)
        {
            string filesXML = fileStorage + "Files.xml";

            XElement xmlTree = XElement.Load(filesXML);

            var element = (from e in xmlTree.Descendants("File")
                           where e.Element("Filename").Value == filename
                           select e);

            string owner = "";
            foreach (var e in element)
            {
                // get the real file owner; i.e. not the one granted access because
                // that one just points to the filename. We still need to chase down
                // the location of the original file.
                if (!e.Element("Batch").Value.Contains("Granted Access Batch"))
                {
                    owner = e.Element("Owner").Value;
                    break;
                }
            }

            return owner;
        }

#if (TEST_XMLHELPER)
        static void Main(string[] args)
        {
            /*
            string xmlString = "<StringBody><Password>ThePassword</Password><Username>Mudit Vats</Username></StringBody>";
            Console.WriteLine(XMLHelper.getSingleValueFromString(xmlString,"Password"));
            Console.WriteLine(XMLHelper.getSingleValueFromString(xmlString, "Username"));

            xmlString = @"..\..\PasswordTest.xml";
            Console.WriteLine(XMLHelper.getSingleValueFromFile(xmlString, "Password"));
            Console.WriteLine(XMLHelper.getSingleValueFromFile(xmlString, "Username"));

            Console.WriteLine("Changing password, to someNewPassword");
            XMLHelper.updateSingleValueFromFile(xmlString, "Password", "someNewPassword");
            XMLHelper.dumpXMLFile(xmlString);

            xmlString = fileStorage + "Users.xml";
            XElement e = XMLHelper.getElementFromFile(xmlString, "User", "Id", "0");
            Console.WriteLine(e.Element("Username").Value);

            xmlString = fileStorage + "Users.xml";
            e = XMLHelper.getElementFromFile(xmlString, "User", "Id", "1");
            e.Element("Password").Value = "user";
            XMLHelper.updateElementFromFile(xmlString, "User", "Id", "1", e);

            Console.WriteLine("Dumping XML...");
            XMLHelper.dumpXMLFile(xmlString);

            xmlString = fileStorage + "Files.xml";
            Console.WriteLine("Dumping XML...");
            XMLHelper.dumpXMLFile(xmlString);

            Console.WriteLine("xxxxxxx");
            e = XMLHelper.getElementFromFile(xmlString, "File", "Id", "0");
            Console.WriteLine(e.ToString());

            e = XMLHelper.getElementFromFile(xmlString, "File", "Id", "1");
            Console.WriteLine(e.ToString());
            */
            //createFilesXML();
            filesAddNode("file1.cs", "a", "Admin");
            filesAddNode("file2.cs", "b", "Mudit");
            filesAddNode("file3.cs", "c", "Admin");
            filesAddNode("file4.cs", "b", "Mudit");
            filesAddNode("file5.cs", "a", "Mudit");

            batchesAddNode("a", "Rick");
            batchesAddNode("b", "Sally");
            batchesAddNode("c", "Jeff");

            getBatchesList("Admin");

            getBatchesXMLList("Admin");

            getBatchesFileListXMLList("a", "Admin");

            getUsersXMLList();

            Console.WriteLine(getUserRole("Admin"));
            Console.WriteLine(getUserRole("Mudit"));
            Console.WriteLine(getUserRole("Professor"));
            Console.WriteLine(getUserRole("TestUser"));

            Console.ReadKey();
        }
#endif

    }
}
