using System;
using System.IO;
using System.Windows.Forms;

using dnlib.DotNet;
using dnlib.DotNet.Resources;

namespace osu_BackgroundChanger
{
    internal class OsuSeasonal
    {
        private const string ResourceName = "osu_seasonal.ResourcesStore.resources";

        public EmbeddedResource Resource;

        private readonly ModuleDefMD _module;
        private ResourceElementSet _elementSet;

        public OsuSeasonal(string path) //TODO: save path in string? for Save/Save as 
        {
            _module = ModuleDefMD.Load(File.ReadAllBytes(path));
            if (_module.Resources.Count != 1) throw new Exception("Didn't find the right amount of resources.");

            Resource = _module.Resources[0] as EmbeddedResource ?? throw new Exception("Didn't find any resources in the file.");
        }

        public void Save(string path)
        {
            //write the resources to the module
            using (var ms = new MemoryStream()) {
                ResourceWriter.Write(_module, ms, _elementSet);
                _module.Resources[0] = new EmbeddedResource(ResourceName, ms.ToArray());
            }

            //save module
            _module.Write(path);

            //TODO: something proper
            MessageBox.Show("Written!");
        }

        public override string ToString() => _module.ToString();

        public ResourceElementSet ResourceSet
        {
            get => _elementSet ?? (_elementSet = ResourceReader.Read(_module, Resource.Data));
            set => _elementSet = value;
        }
    }
}
