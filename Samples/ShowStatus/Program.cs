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

using RadiantPi.Sony.Cledis;

// initialize client
using var client = new SonyCledisClient(new() {
    Host = "192.168.1.190",
    Port = 53595
});

var power = await client.GetPowerStatusAsync();
Console.WriteLine($"PowerStatus: {power}");
if(power == SonyCledisPowerStatus.On) {
    Console.WriteLine($"ModelName: {await client.GetModelNameAsync()}");
    Console.WriteLine($"SerialNumber: {await client.GetSerialNumberAsync()}");
    var input = await client.GetInputAsync();
    Console.WriteLine($"Input: {input}");
    Console.WriteLine($"PictureMode: {await client.GetPictureModeAsync()}");
    Console.WriteLine($"2D3DMode: {await client.Get2D3DModeAsync()}");
    Console.WriteLine($"3DFormat: {await client.Get3DFormatAsync()}");
    Console.WriteLine($"FanMode: {await client.GetFanModeAsync()}");
    Console.WriteLine($"HorizontalPictureShift: {await client.GetHorizontalPictureShiftAsync(input)}");
    Console.WriteLine($"VerticalPictureShift: {await client.GetVerticalPictureShiftAsync(input)}");
    Console.WriteLine($"DualDisplayPort3D4KMode: {await client.GetDualDisplayPort3D4KModeAsync()}");
}