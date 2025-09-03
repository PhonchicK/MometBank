using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MometBank.DataAccess.Models
{
    public class Folder
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<FolderTag> FolderTags { get; set; }
        public virtual ICollection<Gcode> Gcodes { get; set; }
        public virtual ICollection<Model> Models { get; set; }
    }
}
