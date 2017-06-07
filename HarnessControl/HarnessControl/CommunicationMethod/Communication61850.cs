using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HarnessControl
{
    public class Communication61850 : CommunicationBase
    {
        public Communication61850()
        {

        }

        public Communication61850(string[] _SettingInfo)
        {
            if (END == EnumEnd.Backend)
            {
                //BackendServer.HarnessSocket = SocketClient;
                Task.Factory.StartNew(new Action(() =>
                {
                    BackendServer.Start<BackendSocketAccept_Harness>(BackendSocketPort);
                }));
            }
        }

        public override void ParsingXML(XmlNode node)
        {
            throw new NotImplementedException();
        }

        public override void Setup()
        {
            throw new NotImplementedException();
        }
        public override void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
