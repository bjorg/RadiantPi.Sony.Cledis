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

namespace RadiantPi.Sony.Cledis.Exceptions {

    public class SonyCledisUnrecognizedResponseException : ASonyCledisException {

        //--- Constructors ---
        public SonyCledisUnrecognizedResponseException(string message) : base(message) { }
    }
}
