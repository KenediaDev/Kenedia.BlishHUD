using Kenedia.Modules.FashionManager.Models;
using Newtonsoft.Json.Linq;
using SharpDX.MediaFoundation;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.FashionManager.Utility
{
    public class FashionChatCode
    {
        public enum ChatCodePosition
        {
            MainHandSkin,
            MainHandInfusion,

            OffHandSkin,
            OffHandInfusion,

            AquaticWeaponSkin,
            AquaticWeaponInfusion1,
            AquaticWeaponInfusion2,

            AltMainHandSkin,
            AltMainHandInfusion,

            AltOffHandSkin,
            AltOffHandInfusion,

            AltAquaticWeaponSkin,
            AltAquaticWeaponInfusion1,
            AltAquaticWeaponInfusion2,

            AquaBreatherSkin,
            AquaBreatherInfusion1,
            AquaBreatherDye1,
            AquaBreatherDye2,
            AquaBreatherDye3,
            AquaBreatherDye4,

            HelmSkin,
            HelmInfusion1,
            HelmDye1,
            HelmDye2,
            HelmDye3,
            HelmDye4,

            ShouldersSkin,
            ShouldersInfusion1,
            ShouldersDye1,
            ShouldersDye2,
            ShouldersDye3,
            ShouldersDye4,

            ChestSkin,
            ChestInfusion1,
            ChestDye1,
            ChestDye2,
            ChestDye3,
            ChestDye4,

            GlovesSkin,
            GlovesInfusion1,
            GlovesDye1,
            GlovesDye2,
            GlovesDye3,
            GlovesDye4,

            LeggingsSkin,
            LeggingsInfusion1,
            LeggingsDye1,
            LeggingsDye2,
            LeggingsDye3,
            LeggingsDye4,

            FeetSkin,
            FeetInfusion1,
            FeetDye1,
            FeetDye2,
            FeetDye3,
            FeetDye4,

            BackSkin,
            BackInfusion1,
            BackDye1,
            BackDye2,
            BackDye3,
            BackDye4,

            GliderSkin,
            GliderDye1,
            GliderDye2,
            GliderDye3,
            GliderDye4,
        }

        internal static string ParseChatCode(FashionTemplate fashionTemplate)
        {
            int[] lenghts = [120, 96, 75, 46];

            foreach (int l in lenghts)
            {
                int[] values = new int[l];
                var random = new Random();

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = random.Next(0, 65001);
                }

                // Pack the 116 values into a byte array (each value uses 2 bytes)
                byte[] packedBytes = new byte[l * 2];
                for (int i = 0; i < values.Length; i++)
                {
                    packedBytes[i * 2] = (byte)(values[i] & 0xFF);           // Lower byte
                    packedBytes[i * 2 + 1] = (byte)((values[i] >> 8) & 0xFF); // Upper byte
                }

                // Convert the packed byte array into a Base64 string
                string base64String = Convert.ToBase64String(packedBytes);

                Debug.WriteLine($"Bytes: {l} | Length: {base64String.Length} | {base64String}");
            }

            return string.Empty;
        }
    }
}
