
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

            this.Loaded += MainWindow_Loaded;
        }

        private void DisplayRelation(int key1, int key2, string[] columns = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
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
            sw.Stop();
            Console.WriteLine($"+++++++Displaying the relation took {sw.ElapsedMilliseconds} millisenconds");
        }

        private void BuildTree(int degree, SortedDictionary<int, RecordPointer> ids)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
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
            sw.Stop();
            Console.WriteLine($"+++++++Building the tree took {sw.ElapsedMilliseconds} millisenconds");
        }

        public void DisplayTree()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Tree.Items.Clear();
            ViewNode rootView = tree.Display();
            Tree.Items.Add(rootView);
            sw.Stop();
            Console.WriteLine($"+++++++Dislaying the B+ plus tree with a TreeView WPF control took {sw.ElapsedMilliseconds} millisenconds");
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

        private void InsertIntoEmployees_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Employee em = new Employee()
            {
                Id = ParseTextBox(InsertId),
                Gender = InsertGender.Text.ToCharArray()[0],
                FirstName = InsertFirstName.Text,
                LastName = InsertLastName.Text,
                Salary = ParseTextBox(InsertSalary)
            };

            RecordPointer previousEmRp = Employee.FindPreviousEmployee(tree, em.Id);
            RecordPointer emRp = new RecordPointer(defaultE.GetCache().FindPlaceInFile(previousEmRp.Block));

            tree.Insert(em.Id, emRp);
            em.SetRecord(em, emRp.Block, emRp.Offset);
            recordCount++;

            sw.Stop();
            Console.WriteLine($"+++++++Inserting employee {em} into the table took {sw.ElapsedMilliseconds} millisenconds");

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
            Stopwatch sw = new Stopwatch();
            sw.Start();
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
                }
                DisplayRelation(1, recordCount, columns);
            }
            sw.Stop();
            Console.WriteLine($"+++++++Doing a query on the Employees table took {sw.ElapsedMilliseconds} millisenconds");
        }

        private void DeleteFromEmployees_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int id = 0;
            int.TryParse(DeleteId.Text, out id);
            RecordPointer emRp = tree.Delete(id);
            if (emRp.CompareTo(RecordPointer.Empty) != 0) Employee.Empty.DeleteRecord(emRp.Block, emRp.Offset);

            sw.Stop();
            Console.WriteLine($"+++++++Deleting employee with id {id} took {sw.ElapsedMilliseconds} millisenconds");

            DisplayRelation(1, recordCount);
            DisplayTree();
        }

        private void UpdateEmployees_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
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

            sw.Stop();
            Console.WriteLine($"+++++++Updating employee with id {id} took {sw.ElapsedMilliseconds} millisenconds");

            DisplayRelation(1, recordCount);
            DisplayTree();
        }
    }
}
