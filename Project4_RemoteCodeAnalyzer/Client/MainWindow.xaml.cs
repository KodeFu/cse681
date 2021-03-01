/////////////////////////////////////////////////////////////////////
// Client.cs - Remote Code Analyzer Client                         //
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
 * This package one class, Client, server functions to manage
 * user interaction and GUI elements.
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using PluggableRepository;
using System.IO;
using System.Xml.Linq;

namespace Client
{
    // aliases with semantic meaning
    using Msg = CommMessage;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Comm comm_ = null;
        Thread rcvThrd = null;
        MessageDispatcher dispatcher_ = new MessageDispatcher();
        Analytics analyticsDlg = null;

        public MainWindow()
        {
            InitializeComponent();

            // SENDER
            comm_ = new Comm("http://" + LocalMachine.Text, Int32.Parse(LocalPort.Text));

            initializeDispatcher();

            startThread();

            System.IO.Directory.CreateDirectory("../../Storage");

        }

        ~MainWindow()
        {
            try{
                comm_.close();
            }
            finally
            {
                /* nothing to do - just preventing unhandle exception */
            }
        }

        public void Main_Closed(object sender, System.EventArgs e)
        {

        }

        public void Main_OnClose(object sender, System.EventArgs e)
        {
            // Sure kill on the receiver thread; sending a message to gracefully shutdown
            // doesn't work even though we block on getMessage().
            if (rcvThrd.IsAlive)
            {
                rcvThrd.Abort();
            }
        }

        string getLocalHostString()
        {
            return "http://" + LocalMachine.Text + ":" + LocalPort.Text + "/IPluggableComm";
        }

        string getRemoteHostString()
        {
            return "http://" + RemoteMachine.Text + ":" + RemotePort.Text + "/IPluggableComm";
        }

        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            Msg sndMsg = new Msg(CommMessage.MessageType.request);
            sndMsg.command = CommMessage.Command.login;
            sndMsg.author = loginUsername.Text;
            sndMsg.to = getRemoteHostString();
            sndMsg.from = getLocalHostString();
            sndMsg.stringBody = "<StringBody><Password>" + loginPassword.Password + "</Password></StringBody>";

            // send message
            comm_.postMessage(sndMsg);

            return;
        }

        /*----< bind client processing to message types >--------------*/
        /*
         *  This is where we determine how incoming messages are
         *  processed by client.
         */
        void initializeDispatcher()
        {
            // login
            Func<Msg, Msg> action1 = (Msg msg) =>
            {
                string result = XMLHelper.getSingleValueFromString(msg.stringBody, "Result");

                if (result.Equals("ok"))
                {
                    loginStatus.Content = "Login Successful";
                    tabUpload.IsEnabled = true;
                    tabDownload.IsEnabled = true;
                    tabMetrics.IsEnabled = true;
                    tabAccess.IsEnabled = true;
                    tabSearch.IsEnabled = true;
                    tabComment.IsEnabled = true;
                    buttonLogin.IsEnabled = false;
                    loginUsername.IsEnabled = false;
                    loginPassword.IsEnabled = false;
                    LocalMachine.IsEnabled = false;
                    LocalPort.IsEnabled = false;
                    RemoteMachine.IsEnabled = false;
                    RemotePort.IsEnabled = false;
                    commStatus.Text = loginUsername.Text + " is Online";
                    statusLabel.Text = "Status: Ok";
                }
                else
                {
                    loginStatus.Content = "Login Failed";
                }

                Msg reply = new Msg(Msg.MessageType.noReply);
                return reply;
            };
            dispatcher_.addCommand(Msg.Command.login, action1);

            // getBatchList
            Func<Msg, Msg> action2 = (Msg msg) =>
            {
                string result = msg.stringBody;

                XElement xmlTree = XElement.Parse(result);
                var element = (from e in xmlTree.Descendants("Batch")
                               select e.Element("Name").Value);

                ListBox selectedListBox = listboxDownloadBatchesList;
                if (mainTab.SelectedItem == tabMetrics)
                {
                    selectedListBox = listboxMetricsBatchesList;
                }
                else if (mainTab.SelectedItem == tabAccess)
                {
                    selectedListBox = listboxAccessBatchesList;
                }
                else if (mainTab.SelectedItem == tabComment)
                {
                    selectedListBox = listboxCommentBatchesList;
                }

                selectedListBox.Items.Clear();
                foreach (string s in element)
                {
                    selectedListBox.Items.Add(s);
                }

                if (selectedListBox.Items.Count > 1)
                {
                    selectedListBox.SelectedIndex = 0;
                }

                Msg reply = new Msg(Msg.MessageType.noReply);
                return reply;
            };
            dispatcher_.addCommand(Msg.Command.getBatchList, action2);

            // getBatchFileList
            Func<Msg, Msg> action3 = (Msg msg) =>
            {
                string result = msg.stringBody;

                XElement xmlTree = XElement.Parse(result);
                var element = (from e in xmlTree.Descendants("File")
                               select e.Element("Name").Value);

                ListBox selectedListBox = listboxDownloadFilesList;
                if (mainTab.SelectedItem == tabMetrics)
                {
                    selectedListBox = listboxMetricsFilesList;
                }
                else if (mainTab.SelectedItem == tabAccess)
                {
                    selectedListBox = listboxAccessFilesList;
                }
                else if (mainTab.SelectedItem == tabComment)
                {
                    selectedListBox = listboxCommentFilesList;
                }

                selectedListBox.Items.Clear();
                foreach (string s in element)
                {
                    selectedListBox.Items.Add(s);
                }

                if (selectedListBox.Items.Count > 1)
                {
                    selectedListBox.SelectedIndex = 0;
                }

                Msg reply = new Msg(Msg.MessageType.noReply);
                return reply;
            };
            dispatcher_.addCommand(Msg.Command.getBatchFileList, action3);

            // getUsersList
            Func<Msg, Msg> action4 = (Msg msg) =>
            {
                string result = msg.stringBody;

                XElement xmlTree = XElement.Parse(result);
                var element = (from e in xmlTree.Descendants("User")
                               select e.Element("Name").Value);

                ListBox selectedListBox = listboxAccessUsersList;
                
                selectedListBox.Items.Clear();
                foreach (string s in element)
                {
                    if (!s.Equals(loginUsername.Text))
                    {
                        selectedListBox.Items.Add(s);
                    }
                }

                if (selectedListBox.Items.Count > 1)
                {
                    selectedListBox.SelectedIndex = 0;
                }

                Msg reply = new Msg(Msg.MessageType.noReply);
                return reply;
            };
            dispatcher_.addCommand(Msg.Command.getUsersList, action4);

            // getAnalytics
            Func<Msg, Msg> action5 = (Msg msg) =>
            {
                string result = msg.stringBody;
                XElement xmlTree = XElement.Parse(result);

                analyticsDlg.AddAnalytics(xmlTree.ToString());

                Msg reply = new Msg(Msg.MessageType.noReply);
                return reply;
            };
            dispatcher_.addCommand(Msg.Command.getAnalytics, action5);
            
            // searchBatches
            Func<Msg, Msg> action6 = (Msg msg) =>
            {
                string result = msg.stringBody;
                XElement xmlTree = XElement.Parse(result);

                var element = (from e in xmlTree.Descendants("File")
                               select e.Element("Name").Value);

                foreach (string s in element)
                {
                    listboxSearchFilesList.Items.Add(s);
                }

                Msg reply = new Msg(Msg.MessageType.noReply);
                return reply;
            };
            dispatcher_.addCommand(Msg.Command.searchBatch, action6);

            // searchFiles
            Func<Msg, Msg> action7 = (Msg msg) =>
            {
                string result = msg.stringBody;
                XElement xmlTree = XElement.Parse(result);

                var element = (from e in xmlTree.Descendants("File")
                               select e.Element("Name").Value);

                foreach (string s in element)
                {
                    listboxSearchFilesList.Items.Add(s);
                }

                Msg reply = new Msg(Msg.MessageType.noReply);
                return reply;
            };
            dispatcher_.addCommand(Msg.Command.searchFiles, action7);

            // getComments
            Func<Msg, Msg> action8 = (Msg msg) =>
            {
                string result = msg.stringBody;

                if (result.Length > 0)
                {

                    XElement xmlTree = XElement.Parse(result);

                    var element = (from e in xmlTree.Descendants("File")
                                   select e);

                    listboxCommentMyComments.Items.Clear();
                    foreach (var e in element)
                    {
                        string finalString = "";
                        if (!e.Element("Fileame").Value.Equals(""))
                        {
                            finalString = e.Element("Batch").Value + " " + e.Element("Fileame").Value + ": " + e.Element("Comment").Value;
                        }
                        else
                        {
                            finalString = e.Element("Batch").Value + ": " + e.Element("Comment").Value;
                        }
                        listboxCommentMyComments.Items.Add(finalString);
                    }

                }

                Msg reply = new Msg(Msg.MessageType.noReply);
                return reply;
            };
            dispatcher_.addCommand(Msg.Command.getComments, action8);
        }

        /*----< create comm if needed >--------------------------------*/
        /*
         *  - communication may start in several different ways
         *  - we do a lazy initialization of comm_, so this code
         *    will be invoked when needed in a few different code
         *    locations
         */
        void createCommIfNeeded()
        {
            if (comm_ == null)
            {
                string localMachine = "http://" + LocalMachine.Text;
                int localPort = Int32.Parse(LocalPort.Text);
                comm_ = new Comm(localMachine, localPort);
            }
        }

        /*----< filter messages to process >---------------------------*/
        /*
         *  currently doesn't filter anything
         */
        bool doProcess(Msg msg)
        {
            if (msg.type == CommMessage.MessageType.connect)
              return false;

            return true;  // currently doesn't filter anything
        }

        /*----< make msg list display string >-------------------------*/

        string makeMsgDisplayStr(Msg msg)
        {
            string display = msg.type.ToString() + ":" + msg.command + " -- " + msg.to + " " + DateTime.Now.ToString();
            return display;
        }

        /*----< display incoming message >-----------------------------*/

        public void displayInComingMsg(Msg msg)
        {
            Console.WriteLine("I am here!");
            /*
            inMsgListBox.Items.Insert(0, makeMsgDisplayStr(msg));
            if (inMsgListBox.Items.Count > maxInMsgCount)
                inMsgListBox.Items.RemoveAt(maxInMsgCount);
                */
        }

        /*----< start receiver thread >--------------------------------*/

        private void startThread()
        {
            createCommIfNeeded();
            string machine = string.Copy(LocalMachine.Text);  // child thread can't access GUI
            string port = string.Copy(LocalPort.Text);        // ditto
            rcvThrd = new Thread(
              () => rcvProc(machine, port)
            );
            rcvThrd.Start();
        }

        void rcvProc(string localMachine, string localPort)
        {
            createCommIfNeeded();

            while (true)
            {
                Msg msg = comm_.getMessage();
                msg.show(msg.arguments.Count < 7);
                if (doProcess(msg))
                {
                    Action toMainThrd = () =>
                    {
                        displayInComingMsg(msg);
                        Msg result = dispatcher_.doCommand(msg.command, msg);  // our Comm dispatcher
                        result.show();
                    };
                    Dispatcher.BeginInvoke(toMainThrd);  // WPF's dispatcher lets child thread use window

                }
            }
        }

        /*----< set commState based on button states >-----------------*/

       
        private void ButtonUploadSelectDirectory_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();

            dlg.ShowDialog();

            try
            {
                string[] files = Directory.GetFiles(dlg.SelectedPath, "*.cs", SearchOption.AllDirectories);

                if (files.Length > 0)
                {
                    foreach (string file in files)
                    {
                        listBoxUploadQueue.Items.Add(file);
                    }
                }
            }
            catch
            {

            }
        }

        private void ButtonUploadSelectFiles_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();

            dlg.Filter = "Source files (*.cs)|*.cs";
            dlg.Multiselect = true;
            dlg.ShowDialog();

            if (dlg.FileNames.Length>0)
            {
                foreach (string file in dlg.FileNames)
                {
                    listBoxUploadQueue.Items.Add(file);
                }
            }
        }

        string getNewFileName(string fileName, List<string> fileList)
        {
            string fileNameNoExt = Path.GetFileNameWithoutExtension(fileName);

            int fileIndex = 0;
            string testFileName = fileNameNoExt + "_" + fileIndex.ToString() + ".cs";
            fileIndex++;

            while (fileList.Contains(testFileName))
            {
                testFileName = fileNameNoExt + "_" + fileIndex.ToString() + ".cs";
                fileIndex++;
            }

            Console.WriteLine(testFileName);
            return testFileName;
        }

        private void ButtonUploadedClick(object sender, RoutedEventArgs e)
        {
            DateTime dt = DateTime.Now;
            string batchName = DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss") + " " + textboxBatchName.Text;
            //string batchName = "yo";

            if (listBoxUploadQueue.Items.Count > 0)
            {
                // announce batch name
                Msg sndMsg = new Msg(CommMessage.MessageType.request);
                sndMsg.command = CommMessage.Command.announceBatch;
                sndMsg.author = loginUsername.Text;
                sndMsg.to = getRemoteHostString();
                sndMsg.from = getLocalHostString();
                sndMsg.stringBody = "<StringBody><BatchName>" + batchName + "</BatchName></StringBody>";
                comm_.postMessage(sndMsg);

                // handle duplicates
                List<string> destFileList = new List<string>();
                foreach (string file in listBoxUploadQueue.Items)
                {
                    string fileNoPath = Path.GetFileName(file);

                    if (destFileList.Contains(fileNoPath))
                    {
                        string newFileName = getNewFileName(fileNoPath, destFileList);
                        destFileList.Add(newFileName);
                    }
                    else
                    {
                        destFileList.Add(fileNoPath);
                    }
                }

                int index = 0;
                foreach (string file in listBoxUploadQueue.Items)
                {
                    string fileNoPath = Path.GetFileName(file);

                    // annouce new file
                    sndMsg = new Msg(CommMessage.MessageType.request);
                    sndMsg.command = CommMessage.Command.announceFile;
                    sndMsg.author = loginUsername.Text;
                    sndMsg.to = getRemoteHostString();
                    sndMsg.from = getLocalHostString();
                    sndMsg.stringBody = "<StringBody><Filename>" + destFileList[index] + "</Filename><Batch>" + batchName + "</Batch><Owner>" + sndMsg.author + "</Owner></StringBody>";
                    comm_.postMessage(sndMsg);

                    // send the file
                    comm_.postFile(file, sndMsg.author, batchName, true, destFileList[index++]);
                }


                // run analytcs batch name
                sndMsg = new Msg(CommMessage.MessageType.request);
                sndMsg.command = CommMessage.Command.execAnalytics;
                sndMsg.author = loginUsername.Text;
                sndMsg.to = getRemoteHostString();
                sndMsg.from = getLocalHostString();
                sndMsg.stringBody = "<StringBody><BatchName>" + batchName + "</BatchName></StringBody>";
                comm_.postMessage(sndMsg);
            }
        }

        private void ButtonClearQueueClick(object sender, RoutedEventArgs e)
        {
            listBoxUploadQueue.Items.Clear();
        }

        private void buttonDownloadGetFileList_Click(object sender, RoutedEventArgs e)
        {
            // get batches
            Msg sndMsg = new Msg(CommMessage.MessageType.request);
            sndMsg.command = CommMessage.Command.getBatchList;
            sndMsg.author = loginUsername.Text;
            sndMsg.to = getRemoteHostString();
            sndMsg.from = getLocalHostString();
            sndMsg.stringBody = "<StringBody></StringBody>";
            comm_.postMessage(sndMsg);

            // get files for a particular batch (the first one)
        }


        private void listboxDownloadBatchesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string selectedItemName = ((sender as System.Windows.Controls.ListBox).SelectedItem.ToString());

                Console.WriteLine(selectedItemName);

                // get files for a particular batch (the first one)
                // get batches
                Msg sndMsg = new Msg(CommMessage.MessageType.request);
                sndMsg.command = CommMessage.Command.getBatchFileList;
                sndMsg.author = loginUsername.Text;
                sndMsg.to = getRemoteHostString();
                sndMsg.from = getLocalHostString();
                sndMsg.stringBody = "<StringBody><Batch>" + selectedItemName + "</Batch><Owner>" + sndMsg.author + "</Owner></StringBody>";
                comm_.postMessage(sndMsg);
            }
            catch
            {

            }
        }

        private void ButtonDownload_Click(object sender, RoutedEventArgs e)
        {
            string selectedBatch = listboxDownloadBatchesList.SelectedItem.ToString();

            var selectedFiles = listboxDownloadFilesList.SelectedItems;

            if ((selectedBatch == null) || (selectedFiles == null) || (selectedBatch == "") || (selectedFiles.Count == 0))
            {
                MessageBox.Show("Batch or file empty.");
                return;
            }

            string finalString = "<StringBody>";
            foreach (string filename in selectedFiles)
            {
                finalString += "<File><Name>" + filename + "</Name><Batch>" + selectedBatch + "</Batch></File>";
            }
            finalString += "</StringBody>";

            Msg sndMsg = new Msg(CommMessage.MessageType.request);
            sndMsg.command = CommMessage.Command.downloadFiles;
            sndMsg.author = loginUsername.Text;
            sndMsg.to = getRemoteHostString();
            sndMsg.from = getLocalHostString();
            sndMsg.stringBody = finalString;
            comm_.postMessage(sndMsg);

        }

        private void ButtonDownloadBatch_Click(object sender, RoutedEventArgs e)
        {
            string selectedBatch = listboxDownloadBatchesList.SelectedItem.ToString();

            var selectedFiles = listboxDownloadFilesList.Items;

            if (listboxDownloadFilesList.Items.Count == 0)
            {
                MessageBox.Show("No items to download.");
                return;
            }

            string finalString = "<StringBody>";
            foreach (string filename in listboxDownloadFilesList.Items)
            {
                finalString += "<File><Name>" + filename + "</Name><Batch>" + selectedBatch + "</Batch></File>";
            }
            finalString += "</StringBody>";

            Msg sndMsg = new Msg(CommMessage.MessageType.request);
            sndMsg.command = CommMessage.Command.downloadFiles;
            sndMsg.author = loginUsername.Text;
            sndMsg.to = getRemoteHostString();
            sndMsg.from = getLocalHostString();
            sndMsg.stringBody = finalString;
            comm_.postMessage(sndMsg);

        }


        private void listboxMetricsBatchesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string selectedItemName = ((sender as System.Windows.Controls.ListBox).SelectedItem.ToString());

                Console.WriteLine(selectedItemName);

                // get files for a particular batch (the first one)
                // get batches
                Msg sndMsg = new Msg(CommMessage.MessageType.request);
                sndMsg.command = CommMessage.Command.getBatchFileList;
                sndMsg.author = loginUsername.Text;
                sndMsg.to = getRemoteHostString();
                sndMsg.from = getLocalHostString();
                sndMsg.stringBody = "<StringBody><Batch>" + selectedItemName + "</Batch><Owner>" + sndMsg.author + "</Owner></StringBody>";
                comm_.postMessage(sndMsg);
            }
            catch
            {

            }
        }

        private void ButtonViewMetrics_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonAccessGetFileList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mainTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainTab.SelectedItem == tabUpload)
            {
                Console.WriteLine("yo2");
            }
            if (mainTab.SelectedItem == tabDownload)
            {
                //Console.WriteLine("yo2");
                //buttonDownloadGetFileList_Click(null, null);
            }
            if (mainTab.SelectedItem == tabMetrics)
            {
                //Console.WriteLine("yo2");
            }
        }

        private void OnTabDownloadSelected(object sender, RoutedEventArgs e)
        {
            var tab = sender as TabItem;

            if (tab != null)
            {
                buttonDownloadGetFileList_Click(null, null);
            }
        }

        private void OnTabAccessSelected(object sender, RoutedEventArgs e)
        {
            var tab = sender as TabItem;

            if (tab != null)
            {
                buttonDownloadGetFileList_Click(null, null);

                // users list
                Msg sndMsg = new Msg(CommMessage.MessageType.request);
                sndMsg.command = CommMessage.Command.getUsersList;
                sndMsg.author = loginUsername.Text;
                sndMsg.to = getRemoteHostString();
                sndMsg.from = getLocalHostString();
                sndMsg.stringBody = "<StringBody></StringBody>";
                comm_.postMessage(sndMsg);
            }
        }

        private void listboxAccessBatchesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string selectedItemName = ((sender as System.Windows.Controls.ListBox).SelectedItem.ToString());

                Console.WriteLine(selectedItemName);

                // get files for a particular batch (the first one)
                // get batches
                Msg sndMsg = new Msg(CommMessage.MessageType.request);
                sndMsg.command = CommMessage.Command.getBatchFileList;
                sndMsg.author = loginUsername.Text;
                sndMsg.to = getRemoteHostString();
                sndMsg.from = getLocalHostString();
                sndMsg.stringBody = "<StringBody><Batch>" + selectedItemName + "</Batch><Owner>" + sndMsg.author + "</Owner></StringBody>";
                comm_.postMessage(sndMsg);
            }
            catch
            {

            }

        }

        private void ButtonAccessGrantAccess_Click(object sender, RoutedEventArgs e)
        {
            string selectedBatch = listboxAccessBatchesList.SelectedItem.ToString();

            var selectedFiles = listboxAccessFilesList.SelectedItems;

            var selectedUser = listboxAccessUsersList.SelectedItem.ToString();

            if (selectedBatch.Contains("Granted Access Batch"))
            {
                MessageBox.Show("Sorry, can't grant access to files that you've been granted access to. Please choose a non-granted batch instead.");
            }

            string finalString = "<StringBody>";
            foreach (string filename in selectedFiles)
            {
                finalString += "<File><Name>" + filename + "</Name><Batch>" + selectedBatch + "</Batch><GrantAccess>" + selectedUser + "</GrantAccess></File>";
            }
            finalString += "</StringBody>";

            Msg sndMsg = new Msg(CommMessage.MessageType.request);
            sndMsg.command = CommMessage.Command.greatFileAccess;
            sndMsg.author = loginUsername.Text;
            sndMsg.to = getRemoteHostString();
            sndMsg.from = getLocalHostString();
            sndMsg.stringBody = finalString;
            comm_.postMessage(sndMsg);
        }

        private void ButtonMetricsViewMetrics_Click(object sender, RoutedEventArgs e)
        {
            var selectedFile = listboxMetricsFilesList.SelectedItem;
            string selectedBatch = listboxMetricsBatchesList.SelectedItem.ToString();

            // get metrics
            Msg sndMsg = new Msg(CommMessage.MessageType.request);
            sndMsg.command = CommMessage.Command.getAnalytics;
            sndMsg.author = loginUsername.Text;
            sndMsg.to = getRemoteHostString();
            sndMsg.from = getLocalHostString();
            sndMsg.stringBody = "<File><BatchName>" + selectedBatch + "</BatchName></File>"; ;
            comm_.postMessage(sndMsg);

            analyticsDlg = new Analytics();
            analyticsDlg.Show();
        }

        private void Button_SearchClick(object sender, RoutedEventArgs e)
        {
            string searchTerm = editBoxSearch.Text;

            if (searchTerm.Equals(""))
            {
                MessageBox.Show("Enter search text to search.");
                return;
            }

            if ((bool) radioSearchBatch.IsChecked)
            {
                listboxSearchFilesList.Items.Clear();

                // search batch
                Msg sndMsg = new Msg(CommMessage.MessageType.request);
                sndMsg.command = CommMessage.Command.searchBatch;
                sndMsg.author = loginUsername.Text;
                sndMsg.to = getRemoteHostString();
                sndMsg.from = getLocalHostString();
                sndMsg.stringBody = "<StringBody><SearchTerm>" + searchTerm + "</SearchTerm><Owner>" + sndMsg.author + "</Owner></StringBody>";
                comm_.postMessage(sndMsg);
            }
            else
            {
                // search files
                listboxSearchFilesList.Items.Clear();

                // search batch
                Msg sndMsg = new Msg(CommMessage.MessageType.request);
                sndMsg.command = CommMessage.Command.searchFiles;
                sndMsg.author = loginUsername.Text;
                sndMsg.to = getRemoteHostString();
                sndMsg.from = getLocalHostString();
                sndMsg.stringBody = "<StringBody><SearchTerm>" + searchTerm + "</SearchTerm><Owner>" + sndMsg.author + "</Owner></StringBody>";
                comm_.postMessage(sndMsg);
            }
        }

        private void ButtonCommentAddComment_Click(object sender, RoutedEventArgs e)
        {
            string selectedBatch = listboxCommentBatchesList.SelectedItem.ToString();
            string selectedFile = listboxCommentFilesList.SelectedItem.ToString();
            string comment = textBoxComment.Text;

            if (comment.Equals(""))
            {
                MessageBox.Show("Comment field empty.");
                return;
            }

            if ((bool) radioCommentBatch.IsChecked)
            {
                if (selectedBatch == null)
                {
                    MessageBox.Show("No batch selected.");
                    return;
                }

                selectedFile = "";  // don't comment on files. Just batches.
            }
            else
            {
                if ((selectedFile == null) || (selectedBatch == null))
                {
                    MessageBox.Show("No batch or file selected.");
                    return;
                }
            }
            
            string finalString = "<StringBody>";
            finalString += "<File><Name>" + selectedFile + "</Name><Batch>" + selectedBatch + "</Batch><Comment>" + comment + "</Comment></File>";
            finalString += "</StringBody>";

            // add comment
            Msg sndMsg = new Msg(CommMessage.MessageType.request);
            sndMsg.command = CommMessage.Command.addComment;
            sndMsg.author = loginUsername.Text;
            sndMsg.to = getRemoteHostString();
            sndMsg.from = getLocalHostString();
            sndMsg.stringBody = finalString;
            comm_.postMessage(sndMsg);

            // refresh comments
            sndMsg = new Msg(CommMessage.MessageType.request);
            sndMsg.command = CommMessage.Command.getComments;
            sndMsg.author = loginUsername.Text;
            sndMsg.to = getRemoteHostString();
            sndMsg.from = getLocalHostString();
            sndMsg.stringBody = "";
            comm_.postMessage(sndMsg);
        }

        private void OnTabCommentSelected(object sender, RoutedEventArgs e)
        {
            var tab = sender as TabItem;

            if (tab != null)
            {
                buttonDownloadGetFileList_Click(null, null);

                // refresh comments
                Msg sndMsg = new Msg(CommMessage.MessageType.request);
                sndMsg.command = CommMessage.Command.getComments;
                sndMsg.author = loginUsername.Text;
                sndMsg.to = getRemoteHostString();
                sndMsg.from = getLocalHostString();
                sndMsg.stringBody = "";
                comm_.postMessage(sndMsg);
            }
        }
    }
}
