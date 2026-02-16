#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

fn main() {
    // MUST be first - handles install/uninstall/update lifecycle events
    velopack::VelopackApp::build().run();

    snack_pilot_lib::run()
}
