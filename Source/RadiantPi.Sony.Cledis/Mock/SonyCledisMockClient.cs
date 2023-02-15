/*
 * RadiantPi.Sony.Cledis - Communication client for Sony C-LED
 * Copyright (C) 2020-2023 - Steve G. Bjorg
 *
 * This program is free software: you can redistribute it and/or modify it
 * under the terms of the GNU Affero General Public License as published by the
 * Free Software Foundation, either version 3 of the License, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more
 * details.
 *
 * You should have received a copy of the GNU Affero General Public License along
 * with this program. If not, see <https://www.gnu.org/licenses/>.
 */

namespace RadiantPi.Sony.Cledis.Mock;

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using RadiantPi.Sony.Internal;

public class SonyCledisMockClient : ASonyCledisClient {

    //--- Constants ---
    private const long SERIAL_NUMBER = 1234567L;
    private const string MODEL_NAME = "ZRCT-200 [MOCK]";

    //--- Fields ---
    private SonyCledisPowerStatus _power = SonyCledisPowerStatus.StandBy;
    private SonyCledisInput _input = SonyCledisInput.Hdmi1;
    private SonyCledisPictureMode _mode = SonyCledisPictureMode.Mode1;
    private SonyCledisDualDisplayPort3D4KMode _3d4kStatus = SonyCledisDualDisplayPort3D4KMode.On;
    private SonyCledisFanMode _fanMode = SonyCledisFanMode.Mid;
    private SonyCledis2D3DMode _2d3dmode = SonyCledis2D3DMode.Mode2D;
    private SonyCledis3DFormat _3dFormat = SonyCledis3DFormat.FrameSequential;
    private SonyCledisLightOutputMode _lightOutputMode = SonyCledisLightOutputMode.Low;
    private Dictionary<SonyCledisInput, int> _verticalPictureShift = new Dictionary<SonyCledisInput, int>();
    private Dictionary<SonyCledisInput, int> _horizontalPictureShift = new Dictionary<SonyCledisInput, int>();

    //--- Constructors ---
    public SonyCledisMockClient(ILogger? logger = null) : base(logger) { }

    //--- Methods ---
    public override Task<SonyCledisInput> GetInputAsync() => Task.FromResult(_input);
    public override Task<string> GetModelNameAsync() => Task.FromResult(MODEL_NAME);
    public override Task<SonyCledisPictureMode> GetPictureModeAsync() => Task.FromResult(_mode);
    public override Task<SonyCledisPowerStatus> GetPowerStatusAsync() => Task.FromResult(_power);
    public override Task<long> GetSerialNumberAsync() => Task.FromResult(SERIAL_NUMBER);

    public override Task<SonyCledisTemperatures> GetTemperatureAsync() {
        if(_power == SonyCledisPowerStatus.On) {
            var json = GetType().Assembly.ReadManifestResource("RadiantPi.Sony.Cledis.Resources.CledisTemperature.json.gz");
            return Task.FromResult(ConvertTemperatureFromJson(json));
        }
        return Task.FromResult(new SonyCledisTemperatures {
            ControllerTemperature = 33f,
            Modules = new SonyCledisModuleTemperature[0, 0]
        });
    }

    public override Task SetInputAsync(SonyCledisInput input) {
        _input = input;
        return Task.CompletedTask;
    }

    public override Task SetPictureModeAsync(SonyCledisPictureMode mode) {
        _mode = mode;
        return Task.CompletedTask;
    }

    public override Task SetPowerAsync(SonyCledisPower power) {
        switch(power) {
        case SonyCledisPower.On:
            _power = SonyCledisPowerStatus.On;
            break;
        case SonyCledisPower.Off:
            _power = SonyCledisPowerStatus.StandBy;
            break;
        default:
            throw new ArgumentOutOfRangeException(nameof(power));
        }
        return Task.CompletedTask;
    }

    public override Task<SonyCledis2D3DMode> Get2D3DModeAsync()
        => Task.FromResult(_2d3dmode);

    public override Task Set2D3DModeAsync(SonyCledis2D3DMode mode) {
        _2d3dmode = mode;
        return Task.CompletedTask;
    }

    public override Task<SonyCledisDualDisplayPort3D4KMode> GetDualDisplayPort3D4KModeAsync()
        => Task.FromResult(_3d4kStatus);

    public override Task SetDualDisplayPort3D4KModeAsync(SonyCledisDualDisplayPort3D4KMode mode) {
        _3d4kStatus = mode;
        return Task.CompletedTask;
    }

    public override Task<SonyCledis3DFormat> Get3DFormatAsync()
        => Task.FromResult(_3dFormat);

    public override Task Set3DFormatAsync(SonyCledis3DFormat format) {
        _3dFormat = format;
        return Task.CompletedTask;
    }

    public override Task<SonyCledisFanMode> GetFanModeAsync()
        => Task.FromResult(_fanMode);

    public override Task SetFanModeAsync(SonyCledisFanMode mode) {
        _fanMode = mode;
        return Task.CompletedTask;
    }

    public override Task SetLightOutputMode(SonyCledisLightOutputMode mode) {
        _lightOutputMode = mode;
        return Task.CompletedTask;
    }

    public override Task<int> GetHorizontalPictureShiftAsync(SonyCledisInput input)
        => Task.FromResult(_horizontalPictureShift.TryGetValue(input, out var shift) ? shift : 0);

    public override Task SetHorizontalPictureShiftAsync(SonyCledisInput input, int shift) {
        _horizontalPictureShift[input] = shift;
        return Task.CompletedTask;
    }

    public override Task<int> GetVerticalPictureShiftAsync(SonyCledisInput input)
        => Task.FromResult(_verticalPictureShift.TryGetValue(input, out var shift) ? shift : 0);

    public override Task SetVerticalPictureShiftAsync(SonyCledisInput input, int shift) {
        _verticalPictureShift[input] = shift;
        return Task.CompletedTask;
    }

    public override void Dispose() { }
}