using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            Task.Run(() => emailForward());
            host.Run();
        }
        
        public static void emailForward() 
        {
            Hashtable ht = new Hashtable();
            string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
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
