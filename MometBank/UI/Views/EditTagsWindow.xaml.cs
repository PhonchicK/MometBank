using Microsoft.EntityFrameworkCore;
using MometBank.DataAccess;
using MometBank.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace MometBank.UI.Views
{
    public partial class EditTagsWindow : Window, INotifyPropertyChanged
    {
        private readonly BankContext _context;
        private List<Tag> _allTags;

        public ObservableCollection<Tag> PagedTags { get; set; }

        private Tag _selectedTag;
        public Tag SelectedTag
        {
            get => _selectedTag;
            set
            {
                _selectedTag = value;
                EditableTagName = _selectedTag?.Name ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTagSelected));
            }
        }

        private string _editableTagName;
        public string EditableTagName
        {
            get => _editableTagName;
            set { _editableTagName = value; OnPropertyChanged(); }
        }

        public bool IsTagSelected => SelectedTag != null;

        private int _currentPage = 1;
        private const int PageSize = 15;
        private int _totalPages = 1;

        public string PageInfo => $"Sayfa {_currentPage} / {_totalPages}";

        public EditTagsWindow()
        {
            InitializeComponent();
            _context = new BankContext();
            PagedTags = new ObservableCollection<Tag>();
            DataContext = this;

            _ = LoadTagsAsync();
        }

        private async Task LoadTagsAsync()
        {
            _allTags = await _context.Tags.AsNoTracking().OrderBy(t => t.Name).ToListAsync();
            _totalPages = (_allTags.Count + PageSize - 1) / PageSize;
            if (_totalPages == 0) _totalPages = 1;

            RefreshPagedTags();
        }

        private void RefreshPagedTags()
        {
            PagedTags.Clear();
            var tags = _allTags.Skip((_currentPage - 1) * PageSize).Take(PageSize).ToList();
            foreach (var tag in tags)
                PagedTags.Add(tag);

            OnPropertyChanged(nameof(PageInfo));
        }

        private async void SaveOrAddTag_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EditableTagName)) return;

            if (SelectedTag != null)
            {
                // Düzenleme
                SelectedTag.Name = EditableTagName;
                _context.Tags.Update(SelectedTag);
            }
            else
            {
                // Yeni ekleme
                var newTag = new Tag { Name = EditableTagName };
                await _context.Tags.AddAsync(newTag);
            }

            await _context.SaveChangesAsync();
            EditableTagName = string.Empty;
            SelectedTag = null;
            await LoadTagsAsync();
        }

        private async void DeleteTag_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTag == null) return;

            _context.Tags.Remove(SelectedTag);
            await _context.SaveChangesAsync();
            EditableTagName = string.Empty;
            SelectedTag = null;
            await LoadTagsAsync();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                RefreshPagedTags();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                RefreshPagedTags();
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}
