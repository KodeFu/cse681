using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Client
{
    /// <summary>
    /// Interaction logic for Analytics.xaml
    /// </summary>
    public partial class Analytics : Window
    {
        public Analytics()
        {
            InitializeComponent();
        }

        public void AddAnalytics(string analytics)
        {
            XElement xmlTree = XElement.Parse(analytics);

            var elements = (from e in xmlTree.Descendants("File")
                           select e);

            foreach (var e in elements)
            {
                TreeViewItem root = new TreeViewItem();
                root.Header = e.Element("Name").Value;
                root.FontWeight = FontWeights.Bold;

                treeViewAnalytics.Items.Add(root);
                
                string metrics = e.Element("Metrics").Value;
                string[] metricsList = metrics.Split(';');

                TreeViewItem nsItem = null;
                TreeViewItem clItem = null;
                TreeViewItem fnItem = null;

                foreach (string m in metricsList)
                {
                    string[] metricFields = m.Split(':');

                    if (metricFields.Length >= 7)
                    {
                        string type = getTypeName(metricFields[0]);
                        string name = metricFields[1];
                        string loc = metricFields[2];
                        string complexity = metricFields[3];
                        string coupling = metricFields[4];
                        string cohesion = metricFields[5];
                        string mainIndex = metricFields[6];
                        
                        TreeViewItem tvLoc = new TreeViewItem() { Header = "LOC: " + loc};
                        TreeViewItem tvComplexity = new TreeViewItem() { Header = "Complexity: " + complexity};
                        TreeViewItem tvCoupling = new TreeViewItem() { Header = "Coupling: " + coupling };
                        TreeViewItem tvCohesion = new TreeViewItem() { Header = "Cohesion: " + cohesion };
                        TreeViewItem tvMainIndex = new TreeViewItem() { Header = "Maintainability Index: " + mainIndex};
                        
                        if (type.Equals("Namespace"))
                        {
                            nsItem = new TreeViewItem() { Header = type + " " + name };
                            root.Items.Add(nsItem);

                            nsItem.Foreground = Brushes.Magenta;
                            nsItem.FontWeight = FontWeights.Normal;
                        }
                        else if (type.Equals("Class"))
                        {
                            clItem = new TreeViewItem() { Header = type + " " + name };

                            clItem.Foreground = Brushes.Blue;
                            nsItem.FontWeight = FontWeights.Normal;

                            clItem.Items.Add(tvLoc);
                            clItem.Items.Add(tvComplexity);
                            clItem.Items.Add(tvCoupling);
                            clItem.Items.Add(tvCohesion);
                            clItem.Items.Add(tvMainIndex);

                            if (nsItem != null)
                            {
                                nsItem.Items.Add(clItem);
                            }
                            else
                            {
                                root.Items.Add(clItem);
                            }
                        }
                        else if (type.Equals("Function"))
                        {
                            fnItem = new TreeViewItem() { Header = type + " " + name };

                            fnItem.Items.Add(tvLoc);
                            fnItem.Items.Add(tvComplexity);

                            fnItem.Foreground = Brushes.Green;
                            fnItem.FontWeight = FontWeights.Normal;

                            if (clItem != null)
                            {
                                clItem.Items.Add(fnItem);
                            }
                            else
                            {
                                root.Items.Add(fnItem);
                            }
                        }
                        else
                        {
                            TreeViewItem tmp = new TreeViewItem() { Header = type + " " + name };

                            tmp.FontWeight = FontWeights.Normal;

                            root.Items.Add(tmp);
                        }
                    }
                }

                expandAll(treeViewAnalytics.Items);
            }
        }

        void expandAll(ItemCollection treeViewItems)
        {
            foreach (TreeViewItem tvItem in treeViewItems)
            {
                tvItem.IsExpanded = true;

                if (tvItem.Items.Count > 0)
                {
                    expandAll(tvItem.Items);
                }
            }
        }

        void collapseAll(ItemCollection treeViewItems)
        {
            foreach (TreeViewItem tvItem in treeViewItems)
            {
                tvItem.IsExpanded = false;

                if (tvItem.Items.Count > 0)
                {
                    collapseAll(tvItem.Items);
                }
            }
        }


        static string getTypeName(string type)
        {
            switch (type)
            {
                case "n":
                    return "Namespace";
                case "c":
                    return "Class";
                case "f":
                    return "Function";
                case "i":
                    return "Interface";
                default:
                    return type;
            }
        }

        private void RadioButton_ExpandAllChecked(object sender, RoutedEventArgs e)
        {
            expandAll(treeViewAnalytics.Items);
        }

        private void RadioButton_CollapseAllChecked(object sender, RoutedEventArgs e)
        {
            collapseAll(treeViewAnalytics.Items);
        }
    }
}
