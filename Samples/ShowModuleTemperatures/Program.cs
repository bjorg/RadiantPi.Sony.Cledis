/*
 * RadiantPi.Sony.Cledis - Communication client for Sony C-LED
 * Copyright (C) 2020-2022 - Steve G. Bjorg
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

using RadiantPi.Sony.Cledis;
using Spectre.Console;

// initialize client
using var client = new SonyCledisClient(new() {
    Host = "192.168.1.190",
    Port = 53595
});

// connect to device and fetch module temperatures
var temperatures = await AnsiConsole.Status()
    .Spinner(Spinner.Known.Dots)
    .StartAsync("Fetching Sony C-LED Module Temperatures", _ => client.GetTemperatureAsync());

// render module temperatures as a table
var table = new Table();
table.AddColumn("°C");
for(var column = 0; column < temperatures.ColumnCount; ++column) {
    table.AddColumn(new TableColumn($"C{column + 1}").Centered());
}
for(var row = 0; row < temperatures.RowCount; ++row) {
    var line = new string[temperatures.ColumnCount + 1];
    line[0] = $"\nR{row + 1}\n";
    for(var column = 0; column < temperatures.ColumnCount; ++column) {

        // find the highest temperature reading for each module
        var temperature = temperatures.Modules[column, row]
            .CellTemperatures
            .Append(temperatures.Modules[column, row].BoardTemperature)
            .Append(temperatures.Modules[column, row].AmbientTemperature)
            .Max();

        // render temperature with color coding
        var cell = $"\n[{ConvertTemperatureToColorCode(temperature)}]{temperature:0.00}[/]";
        line[column + 1] = cell;
    }
    table.AddRow(line);
}
table.Caption("Sony C-LED Module Temperatures");
AnsiConsole.Write(table);
AnsiConsole.MarkupLine("Legend");
AnsiConsole.MarkupLine($"* [{ConvertTemperatureToColorCode(15f)}]Min - 15°[/]");
AnsiConsole.MarkupLine($"* [{ConvertTemperatureToColorCode(25f)}]Optimal - 25°[/]");
AnsiConsole.MarkupLine($"* [{ConvertTemperatureToColorCode(35f)}]Warning - 35°[/]");
AnsiConsole.MarkupLine($"* [{ConvertTemperatureToColorCode(45f)}]Danger - 45°[/]");
AnsiConsole.MarkupLine($"* [{ConvertTemperatureToColorCode(55f)}]Max - 55°[/]");

// helper functions
static string ConvertTemperatureToColorCode(float temperature) {
    var red = 0f;
    var green = 0f;
    var blue = 0f;
    const float MAX_TEMPERATURE = 55f;
    const float DANGER_TEMPERATURE = 45f;
    const float WARNING_TEMPERATURE = 35f;
    const float OPTIMAL_TEMPERATURE = 25f;
    const float MIN_TEMPERATURE = 15f;
    if(temperature >= MAX_TEMPERATURE) {

        // purple
        red = 255;
        green = 0;
        blue = 255;
    } else if(temperature >= DANGER_TEMPERATURE) {

        // red
        red = 255;
        green = 0;
        blue = 255 * (temperature - DANGER_TEMPERATURE) / 10f; // 255 -> 0
    } else if(temperature >= WARNING_TEMPERATURE) {

        // yellow
        red = 255;
        green = 255 * (1f - (temperature - WARNING_TEMPERATURE) / 10f); // 0 -> 255
        blue = 0;
    } else if(temperature >= OPTIMAL_TEMPERATURE) {

        // green
        red = 255 * (temperature - OPTIMAL_TEMPERATURE) / 10f; // 255 -> 0
        green = 255;
        blue = 0;
    } else if(temperature >= MIN_TEMPERATURE) {

        // blue
        red = 0;
        green = 255 * (temperature - MIN_TEMPERATURE) / 10f; // 255 -> 0
        blue = 255 * (1f - (temperature - MIN_TEMPERATURE) / 10f); // 0 -> 255
    } else {
        red = 0;
        green = 0;
        blue = 255;
    }
    return $"#{(int)Math.Round(red):X2}{(int)Math.Round(green):X2}{(int)Math.Round(blue):X2}";
}