using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace AutoTags
{
    public class FileWriter
    {
        private readonly string filePath;
        public string FilePath => filePath;

        private Dictionary<string, List<string>> baseTags;
        public Dictionary<string, List<string>> BaseTags => baseTags;

        public FileWriter(string filePath)
        {
            this.filePath = filePath;
            if (!File.Exists(filePath)) {
                File.Create(filePath).Close(); 
            }
            baseTags = ReadFromFile();
        }
        public void AddTags(Dictionary<string, List<string>> dictionary)
        {
            if (baseTags.ContainsKey(dictionary.Keys.First())) {
                MessageBox.Show("Дана категорія вже існує.", "Категорія вже існує", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            baseTags.Add(dictionary.Keys.First(), dictionary.Values.First());

            WriteToFile();
            MessageBox.Show("Категорію успішно збережено в файл: " + FilePath, "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void EditTags(string category, string tags)
        {
            if (!baseTags.ContainsKey(category)) {
                MessageBox.Show("Дана категорія не існує.", "Категорія не існує", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            baseTags[category] = new List<string>(tags.Split(", "));

            WriteToFile();
            MessageBox.Show("Теги успішно відредаговано.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void WriteToFile()
        {
            using (StreamWriter writer = new StreamWriter(filePath)) {
                foreach (var kvp in baseTags) {
                    writer.WriteLine(kvp.Key); // Записуємо ключ
                    foreach (var value in kvp.Value) {
                        writer.WriteLine(value); // Записуємо значення
                    }
                    writer.WriteLine(); // Розділяємо записи порожнім рядком
                }
            }
        }

        // Зчитування даних з файлу
        private Dictionary<string, List<string>> ReadFromFile()
        {
            var dictionary = new Dictionary<string, List<string>>();

            using (StreamReader reader = new StreamReader(filePath)) {
                string line;
                string currentKey = null;
                var currentList = new List<string>();

                while ((line = reader.ReadLine()) != null) {
                    if (string.IsNullOrEmpty(line)) {
                        // Якщо порожній рядок, це означає, що закінчилися дані для поточного ключа
                        if (currentKey != null) {
                            dictionary[currentKey] = new List<string>(currentList);
                            currentList.Clear();
                            currentKey = null;
                        }
                    } else {
                        if (currentKey == null) {
                            // Перше непорожнє рядок - це новий ключ
                            currentKey = line;
                        } else {
                            // Інші непорожні рядки - це значення для поточного ключа
                            currentList.Add(line);
                        }
                    }
                }

                // Додаємо останній ключ і значення, якщо є
                if (currentKey != null) {
                    dictionary[currentKey] = new List<string>(currentList);
                }
            }

            return dictionary;
        }

    }
}
