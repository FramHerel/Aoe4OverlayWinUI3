using System;
using System.Collections.Generic;
using System.Text;
using Aoe4OverlayWinUI3.Core.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Aoe4OverlayWinUI3.Messages;

public record PlayerChangedMessage(Player Value);