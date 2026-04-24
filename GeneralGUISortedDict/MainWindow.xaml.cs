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
        // Path to CSV file being loaded
        public static readonly string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MalinStaffNamesV3.csv");

        // SortedDictionary Containing CSV data 
        public static SortedDictionary<int, string> MasterFile = new();

        // Flag for tab pressed
        private bool tabHeld;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Loads data to CSV
            MasterFile = ReadFromCsv(path);

            // Displays CSV data to listbox and unselects by default
            DisplayToListBox();
            SortedListBox.SelectedIndex = -1;          
        }

        // Changes content of sorted listview by searching data based upon Nametextbox
        private void NameInputTextBox_TextChanged(object sender, TextChangedEventArgs e) => DisplayToSorted(input: new Input([IdInputTextBox, NameInputTextBox]));

        // Changes content of sorted listview by searching data based upon Idtextbox
        private void IdInputTextBox_TextChanged(object sender, TextChangedEventArgs e) => DisplayToSorted(input: new Input([IdInputTextBox, NameInputTextBox]));
        private SortedDictionary<int,string> ReadFromCsv(string CsvPath)
        {
            SortedDictionary<int, string> dict = [];
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
            if (!ErrorHelper.GenericError(() => MasterFile.Count == 0, "Error: Textboxes not loaded or CSV not loaded")) return;
            bool opened = false;
            foreach (var line in MasterFile)
            {
                // Opens admin window if value with key of 77 is empty 
                if (!opened && line.Key.ToString().StartsWith("77") && string.IsNullOrWhiteSpace(line.Value))
                {
                    opened = true;
                    OpenAdminGUI(input: new Input([IdInputTextBox, NameInputTextBox]));
                };               
                RawListBox.Items.Add(line);
            }
        }
        private void DisplayToSorted(Input input)
        {
            if (!ErrorHelper.GenericError(() => input.IDBox == null || input.NameBox == null || MasterFile.Count == 0, "Error: Textboxes not loaded or CSV not loaded")) return;
            SortedListBox.Items.Clear();
            var results = new HashSet<(int,string)>();
            var IdText = input.IDBox!.Text.Trim();
            var NameText = input.NameBox!.Text.Trim();
            foreach (var result in MasterFile!)
            {
                // Affirms if found values in dict contain value from both textboxes
                if (result.Key.ToString().Contains(IdText) && result.Value.Contains(NameText, StringComparison.OrdinalIgnoreCase)) results.Add((result.Key,result.Value));                      
            }
            foreach (var item in results)
            {
                // Formats and adds items to listbox
                SortedListBox.Items.Add($"{item.Item1} - {item.Item2}");
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Clears and focuses NameTextBox
            if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control) ClearStaff(action: () => { NameInputTextBox.Clear(); NameInputTextBox.Focus(); } );

            // Clears and focuses IdTextBox
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control) ClearStaff(action: () => { IdInputTextBox.Clear(); IdInputTextBox.Focus(); });

            // Sets Flag bc wpf doesnt handle tab very well
            if (e.Key == Key.Tab) tabHeld = true;
            
            // Adds selected item of sortedlistbox to both textboxes
            if (e.Key == Key.K && tabHeld) 
            {
                SelectPopulate();
                tabHeld = false;
            }
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Opens AdminWindow
            if (e.SystemKey == Key.A && Keyboard.Modifiers == ModifierKeys.Alt)
            {
                OpenAdminGUI(input: new Input([IdInputTextBox, NameInputTextBox]));
                e.Handled = true; 
            }
        }
        // Generic Method for clearing textboxes and focusing them
        private void ClearStaff(Action action)
        {
            action();
        }
        private void SelectPopulate()
        {
            if (!ErrorHelper.GenericError(() => SortedListBox.Items.Count == 0 && SortedListBox.SelectedItem == null, "Error: Item not selected or Data not loaded")) return;

            //  Gets both values by seperating them based on - 
            string[] selected = SortedListBox.SelectedItem.ToString().Split("-");

            // Makes sure no more than 2 values are in collection
            if (selected.Length > 2) return;
            IdInputTextBox.Text = selected[0].Trim();
            NameInputTextBox.Text = selected[1].Trim();
        }
        private void OpenAdminGUI(Input input)
        {
            if (!ErrorHelper.GenericError(() => input.IDBox == null || input.NameBox == null || string.IsNullOrEmpty(input.IDBox.Text) || string.IsNullOrEmpty(input.NameBox.Text), "Error: TextBoxes not loaded or Data not entered")) return;
            string[] messages = [input.IDBox.Text, input.NameBox.Text];

            // Makes instance of window and passes in values of current windows textboxes
            AdminWindow admin = new(messages);
            admin.Owner = this;

            // Shows Admin Window 
            admin.ShowDialog();
        }

        // Makes RawListBox unselectable/readonly
        private void Selected_Raw(object sender, EventArgs e)
        {
            RawListBox.SelectedItem = null;
        }

    }
    public partial class AdminWindow : Window
    {
        private string[] _message;
        private static Random random = new();
        public AdminWindow(string[] messages)
        {
            if (messages == null || messages.Length < 2) throw new ArgumentException("Messages must contain atleast 2 values");
            if (string.IsNullOrWhiteSpace(messages[0]) || string.IsNullOrWhiteSpace(messages[1])) throw new ArgumentException("Messages cannot be empty");
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
            if (!ErrorHelper.GenericError(() =>  MainWindow.MasterFile.Count == 0 || input.IDBox == null || input.NameBox == null || string.IsNullOrEmpty(input.NameBox.Text), "Data not loaded or Text is Empty")) return;
            var name = input.NameBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Name cannot be empty");
                return;
            }            
            int value = 0;
            
            // Tries to find a value that is not a dict.key
            do
            {
                value = random.Next(770000000, 779999999);
            }
            while (MainWindow.MasterFile.ContainsKey(value));
            input.IDBox.Text = value.ToString();

            // Adds Value to dict and displays it to user
            MainWindow.MasterFile[value] = name;
            MessageBox.Show($"{value} - {name} added to dict");
        }
        private void UpdateID(Input input)
        {
            if (!ErrorHelper.GenericError(() => MainWindow.MasterFile.Count == 0 || input.IDBox == null || input.NameBox == null || string.IsNullOrEmpty(input.NameBox.Text), "Data not loaded or Text is Empty")) return;
            var idText = input.IDBox.Text;
            var name = input.NameBox.Text?.Trim();
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

            // Updates item at id that does exist in dict
            MainWindow.MasterFile[id] = name;
            MessageBox.Show($"{id} - {name} updated on to dict");
        }
        private void DeleteID(Input input)
        {
            if (!ErrorHelper.GenericError(() => MainWindow.MasterFile.Count == 0 || input.IDBox == null || input.NameBox == null || string.IsNullOrEmpty(input.NameBox.Text), "Data not loaded or Text is Empty")) return;
            var idText = input.IDBox.Text;
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
            // Removes item from dict
            MainWindow.MasterFile.Remove(id);
            MessageBox.Show($"{id} removed from dict");
        }
        private void ChangeBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateID(input: new Input ([IdInputTextBox, NameInputTextBox]));
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            DeleteID(input: new Input ([IdInputTextBox, NameInputTextBox]));
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            CreateNewID(input: new Input ([IdInputTextBox, NameInputTextBox]));
        }
        private void Admin_Closed(object sender, EventArgs e)
        {
            if (ErrorHelper.GenericError(() => MainWindow.MasterFile.Count == 0, "Data not loaded or Text is Empty")) return;
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