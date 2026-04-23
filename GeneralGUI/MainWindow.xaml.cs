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
using System.Xml.Linq;
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
        public static readonly string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MalinStaffNamesV3.csv");
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MasterFile = ReadFromCsv(path);
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
            bool opened = false;
            foreach (var line in MasterFile)
            {
                if (!opened && line.Key == 77 && string.IsNullOrWhiteSpace(line.Value))
                {
                    opened = true;
                    OpenAdminGUI(input: new Input { Boxes = [IdInputTextBox, NameInputTextBox] });
                }
                ;
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
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.SystemKey == Key.A && Keyboard.Modifiers == ModifierKeys.Alt)
            {
                OpenAdminGUI(input: new Input { Boxes = [IdInputTextBox, NameInputTextBox] });
                e.Handled = true; 
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
        private void OpenAdminGUI(Input input)
        {
            if (input.Boxes == null) return;
            string[] messages = [input.Boxes[0].Text, input.Boxes[1].Text];
            AdminWindow admin = new(messages);
            admin.Owner = this;
            admin.ShowDialog();
        }


    }
    public partial class AdminWindow : Window
    {
        private string[] _message;
        private static Random random = new();
        public AdminWindow(string[] messages)
        {
            InitializeComponent();
            _message = messages;
            PopulateBoxes();
        }
        private void PopulateBoxes()  
        {
            IdInputTextBox.Text = _message[0];
            NameInputTextBox.Text = _message[1];
        }
        private void CreateNewID(Input input)
        {
            if (MainWindow.MasterFile == null) return;
            if (input?.Boxes == null || input.Boxes.Length != 2) return;

            var name = input.Boxes[1].Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Name cannot be empty");
                return;
            }
            
            int value = 0;
            do
            {
                value = random.Next(770000000, 779999999);
            }
            while (MainWindow.MasterFile.ContainsKey(value));
            input.Boxes[0].Text = value.ToString();
            MainWindow.MasterFile[value] = name;
            MessageBox.Show($"{value} - {name} added to dict");
        }
        private void UpdateID(Input input)
        {
            if (MainWindow.MasterFile == null) return;
            if (input?.Boxes == null || input.Boxes.Length != 2) return;
            var idText = input.Boxes[0].Text;
            var name = input.Boxes[1].Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Name cannot be empty");
                return;
            }
            if (!int.TryParse(idText, out int id))
            {
                MessageBox.Show("Invalid ID");
                return;
            }
            if (!MainWindow.MasterFile.ContainsKey(id))
            {
                MessageBox.Show("ID not found");
                return;
            }
                MainWindow.MasterFile[id] = name;
                MessageBox.Show($"{id} - {name} updated on to dict");
        }
        private void DeleteID(Input input)
        {
            if (MainWindow.MasterFile == null) return;
            if (input?.Boxes == null || input.Boxes.Length != 2) return;
            var idText = input.Boxes[0].Text;
            if (!int.TryParse(idText, out int id))
            {
                MessageBox.Show("Invalid ID");
                return;
            }
            if (!MainWindow.MasterFile.ContainsKey(id))
            {
                MessageBox.Show("ID not found");
                return;
            }
            MainWindow.MasterFile.Remove(id);
            MessageBox.Show($"{id} removed from dict");
        }
        private void ChangeBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateID(input: new Input { Boxes = [IdInputTextBox, NameInputTextBox] });
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            DeleteID(input: new Input { Boxes = [IdInputTextBox, NameInputTextBox] });
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            CreateNewID(input: new Input { Boxes = [IdInputTextBox, NameInputTextBox] });
        }
        private void Admin_Closed(object sender, EventArgs e)
        {
            if (MainWindow.MasterFile == null || MainWindow.MasterFile.Count == 0)
                return;

            try
            {
                var lines = MainWindow.MasterFile
                    .Select(kvp => $"{kvp.Key},{kvp.Value}");

                File.WriteAllLines(MainWindow.path, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save data: {ex.Message}");
            }
        }
        private void AdminWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.L && Keyboard.Modifiers == ModifierKeys.Alt)
            {
                this.Close();
            }
        }
    }
}