using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;

namespace Stuart
{
    static class BinaryWriterExtensions
    {
        public static void WriteCollection<T>(this BinaryWriter writer, ICollection<T> collection, Action<T> writeItem)
        {
            writer.Write(collection.Count);

            foreach (var item in collection)
            {
                writeItem(item);
            }
        }


        public static void WriteByteArray(this BinaryWriter writer, byte[] array)
        {
            if (array == null)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(array.Length);
                writer.Write(array);
            }
        }


        public static void WriteColor(this BinaryWriter writer, Color color)
        {
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
            writer.Write(color.A);
        }


        public static void WriteRect(this BinaryWriter writer, Rect rect)
        {
            writer.Write(rect.X);
            writer.Write(rect.Y);
            writer.Write(rect.Width);
            writer.Write(rect.Height);
        }


        public static void WriteVector2(this BinaryWriter writer, Vector2 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
        }
    }


    static class BinaryReaderExtensions
    {
        public static void ReadCollection<T>(this BinaryReader reader, ICollection<T> collection, Func<T> readItem)
        {
            collection.Clear();

            var count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                collection.Add(readItem());
            }
        }


        public static byte[] ReadByteArray(this BinaryReader reader)
        {
            var count = reader.ReadInt32();

            return (count == 0) ? null : reader.ReadBytes(count);
        }


        public static Color ReadColor(this BinaryReader reader)
        {
            Color color;

            color.R = reader.ReadByte();
            color.G = reader.ReadByte();
            color.B = reader.ReadByte();
            color.A = reader.ReadByte();

            return color;
        }


        public static Rect ReadRect(this BinaryReader reader)
        {
            Rect rect;

            rect.X = reader.ReadDouble();
            rect.Y = reader.ReadDouble();
            rect.Width = reader.ReadDouble();
            rect.Height = reader.ReadDouble();

            return rect;
        }


        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            Vector2 vector;

            vector.X = reader.ReadSingle();
            vector.Y = reader.ReadSingle();

            return vector;
        }
    }
}
