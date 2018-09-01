
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private BPlusTree<int> tree;
        private int recordCount = 10000;
        Employee defaultE = Employee.Empty;

        private void DisplayRelation(int key1, int key2, string[] columns = null)
        {
            List<RecordPointer> employeesRP = tree.FindRange(key1, key2);
            List<Employee> employees = new List<Employee>();
            employeesRP.ForEach(rp => employees.Add(defaultE.GetRecord(rp.Block, rp.Offset)));
            employees.ForEach(e => { e.FirstName = e.FirstName.TrimEnd('\0'); e.LastName = e.LastName.TrimEnd('\0'); });
            EmployeesTable.ItemsSource = null;
            EmployeesTable.Columns.Clear();
            EmployeesTable.ItemsSource = employees;
            if (columns == null)
            {
                columns = new string[]
                {
                    "Id",
                    "Gender",
                    "Salary",
                    "FirstName",
                    "LastName"
                };
            }
            foreach (string column in columns)
            {
                EmployeesTable.Columns.Add(new DataGridTextColumn
                {
                    Binding = new Binding($"{column}"),
                    Header = column
                });
            }
        }

        private void BuildTree(int degree, SortedDictionary<int, RecordPointer> ids)
        {
            int block = 0;
            int offset = 0;
            for (int i = 0; i < recordCount; i++)
            {
                Console.WriteLine("+++++++++" + defaultE.GetRecord(block, offset));
                Employee current = defaultE.GetRecord(block, offset);
                if (current.Id > 0)
                {
                    ids.Add(current.Id, new RecordPointer(block, offset));
                }
                else
                {
                    offset += defaultE.RecordSize();
                    continue;
                }
                offset += defaultE.RecordSize();
                if (Block.Size() - offset < defaultE.RecordSize())
                {
                    offset = 0;
                    block++;
                }
            }

            tree = BPlusTree<int>.BuildGroundUp(degree, ids);
        }

        public void DisplayTree()
        {
            Tree.Items.Clear();
            ViewNode rootView = tree.Display();
            Tree.Items.Add(rootView);
        }

        public MainWindow()
        {
            InitializeComponent();

            int degree = 102;
            SortedDictionary<int, RecordPointer> ids = new SortedDictionary<int, RecordPointer>();

            if (!File.Exists(Employee.PathName()))
            {
                Database.CreateMock(recordCount);
            }

            BuildTree(degree, ids);

            DisplayTree();

            DisplayRelation(1, recordCount);

            //List<Employee> employees = new List<Employee>();
            //for (int i = 0; i < 1000; i++)
            //{
            //    employees.Add(new Employee(i, 'M', 19000, "calin", "barburescu"));
            //}
            //employees.Add(new Employee(2, 'F', 19000, "cristina", "gavrila"));
            //employees.Add(new Employee(3, 'F', 19000, "andreia", "preda"));

            //int b = 0, o = 0;
            //Employee ee = Employee.Empty;

            //int block = 0;
            //int offset = 0;
            //foreach (Employee e in employees)
            //{
            //    e.SetRecord(e, block, offset);
            //    Console.WriteLine("---Employee size is " + e.RecordSize());
            //    Console.WriteLine("+++++++++" + e.GetRecord(block, offset));
            //    if (e.FirstName.CompareTo("cristina") == 0)
            //    {
            //        b = block;
            //        o = offset;
            //        ee = e;
            //    }
            //    offset += e.RecordSize();
            //    if (Block.Size() - offset < e.RecordSize())
            //    {
            //        offset = 0;
            //        block++;
            //    }
            //}

            //Employee eeempty = Employee.Empty;
            //Console.WriteLine("1+++++++++" + eeempty.GetRecord(block, offset));
            //eeempty.SetRecord(eeempty, block, offset);
            //Console.WriteLine("2+++++++++" + eeempty.GetRecord(block, offset));

            //Console.WriteLine("3+++++++++" + ee.GetRecord(b, o));
            //ee.DeleteRecord(b, o);
            //Console.WriteLine("4+++++++++" + ee.GetRecord(b, o));



            //tree.InsertMultiple(ids);
            //tree.Delete(21);
            //tree.Delete(22);
            //tree.Delete(20);

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
            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, EventArgs e)
        {
            Button close = this.FindChild<Button>("PART_Close");
            close.Click += Close_Click;
        }

        void Close_Click(object sender, RoutedEventArgs e)
        {
            Employee.Cache.FlushAllCachedBlocks();
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

        //private void DrawLinkedList()
        //{
        //    List<Tuple<Point, Point>> c = LinkedListArrowCoords;
        //    if (!LinkedListArrowCoords.Any())
        //    {
        //        Console.WriteLine("-------------List is empty");
        //    }
        //    foreach (Tuple<Point, Point> el in c)
        //    {
        //        Console.WriteLine("------- Points are" + el.Item1.ToString() + " ; " + el.Item2.ToString());
        //    }
        //    for (int i = 0; i < LinkedListArrowCoords.Count; i++)
        //    {
        //        if (i == LinkedListArrowCoords.Count - 1)
        //        {
        //            break;
        //        }
        //        this.AddChild(new Line { Stroke = Brushes.PowderBlue, StrokeThickness = 2, X1 = c[i].Item1.X, Y1 = c[i].Item1.Y, X2 = c[i + 1].Item2.X, Y2 = c[i + 1].Item2.Y });
        //    }
        //}

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

        //public void LeafFinishedLoading(object sender, RoutedEventArgs e)
        //{
        //    StackPanel s = sender as StackPanel;
        //    Point start = s.PointToScreen(new Point(s.ActualWidth - 5, s.ActualHeight / 2));
        //    Point end = s.PointToScreen(new Point(5, s.ActualHeight / 2));
        //    LinkedListArrowCoords.Add(new Tuple<Point, Point>(start, end));
        //    //Console.WriteLine("-----LL arrow starting point from " + s.ToString() + " is " + start);
        //    //Console.WriteLine("-----LL arrow ending point from " + s.ToString() + " is " + end);
        //}

        public void StackPanelSizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        public void FirstKeyLoaded(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine("----------------" + (sender as Border).PointToScreen(new Point(0, 0)));
        }

        //public void TreeLoaded(object sender, RoutedEventArgs e)
        //{
        //    DrawLinkedList();
        //}

        private void InsertIntoEmployees_Click(object sender, RoutedEventArgs e)
        {
            //Record rec = new Record(rel.AttributeNames, new List<string> {
            //    Id.Text,
            //    Name.Text,
            //    FirstName.Text
            //});
            //rel.File.WriteRecord(rec.ToString());
            Employee em = new Employee()
            {
                Id = ParseTextBox(InsertId),
                Gender = InsertGender.Text.ToCharArray()[0],
                FirstName = InsertFirstName.Text,
                LastName = InsertLastName.Text,
                Salary = ParseTextBox(InsertSalary)
            };

            RecordPointer previousEmRp = tree.Find(em.Id - 1);
            RecordPointer emRp = new RecordPointer(defaultE.GetCache().FindEmptyRecordInBlock(previousEmRp.Block));

            if (emRp.CompareTo(RecordPointer.Empty) == 0)
            {
                int block = defaultE.GetCache().MakeNewBlock();
                emRp.Block = block;
                emRp.Offset = 0;
            }

            tree.Insert(em.Id, emRp);
            em.SetRecord(em, emRp.Block, emRp.Offset);
            recordCount++;

            DisplayRelation(1, recordCount);
            DisplayTree();
        }

        private int ParseTextBox(TextBox tb)
        {
            int x = 0;
            int.TryParse(tb.Text, out x);
            return x;
        }

        private void SelectFromEmployees_Click(object sender, RoutedEventArgs e)
        {
            if (ColumnSelector.Text == "*")
            {
                DisplayRelation(1, recordCount);
            }
            else
            {
                string[] columns = ColumnSelector.Text.Split(',');
                for (int i = 0; i < columns.Length; i++)
                {
                    columns[i] = columns[i].Trim();
                    //columns[i] = columns[i].ToLower();
                }
                DisplayRelation(1, recordCount, columns);
            }
        }

        private void DeleteFromEmployees_Click(object sender, RoutedEventArgs e)
        {
            int id = 0;
            int.TryParse(DeleteId.Text, out id);
            RecordPointer emRp = tree.Delete(id);
            if (emRp.CompareTo(RecordPointer.Empty) != 0) Employee.Empty.DeleteRecord(emRp.Block, emRp.Offset);

            DisplayRelation(1, recordCount);
            DisplayTree();
        }

        private void UpdateEmployees_Click(object sender, RoutedEventArgs e)
        {
            int id = 0;
            int.TryParse(UpdateId.Text, out id);
            Employee em = Employee.Empty;
            RecordPointer emRp = tree.Find(id);
            if (emRp.CompareTo(RecordPointer.Empty) != 0)
            {
                int salary = 0;
                em = em.GetRecord(emRp.Block, emRp.Offset);
                if (UpdateGender.Text.Trim().Length != 0) em.Gender = UpdateGender.Text.Trim().ToCharArray()[0];
                if (UpdateFirstName.Text.Trim().Length != 0) em.FirstName = UpdateFirstName.Text.Trim();
                if (UpdateLastName.Text.Trim().Length != 0) em.LastName = UpdateLastName.Text.Trim();
                if (UpdateSalary.Text.Trim().Length != 0)
                {
                    int.TryParse(UpdateSalary.Text.Trim(), out salary);
                    em.Salary = salary;
                }
                em.SetRecord(em, emRp.Block, emRp.Offset);
            }

            DisplayRelation(1, recordCount);
            DisplayTree();
        }
    }
}
