using numeryseryjneWPF.Services;
using numeryseryjneWPF.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace SerialTrackWPF
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
            ApplyFilters();
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

                    }); ;
                }
            }
            catch (Exception ex)
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
                var confirm = MessageBox.Show("Usunąć numer seryjny?", "Potwierdzenie", MessageBoxButton.YesNo);
                if (confirm == MessageBoxResult.Yes)
                {
                    Debug.WriteLine(item.SerialNumber);
                    var result = await _apiService.DeleteSerialNumberAsync(item.SerialNumber);
                    if (result)
                    {
                        MessageBox.Show("Usunięto pomyślnie.");
                    }
                    else
                    {
                        MessageBox.Show("Błąd przy usuwaniu.");
                    }
                }
            }
        }

        private async void GenerateSerialNumber_Click(object sender, RoutedEventArgs e)
        {
            string product = ProductNameTextBox.Text;

            if (string.IsNullOrWhiteSpace(product))
            {
                MessageBox.Show("Wprowadź nazwę produktu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SerialNumber serialNumber = await _apiService.GenerateSerialNumberAsync(product);
            SerialItem serialItem = ConvertToSerialItem(serialNumber);

        }

        private void ToggleFiltersVisibility(object sender, RoutedEventArgs e)
        {
            if (FiltersPanel.Visibility == Visibility.Collapsed)
            {
                FiltersPanel.Visibility = Visibility.Visible;
                (sender as Button).Content = "Ukryj filtry";
                SerialDataGrid.Margin = new Thickness(00, 200, 0, 0);
            }
            else
            {
                FiltersPanel.Visibility = Visibility.Collapsed;
                (sender as Button).Content = "Pokaż filtry";
                SerialDataGrid.Margin = new Thickness(00, 38, 0, 0);

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

        private SerialItem ConvertToSerialItem(SerialNumber sn)
        {
            if (sn == null) return null;

            return new SerialItem
            {
                id = sn.id,
                SerialNumber = sn.number,
                ProductName = sn.name,
                DateGenerated = sn.CreatedAt
            };
        }

    }


    public class SerialItem
    {
        public int id { get; set; }
        public string SerialNumber { get; set; }
        public string ProductName { get; set; }
        public DateTimeOffset DateGenerated { get; set; }
    }
}
