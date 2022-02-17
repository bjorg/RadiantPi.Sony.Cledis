# Release Notes


## v2.0.1 (2022-02-15)

### Features

* Upgraded `Microsoft.Extensions.Logging` package to 6.0.0

### Fixes

* Fixed a regression where sending was broken because the connection was never opened.


## v2.0 (2022-02-15)

### BREAKING CHANGES

* Changed target framework to .NET 6.0.

### Features

* Upgraded [RadiantPi.Telnet](https://github.com/bjorg/RadiantPi.Telnet) package with auto-reconnect capability.
* Added `Set2D3DModeAsync()` to select either 2D or 3D operation.
* Added `Set3DFormatAsync()` to select 3D image format.
* Added `SetDualDisplayPort3D4KModeAsync()` to enable/disable 3D 4K mode.
* Added `SetFanModeAsync()` to control the fan speed.
* Added `SetHorizontalPictureShiftAsync()` to set the horizontal shift of the image by input.
* Added `SetVerticalPictureShiftAsync()` to set the vertical shift of the image by input.

## v1.0 (2021-12-07)

### Features

* Initial release
