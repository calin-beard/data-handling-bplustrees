using ControlTreeView;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace DataHandlingBPlusTrees
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public string TestRelationName { get; set; } = "Students";
        public int Block { get; set; } = 4096;
        Relation rel { get; set; }

        private void Setup()
        {
            if (File.Exists(this.TestRelationName))
            {
                File.Delete(this.TestRelationName);
            }
        }

        private void DisplayRelation()
        {
            List<Record> records = rel.File.Read();
            Students.ItemsSource = null;
            Students.ItemsSource = records;
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Setup();

            rel = new Relation(this.TestRelationName, new List<Attributte> {
                new Attributte("Id", true, true, true),
                new Attributte("Name", false, true, false),
                new Attributte("FirstName", false, true, false)
            });
            Record r = new Record(rel.AttributeNames, "1,Barburescu,Calin;");
            rel.File.WriteRecord(r.ToString());
            DisplayRelation();

            int degree = 4;
            BPlusTree tree = new BPlusTree(degree);

            Dictionary<string, Pointer> searchKeys = new Dictionary<string, Pointer>
            {
                { "0", new Pointer()},
                { "1", new Pointer()},
                { "2", new Pointer()},
                { "3", new Pointer()},
                { "4", new Pointer()},
                { "5", new Pointer()},
                { "6", new Pointer()},
                { "7", new Pointer()},
                { "8", new Pointer()},
                { "9", new Pointer()},
            };

            tree.AddMultiple(searchKeys);

            Draw(tree.Root, Main);

            //CTreeView sampleCTreeView = new CTreeView();
            //Main.Children.Add(sampleCTreeView);
            //sampleCTreeView.BeginUpdate();
            //sampleCTreeView.Nodes.Add(new CTreeNode("root node", new MyControl()));
            //for (int i = 0; i < 5; i++)
            //{
            //    sampleCTreeView.Nodes[0].Nodes.Add(new CTreeNode("node " + i, new MyControl()));
            //}
            //sampleCTreeView.EndUpdate();
        }

        private void Draw(Node node, StackPanel sp)
        {
            //add new line (stack panel) to the parent stack panel
            if (node.isRoot() || node.Parent.Children.IndexOf(node) == 0)
            {
                StackPanel childPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(10)
                };
                sp.Children.Add(childPanel);
            }

            StackPanel lastChildPanel = sp.Children[sp.Children.Count - 1] as StackPanel;
            StackPanel grandchild = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };
            lastChildPanel.Children.Add(grandchild);

            for (int i = 0; i < node.Keys.Count; i++)
            {
                Label n = new Label
                {
                    Content = node.Keys[i]
                };
                grandchild.Children.Add(n);
                if (i == node.Keys.Count - 1)
                {
                    break;
                }
                Rectangle s = new Rectangle();
                s.Width = 2;
                s.Margin = new Thickness(2);
                s.Stroke = new SolidColorBrush(Colors.Black);
                grandchild.Children.Add(s);
            }

            if (!node.isLeaf())
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    Draw(node.Children[i], sp);
                }
            }
        }

        private void RecordToDb_Click(object sender, RoutedEventArgs e)
        {
            Record rec = new Record(rel.AttributeNames, new List<string> {
                Id.Text,
                Name.Text,
                FirstName.Text
            });
            rel.File.WriteRecord(rec.ToString());
            DisplayRelation();
        }
    }
}
