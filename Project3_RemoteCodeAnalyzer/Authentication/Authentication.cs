/////////////////////////////////////////////////////////////////////
// Authentication.cs - User authentication api                          //
// ver 1.0                                                         //
// Mudit Vats, CSE681-OnLine, Summer 2018                          //
/////////////////////////////////////////////////////////////////////
/*
 * Started this project with C# Console Project wizard
 *   
 * Package Operations:
 * -------------------
 * This package one class, Authentication, with static member functions:
 * 
 * authenticateUser     : authenticate user give a user and password
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PluggableRepository
{
    public class Authentication
    {
        static public bool authenticateUser(string username, string password)
        {
            string xmlString = @"..\..\..\Server\Storage\Users.xml";
            XElement e = XMLHelper.getElementFromFile(xmlString, "User", "Username", username);

            string fileUsername = e.Element("Username").Value;
            string filePassword = e.Element("Password").Value;

            if ((fileUsername == username) && (filePassword == password))
            {
                return true;
            }

            return false;
        }

#if (TEST_AUTHENTICATION)
        static void Main(string[] args)
        {
            if (authenticateUser("Admin", "password"))
            {
                Console.WriteLine("Authenticated user, yay!");
            }
            else
            {
                Console.WriteLine("Could not authenticate user, boo...");
            }
        }
#endif
    }
}
