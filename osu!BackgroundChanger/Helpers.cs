using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace osu_BackgroundChanger
{
    internal static class Helpers
    {
        public static async Task<Bitmap> DeserializeBitmapAsync(byte[] data)
        {
            return await new TaskFactory<Bitmap>().StartNew(() => DeserializeBitmap(data));
        }

        public static Bitmap DeserializeBitmap(byte[] data)
        {
            return (Bitmap)new BinaryFormatter().Deserialize(new MemoryStream(data));
        }

        public static async Task<byte[]> SerializeBitmapAsync(Bitmap bm)
        {
            return await new TaskFactory<byte[]>().StartNew(() => SerializeBitmap(bm));
        }

        public static byte[] SerializeBitmap(Bitmap bm)
        {
            using (var ms = new MemoryStream()) {
                new BinaryFormatter().Serialize(ms, bm);
                return ms.ToArray();
            }
        }
    }
}
