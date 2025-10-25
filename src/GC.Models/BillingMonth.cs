using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GC.Models;

public partial class BillingMonth : ObservableObject {
  
  [ObservableProperty] private DateTime _month;
  [NotifyPropertyChangedFor(nameof(TotalAmount))]
  [NotifyPropertyChangedFor(nameof(TotalCafeAndCo))]
  [NotifyPropertyChangedFor(nameof(TotalGourmet))]
  [NotifyPropertyChangedFor(nameof(CountGourmet))]
  [NotifyPropertyChangedFor(nameof(CountCafeAndCo))]
  [ObservableProperty] private Transaction[] _transactions = [];
  
  // Total of all transactions in this month (sum of all positions' total prices)
  public decimal TotalAmount => Transactions.Sum(t => t.Positions.Sum(p => p.TotalPrice));

  // Total for transactions belonging to Cafe&Co (identified by TransactionType.CafePlusCo)
  public decimal TotalCafeAndCo => Transactions
    .Where(t => t.Type == Transaction.TransactionType.CafePlusCo)
    .Sum(t => t.Positions.Sum(p => p.TotalPrice));

  // Total for transactions belonging to Gourmet (identified by TransactionType.Gourmet)
  public decimal TotalGourmet => Transactions
    .Where(t => t.Type == Transaction.TransactionType.Gourmet)
    .Sum(t => t.Positions.Sum(p => p.TotalPrice));
  
  // Count of Gourmet transactions
  public int CountGourmet => Transactions.Count(t => t.Type == Transaction.TransactionType.Gourmet);

  // Count of Cafe&Co transactions
  public int CountCafeAndCo => Transactions.Count(t => t.Type == Transaction.TransactionType.CafePlusCo);

  public static ObservableCollection<BillingMonth> GetDummyData() {
    var dummyMonths = new ObservableCollection<BillingMonth>();
    // provide 3 months of sample data
    for (int i = 0; i < 3; i++) {
      var monthDate = DateTime.Now.AddMonths(-i);
      var transactions = new Transaction[5];
      for (int j = 0; j < transactions.Length; j++) {
        // create a few positions per transaction
        var positions = new Position[3];
        for (int k = 0; k < positions.Length; k++) {
          var unit = 5.00m + k; // 5.00, 6.00, 7.00
          var qty = k + 1; // 1,2,3
          var support = 0.00m;
          var total = unit * qty + support;
          positions[k] = new Position($"Item {k + 1}", qty, unit, support, total);
        }

        transactions[j] = new Transaction {
          Date = monthDate.AddDays(j + 1),
          Type = (j % 2 == 0) ? Transaction.TransactionType.Gourmet : Transaction.TransactionType.CafePlusCo,
          Positions = positions
        };
      }

      dummyMonths.Add(new BillingMonth {
        Month = new DateTime(monthDate.Year, monthDate.Month, 1),
        Transactions = transactions
      });
    }

    return dummyMonths;
  }
}