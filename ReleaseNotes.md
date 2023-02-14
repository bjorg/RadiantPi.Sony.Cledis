# Release Notes

## v2.6 (TBD)

> LOGO


## v2.5 (2022-05-04)

### Features

* Added `SetLightOutputMode()` to set the light output.


## v2.4 (2022-03-26)

### BREAKING CHANGES

* Removed `Id` property in `SonyCledisModuleTemperature` as it was never set.

### Fixes

* Fixed an issue with serializing the Sony C-LED temperatures data structure during trace logging.


## v2.3 (2022-03-26)

### BREAKING CHANGES

* Renamed `SonyCledis2D3DMode.Select2D` to `Mode2D`. Ditto for `Select3D`.

### Fixes

* Fixed an issue where commands had an extra `\r` character that caused them to fail.


## v2.2 (2022-03-25)

### Features

* Added `Get2D3DModeAsync()` to get current 2D/3D mode.
* Added `Get3DFormatAsync()` to get 3D image format.
* Added `GetFanModeAsync()` to get fan speed.
* Added `GetHorizontalPictureShiftAsync()` to get the horizontal shift of the image by input.
* Added `GetVerticalPictureShiftAsync()` to get the vertical shift of the image by input.
* Added `GetDualDisplayPort3D4KModeAsync()` to determine if 3D 4K mode is enabled.

### Samples

* Added `ShowStatus` to show all values that can be read from the controller.


## v2.1 (2022-02-16)

### Fixes

* Disable auto-reconnect for Telnet connection as it interferes with the communication protocol.


## v2.0.1 (2022-02-16)

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
