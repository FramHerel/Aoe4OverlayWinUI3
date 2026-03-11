using System;
using System.Collections.Generic;
using System.Text;
using Aoe4OverlayWinUI3.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;

namespace Aoe4OverlayWinUI3.ViewModels;

public partial class GameItemViewModel : ObservableObject
{
    //将 GameMatch 模型转换成适合在 DataGrid 中展示的属性

    private readonly GameMatch _model;
    private readonly string _currentProfileId;

    // 直接暴露一个属性来判断胜负，方便 DataGrid 的条件格式化
    public bool IsWin
    {
        get; private set;
    }

    public GameItemViewModel(GameMatch model, string currentProfileId)
    {
        _model = model;
        _currentProfileId = currentProfileId;

        // 从对局数据中找到当前玩家的信息
        var me = _model.Teams
            .SelectMany(t => t)
            .FirstOrDefault(p => p.Player.ProfileId.ToString() == _currentProfileId.ToString());

        // 根据当前玩家的信息初始化展示属性
        Initialize(me);

    }

    // 这些属性直接对应 DataGrid 的列 Binding
    public string Map => _model.Map;
    public string Kind => _model.Kind;
    public string Result
    {
        get;
        private set;
    }
    public string Civilization
    {
        get;
        private set;
    }
    public string Duration
    {
        get; private set;
    }
    public string StartTime => _model.StartedAt.ToLocalTime().ToString("g"); // 本地化时间格式

    private void Initialize(MatchPlayer me)
    {
        // 提取并预处理展示数据
        Result = me?.Result?.ToUpper() ?? "UNKNOWN";
        Civilization = me?.Civilization ?? "N/A";

        // 处理时长：秒 -> mm:ss
        if (_model.Duration.HasValue)
        {
            var t = TimeSpan.FromSeconds(_model.Duration.Value);
            Duration = $"{(int)t.TotalMinutes}:{t.Seconds:D2}";
        }
        else
        {
            Duration = "N/A";
        }

        if (me != null)
        {
            IsWin = me.Result?.ToUpper() == "WIN";
        }
        else
        {
            IsWin = false;

        }
    }
}
