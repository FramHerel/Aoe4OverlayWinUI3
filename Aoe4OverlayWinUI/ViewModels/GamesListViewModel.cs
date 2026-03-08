using Aoe4OverlayWinUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aoe4OverlayWinUI.ViewModels
{
    public class GamesListViewModel
    {
        public ObservableCollection<GameRecord> GameRecords { get; set; } = new ObservableCollection<GameRecord>();



    }
}
