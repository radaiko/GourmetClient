using System;
using System.Collections.Generic;

namespace GourmetClient.ViewModels;

public class GourmetMenuDayViewModel
{
    public GourmetMenuDayViewModel(DateTime day, IReadOnlyList<GourmetMenuViewModel?> menuViewModels)
    {
        Date = day;
        Menus = menuViewModels;
    }

    public DateTime Date { get; }

    public IReadOnlyList<GourmetMenuViewModel?> Menus { get; }
}