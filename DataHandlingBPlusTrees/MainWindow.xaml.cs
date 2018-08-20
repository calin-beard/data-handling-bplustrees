
using System;
using System.Collections.Generic;
using System.Globalization;
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

        private void DisplayRelation(string[] columns = null)
        {
            List<Record> records = rel.File.Read();
            StudentsTable.ItemsSource = null;
            StudentsTable.Columns.Clear();
            StudentsTable.ItemsSource = records;
            if (columns == null)
            {
                columns = rel.AttributeNames.ToArray();
            }
            foreach (string column in columns)
            {
                StudentsTable.Columns.Add(new DataGridTextColumn
                {
                    Binding = new Binding($"Attributes[{column}]"),
                    Header = column
                });
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Setup();

            rel = new Relation(this.TestRelationName, new List<Attributte> {
                new Attributte("id", true, true, true),
                new Attributte("name", false, true, false),
                new Attributte("firstname", false, true, false)
            });
            Record r = new Record(rel.AttributeNames, "1,Barburescu,Calin;");
            rel.File.WriteRecord(r.ToString());
            DisplayRelation();

            int degree = 4;
            BPlusTree tree = new BPlusTree(degree);

            Dictionary<string, Pointer> searchKeys = new Dictionary<string, Pointer>
            {
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

            tree.InsertMultiple(searchKeys);

            Draw(tree.Root, Main);

            //CTreeView sampleCTreeView = new CTreeView();
            //Main.Pointers.Add(sampleCTreeView);
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
            if (node.IsRoot() || Array.IndexOf(node.Parent.Pointers, node) == 0)
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

            for (int i = 0; i < node.Keys.Length; i++)
            {
                Label n = new Label
                {
                    Content = node.Keys[i]
                };
                grandchild.Children.Add(n);
                if (i == node.Keys.Length - 1)
                {
                    break;
                }
                Rectangle s = new Rectangle();
                s.Width = 2;
                s.Margin = new Thickness(2);
                s.Stroke = new SolidColorBrush(Colors.Black);
                grandchild.Children.Add(s);
            }

            if (!node.IsLeaf())
            {
                for (int i = 0; i < node.Pointers.Length; i++)
                {
                    Draw(node.Pointers[i], sp);
                }
            }
        }

        private void InsertIntoStudents_Click(object sender, RoutedEventArgs e)
        {
            Record rec = new Record(rel.AttributeNames, new List<string> {
                Id.Text,
                Name.Text,
                FirstName.Text
            });
            rel.File.WriteRecord(rec.ToString());
            DisplayRelation();
        }

        private void SelectFromStudents_Click(object sender, RoutedEventArgs e)
        {
            if (ColumnSelector.Text == "*")
            {
                DisplayRelation();
            }
            else
            {
                string[] columns = ColumnSelector.Text.Split(',');
                for (int i = 0; i < columns.Length; i++)
                {
                    columns[i] = columns[i].Trim();
                    columns[i] = columns[i].ToLower();
                }
                DisplayRelation(columns);
            }
        }
    }
}
