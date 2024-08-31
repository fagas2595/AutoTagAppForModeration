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

namespace AutoTags
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileWriter fileWriter;
        private BrowserInteract browserInteract;
        public MainWindow()
        {
            fileWriter = new FileWriter("tags.txt");
            browserInteract = new BrowserInteract();

            InitializeComponent();
            RadioButtonGridBrowsers_GenerateCheckBoxes();

            ComboBoxCategories_LoadItems();
            ComboBoxCategories.SelectedIndex = 0;
        }

        private void RadioButtonGridBrowsers_GenerateCheckBoxes()
        {
            foreach (var browser in browserInteract.Browsers) {
                RadioButton radioButton = new RadioButton();
                radioButton.Content = browser;
                radioButton.Tag = browserInteract.Browsers.IndexOf(browser);
                radioButton.Margin = new Thickness(5);
                radioButton.Checked += (sender, e) => {
                    RadioButton rb = (RadioButton)sender;
                    if (rb != null) {
                        browserInteract.SelectBrowser((int)rb.Tag);
                    }
                };

                if ((int)radioButton.Tag == 0) {
                    radioButton.IsChecked = true;
                }
                
                RadioButtonGridBrowsers.Children.Add(radioButton);
            }
        }

        private void ButtonModerate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxUserLogin.Text)) {
                MessageBox.Show("Заповніть текстове поле для логіну", "Логін", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(TextBoxUserPassword.Text)) {
                MessageBox.Show("Заповніть текстове поле для паролю", "Пароль", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(TextBoxUrl.Text)) {
                MessageBox.Show("Заповніть текстове поле для Посилання", "Посилання", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isTagSelected = false;
            List<string> selectedTags = new List<string>();

            foreach (var checkBox in CheckBoxGridTags.Children.OfType<CheckBox>()) {
                if (checkBox.IsChecked == true) {
                    isTagSelected = true;
                    selectedTags.Add((string)checkBox.Content);
                }
            }

            if (!isTagSelected) {
                MessageBox.Show("Оберіть хочаб один тег", "Теги", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string login = TextBoxUserLogin.Text;
            string password = TextBoxUserPassword.Text;
            string url = TextBoxUrl.Text;

            _ = browserInteract.ModerateAsync(url, login, password, selectedTags);
        }

        private void ComboBoxCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == ComboBoxCategories) {
                CheckBoxGridTags.Children.Clear();

                string selectedCategory = (string)ComboBoxCategories.SelectedItem;
                if (string.IsNullOrEmpty(selectedCategory)) {
                    return;
                }
                List<string> tags = fileWriter.BaseTags[selectedCategory];

                foreach (var tag in tags) {
                    CheckBox checkBox = new CheckBox();
                    checkBox.Content = tag;
                    checkBox.Margin = new Thickness(5);
                    CheckBoxGridTags.Children.Add(checkBox);
                }
            } else if (sender == ComboBoxEditCategories) {
                string selectedCategory = (string)ComboBoxEditCategories.SelectedItem;
                
                if (string.IsNullOrEmpty(selectedCategory)) {
                    return;
                }
                List<string> tags = fileWriter.BaseTags[selectedCategory];
                string tagsString = string.Join(", ", tags);
                TextBoxEditTags.Text = tagsString;
            }
        }
        private void ComboBoxCategories_LoadItems()
        {
            ComboBoxCategories.Items.Clear();
            ComboBoxEditCategories.Items.Clear();

            List<string> categories = fileWriter.BaseTags.Keys.ToList();

            foreach (var category in categories) {
                ComboBoxCategories.Items.Add(category);
                ComboBoxEditCategories.Items.Add(category);
            }
            ComboBoxCategories.SelectedIndex = 0;
            ComboBoxEditCategories.SelectedIndex = 0;
        }

        private void ButtonAddTags_Click(object sender, RoutedEventArgs e)
        {
            string category = TextBoxAddCategoryName.Text;
            string tags = TextBoxTags.Text.ToLower();

            if (string.IsNullOrWhiteSpace(category)) {
                MessageBox.Show("Будь ласка, введіть назву категорії.", "Порожнє поле", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(tags)) {
                MessageBox.Show("Будь ласка, введіть тегі для цієї категорії.", "Порожнє поле", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<string> tagsList = tags.Split(", ").ToList();

            Dictionary<string, List<string>> TagsDictionary = new Dictionary<string, List<string>> {
                { category, tagsList }
            };

            fileWriter.AddTags(TagsDictionary);
            ComboBoxCategories_LoadItems();
            TextBoxAddCategoryName.Text = "";
            TextBoxTags.Text = "";
        }

        private async void ButtonTranslateTags_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxTags.Text) || TextBoxTags.Text == (string)TextBoxTags.Tag) {
                MessageBox.Show("Заповніть текстове поле для тегів", "Tags", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            string tags = TextBoxTags.Text;
            string language = RadioButtonTranslateFromRu.IsChecked == true ? "ru" : "en";

            string translatedText = await browserInteract.TranslateTextAsync(tags, language);

            HashSet<string> uniqueWords = new HashSet<string>(tags.Split(", ", StringSplitOptions.RemoveEmptyEntries));

            // Додавання перекладених слів
            foreach (var word in translatedText.Split(", ", StringSplitOptions.RemoveEmptyEntries)) {
                uniqueWords.Add(word);
            }

            // Об'єднання унікальних слів у рядок
            string result = string.Join(", ", uniqueWords);

            TextBoxTags.Text = result;
        }

        private void ButtonEditTags_Click(object sender, RoutedEventArgs e)
        {
            string tags = TextBoxEditTags.Text.ToLower();

            if (string.IsNullOrEmpty(tags)) {
                MessageBox.Show("Заповніть текстове поле для тегів", "Tags", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string category = (string)ComboBoxEditCategories.SelectedItem;
            fileWriter.EditTags(category, tags);
            ComboBoxCategories_LoadItems();
        }
    }
}