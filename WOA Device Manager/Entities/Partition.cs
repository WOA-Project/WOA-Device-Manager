using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WOADeviceManager.Entities
{
    public class Partition
    {
        public int Number { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Size { get; set; }
        public string FileSystem { get; set; }
        public string Name { get; set; }
        public string Flags { get; set; }
    }
}
