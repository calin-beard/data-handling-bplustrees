
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
        private List<Tuple<Point, Point>> LinkedListArrowCoords { get; set; } = new List<Tuple<Point, Point>>();

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
            Record r1 = new Record(rel.AttributeNames, "1,Barburescu,Calin;");
            Record r2 = new Record(rel.AttributeNames, "2,Gavrila,Cristina;");
            Record r3 = new Record(rel.AttributeNames, "3,Preda,Andreia;");
            rel.File.WriteRecord(r1.ToString());
            rel.File.WriteRecord(r2.ToString());
            rel.File.WriteRecord(r3.ToString());
            rel.File.DeleteRecord(4096 + 64 + 64);
            rel.File.DeleteRecord(4096);
            rel.File.DeleteRecord(4096 + 64);
            rel.File.WriteRecord(r1.ToString());
            rel.File.WriteRecord(r2.ToString());
            rel.File.WriteRecord(r3.ToString());
            DisplayRelation();

            int degree = 6;
            BPlusTree<int> tree = new BPlusTree<int>(degree);

            int[] numbers = new int[] { 17, 10, 12, 7, 6, 14, 5, 9, 16, 13, 2, 20, 11, 3, 4, 15, 1, 19, 18, 8 };
            Dictionary<int, RecordPointer<int>> ids = new Dictionary<int, RecordPointer<int>>();

            foreach (int number in numbers)
            {
                ids.Add(number, new RecordPointer<int>(number, 0, 0));
            }

            tree.InsertMultiple(ids);

            RecordPointer<int> rp = tree.Find(7);
            //Console.WriteLine("Found record pointer for 7 --- " + rp.Value);

            List<RecordPointer<int>> recps = tree.FindRange(5, 7);
            foreach(RecordPointer<int> el in recps)
            {
                Console.WriteLine($"Found record pointer for --- {el.Value}");
            }

            //Draw(tree.Root, Main);


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

        //private void Draw(BPlusTree<int>.Node node, StackPanel sp)
        //{
        //    //add new line (stack panel) to the parent stack panel
        //    if (node.IsRoot() || Array.IndexOf(node.Parent.Pointers, node) == 0)
        //    {
        //        StackPanel childPanel = new StackPanel
        //        {
        //            Orientation = Orientation.Horizontal,
        //            HorizontalAlignment = HorizontalAlignment.Center,
        //            Margin = new Thickness(10)
        //        };
        //        sp.Children.Add(childPanel);
        //    }

        //    // adds new node at the end of the row
        //    StackPanel lastChildPanel = sp.Children[sp.Children.Count - 1] as StackPanel;
        //    StackPanel grandchild = new StackPanel
        //    {
        //        Orientation = Orientation.Horizontal,
        //        HorizontalAlignment = HorizontalAlignment.Center,
        //        //Width = 10,
        //        Margin = new Thickness(5)
        //    };
        //    lastChildPanel.Children.Add(grandchild);

        //    // adds keys to the node as labels
        //    for (int i = 0; i < node.Keys.Length; i++)
        //    {
        //        Label n = new Label
        //        {
        //            Content = node.Keys[i],
        //            Foreground = Brushes.White,
        //            HorizontalContentAlignment = HorizontalAlignment.Center
        //        };
        //        Border b = new Border
        //        {
        //            BorderBrush = Brushes.DarkGreen,
        //            BorderThickness = new Thickness(1),
        //            Background = Brushes.ForestGreen,
        //            MinWidth = 22,
        //            Child = n,
        //        };
        //        //b.MouseEnter += new MouseEventHandler(HoverOverKey);
        //        //b.MouseLeave += new MouseEventHandler(HoverOverKey);
        //        grandchild.Children.Add(b);
        //        if (i == 0)
        //        {
        //            b.Loaded += new RoutedEventHandler(FirstKeyLoaded);
        //        }
        //        if (i == node.Keys.Length - 1)
        //        {
        //            grandchild.SizeChanged += new SizeChangedEventHandler(StackPanelSizeChanged);
        //            grandchild.Loaded += new RoutedEventHandler(NodeFinishedLoading);
        //            break;
        //        }
        //    }

        //    // draw the arrows for the linked list
        //    if (node.IsLeaf())
        //    {
        //        grandchild.Loaded += new RoutedEventHandler(LeafFinishedLoading);
        //        //if (Array.IndexOf(node.Parent.Pointers, node) == ArrayHandler.GetIndexOfLastElement(node.Parent.Pointers))
        //        //{
        //        //    Main.Loaded += new RoutedEventHandler(TreeLoaded);
        //        //}
        //    }
        //    else
        //    {
        //        // call the draw method recursively in order to draw the child nodes
        //        for (int i = 0; i < node.Pointers.Length; i++)
        //        {
        //            if (node.Pointers[i] == null)
        //            {
        //                break;
        //            }
        //            Draw(node.Pointers[i], sp);
        //        }
        //    }

        //    if (node.IsRoot())
        //    {
        //        Main.Loaded += new RoutedEventHandler(TreeLoaded);
        //    }
        //}

        private void DrawLinkedList()
        {
            List<Tuple<Point, Point>> c = LinkedListArrowCoords;
            if (!LinkedListArrowCoords.Any())
            {
                Console.WriteLine("-------------List is empty");
            }
            foreach (Tuple<Point, Point> el in c)
            {
                Console.WriteLine("------- Points are" + el.Item1.ToString() + " ; " + el.Item2.ToString());
            }
            for (int i = 0; i < LinkedListArrowCoords.Count; i++)
            {
                if (i == LinkedListArrowCoords.Count - 1)
                {
                    break;
                }
                this.AddChild(new Line { Stroke = Brushes.PowderBlue, StrokeThickness = 2, X1 = c[i].Item1.X, Y1 = c[i].Item1.Y, X2 = c[i+1].Item2.X, Y2 = c[i+1].Item2.Y });
            }
        }

        //public void HoverOverKey(object sender, MouseEventArgs e)
        //{
        //    Console.WriteLine("---- The mouse is at " + e.GetPosition(this));
        //}

        public void NodeFinishedLoading(object sender, RoutedEventArgs e)
        {
            StackPanel s = sender as StackPanel;
            //Console.WriteLine("-----Width of " + s.ToString() + " is " + s.ActualWidth);
            //Console.WriteLine("-----Midpoint of " + s.ToString() + " is " + s.PointToScreen(new Point(s.ActualWidth/2, 0)));
        }

        public void LeafFinishedLoading(object sender, RoutedEventArgs e)
        {
            StackPanel s = sender as StackPanel;
            Point start = s.PointToScreen(new Point(s.ActualWidth - 5, s.ActualHeight / 2));
            Point end = s.PointToScreen(new Point(5, s.ActualHeight / 2));
            LinkedListArrowCoords.Add(new Tuple<Point, Point>(start, end));
            //Console.WriteLine("-----LL arrow starting point from " + s.ToString() + " is " + start);
            //Console.WriteLine("-----LL arrow ending point from " + s.ToString() + " is " + end);
        }

        public void StackPanelSizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        public void FirstKeyLoaded(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine("----------------" + (sender as Border).PointToScreen(new Point(0, 0)));
        }

        public void TreeLoaded(object sender, RoutedEventArgs e)
        {
            DrawLinkedList();
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
