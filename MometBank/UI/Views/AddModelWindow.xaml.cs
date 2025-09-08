using HelixToolkit.Wpf;
using Microsoft.Win32;
using MometBank.DataAccess.Models;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace MometBank.UI.Views
{
    public partial class AddModelWindow : Window
    {
        public Model CreatedModel { get; private set; }

        private string selectedFilePath;
        private Model3DGroup currentModel;

        public AddModelWindow(string selectedFile = null)
        {
            InitializeComponent();

            if(selectedFile != null)
            {
                selectedFilePath = selectedFile;
                FileNameTextBlock.Text = System.IO.Path.GetFileName(selectedFilePath);
                var reader = new StLReader();
                try
                {
                    currentModel = reader.Read(selectedFilePath);
                    Viewport.Children.Clear();
                    Viewport.Children.Add(new SunLight());
                    Viewport.Children.Add(new ModelVisual3D { Content = currentModel });
                    Viewport.ZoomExtents();
                    ModelNameTextBox.Text = Path.GetFileNameWithoutExtension(selectedFilePath);
                    this.CreatedModel = new Model();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Model yüklenirken hata: " + ex.Message);
                }
            }
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "3D Model Files (*.stl)|*.stl"
            };

            if (dlg.ShowDialog() == true)
            {
                selectedFilePath = dlg.FileName;
                FileNameTextBlock.Text = System.IO.Path.GetFileName(selectedFilePath);

                var reader = new StLReader();
                try
                {
                    currentModel = reader.Read(selectedFilePath);
                    Viewport.Children.Clear();
                    Viewport.Children.Add(new SunLight());
                    Viewport.Children.Add(new ModelVisual3D { Content = currentModel });
                    Viewport.ZoomExtents();

                    ModelNameTextBox.Text = System.IO.Path.GetFileNameWithoutExtension(selectedFilePath);
                    this.CreatedModel = new Model();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Model yüklenirken hata: " + ex.Message);
                }
            }
        }

        private void SaveModel_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedFilePath) || string.IsNullOrWhiteSpace(ModelNameTextBox.Text))
            {
                MessageBox.Show("Lütfen model adı girin ve dosya seçin.");
                return;
            }

            var bmp = new RenderTargetBitmap((int)Viewport.ActualWidth, (int)Viewport.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            bmp.Render(Viewport);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                CreatedModel.Thumbnail = stream.ToArray();
            }

            var modelsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models");
            if (!Directory.Exists(modelsDirectory))
                Directory.CreateDirectory(modelsDirectory);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(selectedFilePath);
            var destPath = Path.Combine(modelsDirectory, fileName);
            File.Copy(selectedFilePath, destPath);

            CreatedModel ??= new Model();
            CreatedModel.Name = ModelNameTextBox.Text;
            CreatedModel.OriginalFileName = Path.GetFileName(selectedFilePath);
            CreatedModel.FileSource = destPath;
            CreatedModel.CreatedTime = DateTime.Now;
            CreatedModel.FileSize = new FileInfo(destPath).Length / 1024f; // KB

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
