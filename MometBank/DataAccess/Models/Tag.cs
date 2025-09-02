using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MometBank.DataAccess.Models
{
    public class Tag
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ModelTag> ModelTags { get; set; }
    }
}
