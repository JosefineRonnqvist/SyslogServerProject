using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogServerProject.SyslogHandlers
{
    internal class LogHandler
    {
        private string _outputPath = @"C:\Temp\logs\syslog.csv"; //Location to log data
        private string _clientIP;
        private string _logData;
        bool canAccess = false;
        StreamWriter? logWriter = null;

        public LogHandler(string clientIP, string logData)
        {
            _clientIP = clientIP.Trim();
            _logData = logData.Replace(Environment.NewLine, "").Trim();
        }

        /// <summary>
        /// Open stream writer to write log
        /// </summary>
        public void HandleLog()
        {
            int attempts = 0;

            while (true)
            {
                try
                {
                    logWriter = new StreamWriter(_outputPath, true);// Try to open the file for writing 
                    canAccess = true; 
                    break;
                }
                catch (IOException ex)
                {
                    // Give up after 15 attempts 
                    if (attempts < 15)
                    {
                        attempts++;
                        Thread.Sleep(50);
                    }
                    else
                    {
                        Console.WriteLine(ex.ToString());
                        break;
                    }
                }
            }
            WriteLog();
        }

        /// <summary>
        /// Write a log
        /// </summary>
        void WriteLog()
        {
            // Write the line if the file is accessible 
            if (canAccess)
            {
                string iprep_dest_score = GetValue("iprep_dest_score=");
                string categories = GetValue("categories=");               
                string connsrcip = GetValue("connsrcip=");
                string ip = GetValue("ip=");

                logWriter.WriteLine(DateTime.Now.ToString());
                logWriter.WriteLine($"rule={GetValue("rule=")}");
                logWriter.WriteLine($"categories={categories}");
                logWriter.WriteLine($"ip={ip}");
                logWriter.WriteLine($"connsrcip={connsrcip}");
                logWriter.WriteLine($"conndestip={GetValue("conndestip=")}");
                logWriter.WriteLine($"connnewdestip={GetValue("connnewdestip=")}");
                logWriter.WriteLine($"score={GetValue("score=")}");
                logWriter.WriteLine($"iprep_src_score={GetValue("iprep_src_score=")}");
                logWriter.WriteLine($"iprep_dest={GetValue("iprep_dest=")}");
                logWriter.WriteLine($"iprep_dest_score={iprep_dest_score}");
                logWriter.Close();
                CheckValues checkValues = new();
                checkValues.CheckValue(iprep_dest_score,categories,connsrcip,ip);
            }
        }

        /// <summary>
        /// Search field name and find its value
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private string GetValue(string fieldName)
        {
            string value = "";
            string[] parts = _logData.Split(fieldName);
            if (parts.Length == 2)
            {
                string[] valueParts = parts[1].Split(" ");
                if (valueParts.Length > 0)
                {
                    value = valueParts[0]; 
                }
            }
            return value;
        }
    }
}
