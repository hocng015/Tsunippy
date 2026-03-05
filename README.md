# Tsunippy

Next-gen animation lock compensation for FFXIV. An improved successor to [NoClippy](https://github.com/UnknownX7/NoClippy).

Reduces the effects of lag on animation locks, weaving, and cast actions using adaptive RTT estimation — tighter locks on stable connections, safe buffering during jitter.

## Features

### Animation Lock Compensation
- **Jacobson/Karels RTT estimator** (RFC 6298) — separately tracks smoothed RTT and RTT variance instead of a simple average, providing dynamic network buffering that adapts to connection quality
- **Dynamic RTT floor** — tracks your minimum observed RTT over a sliding window instead of using a hardcoded 40ms floor, adapting per-datacenter and per-time-of-day
- **Graduated packet weight** — multi-level spike dampening (1.0/0.5/0.25/0.1) instead of binary weight, for more nuanced burst handling
- **Context-aware lock database** — learns animation lock durations per action with PvE/PvP stored separately and confidence tracking per entry

### Cast Lock Prediction
Pre-applies the expected caster tax at cast completion instead of waiting for the server response, giving casters a ~RTT head start on the next action. Most impactful for BLM, SMN, RDM, SGE, and other casting jobs.

### Encounter Stats
Tracks GCD clips and wasted GCD time during combat with per-action clip breakdown and running averages.

### Real-Time Diagnostics
Live overlay showing SRTT, RTT variance, dynamic floor, correction details, packet counts, and lock database confidence — useful for tuning and understanding plugin behavior.

## Commands

`/tsunippy` — Open the configuration window

| Argument | Description |
|----------|-------------|
| `on` / `off` / `toggle` | Enable or disable animation lock compensation |
| `dry` | Toggle dry run (calculations only, no lock overrides) |
| `diag` | Toggle the real-time diagnostics overlay |

## Installation

Add the following custom plugin repository in Dalamud Settings:

```
https://raw.githubusercontent.com/hocng015/Tsunippy/main/pluginmaster.json
```

## Support

[![Ko-fi](https://img.shields.io/badge/Ko--fi-Support%20Me-ff5e5b?logo=ko-fi&logoColor=white)](https://ko-fi.com/tsukio_mochi6767)
