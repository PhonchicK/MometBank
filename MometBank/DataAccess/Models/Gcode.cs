using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MometBank.DataAccess.Models
{
    public class Gcode
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string Details { get; set; }
        public float FileSize { get; set; }
        public string FileSource { get; set; }
        public long? ModelId { get; set; }
        public long? FolderId { get; set; }

        public virtual Model? Model { get; set; }
        public virtual Folder? Folder { get; set; }
    }
}
