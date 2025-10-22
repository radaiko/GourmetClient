using CommunityToolkit.Mvvm.ComponentModel;

namespace GC.Models;

public partial class Position(string name, int quantity, decimal unitPrice, decimal support, decimal totalPrice)
  : ObservableObject {
  [ObservableProperty] private string _name = name;
  [ObservableProperty] private int _quantity = quantity;
  [ObservableProperty] private decimal _unitPrice = unitPrice;
  [ObservableProperty] private decimal _support = support;
  [ObservableProperty] private decimal _totalPrice = totalPrice;
}