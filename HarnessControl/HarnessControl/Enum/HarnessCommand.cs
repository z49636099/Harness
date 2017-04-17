using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarnessControl
{
    public static class HarnessCommand
    {
        /// <summary>Backend</summary>
        /// <param name="DataType"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static string GetServerCommand(string DataType, EnumProtocolType Type)
        {
            string Cmd = GetCommand(DataType);
            switch (Type)
            {
                case EnumProtocolType.DNP3:
                case EnumProtocolType.Modbus:
                    return "s" + Cmd;
                case EnumProtocolType.IEC101:
                    return "s101" + Cmd;
                case EnumProtocolType.IEC104:
                    return "s104" + Cmd;
                default:
                    throw new Exception("Get Server Command Fail.");
            }
        }

        /// <summary>Frontend</summary>
        /// <param name="DataType"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static string GetClientCommand(string DataType, EnumProtocolType Type)
        {
            string Cmd = GetCommand(DataType);
            switch (Type)
            {
                case EnumProtocolType.DNP3:
                case EnumProtocolType.Modbus:
                    return "m" + Cmd;
                case EnumProtocolType.IEC101:
                    return "m101" + Cmd;
                case EnumProtocolType.IEC104:
                    return "m104" + Cmd;
                default:
                    throw new Exception("Get Server Command Fail.");
            }
        }

        private static string GetCommand(string DataType)
        {
            switch (DataType)
            {
                case "MSP": return "msp";
                case "MDP": return "mdp";
                case "MST": return "mst";
                case "MIT": return "mit";
                case "MBS": return "mbo";
                case "MVN": return "mmena";
                case "MVS": return "mmenb";
                case "MVF": return "mmenc";
                case "CSCNA": return "msp";
                case "CDCNA": return "mdp";
                case "CRCNA": return "mst";
                case "CBONA": return "mbo";
                case "CSENA": return "mmena";
                case "CSENB": return "mmenb";
                case "CSENC": return "mmenc";
                case "Coil": return "mbcoil";
                case "WCoi": return "mbcoil";
                case "Disc": return "mbdinput";
                case "IReg": return "mbireg";
                case "HReg": return "mbhreg";
                case "WHRe": return "mbhreg";
                case "AI": return "dnpanlgin";
                case "AO": return "dnpanlgout";
                case "BI": return "dnpbinin";
                case "BO": return "dnpbinout";
                case "DI": return "dnpdblin";
                case "CT": return "dnpcntr";
            default:
                    throw new Exception("No such DataType : " + DataType);
        }
    }
    }
}
