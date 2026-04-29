using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityBuilder.Models
{
    public class UpdateProgress
    {
        public int PercentComplete { get; set; } = -1;
        public string Status { get; set; } = "";
        public string Message { get; set; } = "";
        public bool IsIndeterminate { get; set; } = false;
        public long BytesReceived { get; set; }
        public long TotalBytes { get; set; }
    }
}
