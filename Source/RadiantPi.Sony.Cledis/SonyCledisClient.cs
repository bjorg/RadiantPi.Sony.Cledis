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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RadiantPi.Sony.Cledis.Exceptions;
using RadiantPi.Telnet;

namespace RadiantPi.Sony.Cledis {

    public class SonyCledisClientConfig {

        //--- Properties ---
        public string? Host { get; set; }
        public ushort? Port { get; set; }
        public bool? Mock { get; set; }
    }

    public class SonyCledisClient : ASonyCledisClient {

        //--- Fields ---
        private readonly ITelnet _telnet;
        private readonly SemaphoreSlim _mutex = new SemaphoreSlim(1, 1);

        //--- Constructors ---
        public SonyCledisClient(SonyCledisClientConfig config, ILoggerFactory? loggerFactory = null) :
            this(
                new TelnetClient(
                    config.Host ?? throw new ArgumentNullException("config.Host"),
                    config.Port ?? 53595,
                    loggerFactory?.CreateLogger<TelnetClient>()
                ),
                loggerFactory?.CreateLogger<SonyCledisClient>()
            ) { }

        public SonyCledisClient(ITelnet telnet, ILogger<SonyCledisClient>? logger) : base(logger) {
            _telnet = telnet ?? throw new ArgumentNullException(nameof(telnet));
            _telnet.ValidateConnectionAsync = ValidateConnectionAsync;
        }

        //--- Methods ---
        public override async Task<string> GetModelNameAsync()
            => ConvertResponse<string>(await SendAsync("modelname ?"));

        public override async Task<long> GetSerialNumberAsync()
            => ConvertResponse<long>(await SendAsync("serialnum ?"));

        public override async Task<SonyCledisTemperatures> GetTemperatureAsync()
            => ConvertTemperatureFromJson(await SendAsync("temperature ?"));

        public override async Task<SonyCledisPowerStatus> GetPowerStatusAsync()
            => ConvertResponse<string>(await SendAsync("power_status ?")) switch {
                "standby" => SonyCledisPowerStatus.StandBy,
                "on" => SonyCledisPowerStatus.On,
                "updating" => SonyCledisPowerStatus.Updating,
                "startup" => SonyCledisPowerStatus.Startup,
                "shutting_down" => SonyCledisPowerStatus.ShuttingDown,
                "initializing" => SonyCledisPowerStatus.Initializing,
                var value => throw new SonyCledisUnrecognizedResponseException(value)
            };

        public override Task SetPowerAsync(SonyCledisPower power)
            => power switch {
                SonyCledisPower.On => SendCommandAsync("power \"on\""),
                SonyCledisPower.Off => SendCommandAsync("power \"off\""),
                _ => throw new ArgumentException("invalid value", nameof(power))
            };

        public override async Task<SonyCledisInput> GetInputAsync()
            => ConvertResponse<string>(await SendAsync("input ?")) switch {
                "dp1" => SonyCledisInput.DisplayPort1,
                "dp2" => SonyCledisInput.DisplayPort2,
                "dp1_2" => SonyCledisInput.DisplayPortBoth,
                "hdmi1" => SonyCledisInput.Hdmi1,
                "hdmi2" => SonyCledisInput.Hdmi2,
                var value => throw new SonyCledisUnrecognizedResponseException(value)
            };

        public override Task SetInputAsync(SonyCledisInput input)
            => input switch {
                SonyCledisInput.DisplayPort1 => SendCommandAsync("input \"dp1\""),
                SonyCledisInput.DisplayPort2 => SendCommandAsync("input \"dp2\""),
                SonyCledisInput.DisplayPortBoth => SendCommandAsync("input \"dp1_2\""),
                SonyCledisInput.Hdmi1 => SendCommandAsync("input \"hdmi1\""),
                SonyCledisInput.Hdmi2 => SendCommandAsync("input \"hdmi2\""),
                _ => throw new ArgumentException("invalid value", nameof(input))
            };

        public override async Task<SonyCledisPictureMode> GetPictureModeAsync()
            => ConvertResponse<string>(await SendAsync("picture_mode ?")) switch {
                "mode1" => SonyCledisPictureMode.Mode1,
                "mode2" => SonyCledisPictureMode.Mode2,
                "mode3" => SonyCledisPictureMode.Mode3,
                "mode4" => SonyCledisPictureMode.Mode4,
                "mode5" => SonyCledisPictureMode.Mode5,
                "mode6" => SonyCledisPictureMode.Mode6,
                "mode7" => SonyCledisPictureMode.Mode7,
                "mode8" => SonyCledisPictureMode.Mode8,
                "mode9" => SonyCledisPictureMode.Mode9,
                "mode10" => SonyCledisPictureMode.Mode10,
                var value => throw new SonyCledisUnrecognizedResponseException(value)
            };

        public override Task SetPictureModeAsync(SonyCledisPictureMode mode)
            => mode switch {
                SonyCledisPictureMode.Mode1 => SendCommandAsync("picture_mode \"mode1\""),
                SonyCledisPictureMode.Mode2 => SendCommandAsync("picture_mode \"mode2\""),
                SonyCledisPictureMode.Mode3 => SendCommandAsync("picture_mode \"mode3\""),
                SonyCledisPictureMode.Mode4 => SendCommandAsync("picture_mode \"mode4\""),
                SonyCledisPictureMode.Mode5 => SendCommandAsync("picture_mode \"mode5\""),
                SonyCledisPictureMode.Mode6 => SendCommandAsync("picture_mode \"mode6\""),
                SonyCledisPictureMode.Mode7 => SendCommandAsync("picture_mode \"mode7\""),
                SonyCledisPictureMode.Mode8 => SendCommandAsync("picture_mode \"mode8\""),
                SonyCledisPictureMode.Mode9 => SendCommandAsync("picture_mode \"mode9\""),
                SonyCledisPictureMode.Mode10 => SendCommandAsync("picture_mode \"mode10\""),
                _ => throw new ArgumentException("invalid value", nameof(mode))
            };

        public override void Dispose() {
            _mutex.Dispose();
            _telnet.Dispose();
        }

        private void ValidateResponse(string response) {
            Logger?.LogDebug($"Response: {response}");

            // check if response is an error code
            switch(response) {
            case "ok":
                return;
            case "err_cmd":
                throw new SonyCledisCommandUnrecognizedException();
            case "err_option":
                throw new SonyCledisCommandOptionaException();
            case "err_inactive":
                throw new SonyCledisCommandInactiveException();
            case "err_val":
                throw new SonyCledisCommandValueException();
            case "err_auth":
                throw new SonyCledisAuthenticationException();
            case "err_internal1":
                throw new SonyCledisInternal1Exception();
            case "err_internal2":
                throw new SonyCledisInternal2Exception();
            default:
                throw new SonyCledisUnrecognizedResponseException(response);
            }
        }

        private async Task SendCommandAsync(string message)
            => ValidateResponse(await SendAsync(message + "\r"));

        private async Task<string> SendAsync(string message) {
            await _mutex.WaitAsync();
            try {
                TaskCompletionSource<string> responseSource = new();

                // send message and await response
                try {
                    _telnet.MessageReceived += ResponseHandler;
                    await _telnet.SendAsync(message);
                    var response = await responseSource.Task;
                    return response;
                } finally {

                    // remove response handler no matter what
                    _telnet.MessageReceived -= ResponseHandler;
                }

                // local functions
                void ResponseHandler(object? sender, TelnetMessageReceivedEventArgs args)
                    => responseSource.SetResult(args.Message);
            } finally {
                _mutex.Release();
            }
        }

        private async Task ValidateConnectionAsync(ITelnet client, TextReader reader, TextWriter writer) {
            var handshake = await reader.ReadLineAsync();

            // the controller sends 'NOKEY' when there is no need for authentication
            if(handshake != "NOKEY") {
                throw new NotSupportedException("Sony C-LED requires authentication");
            }
            Logger?.LogDebug("unsecured Sony C-LED connection established");
        }
    }
}
