using Microsoft.Win32;
using System.IO;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chrome;
using System.Windows;

namespace AutoTags
{
    public class BrowserInteract
    {
        private string _browserName = null!;
        public string BrowserName => _browserName;

        private List<string> _browsers;
        public List<string> Browsers => _browsers;

        private IWebDriver driver = null!;

        public BrowserInteract()
        {
            _browsers = GetInstalledBrowsers();
        }
        public void SelectBrowser(int browserId)
        {
            if (_browsers.Count == 1) {
                _browserName = _browsers[0];
                return;
            }

            _browserName = _browsers[browserId];
        }

        public void OpenUrl(string url)
        {
            switch (_browserName) {
                case "Google Chrome":
                    driver = new ChromeDriver();
                    break;
                case "Mozilla Firefox":
                    driver = new FirefoxDriver();
                    break;
            }

            driver.Navigate().GoToUrl(url);
        }

        public async Task ModerateAsync(string url, string login, string password, List<string> tags)
        {
            try {
                OpenUrl(url);
                await Task.WhenAll(
                    Task.Run(() => SetValueInElement(By.Name("Login"), login)),
                    Task.Run(() => SetValueInElement(By.Name("Password"), password))
                );
                await Task.Run(() => ClickElement(By.ClassName("btn")));

                driver.Navigate().GoToUrl(url);
                await Task.Delay(Random.Shared.Next(1000, 2000));

                foreach (var tag in tags) {
                    await Task.Run(() => SetValueInElement(By.ClassName("select2-search__field"), tag));
                    await Task.Delay(Random.Shared.Next(1000, 2000));
                    await Task.Run(() => ClickElement(By.XPath($"//li[text()='{tag}']")));
                    await Task.Delay(Random.Shared.Next(500, 1000));
                    await Task.Run(() => ClickElement(By.XPath($"//li[text()='{tag}']")));
                    await Task.Delay(Random.Shared.Next(1000, 2000));
                }

                await Task.Run(() => ClickElement(By.XPath("//input[@type='button' and @value='Сохранить']")));
                await Task.Delay(Random.Shared.Next(1000, 2000));
            } catch (Exception ex) {
                MessageBox.Show("Виникла помилка: " + ex.Message, "Робота програми", MessageBoxButton.OK, MessageBoxImage.Error);
            } finally {
                await CloseAsync();
            }
        }

        public async Task<string> TranslateTextAsync(string text, string sourceLang)
        {
            string targetLang = sourceLang == "en" ? "ru" : "en";

            OpenUrl($"https://translate.google.com/?sl={sourceLang}&tl={targetLang}&op=translate");
            await SetValueInElementAsync(By.ClassName("er8xn"), text);
            await Task.Delay((int)Random.Shared.Next(1000, 2000)); // Затримка для забезпечення часу на переклад

            string translatedText = await GetValueFromElementAsync(By.ClassName("ryNqvb"));
            await CloseAsync();
            return translatedText;
        }

        private async Task SetValueInElementAsync(By type, string value)
        {
            var element = driver.FindElement(type);
            element.SendKeys(value);
            await Task.CompletedTask; // Це може бути замінено на реальний асинхронний метод, якщо Selenium надає таку можливість
        }

        private async Task<string> GetValueFromElementAsync(By type)
        {
            var element = driver.FindElement(type);
            return await Task.FromResult(element.Text); // Знову ж таки, це може бути замінено на реальний асинхронний метод
        }


        private void SetValueInElement(By type, string value)
        {
            var element = driver.FindElement(type);
            element.SendKeys(value);
        }
        private string GetValueFromElement(By type)
        {
            var element = driver.FindElement(type);
            return element.Text;
        }
        private void ClickElement(By type)
        {
            var element = driver.FindElement(type);
            element.Click();
        }

        public void Close()
        {
            driver.Quit();
        }

        public async Task CloseAsync()
        {
            await Task.Run(() => driver.Quit());
        }

        private List<string> GetInstalledBrowsers()
        {
            var browsers = new List<string>();

            // Перевірка браузерів за типовими шляхами
            CheckBrowser(browsers, "Google Chrome", @"C:\Program Files\Google\Chrome\Application\chrome.exe");
            CheckBrowser(browsers, "Mozilla Firefox", @"C:\Program Files\Mozilla Firefox\firefox.exe");

            // Перевірка в реєстрі для додаткових браузерів
            CheckBrowserInRegistry(browsers, "Google Chrome", @"SOFTWARE\Google\Chrome\BLBeacon", "path");
            CheckBrowserInRegistry(browsers, "Mozilla Firefox", @"SOFTWARE\Mozilla\Mozilla Firefox", "CurrentVersion");

            return browsers;
        }

        private void CheckBrowser(List<string> browsers, string name, string path)
        {
            if (File.Exists(path)) {
                browsers.Add(name);
            }
        }

        private void CheckBrowserInRegistry(List<string> browsers, string name, string registryKey, string valueName)
        {
            try {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey)) {
                    if (key != null) {
                        var value = key.GetValue(valueName);
                        if (value != null && File.Exists(value.ToString())) {
                            browsers.Add(name);
                        }
                    }
                }
            } catch (Exception) {
                // Обробка можливих помилок доступу до реєстру
            }
        }
    }
}
