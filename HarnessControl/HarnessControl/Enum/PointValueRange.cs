using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HarnessControl
{
    public static class PointValueRange
    {
        private static Random r = new Random();
        public static int GetRandomValue(string DataType)
        {
            return r.Next(GetRange(DataType));
        }

        public static int[] GetRandomValue(string DataType, int Count)
        {
            int[] IntArray = new int[Count];
            for (int i = 0; i < Count; i++)
            {
                IntArray[i] = GetRandomValue(DataType);
            }
            return IntArray;
        }

        private static int GetRange(string DataType)
        {
            switch (DataType)
            {
                case "AI": return 32767;
                case "AO": return 32767;
                case "BI": return 2;
                case "BO": return 2;
                case "DI": return 4;
                case "CT": return 32767;
                case "Coil": return 2;
                case "Disc": return 2;
                case "IReg": return 32767;
                case "HReg": return 32767;
                case "WCoi": return 2;
                case "WDis": return 2;
                case "WIRe": return 32767;
                case "WHRe": return 32767;
                case "CSCNA": return 2;
                case "CDCNA": return 2;
                case "CRCNA": return 2;
                case "CBONA": return 2;
                case "CSENA": return 32767;
                case "CSENB": return 32767;
                case "CSENC": return 32767;
                case "MSP": return 2;
                case "MDP": return 4;
                case "MST": return 64;
                case "MIT": return 32767;
                case "MBS": return 32767;
                case "MVN": return 32767;
                case "MVS": return 32767;
                case "MVF": return 32767;
                case "DP": return 3;
                case "SP": return 2;
                case "FT": return 65535;
                case "Integer 8": return 127;
                case "Integer 32": return 65535;
                case "Unsigned 8": return 255;
                case "Unsigned 16": return 65535;
                case "Unsigned 32": return 65535;
                default:
                    throw new Exception("Error DataType:" + DataType);
            }
        }
    }
}
