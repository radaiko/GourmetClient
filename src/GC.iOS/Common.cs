namespace GC.iOS;

public static class Common {
  public static nfloat StandardMargin => 16f;
  public static nfloat StandardCellHeight => 50f;
  
  public static UIEdgeInsets SafeArea => new UIEdgeInsets(
    UIApplication.SharedApplication.KeyWindow?.SafeAreaInsets.Top ?? 0,
    UIApplication.SharedApplication.KeyWindow?.SafeAreaInsets.Left ?? 0,
    UIApplication.SharedApplication.KeyWindow?.SafeAreaInsets.Bottom ?? 0,
    UIApplication.SharedApplication.KeyWindow?.SafeAreaInsets.Right ?? 0
  );
}
