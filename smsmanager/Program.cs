using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace smsmanager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            //Thread email = new Thread(new ThreadStart(emailForward));
            //email.Start();
            isXmlExist();
            Task.Run(() => emailForward());
            host.Run();
        }

        public static void isXmlExist()
        {
            string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
            if (!File.Exists(orgCodePath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                //创建根节点  
                XmlNode root = xmlDoc.CreateElement("userSettings");
                xmlDoc.AppendChild(root);
                CreateNode(xmlDoc, root, "username", "admin");
                CreateNode(xmlDoc, root, "password", "admin");
                CreateNode(xmlDoc, root, "emailFowardStatus", "0");
                CreateNode(xmlDoc, root, "smtpHost", "");
                CreateNode(xmlDoc, root, "smtpPort", "");
                CreateNode(xmlDoc, root, "emailKey", "");
                CreateNode(xmlDoc, root, "sendEmial", "");
                CreateNode(xmlDoc, root, "reciveEmial", "");
                CreateNode(xmlDoc, root, "WeChatQYFowardStatus", "");
                CreateNode(xmlDoc, root, "WeChatQYID", "");
                CreateNode(xmlDoc, root, "WeChatQYApplicationID", "");
                CreateNode(xmlDoc, root, "WeChatQYApplicationSecret", "");
                try
                {
                    xmlDoc.Save(orgCodePath);
                }
                catch (Exception e)
                {
                    //显示错误信息  
                    Console.WriteLine(e.Message);
                }
            }
        }
        public static void CreateNode(XmlDocument xmlDoc, XmlNode parentNode, string name, string value)
        {
            XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
            node.InnerText = value;
            parentNode.AppendChild(node);
        }

        public static void emailForward() 
        {
            Hashtable ht = new Hashtable();
            Hashtable htWc = new Hashtable();
            string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
            //Console.WriteLine(File.Exists(orgCodePath));
            while (File.Exists(orgCodePath))
            {
                XmlDocument xmldoc = new XmlDocument();
                try
                {
                    xmldoc.Load(orgCodePath);
                }
                catch 
                {
                    Thread.Sleep(1000);
                    xmldoc.Load(orgCodePath);
                }
                XmlNodeList topM = xmldoc.SelectNodes("//userSettings");
                foreach (XmlElement element in topM)
                {
                    string status = element.GetElementsByTagName("emailFowardStatus")[0].InnerText;
                    string qystatus = element.GetElementsByTagName("WeChatQYFowardStatus")[0].InnerText;
                    //Console.WriteLine(qystatus);
                    if (status=="1")
                    {
                        Thread.Sleep(1000);
                        var psi = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 --messaging-list-sms");
                        psi.RedirectStandardOutput = true;
                        using (var process = System.Diagnostics.Process.Start(psi))
                        {
                            var output = process.StandardOutput.ReadToEnd();
                            if (output != string.Empty && output.Trim() != "No sms messages were found")
                            {
                                //int count = 0;
                                string[] qline = output.Split(Environment.NewLine.ToCharArray());
                                for (int i = 0; i < qline.Count() - 1; i++)
                                {
                                    string[] theRow = qline[i].Split("(");
                                    if (theRow[1].Trim() == "received)")
                                    {
                                        if (!ht.Contains(theRow[0].Trim().Split("SMS/")[1].ToString().Trim())) 
                                        {
                                            string sid = theRow[0].Trim().Split("SMS/")[1].ToString().Trim();
                                            var psi2 = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 -s " + sid);
                                            psi2.RedirectStandardOutput = true;
                                            using (var process2 = System.Diagnostics.Process.Start(psi2))
                                            {
                                                var output2 = process2.StandardOutput.ReadToEnd();
                                                if (output2 != string.Empty)
                                                {
                                                    string[] qline2 = output2.Split(Environment.NewLine.ToCharArray());
                                                    string tel = qline2[3].Split("|")[1].Trim().Split(":")[1].Trim();
                                                    string text = qline2[4].Split("|")[1].Trim().Split(":")[1].Trim();
                                                    ht.Add(sid, tel+"_"+ text);
                                                    MailAddress to = new MailAddress(element.GetElementsByTagName("reciveEmial")[0].InnerText);
                                                    MailAddress from = new MailAddress(element.GetElementsByTagName("sendEmial")[0].InnerText);
                                                    MailMessage mm = new MailMessage(from ,to);
                                                    SmtpClient sc = new SmtpClient(element.GetElementsByTagName("smtpHost")[0].InnerText);
                                                    try
                                                    {
                                                        mm.Subject = "短信转发" + tel;
                                                        mm.Body = text;
                                                        sc.DeliveryMethod = SmtpDeliveryMethod.Network;
                                                        sc.Credentials = new NetworkCredential(element.GetElementsByTagName("sendEmial")[0].InnerText.Split('@')[0], element.GetElementsByTagName("emailKey")[0].InnerText);
                                                        sc.Send(mm);
                                                        Console.WriteLine("转发成功");
                                                    }
                                                    catch (SmtpException ex)
                                                    {
                                                        Console.WriteLine(ex);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (qystatus == "1")
                    {
                        Thread.Sleep(1000);
                        var psi = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 --messaging-list-sms");
                        psi.RedirectStandardOutput = true;
                        using (var process = System.Diagnostics.Process.Start(psi))
                        {
                            var output = process.StandardOutput.ReadToEnd();
                            if (output != string.Empty && output.Trim() != "No sms messages were found")
                            {
                                //int count = 0;
                                string[] qline = output.Split(Environment.NewLine.ToCharArray());
                                for (int i = 0; i < qline.Count() - 1; i++)
                                {
                                    string[] theRow = qline[i].Split("(");
                                    if (theRow[1].Trim() == "received)")
                                    {
                                        if (!htWc.Contains(theRow[0].Trim().Split("SMS/")[1].ToString().Trim()))
                                        {
                                            string sid = theRow[0].Trim().Split("SMS/")[1].ToString().Trim();
                                            var psi2 = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 -s " + sid);
                                            psi2.RedirectStandardOutput = true;
                                            using (var process2 = System.Diagnostics.Process.Start(psi2))
                                            {
                                                var output2 = process2.StandardOutput.ReadToEnd();
                                                if (output2 != string.Empty)
                                                {
                                                    try
                                                    {
                                                        string[] qline2 = output2.Split(Environment.NewLine.ToCharArray());
                                                        string tel = qline2[3].Split("|")[1].Trim().Split(":")[1].Trim();
                                                        string text = qline2[4].Split("|")[1].Trim().Split(":")[1].Trim();
                                                        string corpid = element.GetElementsByTagName("WeChatQYID")[0].InnerText;
                                                        string corpsecret = element.GetElementsByTagName("WeChatQYApplicationSecret")[0].InnerText;
                                                        string agentid = element.GetElementsByTagName("WeChatQYApplicationID")[0].InnerText;
                                                        string url = "https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid=" + corpid + "&corpsecret=" + corpsecret;
                                                        string result = HttpHelper.HttpGet(url);
                                                        JObject jsonObj = JObject.Parse(result);
                                                        string errcode = jsonObj["errcode"].ToString();
                                                        string errmsg = jsonObj["errmsg"].ToString();
                                                        if (errcode == "0" && errmsg == "ok")
                                                        {
                                                            string access_token = jsonObj["access_token"].ToString();
                                                            JObject obj = new JObject();
                                                            JObject obj1 = new JObject();
                                                            obj.Add("touser", "@all");
                                                            obj.Add("toparty", "");
                                                            obj.Add("totag", "");
                                                            obj.Add("msgtype", "text");
                                                            obj.Add("agentid", agentid);
                                                            obj1.Add("content", "短信转发_" + tel + "\n" + text);
                                                            obj.Add("text", obj1);
                                                            obj.Add("safe", 0);
                                                            obj.Add("enable_id_trans", 0);
                                                            obj.Add("enable_duplicate_check", 0);
                                                            obj.Add("duplicate_check_interval", 1800);
                                                            string msgurl = "https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token=" + access_token;
                                                            string msgresult = HttpHelper.Post(msgurl, obj);
                                                            JObject jsonObjresult = JObject.Parse(msgresult);
                                                            string errcode1 = jsonObjresult["errcode"].ToString();
                                                            string errmsg1 = jsonObjresult["errmsg"].ToString();
                                                            if (errcode == "0" && errmsg == "ok")
                                                            {
                                                                htWc.Add(sid, tel + "_" + text);
                                                                Console.WriteLine("企业微信转发成功");
                                                            }
                                                            else
                                                            {
                                                                Console.WriteLine(errmsg);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine(errmsg);
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Console.WriteLine(ex);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://*:8080");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
