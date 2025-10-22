using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GC.Models;

public class MenuOrder : ObservableObject {
  // Database id
  public int Id { get; set; }

  // FK to Menu.Id
  public int MenuId { get; set; }

  public DateTime OrderedAt { get; set; }
  public int Quantity { get; set; }
  public string? Note { get; set; }

}
