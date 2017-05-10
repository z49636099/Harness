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
        public static string GetSlaveCommand(string DataType, EnumProtocolType Type)
        {
            string Cmd = SlaveCommand(DataType);
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
        public static string GetMasterCommand(string DataType, EnumProtocolType Type)
        {
            string Cmd = MasterCommand(DataType);
            switch (Type)
            {
                case EnumProtocolType.DNP3:
                case EnumProtocolType.Modbus:
                    return Cmd;
                case EnumProtocolType.IEC101:
                    return "m101" + DataType.ToLower();
                case EnumProtocolType.IEC104:
                    return "m104" + DataType.ToLower();
                default:
                    throw new Exception("Get Server Command Fail.");
            }
        }

        private static string MasterCommand(string DataType)
        {
            switch (DataType)
            {
                case "Coil": return "mmbreadcoils";
                case "WCoi": return "mmbwritecoil";
                case "Disc": return "mmbreaddinputs";
                case "IReg": return "mmbreadiregs";
                case "HReg": return "mmbreadhregs";
                case "WHRe": return "mmbwritehreg";
                case "AO": return "mdnpanlgcmd";
                case "BO": return "mdnpbincmd";

                default:
                    throw new Exception("No such DataType : " + DataType);
            }
        }

        private static string SlaveCommand(string DataType)
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
