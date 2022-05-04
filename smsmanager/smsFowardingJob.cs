using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using Quartz.Impl;
using Quartz.Simpl;
using Quartz.Xml;
using smsmanager.Models;
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
    public class smsFowardingJob : IJob
    {
        
        public async Task Execute(IJobExecutionContext context)
        {
            //Console.WriteLine(File.Exists(orgCodePath));
            await Task.Run(() => emailForward());
        }

        static Hashtable ht = new Hashtable();
        static Hashtable htWc = new Hashtable();
        static Hashtable htSa = new Hashtable();

        public void emailForward()
        {
            string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
            string smssavedPath = AppDomain.CurrentDomain.BaseDirectory + "smssaved.json";
            //Console.WriteLine(File.Exists(orgCodePath));
            if (File.Exists(orgCodePath))
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
                    if (status == "1")
                    {
                        Thread.Sleep(1000);
                        var psi = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 --messaging-list-sms");
                        psi.RedirectStandardOutput = true;
                        using (var process = System.Diagnostics.Process.Start(psi))
                        {
                            var output = process.StandardOutput.ReadToEnd();
                            process.Kill();
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
                                                process2.Kill();
                                                if (output2 != string.Empty)
                                                {
                                                    string[] qline2 = output2.Split(Environment.NewLine.ToCharArray());
                                                    string tel = qline2[3].Split("|")[1].Trim().Split(":")[1].Trim();
                                                    string text = qline2[4].Split("|      text:")[1].Trim();
                                                    ht.Add(sid, tel + "_" + text);
                                                    MailAddress to = new MailAddress(element.GetElementsByTagName("reciveEmial")[0].InnerText);
                                                    MailAddress from = new MailAddress(element.GetElementsByTagName("sendEmial")[0].InnerText);
                                                    MailMessage mm = new MailMessage(from, to);
                                                    SmtpClient sc = new SmtpClient(element.GetElementsByTagName("smtpHost")[0].InnerText);
                                                    try
                                                    {
                                                        mm.Subject = "短信转发" + tel;
                                                        mm.Body = text;
                                                        sc.DeliveryMethod = SmtpDeliveryMethod.Network;
                                                        sc.Credentials = new NetworkCredential(element.GetElementsByTagName("sendEmial")[0].InnerText.Split('@')[0], element.GetElementsByTagName("emailKey")[0].InnerText);
                                                        sc.Send(mm);
                                                        Console.WriteLine("转发成功");
                                                        mm.Dispose();
                                                        sc.Dispose();
                                                    }
                                                    catch (SmtpException ex)
                                                    {
                                                        mm.Dispose();
                                                        sc.Dispose();
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
                            process.Kill();
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
                                                process2.Kill();
                                                if (output2 != string.Empty)
                                                {
                                                    string[] qline2 = output2.Split(Environment.NewLine.ToCharArray());
                                                    string tel = qline2[3].Split("|")[1].Trim().Split(":")[1].Trim();
                                                    string text = qline2[4].Split("|      text:")[1].Trim();
                                                    htWc.Add(sid, tel + "_" + text);
                                                    try
                                                    {
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
            if (File.Exists(smssavedPath))
            {
                Thread.Sleep(1000);
                var psi = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 --messaging-list-sms");
                psi.RedirectStandardOutput = true;
                using (var process = System.Diagnostics.Process.Start(psi))
                {
                    var output = process.StandardOutput.ReadToEnd();
                    process.Kill();
                    if (output != string.Empty && output.Trim() != "No sms messages were found")
                    {
                        //int count = 0;
                        string[] qline = output.Split(Environment.NewLine.ToCharArray());
                        for (int i = 0; i < qline.Count() - 1; i++)
                        {
                            string[] theRow = qline[i].Split("(");
                            if (theRow[1].Trim() == "received)")
                            {
                                if (!htSa.Contains(theRow[0].Trim().Split("SMS/")[1].ToString().Trim()))
                                {
                                    string sid = theRow[0].Trim().Split("SMS/")[1].ToString().Trim();
                                    var psi2 = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 -s " + sid);
                                    psi2.RedirectStandardOutput = true;
                                    using (var process2 = System.Diagnostics.Process.Start(psi2))
                                    {
                                        var output2 = process2.StandardOutput.ReadToEnd();
                                        process2.Kill();
                                        if (output2 != string.Empty)
                                        {
                                            string[] qline2 = output2.Split(Environment.NewLine.ToCharArray());
                                            string tel = qline2[3].Split("|")[1].Trim().Split(":")[1].Trim();
                                            string text = qline2[4].Split("|      text:")[1].Trim();
                                            string timestamp = qline2[8].Split("|")[1].Trim().Split("timestamp:")[1].Trim();
                                            JArray ja = new JArray();
                                            StreamReader file = new StreamReader(smssavedPath, Encoding.Default);
                                            string jsonstring = file.ReadToEnd();
                                            file.Close();
                                            file.Dispose();
                                            bool SmsExistJudge = false;
                                            if (jsonstring.Length>0)
                                            {
                                                Sms[] datas = JsonConvert.DeserializeObject<Sms[]>(jsonstring);
                                                foreach(Sms item in datas)
                                                {
                                                    if (timestamp + "_" + tel + "_" + sid== item.sid)
                                                    {
                                                        SmsExistJudge = true;
                                                    }
                                                    JObject jobj = new JObject();
                                                    jobj.Add("sid", item.sid);
                                                    jobj.Add("tel", item.tel);
                                                    jobj.Add("text", item.text);
                                                    ja.Add(jobj);
                                                }
                                            }
                                            if (!SmsExistJudge)
                                            {
                                                JObject jobj1 = new JObject();
                                                jobj1.Add("sid", timestamp + "_" + tel + "_" + sid);
                                                jobj1.Add("tel", tel);
                                                jobj1.Add("text", text);
                                                ja.Add(jobj1);
                                            }
                                            using (FileStream fs = new FileStream(smssavedPath, FileMode.OpenOrCreate,FileAccess.ReadWrite, FileShare.ReadWrite))
                                            {
                                                fs.Seek(0, SeekOrigin.Begin);
                                                fs.SetLength(0);
                                                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                                                {
                                                    sw.Write(ja.ToString());
                                                }
                                            }
                                            htSa.Add(sid, tel + "_" + text);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    public class smsFowardingJobScheduler
    {
        public static async Task Start()
        {
            //-------demo1 无参数 简单调用的方法-----------------
            ISchedulerFactory schedulefactory = new StdSchedulerFactory();//实例化调度器工厂
            IScheduler scheduler = await schedulefactory.GetScheduler();//实例化调度器

            IJobDetail job1 = JobBuilder.Create<smsFowardingJob>()//创建一个作业
                .WithIdentity("smsFowardingJob", "groupa")
                .Build();

            ITrigger trigger1 = TriggerBuilder.Create()//创建一个触发器
                .WithIdentity("demotrigger1", "groupa")
                .StartNow()
                .WithSimpleSchedule(b => b.WithIntervalInSeconds(1)//
                .RepeatForever())//无限循环执行
                .Build();

            await scheduler.ScheduleJob(job1, trigger1);//把作业，触发器加入调度器

            //XMLSchedulingDataProcessor processor = new XMLSchedulingDataProcessor(new SimpleTypeLoadHelper());
            //processor.ProcessFileAndScheduleJobs("~/quartz_jobs.xml", scheduler);
            await scheduler.Start();
            //await Task.Delay(TimeSpan.FromSeconds(5));
            //await scheduler.Shutdown();

        }
    }
}
