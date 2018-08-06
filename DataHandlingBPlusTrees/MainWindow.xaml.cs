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

namespace DataHandlingBPlusTrees
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            string path = Directory.GetCurrentDirectory() + @"\test";
            Console.WriteLine("-----------------" + path);
            FileManager fm = new FileManager(path);
            fm.Write("1234567");

            int degree = 4;
            BPlusTree tree = new BPlusTree(degree);

            Dictionary<string, Record> searchKeys = new Dictionary<string, Record>
            {
                { "9", new Record()},
                { "8", new Record()},
                { "7", new Record()},
                { "6", new Record()},
                { "5", new Record()},
                { "4", new Record()},
                { "3", new Record()},
                { "2", new Record()},
                { "1", new Record()},
                { "0", new Record()},
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
    }
}
