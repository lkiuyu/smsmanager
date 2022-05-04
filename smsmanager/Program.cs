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
            //Task.Run(() => emailForward());
            smsFowardingJobScheduler.Start().GetAwaiter();
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
                CreateNode(xmlDoc, root, "WeChatQYFowardStatus", "0");
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


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://*:8080");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
