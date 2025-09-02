using Microsoft.EntityFrameworkCore;
using MometBank.DataAccess;
using MometBank.DataAccess.Models;
using MometBank.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MometBank.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
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
                    _currentPage = 1; // Yeni aramada sayfa sıfırla
                    OnPropertyChanged();
                    LoadModels();
                }
            }
        }

        public ICommand FilterCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

        public string PageInfo => $"Sayfa {_currentPage} / {_totalPages}";

        public bool CanGoNext => _currentPage < _totalPages;
        public bool CanGoPrevious => _currentPage > 1;

        public MainViewModel()
        {
            FilterCommand = new RelayCommand(() => { _currentPage = 1; LoadModels(); });
            NewCommand = new RelayCommand(() => { OpenAddModelWindow_Click(); });
            NextPageCommand = new RelayCommand(NextPage, () => CanGoNext);
            PreviousPageCommand = new RelayCommand(PreviousPage, () => CanGoPrevious);

            LoadModels();
        }

        private void OpenAddModelWindow_Click()
        {
            var window = new Views.AddModelWindow();
            if (window.ShowDialog() == true)
            {
                var model = window.CreatedModel;

                if (model != null)
                {
                    _context.Models.Add(model);
                    _context.SaveChanges();

                    LoadModels(); // Listeyi güncelle
                }
            }
        }

        private void LoadModels()
        {
            var query = _context.Models
                .Include(m => m.ModelTags)
                    .ThenInclude(mt => mt.Tag)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(TagSearchText))
            {
                query = query.Where(m => m.ModelTags.Any(mt => mt.Tag.Name.Contains(TagSearchText)));
            }

            var totalItems = query.Count();

            _totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            if (_totalPages == 0) _totalPages = 1;

            if (_currentPage > _totalPages)
                _currentPage = _totalPages;

            var paged = query
                .OrderByDescending(m => m.CreatedTime)
                .Skip((_currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            PagedModels.Clear();
            foreach (var model in paged)
            {
                PagedModels.Add(model);
            }

            OnPropertyChanged(nameof(PagedModels));
            OnPropertyChanged(nameof(PageInfo));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));

            // Komutların CanExecute durumunu bildir
            (NextPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PreviousPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void NextPage()
        {
            if (CanGoNext)
            {
                _currentPage++;
                LoadModels();
            }
        }

        private void PreviousPage()
        {
            if (CanGoPrevious)
            {
                _currentPage--;
                LoadModels();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
