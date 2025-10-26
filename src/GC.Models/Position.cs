using CommunityToolkit.Mvvm.ComponentModel;

namespace GC.Models;

public partial class Position(string name, int quantity, decimal unitPrice, decimal support, decimal totalPrice)
  : ObservableObject {
  [NotifyPropertyChangedFor("TotalPrice")]
  [ObservableProperty] private string _name = name;
  [ObservableProperty] private int _quantity = quantity;
  [ObservableProperty] private decimal _unitPrice = unitPrice;
  [ObservableProperty] private decimal _support = support;
  public decimal TotalPrice => (UnitPrice - Support) * Quantity;
}
