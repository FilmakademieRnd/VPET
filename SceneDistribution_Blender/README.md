# VPET for Blender

VPET addon for Blender 2.81 and higher.

## Description

VPET allows for real-time manipulation and scene editing through one or multiple mobile clients.

This addon allows using Blender as a host application.

Note: This addon is still in early development.

## Installation

Download the zip file from the [Addon](/Addon) folder.

In Blender go to Preferences -> Add-ons -> Install... and navigate to the downloaded zip file. Once you enabled the Addon it will show up on the right side of your 3D viewport.

## Usage

For a quick introduction, please refer to our [quick-start guide](../.doc/VPET_Blender_Quickstart.md)

## Supported Blender Versions

For now this Addon only supports Blender 2.81 and up.

## Additional Tools

For syncronization between clients (tablets, phones...) and host (Blender), the side tool **SyncServer** is needed.

Refer back to the repository root for more information.

## Dependencies

The current release needs **pyzmq** to work. The Addon has a function to install this via pip. Please refer to our [quick-start guide](../.doc/VPET_Blender_Quickstart.md) for more information.

You could also install **pyzmq** manually but please be aware that (under windows) it needs to be in this location to work:

**c:\Program Files\Blender Foundation\Blender x.xx\x.xx\python\lib\site-packages**

## Network Requirements

The host computer (running Blender) and the client devices must be connected to the same network and be able to reach each other.

Communication happens via ØMQ (ZeroMQ) over ports **5556** and **5565**.

## Additional Resources

* [Quick-start guide](../.doc/VPET_Blender_Quickstart.md)
* [Main repository](https://github.com/FilmakademieRnd/VPET)
* [VPET Web Site](https://animationsinstitut.de/en/research/tools/vpet)

## License

Please review the [License file](LICENSE.TXT).
