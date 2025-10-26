using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GC.Models;

public partial class Day : ObservableObject {
    [ObservableProperty] private Menu _menu1;
    [ObservableProperty] private Menu _menu2;
    [ObservableProperty] private Menu _menu3;
    [ObservableProperty] private Menu _soupAndSalad;

    public Day(Menu menu1, Menu menu2, Menu menu3, Menu soupAndSalad) {
        _menu1 = menu1;
        _menu2 = menu2;
        _menu3 = menu3;
        _soupAndSalad = soupAndSalad;
    }
    public List<Menu> Menus => [Menu1, Menu2, Menu3, SoupAndSalad];
    
    public DateOnly Date => Menu1.Date;
    
    public string Header => Date.ToString("ddd, MMM d", CultureInfo.CurrentCulture);
    
    public static ObservableCollection<Day> GetDummyData() => [
        new(
            new Menu(MenuType.Menu1, "Grilled Chicken with Vegetables", ['A', 'C'], 8.50m, DateOnly.FromDateTime(DateTime.Now)),
            new Menu(MenuType.Menu2, "Spaghetti Bolognese", ['A', 'G'], 7.00m, DateOnly.FromDateTime(DateTime.Now)),
            new Menu(MenuType.Menu3, "Vegetarian Stir Fry", ['A', 'C', 'G'], 7.50m, DateOnly.FromDateTime(DateTime.Now)),
            new Menu(MenuType.SoupAndSalad, "Tomato Soup and Caesar Salad", ['A', 'D', 'G'], 6.00m, DateOnly.FromDateTime(DateTime.Now))
        ),

        new(
            new Menu(MenuType.Menu1, "Beef Tacos", ['A'], 8.00m, DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
            new Menu(MenuType.Menu2, "Penne Alfredo", ['A', 'G'], 7.25m, DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
            new Menu(MenuType.Menu3, "Quinoa Bowl", ['A', 'C'], 7.00m, DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
            new Menu(MenuType.SoupAndSalad, "Minestrone and Garden Salad", ['A', 'D'], 5.75m, DateOnly.FromDateTime(DateTime.Now.AddDays(1)))
        ),

        new(
            new Menu(MenuType.Menu1, "Teriyaki Salmon", ['A', 'D'], 9.00m, DateOnly.FromDateTime(DateTime.Now.AddDays(2))),
            new Menu(MenuType.Menu2, "Chicken Caesar Wrap", ['A', 'D'], 6.75m, DateOnly.FromDateTime(DateTime.Now.AddDays(2))),
            new Menu(MenuType.Menu3, "Lentil Curry", ['A', 'C'], 7.50m, DateOnly.FromDateTime(DateTime.Now.AddDays(2))),
            new Menu(MenuType.SoupAndSalad, "Pumpkin Soup and Mixed Greens", ['A', 'G'], 6.25m, DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
        )
    ];
}