using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GC.Models;

public partial class Menu(MenuType type, string title, char[] allergens, decimal price, DateTime date)
  : ObservableObject {
  [ObservableProperty] private MenuType _type = type;
  [ObservableProperty] private string _title = title;
  [ObservableProperty] private char[] _allergens = allergens;
  [ObservableProperty] private decimal _price = price;
  [ObservableProperty] private DateTime _date = date;

  public int Id { get; set; }
}

public enum MenuType {
  Menu1,
  Menu2,
  Menu3,
  SoupAndSalad
}