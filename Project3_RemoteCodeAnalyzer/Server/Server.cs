﻿/////////////////////////////////////////////////////////////////////
// Server.cs - Remote Code Analyzer Server                         //
// ver 1.0                                                         //
// Mudit Vats, CSE681-OnLine, Summer 2018                          //
//                                                                 //
/////////////////////////////////////////////////////////////////////
/*
 * Code adopted from PluggableRepo project by Prof Jim Fawcett.
 * 
 * Started this project with C# Console Project wizard
 * - Added references to:
 *   - System.Xml.
 *   - System.Xml.Linq
 *   
 * Package Operations:
 * -------------------
 * This package one class, Server, server function to handle creating and responding to
 * server requests.
 * 
 * createCommIfNeeded     : authenticate user give a user and password
 * initializeDispatcher   : initialize responders to messages
 * start                  : start the receiver thread
 * doReply                : filter replies; choose when to send a reply
 * threadProc             : receiving thread function
 * Main                   : main entry point
 * 
 * 
 * Maintenance History:
 * --------------------
  * ver 1.0 : 25 Aug 2018
 * - first release / Prof. Jim Fawcett
 * ver 1.1 : 25 Aug 2018
 *  - added message types for Remote Code Analyer client
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;

namespace PluggableRepository
{
    using Msg = CommMessage;

    public struct ServerEnvironment
    {
        public const string storagePath = "../../Storage/";
    }

    class Server
    {
        Comm comm_ = null;
        Thread rcvThrd = null;
        MessageDispatcher dispatcher_ = new MessageDispatcher();

        public Server()
        {
            Console.Title = "Repository Server";
            initializeDispatcher();

            // create directory structure for files
            System.IO.Directory.CreateDirectory(ServerEnvironment.storagePath);

            foreach (string s in XMLHelper.getUsersList())
            {
                System.IO.Directory.CreateDirectory("../../Storage/" + s);
            }
        }

        ~Server()
        {
            try
            {
                rcvThrd.Abort();
                comm_.close();
            }
            finally
            {
                /* nothing to do - just preventing unhandle exception */
            }
        }

        /*----< create comm if needed >--------------------------------*/
        /*
         *  You can only start one listerner on a given port for this machine.
         *  If two instances of server are started, that is attempted,
         *  resulting in a WCF exception.  In order to shutdown cleanly
         *  in this circumstance, we need to surppress Finialization.
         */
        void createCommIfNeeded()
        {
            try
            {
                if (comm_ == null)
                {
                    string serverMachine = "http://localhost";
                    int serverPort = 8080;
                    comm_ = new Comm(serverMachine, serverPort);
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n-- {0}", ex.Message);
                GC.SuppressFinalize(this);
                System.Diagnostics.Process.GetCurrentProcess().Close();
            }
        }
        /*----< here server responses to messages are defined >--------*/

        void initializeDispatcher()
        {
            // login
            Func<Msg, Msg> action1 = (Msg msg) =>
            {
                Msg replyMsg = new Msg(Msg.MessageType.reply);
                replyMsg.command = CommMessage.Command.login;
                replyMsg.author = "server";
                replyMsg.to = msg.from;
                replyMsg.from = msg.to;

                if (Authentication.authenticateUser(msg.author, XMLHelper.getSingleValueFromString(msg.stringBody, "Password")))
                {
                    replyMsg.stringBody = "<StringBody><Result>" + "ok" + "</Result></StringBody>";
                }
                else
                {
                    replyMsg.stringBody = "<StringBody><Result>" + "error" + "</Result></StringBody>";
                }
                
                return replyMsg;
            };
            dispatcher_.addCommand(Msg.Command.login, action1);

            // annouceBatch
            Func<Msg, Msg> action2 = (Msg msg) =>
            {
                //Console.WriteLine("received announce, need to save batch {0}", XMLHelper.getSingleValueFromString(msg.stringBody, "BatchName"));

                System.IO.Directory.CreateDirectory(ServerEnvironment.storagePath);
                System.IO.Directory.CreateDirectory(ServerEnvironment.storagePath + msg.author);


                Msg replyMsg = new Msg(Msg.MessageType.noReply);
                replyMsg.command = CommMessage.Command.announceBatch;
                replyMsg.author = "server";
                replyMsg.to = msg.from;
                replyMsg.from = msg.to;
                replyMsg.stringBody = "<StringBody><Result>" + "ok" + "</Result></StringBody>";
                return replyMsg;
            };
            dispatcher_.addCommand(Msg.Command.announceBatch, action2);

            // annouceFile
            Func<Msg, Msg> action3 = (Msg msg) =>
            {
                XMLHelper.filesAddNode(
                    XMLHelper.getSingleValueFromString(msg.stringBody, "Filename"), 
                    XMLHelper.getSingleValueFromString(msg.stringBody, "Batch"),
                    XMLHelper.getSingleValueFromString(msg.stringBody, "Owner")
                    );

                Msg replyMsg = new Msg(Msg.MessageType.noReply);
                replyMsg.command = CommMessage.Command.announceFile;
                replyMsg.author = "server";
                replyMsg.to = msg.from;
                replyMsg.from = msg.to;
                replyMsg.stringBody = "<StringBody><Result>" + "ok" + "</Result></StringBody>";
                return replyMsg;
            };
            dispatcher_.addCommand(Msg.Command.announceFile, action3);

            // getBatchList
            Func<Msg, Msg> action4 = (Msg msg) =>
            {
                Msg replyMsg = new Msg(Msg.MessageType.reply);
                replyMsg.command = CommMessage.Command.getBatchList;
                replyMsg.author = "server";
                replyMsg.to = msg.from;
                replyMsg.from = msg.to;
                replyMsg.stringBody = XMLHelper.getBatchesXMLList(msg.author);
                return replyMsg;
            };
            dispatcher_.addCommand(Msg.Command.getBatchList, action4);

            // getBatchList
            Func<Msg, Msg> action5 = (Msg msg) =>
            {
                Msg replyMsg = new Msg(Msg.MessageType.reply);
                replyMsg.command = CommMessage.Command.getBatchFileList;
                replyMsg.author = "server";
                replyMsg.to = msg.from;
                replyMsg.from = msg.to;
                replyMsg.stringBody = XMLHelper.getBatchesFileListXMLList(
                    XMLHelper.getSingleValueFromString(msg.stringBody, "Batch"),
                    XMLHelper.getSingleValueFromString(msg.stringBody, "Owner")
                    );
                return replyMsg;
            };
            dispatcher_.addCommand(Msg.Command.getBatchFileList, action5);

            // download files
            Func<Msg, Msg> action6 = (Msg msg) =>
            {
                string result = msg.stringBody;

                XElement xmlTree = XElement.Parse(result);
                var element = (from e in xmlTree.Descendants("File")
                               select e);

                foreach (var s in element)
                {
                    string fileName = s.Element("Name").Value;
                    string batchName = s.Element("Batch").Value;
                    string realFileOwner = XMLHelper.getFileOwner(fileName, batchName);
                    comm_.postFile(fileName, realFileOwner, false);
                }


                Msg replyMsg = new Msg(Msg.MessageType.noReply);
                replyMsg.command = CommMessage.Command.downloadFiles;
                replyMsg.author = "server";
                replyMsg.to = msg.from;
                replyMsg.from = msg.to;
                replyMsg.stringBody = "";
                return replyMsg;
            };
            dispatcher_.addCommand(Msg.Command.downloadFiles, action6);

            // getUsersList
            Func<Msg, Msg> action7 = (Msg msg) =>
            {
                Msg replyMsg = new Msg(Msg.MessageType.reply);
                replyMsg.command = CommMessage.Command.getUsersList;
                replyMsg.author = "server";
                replyMsg.to = msg.from;
                replyMsg.from = msg.to;
                replyMsg.stringBody = XMLHelper.getUsersXMLList();
                return replyMsg;
            };
            dispatcher_.addCommand(Msg.Command.getUsersList, action7);

            // grantFileAccess
            Func<Msg, Msg> action8 = (Msg msg) =>
            {
                string result = msg.stringBody;

                XElement xmlTree = XElement.Parse(result);
                var element = (from e in xmlTree.Descendants("File")
                               select e);

                foreach (var e in element)
                {
                    XMLHelper.grantFileAccess(e.Element("Name").Value, msg.author, e.Element("GrantAccess").Value);
                }

                Msg replyMsg = new Msg(Msg.MessageType.reply);
                replyMsg.command = CommMessage.Command.greatFileAccess;
                replyMsg.author = "server";
                replyMsg.to = msg.from;
                replyMsg.from = msg.to;
                replyMsg.stringBody = XMLHelper.getUsersXMLList();
                return replyMsg;
            };
            dispatcher_.addCommand(Msg.Command.greatFileAccess, action8);
        }

        bool start()
        {
            try
            {
                createCommIfNeeded();
                rcvThrd = new Thread(threadProc);
                rcvThrd.IsBackground = true;
                rcvThrd.Start();
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n  -- {0}", ex.Message);
                return false;
            }
        }
        /*----< filters messages to which server replies >-------------*/

        bool doReply(Msg msg, Msg reply)
        {
            if (msg.type == Msg.MessageType.noReply)
                return false;
            if (msg.type == Msg.MessageType.connect)
                return false;
            if (reply.type == Msg.MessageType.procError)
                return false;
            return true;
        }
        /*----< receive thread processing >----------------------------*/

        void threadProc()
        {
            while (true)
            {
                try
                {
                    CommMessage msg = comm_.getMessage();
                    Console.Write("\n  Received {0} message : {1}", msg.type.ToString(), msg.command.ToString());
                    CommMessage reply = dispatcher_.doCommand(msg.command, msg);
                    if (reply.command == Msg.Command.show)
                    {
                        reply.show(reply.arguments.Count < 7);
                        Console.Write("  -- no reply sent");
                    }
                    if (doReply(msg, reply))
                        comm_.postMessage(reply);
                }
                catch
                {
                    break;
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("Starting Remote Code Analyzer Server");
            Console.WriteLine();

            Server server = new Server();
            if (!server.start())
            {
                return;
            }
            
            Console.WriteLine("Press any key...");
            Console.ReadKey();

            return;
        }
    }
}
