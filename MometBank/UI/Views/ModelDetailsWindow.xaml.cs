using MometBank.DataAccess;
using MometBank.DataAccess.Models;
using System.Windows;

namespace MometBank.UI.Views
{
    public partial class ModelDetailsWindow : Window
    {
        private readonly BankContext _context;
        public Model Model { get; set; }

        public ModelDetailsWindow(Model model, BankContext context)
        {
            InitializeComponent();
            _context = context;
            Model = model;
            DataContext = Model;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _context.Update(Model);
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
    }
}
