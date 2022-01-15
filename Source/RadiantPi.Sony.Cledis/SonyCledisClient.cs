/*
 * RadiantPi.Sony.Cledis - Communication client for Sony C-LED
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
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
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

        //--- Class Methods ---
        protected static string GetInputName(SonyCledisInput input)
            => input switch {
                SonyCledisInput.DisplayPort1 => "dp1",
                SonyCledisInput.DisplayPort2 => "dp2",
                SonyCledisInput.DisplayPortBoth => "dp1_2",
                SonyCledisInput.Hdmi1 => "hdmi1",
                SonyCledisInput.Hdmi2 => "hdmi2",
                _ => throw new ArgumentException("invalid value", nameof(input))
            };


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
        public override Task<string> GetModelNameAsync()
            => LogRequestResponse(async () => ConvertResponse<string>(await SendAsync("modelname ?")));

        public override Task<long> GetSerialNumberAsync()
            => LogRequestResponse(async () => ConvertResponse<long>(await SendAsync("serialnum ?")));

        public override Task<SonyCledisTemperatures> GetTemperatureAsync()
            => LogRequestResponse(async () => ConvertTemperatureFromJson(await SendAsync("temperature ?")));

        public override Task<SonyCledisPowerStatus> GetPowerStatusAsync()
            => LogRequestResponse(async () => ConvertResponse<string>(await SendAsync("power_status ?")) switch {
                "standby" => SonyCledisPowerStatus.StandBy,
                "on" => SonyCledisPowerStatus.On,
                "updating" => SonyCledisPowerStatus.Updating,
                "startup" => SonyCledisPowerStatus.Startup,
                "shutting_down" => SonyCledisPowerStatus.ShuttingDown,
                "initializing" => SonyCledisPowerStatus.Initializing,
                var value => throw new SonyCledisUnrecognizedResponseException(value)
            });

        public override Task SetPowerAsync(SonyCledisPower power)
            => power switch {
                SonyCledisPower.On => LogRequest(() => SendCommandAsync("power \"on\""), power),
                SonyCledisPower.Off => LogRequest(() => SendCommandAsync("power \"off\""), power),
                _ => throw new ArgumentException("invalid value", nameof(power))
            };

        public override Task<SonyCledisInput> GetInputAsync()
            => LogRequestResponse(async () => ConvertResponse<string>(await SendAsync("input ?")) switch {
                "dp1" => SonyCledisInput.DisplayPort1,
                "dp2" => SonyCledisInput.DisplayPort2,
                "dp1_2" => SonyCledisInput.DisplayPortBoth,
                "hdmi1" => SonyCledisInput.Hdmi1,
                "hdmi2" => SonyCledisInput.Hdmi2,
                var value => throw new SonyCledisUnrecognizedResponseException(value)
            });

        public override Task SetInputAsync(SonyCledisInput input)
            => LogRequest(() => SendCommandAsync($"input \"{GetInputName(input)}\""), input);

        public override Task<SonyCledisPictureMode> GetPictureModeAsync()
            => LogRequestResponse(async () => ConvertResponse<string>(await SendAsync("picture_mode ?")) switch {
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
            });

        public override Task SetPictureModeAsync(SonyCledisPictureMode mode)
            => mode switch {
                SonyCledisPictureMode.Mode1 => LogRequest(() => SendCommandAsync("picture_mode \"mode1\""), mode),
                SonyCledisPictureMode.Mode2 => LogRequest(() => SendCommandAsync("picture_mode \"mode2\""), mode),
                SonyCledisPictureMode.Mode3 => LogRequest(() => SendCommandAsync("picture_mode \"mode3\""), mode),
                SonyCledisPictureMode.Mode4 => LogRequest(() => SendCommandAsync("picture_mode \"mode4\""), mode),
                SonyCledisPictureMode.Mode5 => LogRequest(() => SendCommandAsync("picture_mode \"mode5\""), mode),
                SonyCledisPictureMode.Mode6 => LogRequest(() => SendCommandAsync("picture_mode \"mode6\""), mode),
                SonyCledisPictureMode.Mode7 => LogRequest(() => SendCommandAsync("picture_mode \"mode7\""), mode),
                SonyCledisPictureMode.Mode8 => LogRequest(() => SendCommandAsync("picture_mode \"mode8\""), mode),
                SonyCledisPictureMode.Mode9 => LogRequest(() => SendCommandAsync("picture_mode \"mode9\""), mode),
                SonyCledisPictureMode.Mode10 => LogRequest(() => SendCommandAsync("picture_mode \"mode10\""), mode),
                _ => throw new ArgumentException("invalid value", nameof(mode))
            };

        public override Task Set2D3DModeAsync(SonyCledis2D3DMode mode)
            => mode switch {
                SonyCledis2D3DMode.Select2D => LogRequest(() => SendCommandAsync("2d3d_sel \"2d\""), mode),
                SonyCledis2D3DMode.Select3D => LogRequest(() => SendCommandAsync("2d3d_sel \"3d\""), mode),
                _ => throw new ArgumentException("invalid value", nameof(mode))
            };

        public override Task SetDualDisplayPort3D4KModeAsync(SonyCledisDualDisplayPort3D4KMode mode)
            => mode switch {
                SonyCledisDualDisplayPort3D4KMode.Off => LogRequest(() => SendCommandAsync("dp_dual_3d_4k \"off\""), mode),
                SonyCledisDualDisplayPort3D4KMode.On => LogRequest(() => SendCommandAsync("dp_dual_3d_4k \"on\""), mode),
                _ => throw new ArgumentException("invalid value", nameof(mode))
            };

        public override Task Set3DFormatAsync(SonyCledis3DFormat format)
            => format switch {
                SonyCledis3DFormat.DualInput => LogRequest(() => SendCommandAsync("3d_format \"dualinput\""), format),
                SonyCledis3DFormat.FrameSequential => LogRequest(() => SendCommandAsync("3d_format \"framesequential\""), format),
                _ => throw new ArgumentException("invalid value", nameof(format))
            };

        public override Task SetFanModeAsync(SonyCledisFanMode mode)
            => mode switch {
                SonyCledisFanMode.Low => LogRequest(() => SendCommandAsync("unit_fan_mode \"low\""), mode),
                SonyCledisFanMode.Mid => LogRequest(() => SendCommandAsync("unit_fan_mode \"mid\""), mode),
                SonyCledisFanMode.Stop => LogRequest(() => SendCommandAsync("unit_fan_mode \"stop\""), mode),
                _ => throw new ArgumentException("invalid value", nameof(mode))
            };

        public override Task SetHorizontalPictureShiftAsync(SonyCledisInput input, int shift)
            => LogRequest(() => SendCommandAsync($"pic_shift_h_ch --{GetInputName(input)} {shift}"), input, shift);

        public override Task SetVerticalPictureShiftAsync(SonyCledisInput input, int shift)
            => LogRequest(() => SendCommandAsync($"pic_shift_v_ch --{GetInputName(input)} {shift}"), input, shift);

        public override void Dispose() {
            _mutex.Dispose();
            _telnet.Dispose();
        }

        private void ValidateResponse(string response) {

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
            => ValidateResponse(await SendAsync(message + "\r").ConfigureAwait(false));

        private async Task<string> SendAsync(string message) {
            await _mutex.WaitAsync().ConfigureAwait(false);
            try {
                TaskCompletionSource<string> responseSource = new();

                // send message and await response
                try {
                    _telnet.MessageReceived += ResponseHandler;
                    await _telnet.SendAsync(message).ConfigureAwait(false);
                    return await responseSource.Task.ConfigureAwait(false);
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
            var handshake = await reader.ReadLineAsync().ConfigureAwait(false);

            // the controller sends 'NOKEY' when there is no need for authentication
            if(handshake != "NOKEY") {
                throw new NotSupportedException("Sony C-LED requires authentication");
            }
            Logger?.LogInformation("Sony C-LED connection established");
        }

        private void LogDebugJson(string message, object? response) {
            if(Logger?.IsEnabled(LogLevel.Debug) ?? false) {
                var serializedResponse = JsonSerializer.Serialize(response, g_jsonSerializerOptions);
                Logger?.LogDebug($"{message}: {serializedResponse}");
            }
        }

        private async Task<T> LogRequestResponse<T>(Func<Task<T>> callback, [CallerMemberName] string methodName = "") {
            Logger?.LogDebug($"{methodName} request");
            var response = await callback().ConfigureAwait(false);
            LogDebugJson($"{methodName} response", response);
            return response;
        }

        private Task LogRequest<T>(Func<Task> callback, T parameter, [CallerMemberName] string methodName = "") {
            Logger?.LogDebug($"{methodName} request: {parameter}");
            return callback();
        }

        private Task LogRequest<T1, T2>(Func<Task> callback, T1 parameter1, T2 parameter2, [CallerMemberName] string methodName = "") {
            Logger?.LogDebug($"{methodName} request: {parameter1}, {parameter2}");
            return callback();
        }
    }
}
