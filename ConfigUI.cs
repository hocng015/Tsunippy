using System;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;

namespace Tsunippy
{
    public static class ConfigUI
    {
        public static bool isVisible = false;
        public static void ToggleVisible() => isVisible ^= true;

        public static void Draw()
        {
            if (!isVisible) return;

            ImGui.SetNextWindowSize(new Vector2(500, 0) * ImGuiHelpers.GlobalScale);
            ImGui.Begin("Tsunippy Configuration", ref isVisible, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
            Modules.Modules.Draw();
            ImGui.End();
        }
    }
}
