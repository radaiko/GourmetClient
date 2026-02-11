use velopack::*;

#[tauri::command]
async fn check_for_updates() -> Result<Option<String>, String> {
    let source = sources::HttpSource::new(
        "https://github.com/radaiko/GourmetClient/releases/latest/download",
    );
    let um = UpdateManager::new(source, None, None).map_err(|e| e.to_string())?;

    match um.check_for_updates().map_err(|e| e.to_string())? {
        UpdateCheck::UpdateAvailable(info) => {
            Ok(Some(info.TargetFullRelease.Version.clone()))
        }
        _ => Ok(None),
    }
}

#[tauri::command]
async fn install_update() -> Result<(), String> {
    let source = sources::HttpSource::new(
        "https://github.com/radaiko/GourmetClient/releases/latest/download",
    );
    let um = UpdateManager::new(source, None, None).map_err(|e| e.to_string())?;

    if let UpdateCheck::UpdateAvailable(info) =
        um.check_for_updates().map_err(|e| e.to_string())?
    {
        um.download_updates(&info, None)
            .map_err(|e| e.to_string())?;
        um.apply_updates_and_restart(&info.TargetFullRelease)
            .map_err(|e| e.to_string())?;
    }
    Ok(())
}

pub fn run() {
    tauri::Builder::default()
        .invoke_handler(tauri::generate_handler![check_for_updates, install_update])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
