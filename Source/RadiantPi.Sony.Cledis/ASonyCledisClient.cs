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
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RadiantPi.Sony.Cledis.Exceptions;

namespace RadiantPi.Sony.Cledis {
    public abstract class ASonyCledisClient : ISonyCledis {

        //--- Constructors ---
        protected ASonyCledisClient(ILogger? logger) => Logger = logger;

        //--- Properties ---
        protected ILogger? Logger { get; }

        //--- Abstract Methods ---
        public abstract void Dispose();
        public abstract Task<SonyCledisInput> GetInputAsync();
        public abstract Task<string> GetModelNameAsync();
        public abstract Task<SonyCledisPictureMode> GetPictureModeAsync();
        public abstract Task<SonyCledisPowerStatus> GetPowerStatusAsync();
        public abstract Task<long> GetSerialNumberAsync();
        public abstract Task<SonyCledisTemperatures> GetTemperatureAsync();
        public abstract Task SetInputAsync(SonyCledisInput input);
        public abstract Task SetPictureModeAsync(SonyCledisPictureMode mode);
        public abstract Task SetPowerAsync(SonyCledisPower power);

        //--- Methods ---
        protected T ConvertResponse<T>(string response) {

            // check if response is an error code
            switch(response) {
            case "ok":
                throw new SonyCledisCommandUnexpectedResponseException();
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
                if(Logger?.IsEnabled(LogLevel.Debug) ?? false) {
                    var serializedResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions {
                        WriteIndented = true,
                        Converters = {
                            new JsonStringEnumConverter()
                        }
                    });
                    Logger?.LogDebug($"response: {serializedResponse}");
                }
                return JsonSerializer.Deserialize<T>(response)
                    ?? throw new SonyCledisCommandNullResponseException();
            }
        }

        protected SonyCledisTemperatures ConvertTemperatureFromJson(string json) {
            SonyCledisTemperatures result = new();
            var data = ConvertResponse<List<Dictionary<string, float>>>(json)

                // TODO: better exception
                ?? throw new Exception();
            Dictionary<(int Column,int Row), SonyCledisModuleTemperature> modules = new();

            // loop over all entries in response
            foreach(var entry in data) {
                var (key, value) = entry.First();

                // check what the entry describes
                if(key == "controller") {

                    // set controller temperature
                    result.ControllerTemperature = value;
                } else if(
                    (key.StartsWith("u", StringComparison.Ordinal))
                    && (key.Length >= 6)
                    && (key[5] == '_')
                    && int.TryParse(key.Substring(1, 2), out var column)
                    && int.TryParse(key.Substring(3, 2), out var row)
                ) {

                    // get module entry or create one
                    if(!modules.TryGetValue((column, row), out var module)) {
                        module = new() {
                            Row = row,
                            Column = column
                        };
                        modules.Add((column, row), module);
                    }

                    // check what part of the module the entry describes
                    if(key.EndsWith("_board", StringComparison.Ordinal)) {

                        // set module board temperature
                        module.BoardTemperature = value;
                    } else if(key.EndsWith("_ambient", StringComparison.Ordinal)) {

                        // set module ambient temperature
                        module.AmbientTemperature = value;
                    } else if(key.EndsWith("_cell1")) {

                        // set module cell temperature
                        module.CellTemperatures[0] = value;
                    } else if(key.EndsWith("_cell2")) {

                        // set module cell temperature
                        module.CellTemperatures[1] = value;
                    } else if(key.EndsWith("_cell3")) {

                        // set module cell temperature
                        module.CellTemperatures[2] = value;
                    } else if(key.EndsWith("_cell4")) {

                        // set module cell temperature
                        module.CellTemperatures[3] = value;
                    } else if(key.EndsWith("_cell5")) {

                        // set module cell temperature
                        module.CellTemperatures[4] = value;
                    } else if(key.EndsWith("_cell6")) {

                        // set module cell temperature
                        module.CellTemperatures[5] = value;
                    } else if(key.EndsWith("_cell7")) {

                        // set module cell temperature
                        module.CellTemperatures[6] = value;
                    } else if(key.EndsWith("_cell8")) {

                        // set module cell temperature
                        module.CellTemperatures[7] = value;
                    } else if(key.EndsWith("_cell9")) {

                        // set module cell temperature
                        module.CellTemperatures[8] = value;
                    } else if(key.EndsWith("_cell10")) {

                        // set module cell temperature
                        module.CellTemperatures[9] = value;
                    } else if(key.EndsWith("_cell11")) {

                        // set module cell temperature
                        module.CellTemperatures[10] = value;
                    } else if(key.EndsWith("_cell12")) {

                        // set module cell temperature
                        module.CellTemperatures[11] = value;
                    } else {
                        Logger?.LogWarning($"unrecognized module entry: '{key}' = {value}");
                    }
                } else {
                    Logger?.LogWarning($"unrecognized entry: '{key}' = {value}");
                }
            }

            // check if any module information was found
            if(modules.Any()) {

                // determine the dimensions of the wall
                var maxRow = modules.Keys.Max(location => location.Row);
                var maxColumn = modules.Keys.Max(location => location.Column);
                result.Modules = new SonyCledisModuleTemperature[maxColumn, maxRow];

                // assign module information to corresponding array location
                foreach(var (location, module) in modules) {
                    result.Modules[location.Column - 1, location.Row - 1] = module;
                }
            } else {

                // not modules found; initialize an empty array
                result.Modules = new SonyCledisModuleTemperature[0, 0];
            }
            return result;
        }
    }
}