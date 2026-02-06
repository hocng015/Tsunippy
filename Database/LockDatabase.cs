using System;
using System.Collections.Generic;

namespace Tsunippy.Database
{
    /// <summary>
    /// Represents the game context for keying lock database entries.
    /// PvP and PvE can have different animation lock values for the same action.
    /// </summary>
    public enum GameContext : byte
    {
        PvE = 0,
        PvP = 1,
    }

    /// <summary>
    /// A single entry in the lock database tracking the observed animation lock
    /// for a specific (actionID, context) pair.
    /// </summary>
    [Serializable]
    public class LockEntry
    {
        /// <summary>The mean observed animation lock value in seconds.</summary>
        public float MeanLock { get; set; }

        /// <summary>Number of times this lock has been observed.</summary>
        public int SampleCount { get; set; }

        /// <summary>
        /// Confidence level (0..1) based on sample count.
        /// Reaches 1.0 at 10 samples. Used to determine whether to trust
        /// this entry or fall back to the default lock value.
        /// </summary>
        public float Confidence => Math.Min(SampleCount / 10f, 1f);
    }

    /// <summary>
    /// Context-aware animation lock database with confidence tracking.
    ///
    /// Improvements over NoClippy's Dictionary&lt;uint, float&gt;:
    /// 1. Keyed by (actionID, PvE/PvP context) — separate lock values per mode
    /// 2. Tracks sample count for confidence estimation
    /// 3. Uses incremental mean calculation for stability
    /// 4. Low-confidence entries fall back to defaults
    /// 5. Caps sample count at 1000 to allow eventual adaptation to game patches
    /// </summary>
    [Serializable]
    public class LockDatabase
    {
        /// <summary>
        /// The database entries, keyed by "{actionID}_{contextByte}".
        /// String keys are used for JSON serialization compatibility.
        /// </summary>
        public Dictionary<string, LockEntry> Entries { get; set; } = new();

        private static string MakeKey(uint actionID, GameContext ctx) => $"{actionID}_{(byte)ctx}";

        /// <summary>
        /// Get the known animation lock for an action in a given context.
        /// Returns defaultLock if the action is unknown or has insufficient confidence.
        /// </summary>
        /// <param name="actionID">The resolved spell ID.</param>
        /// <param name="context">PvE or PvP.</param>
        /// <param name="defaultLock">Fallback value (default 0.5s, the game's default).</param>
        /// <returns>The predicted animation lock in seconds.</returns>
        public float GetLock(uint actionID, GameContext context, float defaultLock = 0.5f)
        {
            var key = MakeKey(actionID, context);
            if (!Entries.TryGetValue(key, out var entry))
                return defaultLock;

            // Sanity check: animation locks should never be below 0.5s
            if (entry.MeanLock < 0.5f)
                return defaultLock;

            // Require at least 30% confidence (3 samples) to use the learned value
            return entry.Confidence >= 0.3f ? entry.MeanLock : defaultLock;
        }

        /// <summary>
        /// Record a new animation lock observation from a server response.
        /// Updates the entry using incremental mean calculation.
        /// </summary>
        /// <param name="actionID">The resolved spell ID.</param>
        /// <param name="context">PvE or PvP.</param>
        /// <param name="lockValue">The server-reported animation lock in seconds.</param>
        /// <returns>True if this was a new or changed value, false if unchanged.</returns>
        public bool RecordLock(uint actionID, GameContext context, float lockValue)
        {
            var key = MakeKey(actionID, context);

            if (!Entries.TryGetValue(key, out var entry))
            {
                // New entry
                Entries[key] = new LockEntry { MeanLock = lockValue, SampleCount = 1 };
                return true;
            }

            // Same value — just bump count for confidence
            if (Math.Abs(entry.MeanLock - lockValue) < 0.0001f)
            {
                if (entry.SampleCount < 1000)
                    entry.SampleCount++;
                return false;
            }

            // Different value — incremental mean update
            // Cap at 1000 samples so the mean can eventually adapt to game patches
            entry.SampleCount = Math.Min(entry.SampleCount + 1, 1000);
            entry.MeanLock += (lockValue - entry.MeanLock) / entry.SampleCount;
            return true;
        }

        /// <summary>
        /// Check whether we have a confident entry for this action.
        /// </summary>
        public bool HasConfidentEntry(uint actionID, GameContext context)
        {
            var key = MakeKey(actionID, context);
            return Entries.TryGetValue(key, out var entry) && entry.Confidence >= 0.5f;
        }

        /// <summary>
        /// Get the entry for diagnostics display. May return null.
        /// </summary>
        public LockEntry GetEntry(uint actionID, GameContext context)
        {
            var key = MakeKey(actionID, context);
            return Entries.TryGetValue(key, out var entry) ? entry : null;
        }
    }
}
