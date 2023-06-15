using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using smsmanagers;
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
            isXmlExist();
            var host = CreateHostBuilder(args).Build();
            Thread email = new Thread(new ThreadStart(smsForward.smsTextForward));
            email.Start();
            //Task.Run(() => emailForward());
            //smsFowardingJobScheduler.Start().GetAwaiter();
            host.Run();
        }

        public static void isXmlExist()
        {
            string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
            string smssavedPath = AppDomain.CurrentDomain.BaseDirectory + "smssaved.json";
            if (!File.Exists(orgCodePath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                //创建根节点  
                XmlNode root = xmlDoc.CreateElement("userSettings");
                xmlDoc.AppendChild(root);
                CreateNode(xmlDoc, root, "urlport", "8080");
                CreateNode(xmlDoc, root, "username", "admin");
                CreateNode(xmlDoc, root, "password", "admin");
                CreateNode(xmlDoc, root, "emailFowardStatus", "0");
                CreateNode(xmlDoc, root, "smtpHost", "");
                CreateNode(xmlDoc, root, "smtpPort", "");
                CreateNode(xmlDoc, root, "emailKey", "");
                CreateNode(xmlDoc, root, "sendEmial", "");
                CreateNode(xmlDoc, root, "reciveEmial", "");
                CreateNode(xmlDoc, root, "WeChatQYFowardStatus", "0");
                CreateNode(xmlDoc, root, "WeChatQYID", "");
                CreateNode(xmlDoc, root, "WeChatQYApplicationID", "");
                CreateNode(xmlDoc, root, "WeChatQYApplicationSecret", "");
                CreateNode(xmlDoc, root, "pushPlusFowardStatus", "0");
                CreateNode(xmlDoc, root, "pushPlusToken", "");
                CreateNode(xmlDoc, root, "tgBotFowardStatus", "0");
                CreateNode(xmlDoc, root, "tgBotToken", "");
                CreateNode(xmlDoc, root, "tgBotChatID", "");
                CreateNode(xmlDoc, root, "ddBotFowardStatus", "0");
                CreateNode(xmlDoc, root, "ddBotAccToken", "");
                CreateNode(xmlDoc, root, "ddBotSecret", "");

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
            if (!File.Exists(smssavedPath))
            {
                FileStream fs = new FileStream(smssavedPath, FileMode.Create);
                fs.Close();
                fs.Dispose();
            }
        }
        public static void CreateNode(XmlDocument xmlDoc, XmlNode parentNode, string name, string value)
        {
            XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
            node.InnerText = value;
            parentNode.AppendChild(node);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(orgCodePath);
                    XmlNodeList topM = xmldoc.SelectNodes("//userSettings");
                    foreach (XmlElement element in topM)
                    {
                        try
                        {
                            string urlport = element.GetElementsByTagName("urlport")[0].InnerText;
                            if (!string.IsNullOrEmpty(urlport))
                            {
                                webBuilder.UseUrls("http://*:" + urlport);
                            }
                            else
                            {
                                webBuilder.UseUrls("http://*:8080");
                            }
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine($"Error: {e.Message}");
                            Console.WriteLine("已使用默认端口8080，请检查配置文件内的端口是否填写正确");
                            webBuilder.UseUrls("http://*:8080");
                        }
                    }
                    webBuilder.UseStartup<Startup>();
                });
    }
}
