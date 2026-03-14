using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Aoe4OverlayWinUI3.ViewModels;

public partial class OverlayViewModel : ObservableRecipient
{
    // TODO: 添加游戏状态
    public string MatchStatus { get; set; } = "Waiting for game...";
}
