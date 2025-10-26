using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using GC.Common;

namespace GC.Models;

public partial class Transaction : ObservableObject {
  [ObservableProperty] private TransactionType _type;
  [ObservableProperty] private DateTime _date;
  [ObservableProperty] private Position[] _positions = [];

  public decimal TotalAmount => Positions.Sum(p => p.TotalPrice);

  public string Hash =>
    Crypto.ComputeSHA256Hash(
      $"{Type}|{Date:o}|{string.Join("|", Positions.Select(p => $"{p.Name}|{p.Quantity}|{p.UnitPrice}|{p.Support}"))}");

  public enum TransactionType {
    Gourmet = 0,
    CafePlusCo = 1
  }
}