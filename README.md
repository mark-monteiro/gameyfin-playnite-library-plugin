# Gameyfin Playnite Library Plugin

A playnite library plugin for [Gameyfin](https://github.com/grimsi/gameyfin)

## Features

- Import your Gameyfin library into Playnite
- Filter which games are imported into Playnite by platform
- Authenticate with Gameyfin if it is behind a forward-auth provider like Authelia
- Download your games as .zip files from the Playnite UI

## Installing

This addon has not yet been published with the [Playnite add-on database](https://github.com/JosefNemec/PlayniteAddonDatabase).
For now, in order to load the addon, you need to build it manually then load it in Playnite using the developer settings:

1. To build the extension, open the solution in Visual Studio and build it.
2. Details on how to load the built extension in playnite can be found in the [Playnite documentation](https://github.com/JosefNemec/PlayniteAddonDatabase).

## Usage

### Setup

Once the extension is loaded, usage is fairly straightforward. Open up the addon settings, enter the required information,
and authenticate, if necessary. Each option on the settings page has some notes to clarify what it means.

Once the settings have all been set correctly, trigger a refresh of the library using the "Update Game Library" menu. You
will see your Gameyfin games populated in the library after a short time.

### Downloading Games

Automated install and launching of games is not yet supported. All you can do for now is download the games as a .zip file.
This can be done by opening a game detail, clicking the 'More' button, then selecting 'Download'.
