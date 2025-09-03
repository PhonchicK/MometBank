using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MometBank.DataAccess.Models
{
    public class FolderTag
    {
        public long FolderId { get; set; }
        public long TagId { get; set; }

        public virtual Folder Folder { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
