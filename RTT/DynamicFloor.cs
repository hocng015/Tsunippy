using System;

namespace Tsunippy.RTT
{
    /// <summary>
    /// Tracks the minimum observed RTT over a sliding window to compute a dynamic floor.
    ///
    /// Replaces NoClippy's hardcoded simulatedRTT = 0.04f (40ms).
    ///
    /// The floor = MinRTT * ScalingFactor, which adapts to:
    /// - Different datacenters (NA vs EU vs JP)
    /// - Time of day (off-peak vs peak server load)
    /// - Instance vs overworld server processing differences
    ///
    /// A player with 20ms min RTT to a fast datacenter gets floor ~17ms instead of 40ms.
    /// Falls back to 0.04f until sufficient samples are collected.
    /// </summary>
    public class DynamicFloor
    {
        private readonly float[] samples;
        private int head = 0;
        private int count = 0;
        private float cachedMin = float.MaxValue;
        private bool dirty = true;

        /// <summary>
        /// Scaling factor applied to MinRTT to compute the floor.
        /// 0.85 means floor = 85% of the lowest observed RTT.
        /// This provides a small safety margin below the absolute minimum.
        /// </summary>
        public float ScalingFactor { get; set; } = 0.85f;

        /// <summary>The size of the sliding window (number of RTT samples retained).</summary>
        public int WindowSize => samples.Length;

        /// <summary>The default floor used before sufficient data is collected.</summary>
        public const float DefaultFloor = 0.04f;

        /// <summary>Minimum allowed floor to prevent unreasonably aggressive values.</summary>
        public const float MinimumFloor = 0.01f;

        /// <summary>
        /// Create a new DynamicFloor tracker.
        /// </summary>
        /// <param name="windowSize">Number of RTT samples to retain. Default 100 gives
        /// a good balance between adaptiveness and stability.</param>
        public DynamicFloor(int windowSize = 100)
        {
            samples = new float[Math.Max(windowSize, 10)];
        }

        /// <summary>Add a new RTT sample to the sliding window.</summary>
        public void AddSample(float rtt)
        {
            if (rtt <= 0 || !float.IsFinite(rtt)) return;

            samples[head] = rtt;
            head = (head + 1) % samples.Length;
            if (count < samples.Length) count++;
            dirty = true;
        }

        /// <summary>The minimum RTT observed in the current sliding window.</summary>
        public float MinRTT
        {
            get
            {
                if (!dirty) return cachedMin;
                cachedMin = float.MaxValue;
                for (int i = 0; i < count; i++)
                {
                    if (samples[i] < cachedMin)
                        cachedMin = samples[i];
                }
                dirty = false;
                return cachedMin;
            }
        }

        /// <summary>
        /// The computed dynamic floor: MinRTT * ScalingFactor.
        /// Falls back to DefaultFloor (40ms) if insufficient data.
        /// Clamped to MinimumFloor (10ms) to prevent dangerously low values.
        /// </summary>
        public float Floor
        {
            get
            {
                if (!HasSufficientData)
                    return DefaultFloor;

                var computed = MinRTT * ScalingFactor;
                return Math.Max(Math.Min(computed, DefaultFloor), MinimumFloor);
            }
        }

        /// <summary>Whether enough samples have been collected for reliable floor estimation.</summary>
        public bool HasSufficientData => count >= 5;

        /// <summary>Number of samples currently in the window.</summary>
        public int CurrentSampleCount => count;

        /// <summary>Reset the tracker, clearing all samples.</summary>
        public void Reset()
        {
            head = 0;
            count = 0;
            cachedMin = float.MaxValue;
            dirty = true;
        }
    }
}
