using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MometBank.DataAccess.Models
{
    public class ModelTag
    {
        public long ModelId { get; set; }
        public long TagId { get; set; }

        public virtual Model Model { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
