using MometBank.DataAccess;
using MometBank.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace MometBank.UI.Views
{
    public partial class GcodeDetailsWindow : Window
    {
        private readonly BankContext _context;
        public Gcode Gcode { get; set; }

        public GcodeDetailsWindow(Gcode gcode, BankContext context)
        {
            InitializeComponent();
            _context = context;

            // Asenkron yükleme (UI donmasın diye)
            _ = LoadGcodeAsync(gcode.Id);
        }

        private async Task LoadGcodeAsync(long id)
        {
            Gcode = await _context.Gcodes.FirstAsync(g => g.Id == id);
            DataContext = this;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _context.SaveChangesAsync();
                MessageBox.Show("Gcode başarıyla kaydedildi.", "Bilgi",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kaydetme hatası: " + ex.Message);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bu Gcode'u silmek istediğinize emin misiniz?", "Onay",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Gcodes.Remove(Gcode);
                    await _context.SaveChangesAsync();
                    MessageBox.Show("Gcode silindi.", "Bilgi",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Silme hatası: " + ex.Message);
                }
            }
        }

        private async void OpenDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Gcode.FileSource) && File.Exists(Gcode.FileSource))
                {
                    await Task.Run(() =>
                    {
                        Process.Start(new ProcessStartInfo(Gcode.FileSource)
                        {
                            UseShellExecute = true
                        });
                    });
                }
                else
                {
                    MessageBox.Show("Dosya bulunamadı: " + Gcode.FileSource);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dosya açma hatası: " + ex.Message);
            }
        }
    }
}
