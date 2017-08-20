using System;

using dnlib.DotNet;
using dnlib.DotNet.Resources;

namespace osu_BackgroundChanger
{
    internal class OsuSeasonal
    {
        private readonly ModuleDefMD _module;
        public EmbeddedResource Resource;

        public OsuSeasonal(string path)
        {
            _module = ModuleDefMD.Load(path);
            if (_module.Resources.Count != 1) throw new Exception("Didn't find the right amount of resources.");

            Resource = _module.Resources[0] as EmbeddedResource ?? throw new Exception("Didn't find any resources in the file.");
        }

        public override string ToString() => _module.ToString();
        public ResourceElementSet ResourceSet => ResourceReader.Read(_module, Resource.Data);
    }
}
