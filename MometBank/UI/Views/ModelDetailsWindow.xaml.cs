using Microsoft.EntityFrameworkCore;
using MometBank.DataAccess;
using MometBank.DataAccess.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace MometBank.UI.Views
{
    public class TagSelection
    {
        public Tag Tag { get; set; }
        public bool IsSelected { get; set; }
    }

    public partial class ModelDetailsWindow : Window
    {
        private readonly BankContext _context;
        public Model Model { get; set; }

        private int _currentPage = 1;
        private const int PageSize = 8;
        private int _totalPages = 1;

        private List<TagSelection> _allTagSelections;
        public ObservableCollection<TagSelection> PagedTags { get; set; }

        public string PageInfo => $"Sayfa {_currentPage} / {_totalPages}";

        public ModelDetailsWindow(Model model, BankContext context)
        {
            InitializeComponent();
            _context = context;
            PagedTags = new ObservableCollection<TagSelection>();

            // Model + Taglar yükle
            Model = _context.Models
                .Include(m => m.ModelTags)
                .ThenInclude(mt => mt.Tag)
                .First(m => m.Id == model.Id);

            LoadTags();
            DataContext = this;
        }

        private void LoadTags()
        {
            var allTags = _context.Tags.AsNoTracking().OrderBy(t => t.Name).ToList();

            var modelTagIds = Model.ModelTags.Select(mt => mt.TagId).ToHashSet();

            // önce modele ekli olan taglar (seçili)
            var selected = allTags
                .Where(t => modelTagIds.Contains(t.Id))
                .Select(t => new TagSelection { Tag = t, IsSelected = true });

            // sonra ekli olmayan taglar
            var unselected = allTags
                .Where(t => !modelTagIds.Contains(t.Id))
                .Select(t => new TagSelection { Tag = t, IsSelected = false });

            _allTagSelections = selected.Concat(unselected).ToList();

            _totalPages = (_allTagSelections.Count + PageSize - 1) / PageSize;
            if (_totalPages == 0) _totalPages = 1;

            RefreshPagedTags();
        }

        private void RefreshPagedTags()
        {
            PagedTags.Clear();
            var tags = _allTagSelections.Skip((_currentPage - 1) * PageSize).Take(PageSize).ToList();
            foreach (var tag in tags)
                PagedTags.Add(tag);

            DataContext = null;
            DataContext = this;
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // güncel seçimleri uygula
                var selectedTags = _allTagSelections.Where(t => t.IsSelected).Select(t => t.Tag.Id).ToList();

                // mevcut tagleri sil
                var toRemove = Model.ModelTags.Where(mt => !selectedTags.Contains(mt.TagId)).ToList();
                foreach (var item in toRemove)
                    _context.ModelTags.Remove(item);

                // yeni tagleri ekle
                var currentTagIds = Model.ModelTags.Select(mt => mt.TagId).ToHashSet();
                var toAdd = selectedTags.Where(id => !currentTagIds.Contains(id)).ToList();
                foreach (var id in toAdd)
                    _context.ModelTags.Add(new ModelTag { ModelId = Model.Id, TagId = id });

                _context.SaveChanges();

                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Kaydetme hatası: " + ex.Message);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var file in files)
                {
                    if (Path.GetExtension(file).Equals(".gcode", StringComparison.OrdinalIgnoreCase))
                    {
                        await AddGcodeAsync(file);
                    }
                }
            }
        }

        private async Task AddGcodeAsync(string filePath)
        {
            try
            {
                var window = new AddGcodeWindow(filePath)
                {
                    Owner = this
                };
                if (window.ShowDialog() == true)
                {
                    var gcode = window.CreatedGcode;

                    // FolderId dışarıdan atanıyor
                    gcode.ModelId = Model.Id;

                    _context.Gcodes.Add(gcode);
                    await _context.SaveChangesAsync();
                    // UI güncelle
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Model.Gcodes.Add(gcode);
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gcode eklenirken hata: " + ex.Message);
            }
        }
    }
}
