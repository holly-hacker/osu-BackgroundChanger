using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
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
	}
}
