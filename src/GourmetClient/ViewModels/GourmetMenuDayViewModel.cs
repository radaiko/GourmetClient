namespace GourmetClient.ViewModels;

using System;
using System.Collections.Generic;

public class GourmetMenuDayViewModel
{
    public GourmetMenuDayViewModel(DateTime day, IReadOnlyList<GourmetMenuMealViewModel> menuViewModels)
    {
        Date = day;
        Meals = menuViewModels;
    }

    public DateTime Date { get; }

    public IReadOnlyList<GourmetMenuMealViewModel> Meals { get; }
}