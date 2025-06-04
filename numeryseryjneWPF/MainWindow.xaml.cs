using numeryseryjneWPF.Services;
using numeryseryjneWPF.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace SerialTrack
{
    public partial class MainWindow : Window
    {
        private ApiService _apiService;
        public ObservableCollection<SerialItem> SerialItems { get; set; }
        public ObservableCollection<SerialItem> FilteredSerialItems { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            SerialItems = new ObservableCollection<SerialItem>();
            FilteredSerialItems = new ObservableCollection<SerialItem>();
            SerialDataGrid.ItemsSource = SerialItems;
            InitializeServerDiscovery();
        }

        private async void InitializeServerDiscovery()
        {
            try
            {
                Debug.WriteLine("test");
                var discoveryService = new ServerDiscoveryService();
                Debug.WriteLine("Before calling discovery");
                string? serverUrl = await discoveryService.DiscoverServerAsync();
                Debug.WriteLine("After");

                if (serverUrl != null)
                {
                    _apiService = new ApiService(serverUrl);
                    FetchSerialNumbers();
                }
                else
                {
                    Debug.WriteLine("Server was null");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in InitializeServerDiscovery: {ex}");
            }
        }

        private async void FetchSerialNumbers()
        {
            try
            {
                var serialNumbers = await _apiService.GetSerialsAsync();
                SerialItems.Clear();
                foreach (var serial in serialNumbers)
                {
                    Debug.WriteLine(serial.CreatedAt);
                    SerialItems.Add(new SerialItem()
                    {
                        SerialNumber = serial.number,
                        ProductName = serial.name,
                        DateGenerated = serial.CreatedAt

                    });;
                }
            } catch (Exception ex)
            {
                MessageBox.Show($"Error podczas zbierania numerow seryjnych: {ex.Message}");
            }
            SerialDataGrid.ItemsSource = FilteredSerialItems;
        }

        private async void DeleteSerial_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Kliknięto usuń");
            if (sender is Button btn && btn.Tag is SerialItem item)
            {
                Debug.WriteLine($"Tag: {btn.Tag}");
                if (btn.Tag is SerialItem item)
                {
                    Debug.WriteLine($"Usuwanie: {item.SerialNumber}");
                }
                else
                {
                    Debug.WriteLine("Tag nie jest typu SerialItem!");
                }
                var confirm = MessageBox.Show("Usunąć numer seryjny?", "Potwierdzenie", MessageBoxButton.YesNo);
                if (confirm == MessageBoxResult.Yes)
                {
                    Debug.WriteLine(item.SerialNumber);
                    var result = await _apiService.DeleteSerialNumberAsync(item.SerialNumber);
                    if (result)
                    {
                        MessageBox.Show("Usunięto pomyślnie.");
                        await LoadSerialsAsync();
                    }
                    else
                    {
                        MessageBox.Show("Błąd przy usuwaniu.");
                    }
                }
            }
        }

        private async void EditSerial_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is SerialItem item)
            {
                string newSerial = Microsoft.VisualBasic.Interaction.InputBox("Nowy numer seryjny:", "Edycja", item.SerialNumber);

                if (!string.IsNullOrWhiteSpace(newSerial) && newSerial != item.SerialNumber)
                {
                    item.SerialNumber = newSerial;
                    var result = await _apiService.UpdateSerialNumberAsync(item);
                    if (result)
                    {
                        MessageBox.Show("Zaktualizowano pomyślnie.");
                        await LoadSerialsAsync();
                    }
                    else
                    {
                        MessageBox.Show("Błąd przy edycji.");
                    }
                }
            }
        }

        private async void GenerateSerialNumber_Click(object sender, RoutedEventArgs e)
        {
            string product = ProductTextBox.Text;

            if (string.IsNullOrWhiteSpace(product))
            {
                MessageBox.Show("Wprowadź nazwę produktu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SerialItem serialItem = await _apiService.GenerateSerialNumberAsync(product);

            if (serialItem != null)
            {
                SerialNumberTextBlock.Text = $"Numer seryjny: {serialItem.SerialNumber}";
                ProductNameTextBlock.Text = $"Produkt: {serialItem.ProductName}";
                DateGeneratedTextBlock.Text = $"Data wygenerowania: {serialItem.DateGenerated.LocalDateTime.ToString("g")}";
            }
            else
            {
                MessageBox.Show("Nie udało się wygenerować numeru seryjnego.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleFiltersVisibility(object sender, RoutedEventArgs e)
        {
            if (FiltersPanel.Visibility == Visibility.Collapsed)
            {
                FiltersPanel.Visibility = Visibility.Visible;
                (sender as Button).Content = "Ukryj filtry";
            }
            else
            {
                FiltersPanel.Visibility = Visibility.Collapsed;
                (sender as Button).Content = "Pokaż filtry";
            }
        }


        private void MainWindow_MouseDown(object sender, RoutedEventArgs e)
        {
            if (SerialDataGrid.IsReadOnly == false)
            {
                SerialDataGrid.IsReadOnly = true;
            }
        }

        private void ApplyFilters()
        {
            var filtered = SerialItems.AsEnumerable();

            // Filtruj według numeru seryjnego
            if (!string.IsNullOrEmpty(SerialNumberFilterTextBox.Text))
            {
                filtered = filtered.Where(item => item.SerialNumber.Contains(SerialNumberFilterTextBox.Text));
            }

            // Filtruj według nazwy produktu
            if (!string.IsNullOrEmpty(ProductNameFilterTextBox.Text))
            {
                filtered = filtered.Where(item => item.ProductName.Contains(ProductNameFilterTextBox.Text));
            }

            // Filtruj według daty
            if (DateFilter.SelectedDate.HasValue)
            {
                filtered = filtered.Where(item => item.DateGenerated.Date == DateFilter.SelectedDate.Value.Date);
            }

            // Zaktualizuj filtrowaną kolekcję
            FilteredSerialItems.Clear();
            foreach (var item in filtered)
            {
                FilteredSerialItems.Add(item);
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeServerDiscovery();
            ApplyFilters();
        }

    }

    public class SerialItem
    {
        public string SerialNumber { get; set; }
        public string ProductName { get; set; }
        public DateTimeOffset DateGenerated { get; set; }
    }
}
