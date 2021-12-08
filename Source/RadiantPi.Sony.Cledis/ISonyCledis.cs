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
using System.Threading.Tasks;

namespace RadiantPi.Sony.Cledis {

    public enum SonyCledisPower {
        Undefined,
        On,
        Off
    }

    public enum SonyCledisPowerStatus {
        Undefined,
        StandBy,
        On,
        Updating,
        Startup,
        ShuttingDown,
        Initializing
    }

    public enum SonyCledisInput {
        Undefined,
        Hdmi1,
        Hdmi2,
        DisplayPort1,
        DisplayPort2,
        DisplayPortBoth
    }

    public enum SonyCledisPictureMode {
        Undefined,
        Mode1,
        Mode2,
        Mode3,
        Mode4,
        Mode5,
        Mode6,
        Mode7,
        Mode8,
        Mode9,
        Mode10
    }

    public class SonyCledisTemperatures {

        //--- Properties ---
        public float ControllerTemperature { get; set; }
        public SonyCledisModuleTemperature[,] Modules { get; set; } = new SonyCledisModuleTemperature[0, 0];
        public int RowCount => Modules.GetLength(1);
        public int ColumnCount => Modules.GetLength(0);
    }

    public class SonyCledisModuleTemperature {

        //--- Properties ---
        public string? Id { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public float AmbientTemperature { get; set; }
        public float BoardTemperature { get; set; }
        public float[] CellTemperatures { get; set; } = new float[12];
    }

    public interface ISonyCledis : IDisposable {

        //--- Methods ---
        Task<string> GetModelNameAsync();
        Task<long> GetSerialNumberAsync();
        Task<SonyCledisTemperatures> GetTemperatureAsync();
        Task<SonyCledisPowerStatus> GetPowerStatusAsync();
        Task SetPowerAsync(SonyCledisPower power);
        Task<SonyCledisInput> GetInputAsync();
        Task SetInputAsync(SonyCledisInput input);
        Task<SonyCledisPictureMode> GetPictureModeAsync();
        Task SetPictureModeAsync(SonyCledisPictureMode mode);
    }
}
