using Dalamud.Configuration;

namespace Tsunippy
{
    public partial class Configuration : IPluginConfiguration
    {
        public int Version { get; set; }

        public void Initialize() { }

        public void Save()
        {
            Modules.Modules.CheckModules();
            DalamudApi.PluginInterface.SavePluginConfig(this);
        }
    }
}
