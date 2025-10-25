namespace GC.iOS;

/// <summary>
/// Handles scene lifecycle events for the iOS app.
/// Manages the app's window and responds to scene state changes.
/// </summary>
[Register("SceneDelegate")]
public class SceneDelegate : UIResponder, IUIWindowSceneDelegate
{
    /// <summary>
    /// The window associated with this scene.
    /// </summary>
    [Export("window")]
    public UIWindow? Window { get; set; }

    /// <summary>
    /// Called when the scene is about to connect to a session.
    /// Use this to configure the window and attach it to the scene.
    /// </summary>
    /// <param name="scene">The scene being connected.</param>
    /// <param name="session">The session for this scene.</param>
    /// <param name="connectionOptions">Options for connecting the scene.</param>
    [Export("scene:willConnectToSession:options:")]
    public void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
    {
        // Use this method to optionally configure and attach the UIWindow `window` to the provided UIWindowScene `scene`.
        // If using a storyboard, the `window` property will automatically be initialized and attached to the scene.
        // This delegate does not imply the connecting scene or session are new (see UIApplicationDelegate `GetConfiguration` instead).
    }

    /// <summary>
    /// Called when the scene is disconnected from the system.
    /// Release resources that can be recreated when the scene reconnects.
    /// </summary>
    /// <param name="scene">The scene being disconnected.</param>
    [Export("sceneDidDisconnect:")]
    public void DidDisconnect(UIScene scene)
    {
        // Called as the scene is being released by the system.
        // This occurs shortly after the scene enters the background, or when its session is discarded.
        // Release any resources associated with this scene that can be re-created the next time the scene connects.
        // The scene may re-connect later, as its session was not neccessarily discarded (see UIApplicationDelegate `DidDiscardSceneSessions` instead).
    }

    /// <summary>
    /// Called when the scene becomes active.
    /// Restart tasks that were paused when the scene was inactive.
    /// </summary>
    /// <param name="scene">The scene that became active.</param>
    [Export("sceneDidBecomeActive:")]
    public void DidBecomeActive(UIScene scene)
    {
        // Called when the scene has moved from an inactive state to an active state.
        // Use this method to restart any tasks that were paused (or not yet started) when the scene was inactive.
    }

    /// <summary>
    /// Called when the scene will resign active state.
    /// Pause ongoing tasks.
    /// </summary>
    /// <param name="scene">The scene resigning active state.</param>
    [Export("sceneWillResignActive:")]
    public void WillResignActive(UIScene scene)
    {
        // Called when the scene will move from an active state to an inactive state.
        // This may occur due to temporary interruptions (ex. an incoming phone call).
    }

    /// <summary>
    /// Called when the scene is about to enter the foreground.
    /// Undo changes made when entering the background.
    /// </summary>
    /// <param name="scene">The scene entering the foreground.</param>
    [Export("sceneWillEnterForeground:")]
    public void WillEnterForeground(UIScene scene)
    {
        // Called as the scene transitions from the background to the foreground.
        // Use this method to undo the changes made on entering the background.
    }

    /// <summary>
    /// Called when the scene enters the background.
    /// Save data and release resources.
    /// </summary>
    /// <param name="scene">The scene entering the background.</param>
    [Export("sceneDidEnterBackground:")]
    public void DidEnterBackground(UIScene scene)
    {
        // Called as the scene transitions from the foreground to the background.
        // Use this method to save data, release shared resources, and store enough scene-specific state information
        // to restore the scene back to its current state.
    }
}