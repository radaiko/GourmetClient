#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

fn main() {
    // MUST be first - handles install/uninstall/update lifecycle events
    velopack::VelopackApp::build().run();

    gourmet_client_lib::run()
}
