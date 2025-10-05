# MVU vs MVVM: Code Comparison

## State Management

### MVU (GourmetClient.MVU)
```csharp
// State is immutable record
public record AppState(
  bool IsLoading = false,
  int CurrentPageIndex = 0,
  string? ErrorMessage = null,
  string UserName = ""
) {
  public static AppState Initial => new(Settings: new AppSettings());
}

// Messages for updates
public record NavigateToPage(int PageIndex) : Msg;
public record ClearError : Msg;

// Dispatcher
dispatcher.Dispatch(new NavigateToPage(1));
```

### MVVM (GC.ViewModels)
```csharp
// State is mutable class with property notification
public partial class MainViewModel : ObservableObject {
  [ObservableProperty]
  private int _currentPageIndex = 0;

  [ObservableProperty]
  private string? _errorMessage;

  [ObservableProperty]
  private string _userName = "";

  [RelayCommand]
  private void NavigateToPage(int pageIndex) {
    CurrentPageIndex = Math.Clamp(pageIndex, 0, 3);
  }

  [RelayCommand]
  private void ClearError() {
    ErrorMessage = null;
  }
}

// Commands
viewModel.NavigateToPageCommand.Execute(1);
```

## View Creation

### MVU (MainView.iOS.cs)
```csharp
public static class MainViewIOS
{
    public static Control Create(AppState state, Action<Msg> dispatch)
    {
        var mainGrid = new Grid { /* ... */ };
        
        // Event handlers dispatch messages
        button.Click += (_, _) => dispatch(new NavigateToPage(1));
        
        return mainGrid;
    }
}

// Usage
var view = MainViewIOS.Create(state, dispatcher.Dispatch);
```

### MVVM (MainView.iOS.cs)
```csharp
public static class MainViewIOS
{
    public static Control Create(MainViewModel viewModel)
    {
        var mainGrid = new Grid { /* ... */ };
        
        // Event handlers call commands
        button.Click += (_, _) => viewModel.NavigateToPageCommand.Execute(1);
        
        return mainGrid;
    }
}

// Usage
var view = MainViewIOS.Create(viewModel);
```

## Update Cycle

### MVU Pattern
```
User Action
    ↓
Dispatch Message
    ↓
Update State (pure function)
    ↓
Create New State
    ↓
Render View with New State
    ↓
Display
```

### MVVM Pattern
```
User Action
    ↓
Execute Command
    ↓
Modify ViewModel Property
    ↓
Raise PropertyChanged Event
    ↓
View Observes Change
    ↓
Rebuild View
    ↓
Display
```

## Platform Detection

### Both Use Same Pattern
```csharp
if (PlatformDetector.IsIOS) {
    // Use iOS-specific view
    return MainViewIOS.Create(...);
} else {
    // Use desktop view
    return MainViewDesktop.Create(...);
}
```

## Navigation Example

### MVU
```csharp
// In View
ellipse.PointerPressed += (_, _) => dispatch(new NavigateToPage(pageIndex));

// In Update.cs
NavigateToPage nav => HandleIosNavigateToPage(nav.PageIndex, state),

// Handler
private static (AppState, Cmd<Msg>) HandleIosNavigateToPage(int targetIndex, AppState state)
{
    var newIndex = targetIndex < 0 ? 0 : (targetIndex > 3 ? 3 : targetIndex);
    var newState = state with { CurrentPageIndex = newIndex };
    return (newState, Cmd.None<Msg>());
}
```

### MVVM
```csharp
// In View
ellipse.PointerPressed += (_, _) => viewModel.NavigateToPageCommand.Execute(pageIndex);

// In ViewModel
[RelayCommand]
private void NavigateToPage(int pageIndex) {
    CurrentPageIndex = Math.Clamp(pageIndex, 0, 3);
}

// Reactive update in Host
private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
    if (e.PropertyName == nameof(MainViewModel.CurrentPageIndex)) {
        UpdateContent(); // Rebuild view
    }
}
```

## Swipe Gesture Handling

### MVU
```csharp
mainGrid.PointerReleased += (_, e) =>
{
    if (!swipeActive) return;
    swipeActive = false;
    var point = e.GetPosition(mainGrid);
    var deltaX = point.X - swipeStartX;
    const double threshold = 80;
    if (deltaX <= -threshold && state.CurrentPageIndex < 3)
    {
        dispatch(new NavigateToPage(state.CurrentPageIndex + 1));
    }
};
```

### MVVM
```csharp
mainGrid.PointerReleased += (_, e) =>
{
    if (!swipeActive) return;
    swipeActive = false;
    var point = e.GetPosition(mainGrid);
    var deltaX = point.X - swipeStartX;
    const double threshold = 80;
    if (deltaX <= -threshold && viewModel.CurrentPageIndex < 3)
    {
        viewModel.NavigateToPageCommand.Execute(viewModel.CurrentPageIndex + 1);
    }
};
```

## Error Handling

### MVU
```csharp
// Show error
dispatch(new ErrorOccurred("Connection failed"));

// In Update
ErrorOccurred err => (state with { ErrorMessage = err.Message }, Cmd.None<Msg>()),

// Clear error
dispatch(new ClearError());

// In Update
ClearError => (state with { ErrorMessage = null }, Cmd.None<Msg>())
```

### MVVM
```csharp
// Show error
viewModel.ErrorMessage = "Connection failed";

// Clear error
viewModel.ClearErrorCommand.Execute(null);

// Or directly
viewModel.ErrorMessage = null;
```

## Page Content Switching

### MVU
```csharp
private static Control CreatePageContent(AppState state, Action<Msg> dispatch)
{
    return state.CurrentPageIndex switch
    {
        0 => MenuViewIOS.Create(state, dispatch),
        1 => BillingView.Create(state, dispatch),
        2 => SettingsView.Create(state, dispatch),
        3 => AboutView.Create(state, dispatch),
        _ => new TextBlock { Text = "Unknown" }
    };
}
```

### MVVM
```csharp
private static Control CreatePageContent(MainViewModel viewModel)
{
    return viewModel.CurrentPageIndex switch
    {
        0 => MenuViewIOS.Create(viewModel),
        1 => BillingViewIOS.Create(viewModel),
        2 => SettingsViewIOS.Create(viewModel),
        3 => AboutViewIOS.Create(viewModel),
        _ => new TextBlock { Text = "Unknown" }
    };
}
```

## View Lifecycle

### MVU
```csharp
public class MainViewHostControl : HostControl {
  private readonly MvuDispatcher _dispatcher;

  public MainViewHostControl() {
    _dispatcher = new MvuDispatcher(AppState.Initial);
    _dispatcher.SetStateChangedCallback(OnStateChanged);
    UpdateView(_dispatcher.CurrentState);
  }

  private void OnStateChanged(AppState newState) {
    Dispatcher.UIThread.Post(() => UpdateView(newState));
  }

  private void UpdateView(AppState state) {
    Content = MainView.Create(state, _dispatcher.Dispatch);
  }
}
```

### MVVM
```csharp
public class MainViewHostControl : UserControl {
  private readonly MainViewModel _viewModel;

  public MainViewHostControl() {
    _viewModel = new MainViewModel();
    DataContext = _viewModel;
    
    if (PlatformDetector.IsIOS) {
      _viewModel.PropertyChanged += OnViewModelPropertyChanged;
      UpdateContent();
    }
  }

  private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
    if (e.PropertyName == nameof(MainViewModel.CurrentPageIndex)) {
      UpdateContent();
    }
  }

  private void UpdateContent() {
    Content = MainViewIOS.Create(_viewModel);
  }
}
```

## Key Differences Summary

| Aspect | MVU | MVVM |
|--------|-----|------|
| **State** | Immutable records | Mutable properties |
| **Updates** | Messages & pure functions | Commands & property setters |
| **Change Notification** | Callback on state change | PropertyChanged events |
| **Architecture** | Functional (Elm-inspired) | Object-oriented |
| **Boilerplate** | Message definitions | Attribute annotations |
| **Testing** | Pure functions, easy | Mock ViewModels |
| **Complexity** | Message routing | Property binding |
| **Type Safety** | Exhaustive pattern matching | Runtime property names |

## Similarities

Both implementations:
- ✅ Use exact same UI layout and styling
- ✅ Share platform detection logic
- ✅ Handle gestures identically
- ✅ Support same navigation patterns
- ✅ Provide same user experience
- ✅ Use Avalonia UI framework
- ✅ Follow single responsibility principle
- ✅ Separate concerns (View/State/Logic)

## When to Use Which?

### Use MVU when:
- You prefer functional programming
- You want immutable state
- You need time-travel debugging
- You value predictability over flexibility
- You're building complex state machines

### Use MVVM when:
- You prefer object-oriented programming
- You're familiar with WPF/Xamarin patterns
- You want standard .NET tooling (CommunityToolkit)
- You need two-way binding
- You're migrating from existing MVVM code

## Migration Tips

### MVU → MVVM
1. Convert `record` states to `class` ViewModels
2. Replace `dispatch(new Msg())` with `Command.Execute()`
3. Change `state with { ... }` to property assignments
4. Add `[ObservableProperty]` attributes
5. Subscribe to `PropertyChanged` events

### MVVM → MVU
1. Convert ViewModels to `record` states
2. Define messages for all state changes
3. Create `Update` function with pattern matching
4. Replace commands with message dispatch
5. Use `Cmd` for side effects
