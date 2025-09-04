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
using System.Windows.Input;

namespace MometBank.UI.Views
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly BankContext _context = new BankContext();
        private int _currentPage = 1;
        private int _totalPages = 1;
        private const int PageSize = 12;

        public ObservableCollection<Folder> PagedFolders { get; set; } = new();

        private int _currentFolderPage = 1;
        private int _totalFolderPages = 1;
        private const int FolderPageSize = 8;

        private Folder _currentFolder;
        public string CurrentFolderName => _currentFolder?.Name ?? "Kök";

        public string FolderPageInfo => $"Klasör Sayfa {_currentFolderPage} / {_totalFolderPages}";

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
            _ = LoadItemsAsync();
        }

        private async Task LoadModelsAsync()
        {
            long? currentFolderId = _currentFolder?.Id ?? null;
            var query = _context.Models
                .Include(m => m.ModelTags)
                    .ThenInclude(mt => mt.Tag)
                    .Where(m => m.FolderId == currentFolderId)
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
                model.FolderId = _currentFolder?.Id ?? null;
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

        private async Task LoadItemsAsync()
        {
            var folderQuery = _context.Folders
                .AsQueryable();
            var totalFolders = await folderQuery.CountAsync();
            _totalFolderPages = (int)Math.Ceiling(totalFolders / (double)FolderPageSize);
            if (_totalFolderPages == 0) _totalFolderPages = 1;
            if (_currentFolderPage > _totalFolderPages)
                _currentFolderPage = _totalFolderPages;
            var pagedFolders = await folderQuery
                .OrderBy(f => f.Name)
                .Skip((_currentFolderPage - 1) * FolderPageSize)
                .Take(FolderPageSize)
                .ToListAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                PagedFolders.Clear();
                foreach (var folder in pagedFolders)
                {
                    PagedFolders.Add(folder);
                }
                OnPropertyChanged(nameof(PagedFolders));
                OnPropertyChanged(nameof(CurrentFolderName));
                OnPropertyChanged(nameof(FolderPageInfo));
            });
            await LoadModelsAsync();
        }


        private async void NextFolderPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentFolderPage < _totalFolderPages)
            {
                _currentFolderPage++;
                await LoadItemsAsync();
            }
        }

        private async void PreviousFolderPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentFolderPage > 1)
            {
                _currentFolderPage--;
                await LoadItemsAsync();
            }
        }

        private async void FolderItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Folder folder)
            {
                _currentFolder = folder;
                OnPropertyChanged(nameof(CurrentFolderName));
                _currentFolderPage = 1;
                await LoadModelsAsync();
            }
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _currentFolder = null;
            OnPropertyChanged(nameof(CurrentFolderName));
            await LoadModelsAsync();
        }

        private async void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddFolderWindow(_context);
            if (window.ShowDialog() == true)
            {
                await LoadItemsAsync();
            }
        }
        
        private async void TagsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new EditTagsWindow().ShowDialog();
        }

        // === INotifyPropertyChanged ===
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
