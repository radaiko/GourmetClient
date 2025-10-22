using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GC.Models;

public partial class Transaction : ObservableObject {
  
  [ObservableProperty] private TransactionType _type;
  [ObservableProperty] private DateTime _date;
  [ObservableProperty] private Position[] _positions = [];
  

  public enum TransactionType {
    Gourmet,
    CafePlusCo
  }
}