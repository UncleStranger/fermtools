﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;


namespace fermtools
{
    class MinerRemote
    {
        public CommandSet statcmd;
        public CommandSet restartcmd;
        public SatisticResult statres;
        public RestartResult restartres;
        public string server;
        public StringBuilder report;
        public int port;
        public MinerRemote()
        {
            statcmd = new CommandSet("miner_getstat1");
            restartcmd = new CommandSet("miner_restart");
            statres = new SatisticResult();
            report = new StringBuilder();
            server = "127.0.0.1";
            port = 3333;
        }
        public MinerRemote(int p)
        {
            statcmd = new CommandSet("miner_getstat1");
            restartcmd = new CommandSet("miner_restart");
            statres = new SatisticResult();
            report = new StringBuilder();
            server = "127.0.0.1";
            port = p;
        }
        public bool GetStatistic()
        {
            report.Clear();
            string json = JsonConvert.SerializeObject(statcmd, Formatting.Indented);
            if (Communicate(ref json))
            {
                try { statres = JsonConvert.DeserializeObject<SatisticResult>(json); }
                catch { return false; }
                if (statres != null)
                {
                    if (statres.result.Count == 9)
                    {
                        report.AppendLine("Version: Claymore " + statres.result[0]);
                        report.AppendLine("Runing, min: " + statres.result[1]);
                        report.AppendLine("ETH  (hr, sh, rej): " + statres.result[2]);
                        report.AppendLine("ETH hr for GPUs: " + statres.result[3]);
                        report.AppendLine("DCR  (hr, sh, rej): " + statres.result[4]);
                        report.AppendLine("DCR hr for GPUs: " + statres.result[5]);
                        report.AppendLine("GPUs  (T, fan %): " + statres.result[6]);
                        report.AppendLine(statres.result[7]);
                        report.AppendLine("ETH inv,ETH pool sw,DCR inv,DCR pool sw: " + statres.result[8]);
                    }
                    else
                        report.AppendLine("This version Fermtools is not compatible with the version miner.");
                    return true;
                }
            }
            return false;
        }
        public bool RestartMiner()
        {
            string json = JsonConvert.SerializeObject(restartcmd, Formatting.Indented);
            if (Communicate(ref json))
            {
                try { restartres = JsonConvert.DeserializeObject<RestartResult>(json); }
                catch { return false; }
                if (statres != null)
                    return true;
            }
            return false;
        }
        private bool Communicate(ref string json)
        {
            bool bRes = false;
            if (string.IsNullOrEmpty(server))
                return bRes;
            if (port <= 0)
                return bRes;
            try
            {
                TcpClient client = new TcpClient(server, port);
                client.ReceiveTimeout = 2000;
                client.SendTimeout = 2000;
                NetworkStream stream = client.GetStream();

                Byte[] data = System.Text.Encoding.ASCII.GetBytes(json);
                stream.Write(data, 0, data.Length);

                data = new Byte[512];
                int bytes = stream.Read(data, 0, data.Length);
                stream.Close();
                client.Close();

                if (bytes > 0)
                {
                    json = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                    bRes = true;
                }
            }
            catch { }
            return bRes;
        }
       
        public class CommandSet
        {
            public int id { get; set; }
            public string jsonrpc { get; set; }
            public string method { get; set; }
            public CommandSet(string cmd)
            {
                id = 0;
                jsonrpc = "2.0";
                method = cmd;
            }
        }
        public class SatisticResult
        {
            public SatisticResult()
            {
                result = new List<string>();
            }
            public int id { get; set; }
            public object error { get; set; }
            public List<string> result { get; set; }
        }
        public class RestartResult
        {
            public int id { get; set; }
            public object error { get; set; }
            public bool result { get; set; }
        }
    }
}
