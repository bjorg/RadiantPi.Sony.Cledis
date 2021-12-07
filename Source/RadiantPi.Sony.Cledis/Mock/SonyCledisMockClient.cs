/*
 * RadiantPi.Trinnov.Altitude - Communication client for Trinnov Altitude
 * Copyright (C) 2020-2021 - Steve G. Bjorg
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RadiantPi.Sony.Internal;

namespace RadiantPi.Sony.Cledis.Mock {

    public class SonyCledisMockClient : ASonyCledisClient {

        //--- Constants ---
        private const long SERIAL_NUMBER = 1234567L;
        private const string MODEL_NAME = "ZRCT-200 [MOCK]";

        //--- Fields ---
        private SonyCledisPowerStatus _power = SonyCledisPowerStatus.StandBy;
        private SonyCledisInput _input = SonyCledisInput.Hdmi1;
        private SonyCledisPictureMode _mode = SonyCledisPictureMode.Mode1;

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

        public override void Dispose() { }
    }
}