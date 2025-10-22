using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GC.Models;

public partial class Day : ObservableObject {
    [ObservableProperty] private DateTime _date;
    [ObservableProperty] private Menu _menu1;
    [ObservableProperty] private Menu _menu2;
    [ObservableProperty] private Menu _menu3;
    [ObservableProperty] private Menu _soupAndSalad;

    public Day(DateTime date, Menu menu1, Menu menu2, Menu menu3, Menu soupAndSalad) {
        _date = date;
        _menu1 = menu1;
        _menu2 = menu2;
        _menu3 = menu3;
        _soupAndSalad = soupAndSalad;
    }
    
    // Helper property so Day can be used directly as the DataContext for DayCard
    public string Header => Date.ToString("ddd, MMM d", CultureInfo.CurrentCulture);

    public List<Menu> Menus => new List<Menu> { Menu1, Menu2, Menu3, SoupAndSalad };
    
    // TODO: add ordering / canceling implementation with a task to execute, only one menu is orderable
    
    public static ObservableCollection<Day> GetDummyData() => [
        new(
            DateTime.Now,
            new Menu(MenuType.Menu1, "Grilled Chicken with Vegetables", new[] { 'A', 'C' }, 8.50m, DateTime.Now),
            new Menu(MenuType.Menu2, "Spaghetti Bolognese", new[] { 'A', 'G' }, 7.00m, DateTime.Now),
            new Menu(MenuType.Menu3, "Vegetarian Stir Fry", new[] { 'A', 'C', 'G' }, 7.50m, DateTime.Now),
            new Menu(MenuType.SoupAndSalad, "Tomato Soup and Caesar Salad", new[] { 'A', 'D', 'G' }, 6.00m, DateTime.Now)
        ),

        new(
            DateTime.Now.AddDays(1),
            new Menu(MenuType.Menu1, "Beef Tacos", new[] { 'A' }, 8.00m, DateTime.Now.AddDays(1)),
            new Menu(MenuType.Menu2, "Penne Alfredo", new[] { 'A', 'G' }, 7.25m, DateTime.Now.AddDays(1)),
            new Menu(MenuType.Menu3, "Quinoa Bowl", new[] { 'A', 'C' }, 7.00m, DateTime.Now.AddDays(1)),
            new Menu(MenuType.SoupAndSalad, "Minestrone and Garden Salad", new[] { 'A', 'D' }, 5.75m, DateTime.Now.AddDays(1))
        ),

        new(
            DateTime.Now.AddDays(2),
            new Menu(MenuType.Menu1, "Teriyaki Salmon", new[] { 'A', 'D' }, 9.00m, DateTime.Now.AddDays(2)),
            new Menu(MenuType.Menu2, "Chicken Caesar Wrap", new[] { 'A', 'D' }, 6.75m, DateTime.Now.AddDays(2)),
            new Menu(MenuType.Menu3, "Lentil Curry", new[] { 'A', 'C' }, 7.50m, DateTime.Now.AddDays(2)),
            new Menu(MenuType.SoupAndSalad, "Pumpkin Soup and Mixed Greens", new[] { 'A', 'G' }, 6.25m, DateTime.Now.AddDays(2))
        )
    ];
}