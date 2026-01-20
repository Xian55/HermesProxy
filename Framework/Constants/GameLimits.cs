/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace Framework.Constants
{
    /// <summary>
    /// Game-defined limits for names and other strings.
    /// These limits have been consistent since vanilla WoW.
    /// </summary>
    /// <remarks>
    /// WoW names allow certain accented Latin characters (é, è, ñ, ö, ß, etc.)
    /// which are 2 bytes in UTF-8. The byte limits assume worst-case where
    /// all characters are 2-byte UTF-8 encoded.
    /// </remarks>
    public static class GameLimits
    {
        /// <summary>
        /// Maximum character count for player names (2-12 characters).
        /// </summary>
        public const int MaxPlayerNameChars = 12;

        /// <summary>
        /// Maximum byte size for player names in UTF-8 (12 chars * 2 bytes).
        /// </summary>
        public const int MaxPlayerNameBytes = MaxPlayerNameChars * 2;

        /// <summary>
        /// Maximum character count for pet names (2-12 characters).
        /// </summary>
        public const int MaxPetNameChars = 12;

        /// <summary>
        /// Maximum byte size for pet names in UTF-8 (12 chars * 2 bytes).
        /// </summary>
        public const int MaxPetNameBytes = MaxPetNameChars * 2;

        /// <summary>
        /// Maximum character count for guild names (2-24 characters).
        /// </summary>
        public const int MaxGuildNameChars = 24;

        /// <summary>
        /// Maximum byte size for guild names in UTF-8 (24 chars * 2 bytes).
        /// </summary>
        public const int MaxGuildNameBytes = MaxGuildNameChars * 2;

        /// <summary>
        /// Maximum character count for arena team names (2-24 characters).
        /// </summary>
        public const int MaxArenaTeamNameChars = 24;

        /// <summary>
        /// Maximum byte size for arena team names in UTF-8 (24 chars * 2 bytes).
        /// </summary>
        public const int MaxArenaTeamNameBytes = MaxArenaTeamNameChars * 2;
    }
}
