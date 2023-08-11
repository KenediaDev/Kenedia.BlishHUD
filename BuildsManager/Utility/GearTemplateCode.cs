using System;

namespace Kenedia.Modules.BuildsManager.Utility
{
    public static class GearTemplateCode
    {
 
        public static string PrepareBase64String(string code)
        {
            if (code.StartsWith("[&")) code = code.Substring(2);
            if (code.EndsWith("]")) code = code.Substring(0, code.Length - 1);
            return code;
        }

        public static string DecodeBase64ToBytes(string code)
        {
            if (code.StartsWith("[&")) code = code.Substring(2);
            if (code.EndsWith("]")) code = code.Substring(0, code.Length - 1);
            return code;
        }

        public static byte[] RemoveFromStart(byte[] array, int index)
        {
            int newArrayLength = array.Length - index;
            byte[] newArray = new byte[newArrayLength];

            Array.Copy(array, index, newArray, 0, newArrayLength);

            return newArray;
        }

        public static string EncodeShortsToBase64(short[] shorts)
        {
            // Convert the array of shorts to bytes
            byte[] byteArray = new byte[shorts.Length * 2];
            for (int i = 0; i < shorts.Length; i++)
            {
                byte[] shortBytes = BitConverter.GetBytes(shorts[i]);
                Array.Copy(shortBytes, 0, byteArray, i * 2, 2);
            }

            // Encode the byte array using Base64
            string encodedString = $"[&{Convert.ToBase64String(byteArray)}]";
            return encodedString;
        }

        public static short[] DecodeBase64ToShorts(string encodedString)
        {
            encodedString = PrepareBase64String(encodedString);
            short[] shortArray = new short[0];

            try
            {
                byte[] byteArray = Convert.FromBase64String(encodedString);
                shortArray = new short[byteArray.Length / 2];

                for (int i = 0; i < shortArray.Length; i++)
                {
                    shortArray[i] = BitConverter.ToInt16(byteArray, i * 2);
                }
            }
            catch { }

            return shortArray;
        }
    }
}
