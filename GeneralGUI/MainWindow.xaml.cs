using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace GeneralGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static Dictionary<int, string>? MasterFile;
        private bool tabHeld;
        private readonly string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MalinStaffNamesV3.csv");
        public MainWindow()
        {
            MasterFile = ReadFromCsv(path);
            InitializeComponent();
            DisplayToListBox();
            SortedListBox.SelectedIndex = -1;
        }
        private void NameInputTextBox_TextChanged(object sender, TextChangedEventArgs e) => DisplayToSorted(input: new Input { Boxes = [IdInputTextBox, NameInputTextBox] });

        private void IdInputTextBox_TextChanged(object sender, TextChangedEventArgs e) => DisplayToSorted(input: new Input { Boxes = [IdInputTextBox, NameInputTextBox] });
        private Dictionary<int,string> ReadFromCsv(string CsvPath)
        {
            Dictionary<int, string> dict = [];
            if (!File.Exists(CsvPath)) throw new FileNotFoundException($"CSV not found at{CsvPath}");
            foreach (string line in File.ReadAllLines(CsvPath))
            {
                string[] column = line.Split(',');
                if (!int.TryParse(column[0], out int value)) continue;
                dict.Add(value, column[1]);                
            }
            return dict;
        }
        private void DisplayToListBox()
        {
            if (MasterFile == null || MasterFile.Count == 0)
            {
                MessageBox.Show("CSV Not Loaded"); 
                return;
            }
            
            foreach(var line in MasterFile)
            {
                RawListBox.Items.Add(line);
            }
        }
        private void DisplayToSorted(Input input)
        {
            if (input.Boxes == null) return;
            SortedListBox.Items.Clear();
            var results = new HashSet<(int,string)>();
            if (input.Boxes.Length != 2 || input.Boxes == null) return;
            if (MasterFile == null) return ;
            var IdText = input.Boxes[0].Text.Trim();
            var NameText = input.Boxes[1].Text.Trim() ?? "";
            foreach (var result in MasterFile)
            {
                if (result.Key.ToString().Contains(IdText) && result.Value.Contains(NameText, StringComparison.OrdinalIgnoreCase)) results.Add((result.Key,result.Value));                      
            }
            foreach (var item in results)
            {
                SortedListBox.Items.Add($"{item.Item1} - {item.Item2}");
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control) ClearStaffName();

            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control) ClearStaffID();

            if (e.Key == Key.Tab) tabHeld = true;
            
            if (e.Key == Key.K && tabHeld) 
            {
                SelectPopulate();
                tabHeld = false;
            }
        }
        private void ClearStaffName()
        {
            NameInputTextBox.Clear();
            NameInputTextBox.Focus();
        }
        private void ClearStaffID()
        {
            IdInputTextBox.Clear();
            IdInputTextBox.Focus();
        }
        private void SelectPopulate()
        {
            if(SortedListBox.Items.Count == 0) return;
            if(SortedListBox.SelectedItem == null) return;
            string[] selected = SortedListBox.SelectedItem.ToString().Split("-");
            if (selected.Length > 2) return;
            IdInputTextBox.Text = selected[0].Trim();
            NameInputTextBox.Text = selected[1].Trim();
        }
    }
}