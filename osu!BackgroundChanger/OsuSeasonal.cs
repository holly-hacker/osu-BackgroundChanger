using System;
using System.IO;

using dnlib.DotNet;
using dnlib.DotNet.Resources;

namespace osu_BackgroundChanger
{
    internal class OsuSeasonal
    {
        public EmbeddedResource Resource;

        private readonly ModuleDefMD _module;
        private ResourceElementSet _elementSet;

        public OsuSeasonal(string path)
        {
            _module = ModuleDefMD.Load(path);
            if (_module.Resources.Count != 1) throw new Exception("Didn't find the right amount of resources.");

            Resource = _module.Resources[0] as EmbeddedResource ?? throw new Exception("Didn't find any resources in the file.");
        }

        public override string ToString() => _module.ToString();
        public ResourceElementSet ResourceSet
        {
            get => _elementSet ?? (_elementSet = ResourceReader.Read(_module, Resource.Data));
            set => _elementSet = value;
        }
	}
}
