using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlusPlayer.Utility
{
    public class MediaData
    {
        public Uri MediaUri;
        public TimeSpan Duration = TimeSpan.Zero;
        public bool Success;
        public bool Failure;
        public bool Done { get { return (Success || Failure); } }
    }
}
