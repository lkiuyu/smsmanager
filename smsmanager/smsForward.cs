using ModemManager1.DBus;
using System;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Tmds.DBus;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using smsmanager;
using Newtonsoft.Json;
using smsmanager.Models;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography;
using System.Text;

namespace smsmanagers
{
    public static class smsForward
    {
        public static void smsTextForward()
        {
            string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
            string smssavedPath = AppDomain.CurrentDomain.BaseDirectory + "smssaved.json";
            if (File.Exists(orgCodePath)&& File.Exists(smssavedPath))
            {
                Task.Run(async () =>
                {
                    using (var connection = new Connection(Address.System))
                    {
                        await connection.ConnectAsync();
                        var objectPath = new ObjectPath("/org/freedesktop/ModemManager1/Modem/0");
                        var service = "org.freedesktop.ModemManager1";
                        var imsg = connection.CreateProxy<IMessaging>(service, objectPath);
                        await imsg.WatchAddedAsync(
                         async change =>
                         {
                             if (change.received)
                             {
                                 string status = "";
                                 string qystatus = "";
                                 string ppstatus = "";
                                 string tgbotStatus = "";
                                 string smtpHost = "";
                                 string smtpPort = "";
                                 string emailKey = "";
                                 string sendEmial = "";
                                 string reciveEmial = "";
                                 string corpid = "";
                                 string corpsecret = "";
                                 string agentid = "";
                                 string pptoken = "";
                                 string tgbToken = "";
                                 string tgbChatID = "";
                                 XmlDocument xmldoc = new XmlDocument();
                                 xmldoc.Load(orgCodePath);
                                 XmlNodeList topM = xmldoc.SelectNodes("//userSettings");
                                 foreach (XmlElement element in topM)
                                 {
                                     status = element.GetElementsByTagName("emailFowardStatus")[0].InnerText;
                                     qystatus = element.GetElementsByTagName("WeChatQYFowardStatus")[0].InnerText;
                                     ppstatus = element.GetElementsByTagName("pushPlusFowardStatus")[0].InnerText;
                                     tgbotStatus = element.GetElementsByTagName("tgBotFowardStatus")[0].InnerText;
                                     smtpHost = element.GetElementsByTagName("smtpHost")[0].InnerText;
                                     smtpPort = element.GetElementsByTagName("smtpPort")[0].InnerText;
                                     emailKey = element.GetElementsByTagName("emailKey")[0].InnerText;
                                     sendEmial = element.GetElementsByTagName("sendEmial")[0].InnerText;
                                     reciveEmial = element.GetElementsByTagName("reciveEmial")[0].InnerText;
                                     corpid = element.GetElementsByTagName("WeChatQYID")[0].InnerText;
                                     corpsecret = element.GetElementsByTagName("WeChatQYApplicationSecret")[0].InnerText;
                                     agentid = element.GetElementsByTagName("WeChatQYApplicationID")[0].InnerText;
                                     pptoken = element.GetElementsByTagName("pushPlusToken")[0].InnerText;
                                     tgbToken = element.GetElementsByTagName("tgBotToken")[0].InnerText;
                                     tgbChatID = element.GetElementsByTagName("tgBotChatID")[0].InnerText;

                                 }

                                 var isms = connection.CreateProxy<ISms>("org.freedesktop.ModemManager1", change.path);
                                 string sid = change.path.ToString();
                                 string tel = await isms.GetNumberAsync();
                                 string stime = (await isms.GetTimestampAsync()).Replace("T", " ").Replace("+08:00", " ");

                                 string smscontent = "";
                                 do
                                 {
                                     smscontent = "";
                                     smscontent = await isms.GetTextAsync();
                                 } while (string.IsNullOrEmpty(smscontent));
                                 string body = "发信电话:" + tel + "\n" + "时间:" + stime + "\n" + "短信内容:" + smscontent;
                                 Console.WriteLine(body);
                                 if (status == "1")
                                 {
                                     MailAddress to = new MailAddress(reciveEmial);
                                     MailAddress from = new MailAddress(sendEmial, "SMSForwad");
                                     MailMessage mm = new MailMessage(from, to);
                                     SmtpClient sc = new SmtpClient(smtpHost);
                                     try
                                     {
                                         mm.Subject = "短信转发" + tel;
                                         mm.Body = body;
                                         sc.DeliveryMethod = SmtpDeliveryMethod.Network;
                                         sc.Credentials = new NetworkCredential(sendEmial, emailKey);
                                         sc.Send(mm);
                                         Console.WriteLine("邮箱转发成功");
                                         mm.Dispose();
                                         sc.Dispose();
                                     }
                                     catch (SmtpException ex)
                                     {
                                         mm.Dispose();
                                         sc.Dispose();
                                         Console.WriteLine(ex.Message);
                                         Console.WriteLine("出错了，尝试确认下配置文件中的邮箱信息是否正确");
                                     }
                                 }
                                 if (qystatus == "1")
                                 {
                                     try
                                     {
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
                                             obj.Add("agentid",Convert.ToInt32(agentid));
                                             obj1.Add("content", "短信转发\n" + body);
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
                                             if (errcode1 == "0" && errmsg1 == "ok")
                                             {
                                                 Console.WriteLine("企业微信转发成功");
                                             }
                                             else
                                             {
                                                 Console.WriteLine(errmsg1);
                                             }
                                         }
                                         else
                                         {
                                             Console.WriteLine(errmsg);
                                         }
                                     }
                                     catch (Exception ex)
                                     {
                                         Console.WriteLine(ex.Message);
                                         Console.WriteLine("出错了，尝试确认下配置文件中的邮箱信息是否正确");
                                     }
                                 }
                                 if (ppstatus=="1")
                                 {
                                     try
                                     {
                                         string pushPlusUrl = "http://www.pushplus.plus/send/";
                                         JObject obj = new JObject();
                                         obj.Add("token", pptoken);
                                         obj.Add("title", "短信转发" + tel);
                                         obj.Add("content", body);
                                         obj.Add("topic", "");
                                         string msgresult = HttpHelper.Post(pushPlusUrl, obj);
                                         JObject jsonObjresult = JObject.Parse(msgresult);
                                         string code = jsonObjresult["code"].ToString();
                                         string errmsg = jsonObjresult["msg"].ToString();
                                         if (code == "200")
                                         {
                                             Console.WriteLine("pushplus转发成功");
                                         }
                                         else
                                         {
                                             Console.WriteLine(errmsg);
                                         }
                                     }
                                     catch (Exception ex)
                                     {
                                         Console.WriteLine(ex.Message);
                                     }
                                 }
                                 if (tgbotStatus=="1")
                                 {
                                     try
                                     {
                                         string url = "https://api.telegram.org/bot" + tgbToken + "/sendMessage?chat_id=" + tgbChatID + "&text=";
                                         url += System.Web.HttpUtility.UrlEncode(body);
                                         string msgresult = HttpHelper.HttpGet(url);
                                         JObject jsonObjresult = JObject.Parse(msgresult);
                                         string tgstatus = jsonObjresult["ok"].ToString();
                                         if (tgstatus == "True")
                                         {
                                             Console.WriteLine("TGBot转发成功");
                                         }
                                         else
                                         {
                                             Console.WriteLine(jsonObjresult["error_code"].ToString());
                                             Console.WriteLine(jsonObjresult["description"].ToString());
                                         }
                                     }
                                     catch (Exception ex)
                                     {
                                         Console.WriteLine(ex.Message);
                                     }
                                     
                                 }

                                 JArray ja = new JArray();
                                 StreamReader file = new StreamReader(smssavedPath, Encoding.Default);
                                 string jsonstring = file.ReadToEnd();
                                 file.Close();
                                 file.Dispose();
                                 bool SmsExistJudge = false;
                                 if (jsonstring.Length > 0)
                                 {
                                     Sms[] datas = JsonConvert.DeserializeObject<Sms[]>(jsonstring);
                                     foreach (Sms item in datas)
                                     {
                                         if (stime + "_" + tel + "_" + sid == item.sid)
                                         {
                                             SmsExistJudge = true;
                                         }
                                         JObject jobj = new JObject
                                         {
                                             { "sid", item.sid },
                                             { "tel", item.tel },
                                             { "text", item.text }
                                         };
                                         ja.Add(jobj);
                                     }
                                 }
                                 if (!SmsExistJudge)
                                 {
                                     JObject jobj1 = new JObject
                                     {
                                         { "sid", stime + "_" + tel + "_" + sid },
                                         { "tel", tel },
                                         { "text", smscontent }
                                     };
                                     ja.Add(jobj1);
                                 }
                                 using (FileStream fs = new FileStream(smssavedPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                                 {
                                     fs.Seek(0, SeekOrigin.Begin);
                                     fs.SetLength(0);
                                     using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                                     {
                                         sw.Write(ja.ToString());
                                     }
                                 }
                             }
                         }
                     );
                        await Task.Delay(int.MaxValue);
                    }
                }).Wait();
            }
        }

        

    }
}
