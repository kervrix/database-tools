# Application updates

Install the `Kervrix.DatabaseTools-win-Setup.exe` asset from a GitHub Release once.
The standalone `DatabaseTools.exe` is still published, but does not enable automatic updates.
Existing connection profiles stay in `%APPDATA%/Kervrix/DatabaseTools/settings.json`.

The installed application checks public releases at https://github.com/kervrix/database-tools
on startup. It downloads a newer stable version in the background and shows
**Restart to update**. Updates are applied only by that button; automatic application
on startup is disabled. Database operations, modal dialogs, and other running
DatabaseTools instances prevent an update restart. Settings also offers **Check for updates**.
Network errors do not block database operations.

To release, push a tag such as `v1.0.0`, then `v1.0.1` for the next version.
The Windows workflow builds the self-contained application and runs Velopack 1.2.161.
It attaches the installer, portable package, full update package and release feed to
the GitHub Release. Keep all generated `Releases/*` assets together.
The repository and releases must be public; no GitHub token is embedded in the client.
PR/main builds produce CI-version artifacts without publishing a release.

Validate the first rollout by installing one release, publishing a higher version,
and checking the downloaded update and explicit restart from the installed application.
Running from `bin/Debug` is not an installed Velopack application.
