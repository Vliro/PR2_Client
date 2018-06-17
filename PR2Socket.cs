using System;
using System.Dynamic;
using Newtonsoft.Json;

namespace PR2_Client
{
    class PR2Socket : BaseSocket
    {
        private LevelHandler m_Level;
        private string user;
        private string pass;
        private string token;
        private int server_id;
        public PR2Socket(string ip, int port, string user, string pass, int server_id) : base(ip, port, user, pass, server_id)
        {
            m_Event += new PR2Event(handleRecvLoginID);
            m_Event += new PR2Event(handleRecvSlot);

            this.server_id = server_id;
            this.user = user;
            this.pass = pass;
            if(isConnected)
            {
                Send("request_login_id`", null);
            }
        }
        private void handleRecvLoginID(PR2Packet m_Pkt)
        {
            if (m_Pkt.opCode.Equals("setLoginID"))
            {
                String m_Result = HttpHandler.Send(user, pass, Int32.Parse(m_Pkt.data[0]), server_id);
                pass = "";
                dynamic m_Exp = new ExpandoObject();
                m_Exp = JsonConvert.DeserializeObject(m_Result);
                token = m_Exp.token;
                m_Level = new LevelHandler(HttpHandler.getCampaign(token));
                Send("get_customize_info`", null);
                Send("set_chat_room`", new String[] { "main" });
                Send("set_right_room`", new String[]{"campaign"});
                m_Level.WriteLevels(true);
            }
        }
        private void handleRecvSlot(PR2Packet m_Pkt)
        {
            if(m_Pkt.opCode.Contains("fillSlot"))
            {
                String m_LevelData = m_Pkt.opCode.Substring(8);
                String[] m_Data = m_LevelData.Split("_");
                int m_LevelID = Int32.Parse(m_Data[0]);
                short slot = short.Parse(m_Pkt.data[0]);
                string name = m_Pkt.data[1];
                int rank = Int32.Parse(m_Pkt.data[2]);
                m_Level.updateLevelSlot(slot, name, rank, m_LevelID);
            } else if(m_Pkt.opCode.Contains("clearSlot")) {
                String m_LevelData = m_Pkt.opCode.Substring(9);
                String[] m_Data = m_LevelData.Split("_");
                short slot = short.Parse(m_Pkt.data[0]);
                int m_LevelID = Int32.Parse(m_Data[0]);
                m_Level.updateLevelSlot(slot, "", -1, m_LevelID);
            }
        }
    }
}