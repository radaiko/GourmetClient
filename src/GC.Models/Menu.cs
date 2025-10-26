using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GC.Models;

public partial class Menu(MenuType type, string title, char[] allergens, decimal price, DateOnly date) : ObservableObject {
  [ObservableProperty] private MenuType _type = type;
  [ObservableProperty] private string _title = title;
  [ObservableProperty] private char[] _allergens = allergens;
  [ObservableProperty] private decimal _price = price;
  [ObservableProperty] private DateOnly _date = date;

  public int Id { get; set; }
  
  public string Hash => Common.Crypto.ComputeSHA256Hash($"{Type}|{Title}|{new string(Allergens)}|{Price}|{Date:o}");
}

public enum MenuType {
  Menu1 = 0,
  Menu2 = 1,
  Menu3 = 2,
  SoupAndSalad = 3
}