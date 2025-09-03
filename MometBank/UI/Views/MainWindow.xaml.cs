using Microsoft.EntityFrameworkCore;
using MometBank.DataAccess;
using MometBank.DataAccess.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MometBank.UI.Views
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly BankContext _context = new BankContext();
        private int _currentPage = 1;
        private int _totalPages = 1;
        private const int PageSize = 12;

        public ObservableCollection<Model> PagedModels { get; set; } = new();

        private string _tagSearchText;
        public string TagSearchText
        {
            get => _tagSearchText;
            set
            {
                if (_tagSearchText != value)
                {
                    _tagSearchText = value;
                    _currentPage = 1;
                    OnPropertyChanged();
                    _ = LoadModelsAsync();
                }
            }
        }

        private string _selectedSearchMode = "Genel";
        public string SelectedSearchMode
        {
            get => _selectedSearchMode;
            set
            {
                if (_selectedSearchMode != value)
                {
                    _selectedSearchMode = value;
                    OnPropertyChanged();
                    _currentPage = 1;
                    _ = LoadModelsAsync();
                }
            }
        }

        private string _selectedGcodeFilter = "Tümü";
        public string SelectedGcodeFilter
        {
            get => _selectedGcodeFilter;
            set
            {
                if (_selectedGcodeFilter != value)
                {
                    _selectedGcodeFilter = value;
                    _currentPage = 1;
                    OnPropertyChanged();
                    _ = LoadModelsAsync();
                }
            }
        }

        public string PageInfo => $"Sayfa {_currentPage} / {_totalPages}";

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _ = LoadModelsAsync();
        }

        private async Task LoadModelsAsync()
        {
            var query = _context.Models
                .Include(m => m.ModelTags)
                    .ThenInclude(mt => mt.Tag)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(TagSearchText))
            {
                switch (SelectedSearchMode)
                {
                    case "Genel":
                        query = query.Where(m =>
                            m.Name.Contains(TagSearchText) ||
                            m.Details.Contains(TagSearchText) ||
                            m.ModelTags.Any(mt => mt.Tag.Name.Contains(TagSearchText)));
                        break;

                    case "İsim":
                        query = query.Where(m => m.Name.Contains(TagSearchText));
                        break;

                    case "Açıklama":
                        query = query.Where(m => m.Details.Contains(TagSearchText));
                        break;

                    case "Tag":
                        query = query.Where(m => m.ModelTags.Any(mt => mt.Tag.Name.Contains(TagSearchText)));
                        break;
                }
            }
            switch (SelectedGcodeFilter)
            {
                case "Gcode Var":
                    query = query.Include(m => m.Gcodes).Where(m => m.Gcodes.Any());
                    break;

                case "Gcode Yok":
                    query = query.Include(m => m.Gcodes).Where(m => !m.Gcodes.Any());
                    break;
            }

            var totalItems = await query.CountAsync();
            _totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            if (_totalPages == 0) _totalPages = 1;

            if (_currentPage > _totalPages)
                _currentPage = _totalPages;

            var paged = await query
                .OrderByDescending(m => m.CreatedTime)
                .Skip((_currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                PagedModels.Clear();
                foreach (var model in paged)
                {
                    PagedModels.Add(model);
                }

                OnPropertyChanged(nameof(PagedModels));
                OnPropertyChanged(nameof(PageInfo));
            });
        }

        // === Buton Eventleri ===
        private async void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = 1;
            await LoadModelsAsync();
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddModelWindow();
            if (window.ShowDialog() == true)
            {
                var model = window.CreatedModel;
                if (model != null)
                {
                    _context.Models.Add(model);
                    _context.SaveChanges();
                    _ = LoadModelsAsync();
                }
            }
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadModelsAsync();
            }
        }

        private async void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadModelsAsync();
            }
        }

        private async void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Hangi model seçilmiş bul
            var button = sender as Button;
            if (button?.DataContext is Model selectedModel)
            {
                // 2. Detay penceresini aç
                var detailsWindow = new ModelDetailsWindow(selectedModel, _context);
                if (detailsWindow.ShowDialog() == true)
                {
                    // Kaydedildiyse listeyi yenile
                    await LoadModelsAsync();
                }
            }
        }


        // === INotifyPropertyChanged ===
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
