using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MometBank.DataAccess.Models
{
    public class Model
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string FileSource { get; set; }
        public DateTime CreatedTime { get; set; }
        public string OriginalFileName { get; set; }
        public float FileSize { get; set; }
        public byte[] Thumbnail { get; set; }

        public virtual ICollection<ModelTag> ModelTags { get; set; }

        [NotMapped]
        public BitmapImage ThumbnailImage
        {
            get
            {
                if (Thumbnail == null) return null;
                using var ms = new MemoryStream(Thumbnail);
                var img = new BitmapImage();
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.StreamSource = ms;
                img.EndInit();
                return img;
            }
        }
    }
}
