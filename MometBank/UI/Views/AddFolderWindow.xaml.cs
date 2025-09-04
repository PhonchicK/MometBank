using MometBank.DataAccess;
using MometBank.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MometBank.UI.Views
{
    /// <summary>
    /// Interaction logic for AddFolderWindow.xaml
    /// </summary>
    public partial class AddFolderWindow : Window
    {
        private readonly BankContext _context;

        public Folder CreatedFolder { get; private set; }

        public AddFolderWindow(BankContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FolderNameTextBox.Text))
            {
                MessageBox.Show("Klasör adı boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var folder = new Folder
            {
                Name = FolderNameTextBox.Text,
                Details = FolderDetailsTextBox.Text,
                CreatedAt = DateTime.Now
            };

            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();

            CreatedFolder = folder;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
