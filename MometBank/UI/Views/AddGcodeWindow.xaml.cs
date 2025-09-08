using Microsoft.Win32;
using MometBank.DataAccess.Models;
using System;
using System.IO;
using System.Windows;

namespace MometBank.UI.Views
{
    public partial class AddGcodeWindow : Window
    {
        public Gcode CreatedGcode { get; private set; }
        public string SelectedFileName { get; set; }

        public string Name { get; set; }
        public string Details { get; set; }

        private string _selectedFilePath;

        public AddGcodeWindow(string filePath = null)
        {
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                _selectedFilePath = filePath;
                SelectedFileName = Path.GetFileName(_selectedFilePath);
                OnPropertyChanged(nameof(SelectedFileName));
            }
            DataContext = this;
        }

        // 📂 Dosya seçme
        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Gcode Files (*.gcode)|*.gcode",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedFilePath = dialog.FileName;
                SelectedFileName = Path.GetFileName(_selectedFilePath);
                OnPropertyChanged(nameof(SelectedFileName));
            }
        }

        // ✅ Ekle
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedFilePath))
            {
                MessageBox.Show("Lütfen bir gcode dosyası seçin.");
                return;
            }

            var fileInfo = new FileInfo(_selectedFilePath);

            CreatedGcode = new Gcode
            {
                Name = string.IsNullOrWhiteSpace(Name) ? Path.GetFileNameWithoutExtension(fileInfo.Name) : Name,
                FileName = fileInfo.Name,
                FileSize = (float)Math.Round(fileInfo.Length / 1024f, 2), // KB
                FileSource = _selectedFilePath,
                Details = Details
            };

            DialogResult = true;
            Close();
        }

        // ❌ İptal
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // --- INotifyPropertyChanged ---
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}
