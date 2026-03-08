using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aoe4OverlayWinUI.Models
{
    public class GameRecord
    {
        public string? Team1 { get; set; }
        public string? Team2 { get; set; }
        public string? Map { get; set; }
        public string? StartedTime { get; set; }
        public string? Mode { get; set; }
        public string? Result { get; set; }
        public string? DeltaRating { get; set; }
    }
}
