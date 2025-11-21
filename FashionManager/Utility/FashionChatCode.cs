using Google.Protobuf;
using Kenedia.Modules.FashionManager.Models;
using Kenedia.Modules.FashionManager.Models.Proto.FashionProto;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace Kenedia.Modules.FashionManager.Utility
{
    public class FashionChatCode
    {
        internal static string ParseChatCode(FashionTemplate fashionTemplate)
        {

            var rnd = new Random();

            uint smallskin()
            {
                return (uint)rnd.Next(250, 1000);
            }

            uint infusion()
            {
                //return (uint)rnd.Next(1, 1000);
                return 234;
            }

            uint dye()
            {
                //return (uint)rnd.Next(1000, 9999);
                return 1234;
            }

            uint skin()
            {
                return (uint)rnd.Next(40000, 100000);
            }



            var fashion = new Fashion()
            {
                AquaBreather = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },

                Head = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },
                Shoulders = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },
                Chest = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },
                Gloves = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },
                Legs = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },
                Boots = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },

                Back = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },

                Amulet = new()
                {
                    Skin = skin(),
                    Enrichment = 1,
                },
                Ring1 = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                    Infusion2 = infusion(),
                    Infusion3 = infusion(),
                },
                Ring2 = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                    Infusion2 = infusion(),
                    Infusion3 = infusion(),
                },
                Accessory1 = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                },
                Accessory2 = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                },

                MainHand = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                },
                OffHand = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                },
                AquaticWeapon = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                },

                AltMainHand = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                },
                AltOffHand = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                },
                AltAquaticWeapon = new()
                {
                    Skin = skin(),
                    Infusion1 = infusion(),
                },

                Raptor = new()
                {
                    Skin = skin(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },

                Springer = new()
                {
                    Skin = skin(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },

                Skimmer = new()
                {
                    Skin = skin(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },

                Jackal = new()
                {
                    Skin = skin(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },

                Griffon = new()
                {
                    Skin = skin(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },

                Warclaw = new()
                {
                    Skin = skin(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },

                Skyscale = new()
                {
                    Skin = skin(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },

                Turtle = new()
                {
                    Skin = skin(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },

                Beetle = new()
                {
                    Skin = skin(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },

                MailCarrier = smallskin(),

                Finisher = smallskin(),

                Glider = new()
                {
                    Skin = skin(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },

                Outfit = new()
                {
                    Skin = skin(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },

                Miniature = smallskin(),

                Chair = smallskin(),

                FishingRod = smallskin(),

                HarvestingTool = smallskin(),

                LoggingTool = smallskin(),

                MiningTool = smallskin(),

                HeldItem = smallskin(),

                Instrument = smallskin(),

                JadeBot = smallskin(),

                Skiff = new()
                {
                    Skin = skin(),
                    Dye1 = dye(),
                    Dye2 = dye(),
                    Dye3 = dye(),
                    Dye4 = dye(),
                },

                Tonic = smallskin(),

                Toy = smallskin(),
            };

            //Debug.WriteLine($"Serialized Data: {Convert.ToBase64String(fashion.ToByteArray())}");

            byte[] serializedData = SerializeAndCompress(fashion);
            string code = Convert.ToBase64String(serializedData);
            Debug.WriteLine($"Serialized and Compressed Data: {code}");

            Fashion decompiledFashion = DecompressAndDeserialize<Fashion>(Convert.FromBase64String(code));


            Debug.WriteLine($"infusion() {decompiledFashion.Head.Infusion1} | {decompiledFashion.Head.Infusion1 == fashion.Head.Infusion1}");
            Debug.WriteLine($"infusion() {decompiledFashion.Ring1.Infusion2} | {decompiledFashion.Ring1.Infusion2 == fashion.Ring1.Infusion2}");
            Debug.WriteLine($"Infusion3 {decompiledFashion.Ring2.Infusion3} | {decompiledFashion.Ring2.Infusion3 == fashion.Ring2.Infusion3}");
            Debug.WriteLine($"Skin {decompiledFashion.Head.Skin} | {decompiledFashion.Head.Skin == fashion.Head.Skin}");
            Debug.WriteLine($"Dye1 {decompiledFashion.Head.Dye1} | {decompiledFashion.Head.Dye1 == fashion.Head.Dye1}");
            Debug.WriteLine($"Dye2 {decompiledFashion.Head.Dye2} | {decompiledFashion.Head.Dye2 == fashion.Head.Dye2}");
            Debug.WriteLine($"Dye3 {decompiledFashion.Head.Dye3} | {decompiledFashion.Head.Dye3 == fashion.Head.Dye3}");
            Debug.WriteLine($"Dye4 {decompiledFashion.Head.Dye4} | {decompiledFashion.Head.Dye4 == fashion.Head.Dye4}");
            Debug.WriteLine($"Smallskin {decompiledFashion.MiningTool} | {decompiledFashion.MiningTool == fashion.MiningTool}");

            return serializedData.ToString();
        }

        public static byte[] SerializeAndCompress<T>(T message) where T : IMessage<T>
        {
            using (var memoryStream = new MemoryStream())
            {
                // Serialize the protobuf message into the memory stream
                message.WriteTo(memoryStream);
                byte[] protobufData = memoryStream.ToArray();

                // Compress the serialized protobuf data
                using (var compressedStream = new MemoryStream())
                using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                {
                    gzipStream.Write(protobufData, 0, protobufData.Length);
                    gzipStream.Close();
                    return compressedStream.ToArray();
                }
            }
        }

        public static T DecompressAndDeserialize<T>(byte[] compressedData) where T : IMessage<T>, new()
        {
            using (var compressedStream = new MemoryStream(compressedData))
            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                // Decompress the byte array
                gzipStream.CopyTo(resultStream);
                byte[] decompressedData = resultStream.ToArray();

                // Deserialize the decompressed data into a protobuf message
                T message = new T();
                message.MergeFrom(decompressedData);
                return message;
            }
        }
    }
}
