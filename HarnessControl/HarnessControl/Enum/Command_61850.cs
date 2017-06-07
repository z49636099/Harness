using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HarnessControl
{
    class Command_61850
    {
        public static string GetCommandFolder(string ShellCommand)
        {
            switch (ShellCommand)
            {
                case "show.sh": return "sql";
                case "show2.sh": return "sql";
                case "import.sh": return "sql";
                case "submit.sh": return "sql";
                case "update.sh": return "sql";
                case "connect.sh": return "sql";
                case "importsim.sh": return "sql";
                case "del_client.sh": return "sql";
                case "del_server.sh": return "sql";
                case "start_client.sh": return "scripts";
                case "start_server.sh": return "scripts";
                default:
                    throw new Exception("No such Command : " + ShellCommand);
            }
        }

        public static string GetDataType(string DataType)
        {
            switch (DataType)
            {
                case "FT": return "FLOAT";
                case "SP": return "BOOLEAN";
                case "DP": return "BITSTRING";
                case "Integer 8": return "INTEGER";
                case "Integer 32": return "INTEGER";
                case "Unsigned 8": return "INTEGER";
                case "Unsigned 16": return "UNSIGNED";
                case "Unsigned 32": return "UNSIGNED";
                default:
                    throw new Exception("No such DataType : " + DataType);
            }
        }
        
        public static string GetCommandType(string DT)
        {
            string Value = string.Empty;
            switch (DT)
            {
                case "INTEGER":
                case "UNSIGNED":
                case "BOOLEAN":
                    Value = "int_val";
                    break; ;
                case "BITSTRING":
                case "TIMESTAMP":
                case "STRING":
                case "OCTETSTRING":
                case "MMSSTRING":
                    Value = "string_val";
                    break;
                case "FLOAT":
                    Value = "float_val";
                    break; ;
            }
            return Value;
        }
    }
}
