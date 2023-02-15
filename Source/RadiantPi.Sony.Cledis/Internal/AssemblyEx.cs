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

namespace RadiantPi.Sony.Internal;

using System.IO;
using System.IO.Compression;
using System.Text;

internal static class AssemblyEx {

    //--- Extension Methods ---
    public static string ReadManifestResource(this System.Reflection.Assembly assembly, string resourceName, bool convertLineEndings = true) {

        // load resource stream
        using var resource = assembly.GetManifestResourceStream(resourceName) ?? throw new ApplicationException($"unable to locate embedded resource: '{resourceName}'");

        // check if resource stream has to be decompressed
        using var stream = resourceName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase)
            ? new GZipStream(resource, CompressionMode.Decompress)
            : resource;

        // parse resource stream into a string
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var result = reader.ReadToEnd();

        // optionally remove carriage return characters
        if(convertLineEndings) {
            result = result.Replace("\r", "");
        }
        return result;
    }
}