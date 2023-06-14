using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModemManager1.DBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using smsmanager.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Tmds.DBus;
using static System.Net.Mime.MediaTypeNames;

namespace smsmanager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IWebHostEnvironment _hostEnvironment;
        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }
        public ActionResult login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult login(User user)
        {
            if (user.uname == null || user.upassword == null)
            {
                ModelState.AddModelError("", "用户名或密码为空");
                return View(user);
            }
            else
            {
                string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(orgCodePath);
                XmlNodeList topM = xmldoc.SelectNodes("//userSettings");
                bool userJudge = false;
                foreach (XmlElement element in topM)
                {
                    string username = element.GetElementsByTagName("username")[0].InnerText;
                    string password = element.GetElementsByTagName("password")[0].InnerText;
                    if (user.uname == username && user.upassword == password) 
                    {
                        userJudge = true;
                    }
                }
                if (userJudge)
                {
                    HttpContext.Session.SetString("uname", user.uname);
                    return View("Index");
                }
                else
                {
                    ModelState.AddModelError("", "用户名或密码错误，登录失败！");
                    return View(user);
                }

            }
        }
        public ActionResult GetLogo()
        {
            var strPath = Path.Combine(_hostEnvironment.WebRootPath, "layuimini/images/logo.png");
            using (var sw = new FileStream(strPath, FileMode.Open))
            {
                var bytes = new byte[sw.Length];
                sw.Read(bytes, 0, bytes.Length);
                sw.Close();
                sw.Dispose();
                
                return new FileContentResult(bytes, "image/png");
            }
        }
        public ActionResult Welcome()
        {
            using (var connection = new Connection(Address.System))
            {
                connection.ConnectAsync();
                var objectPath = new ObjectPath("/org/freedesktop/ModemManager1/Modem/0");
                var service = "org.freedesktop.ModemManager1";
                var imsg = connection.CreateProxy<IMessaging>(service, objectPath);
                var smspathlist=imsg.GetMessagesAsync().Result;
                int sendcount = 0;
                int tempcount = 0;
                int receivecount = 0;
                foreach (var item in smspathlist)
                {
                    ISms isms = connection.CreateProxy<ISms>("org.freedesktop.ModemManager1", item);
                    if (isms.GetStateAsync().Result.ToString()=="5")
                    {
                        sendcount++;
                    }
                    if (isms.GetStateAsync().Result.ToString() == "3")
                    {
                        receivecount++;
                    }
                    if (isms.GetStateAsync().Result.ToString() == "0")
                    {
                        tempcount++;
                    }
                    ViewBag.smscount = sendcount + tempcount + receivecount;
                    ViewBag.sendcount = sendcount;
                    ViewBag.tempcount = tempcount;
                    ViewBag.receivecount = receivecount;
                }
            }
            return View();
        }
        public ActionResult Index()
        {
            if (HttpContext.Session.GetString("uname") == null)
            {
                return Content("<script type='text/javascript'>alert('登录失效!');window.location.href='login';</script>");
            }
            else
            {
                string adminname = HttpContext.Session.GetString("uname");
                ViewBag.name = adminname;
                return View();
            }
        }
        public ActionResult LoginOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("login");
        }

        public ActionResult Sendedsms()
        {
            return View();
        }

        public ActionResult Sms_list(actionInputMModel input)
        {
            using (var connection = new Connection(Address.System))
            {
                connection.ConnectAsync();
                var objectPath = new ObjectPath("/org/freedesktop/ModemManager1/Modem/0");
                var service = "org.freedesktop.ModemManager1";
                var imsg = connection.CreateProxy<IMessaging>(service, objectPath);
                var smspathlist = imsg.GetMessagesAsync().Result;
                List<Sms> list = new List<Sms>();
                foreach (var item in smspathlist)
                {
                    ISms isms = connection.CreateProxy<ISms>("org.freedesktop.ModemManager1", item);
                    if (isms.GetStateAsync().Result.ToString() == "5")
                    {
                        Sms s = new Sms();
                        s.sid = item.ToString();
                        s.tel = isms.GetNumberAsync().Result;
                        s.text = isms.GetTextAsync().Result;
                        if (!string.IsNullOrEmpty(input.tel) && !string.IsNullOrEmpty(input.text))
                        {
                            if (Contains(s.tel, input.tel) && Contains(s.text, input.text))
                            {
                                list.Add(s);
                            }
                        }
                        else if (string.IsNullOrEmpty(input.tel) && !string.IsNullOrEmpty(input.text))
                        {
                            if (Contains(s.text, input.text))
                            {
                                list.Add(s);
                            }
                        }
                        else if (!string.IsNullOrEmpty(input.tel) && string.IsNullOrEmpty(input.text))
                        {
                            if (Contains(s.tel, input.tel))
                            {
                                list.Add(s);
                            }
                        }
                        else
                        {
                            list.Add(s);
                        }
                    }
                    
                }
                if (list.Count!=0)
                {
                    var newdata = new { code = 0, msg = "", count = list.Count, data = list.OrderByDescending(b => b.sid).Skip((input.page - 1) * input.limit).Take(input.limit).ToList() };//构造出适配layui表格的json格式
                    return Json(newdata);
                }
                else
                {
                    return Json(new { code = 0, msg = "", count = 0, data = "" });
                }
            }
        }
        public ActionResult ViewSms(string tel, string smstext)
        {
            ViewBag.tel = tel;
            ViewBag.smstext = smstext;
            return View(); 
        }
        public ActionResult DeleteSms(string id,string from)
        {
            using (var connection = new Connection(Address.System))
            {
                connection.ConnectAsync();
                var objectPath = new ObjectPath("/org/freedesktop/ModemManager1/Modem/0");
                var smsPath = new ObjectPath(id);
                var service = "org.freedesktop.ModemManager1";
                var imsg = connection.CreateProxy<IMessaging>(service, objectPath);
                imsg.DeleteAsync(smsPath);
            }
            return RedirectToAction(from);
        }

        //public void doubleDeleteSms(int id) 
        //{
        //    var psi = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 --messaging-delete-sms=" + id);
        //    psi.RedirectStandardOutput = true;
        //    psi.UseShellExecute = false;
        //    psi.RedirectStandardError = true;
        //    using (var process = System.Diagnostics.Process.Start(psi))
        //    {
        //        var output = process.StandardOutput.ReadToEnd();
        //        var error = process.StandardError.ReadToEnd();
        //        process.Kill();
        //        if (error.Trim() == "couldn't delete SMS: 'GDBus.Error:org.freedesktop.ModemManager1.Error.Core.Failed: Couldn't delete 1 parts from this SMS'")
        //        {
        //            doubleDeleteSms(id);
        //        }
        //    }
            
        //}

        public ActionResult Tempdsms()
        {
            return View();
        }
        public ActionResult TempSms_list(actionInputMModel input)
        {
            using (var connection = new Connection(Address.System))
            {
                connection.ConnectAsync();
                var objectPath = new ObjectPath("/org/freedesktop/ModemManager1/Modem/0");
                var service = "org.freedesktop.ModemManager1";
                var imsg = connection.CreateProxy<IMessaging>(service, objectPath);
                var smspathlist = imsg.GetMessagesAsync().Result;
                List<Sms> list = new List<Sms>();
                foreach (var item in smspathlist)
                {
                    ISms isms = connection.CreateProxy<ISms>("org.freedesktop.ModemManager1", item);
                    if (isms.GetStateAsync().Result.ToString() == "0")
                    {
                        Sms s = new Sms();
                        s.sid = item.ToString();
                        s.tel = isms.GetNumberAsync().Result;
                        s.text = isms.GetTextAsync().Result;
                        if (!string.IsNullOrEmpty(input.tel) && !string.IsNullOrEmpty(input.text))
                        {
                            if (Contains(s.tel, input.tel) && Contains(s.text, input.text))
                            {
                                list.Add(s);
                            }
                        }
                        else if (string.IsNullOrEmpty(input.tel) && !string.IsNullOrEmpty(input.text))
                        {
                            if (Contains(s.text, input.text))
                            {
                                list.Add(s);
                            }
                        }
                        else if (!string.IsNullOrEmpty(input.tel) && string.IsNullOrEmpty(input.text))
                        {
                            if (Contains(s.tel, input.tel))
                            {
                                list.Add(s);
                            }
                        }
                        else
                        {
                            list.Add(s);
                        }
                    }

                }
                if (list.Count != 0)
                {
                    var newdata = new { code = 0, msg = "", count = list.Count, data = list.OrderByDescending(b => b.sid).Skip((input.page - 1) * input.limit).Take(input.limit).ToList() };//构造出适配layui表格的json格式
                    return Json(newdata);
                }
                else
                {
                    return Json(new { code = 0, msg = "", count = 0, data = "" });
                }
            }
        }
        public ActionResult EditSms(string id, string tel, string smstext)
        {
            ViewBag.sid = id;
            ViewBag.tel = tel;
            ViewBag.smstext = smstext;
            return View();
        }
        [HttpPost]
        public ActionResult EditingSms(string id, string tel, string text)
        {
            using (var connection = new Connection(Address.System))
            {
                connection.ConnectAsync();
                var objectPath = new ObjectPath("/org/freedesktop/ModemManager1/Modem/0");
                var service = "org.freedesktop.ModemManager1";
                var imsg = connection.CreateProxy<IMessaging>(service, objectPath);
                var smsPath = new ObjectPath(id);
                imsg.DeleteAsync(smsPath);
                Dictionary<string, object> smsdict = new Dictionary<string, object> { { "text", text }, { "number", tel } };
                var sendsmsPath = imsg.CreateAsync(smsdict).Result;
                ViewBag.js = "<script type='text/javascript'>alert('修改成功！');layui.use(['form'], function() {var form = layui.form,layer = layui.layer,$ = layui.$;function close(){var iframeIndex = parent.layer.getFrameIndex(window.name);parent.layer.close(iframeIndex);window.parent.location.reload();}close();});</script>";
                return View("EditSms");
            }
        }
        public ActionResult SendSms(string id)
        {
            using (var connection = new Connection(Address.System))
            {
                connection.ConnectAsync();
                var smsPath = new ObjectPath(id);
                var isms = connection.CreateProxy<ISms>("org.freedesktop.ModemManager1", smsPath);
                isms.SendAsync().Wait();
                return RedirectToAction("Tempdsms");
            }
        }
        [HttpPost]
        public ActionResult TempSaveSms(string tel, string text)
        {
            using (var connection = new Connection(Address.System))
            {
                connection.ConnectAsync();
                var objectPath = new ObjectPath("/org/freedesktop/ModemManager1/Modem/0");
                var service = "org.freedesktop.ModemManager1";
                var imsg = connection.CreateProxy<IMessaging>(service, objectPath);
                Dictionary<string, object> smsdict = new Dictionary<string, object> { { "text", text }, { "number", tel } };
                var sendsmsPath = imsg.CreateAsync(smsdict).Result;
                return RedirectToAction("Tempdsms");
            }
        }
        public ActionResult SendingSms()
        {
            return View(); 
        }
        [HttpPost]
        public ActionResult SendingSms(string tel,string text)
        {

            using (var connection = new Connection(Address.System))
            {
                connection.ConnectAsync();
                var objectPath = new ObjectPath("/org/freedesktop/ModemManager1/Modem/0");
                var service = "org.freedesktop.ModemManager1";
                var imsg = connection.CreateProxy<IMessaging>(service, objectPath);
                Dictionary<string, object> smsdict = new Dictionary<string, object> { { "text", text }, { "number", tel } };
                var sendsmsPath = imsg.CreateAsync(smsdict).Result;
                var isms = connection.CreateProxy<ISms>("org.freedesktop.ModemManager1", sendsmsPath);
                isms.SendAsync().Wait();
                return View("Sendedsms");
            }
        }
        public ActionResult Receivesms()
        {
            return View();
        }
        public ActionResult ReceiveSms_list(actionInputMModel input)
        {
            using (var connection = new Connection(Address.System))
            {
                connection.ConnectAsync();
                var objectPath = new ObjectPath("/org/freedesktop/ModemManager1/Modem/0");
                var service = "org.freedesktop.ModemManager1";
                var imsg = connection.CreateProxy<IMessaging>(service, objectPath);
                var smspathlist = imsg.GetMessagesAsync().Result;
                List<Sms> list = new List<Sms>();
                foreach (var item in smspathlist)
                {
                    ISms isms = connection.CreateProxy<ISms>("org.freedesktop.ModemManager1", item);
                    if (isms.GetStateAsync().Result.ToString() == "3")
                    {
                        Sms s = new Sms();
                        s.sid = item.ToString();
                        s.tel = isms.GetNumberAsync().Result;
                        s.text = isms.GetTextAsync().Result;
                        if (!string.IsNullOrEmpty(input.tel) && !string.IsNullOrEmpty(input.text))
                        {
                            if (Contains(s.tel, input.tel) && Contains(s.text, input.text))
                            {
                                list.Add(s);
                            }
                        }
                        else if (string.IsNullOrEmpty(input.tel) && !string.IsNullOrEmpty(input.text))
                        {
                            if (Contains(s.text, input.text))
                            {
                                list.Add(s);
                            }
                        }
                        else if (!string.IsNullOrEmpty(input.tel) && string.IsNullOrEmpty(input.text))
                        {
                            if (Contains(s.tel, input.tel))
                            {
                                list.Add(s);
                            }
                        }
                        else
                        {
                            list.Add(s);
                        }
                    }
                }
                if (list.Count != 0)
                {
                    var newdata = new { code = 0, msg = "", count = list.Count, data = list.OrderByDescending(b => b.sid).Skip((input.page - 1) * input.limit).Take(input.limit).ToList() };//构造出适配layui表格的json格式
                    return Json(newdata);
                }
                else
                {
                    return Json(new { code = 0, msg = "", count = 0, data = "" });
                }
            }
        }
        public ActionResult DeleteSmsHistory(string id)
        {
            string smssavedPath = AppDomain.CurrentDomain.BaseDirectory + "smssaved.json";
            StreamReader file = new StreamReader(smssavedPath, Encoding.Default);
            string jsonstring = file.ReadToEnd();
            file.Close();
            file.Dispose();
            JArray ja = new JArray();
            if (jsonstring.Length > 0)
            {
                Sms[] datas = JsonConvert.DeserializeObject<Sms[]>(jsonstring);
                foreach (Sms item in datas)
                {
                    if (item.sid!= id)
                    {
                        JObject jobj = new JObject
                        {
                            { "sid", item.sid },
                            { "tel", item.tel },
                            { "text", item.text }
                        };
                        ja.Add(jobj);
                    }
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
                return RedirectToAction("ReceivesmsHistory");
            }
            else
            {
                return RedirectToAction("ReceivesmsHistory");
            }

        }

        public ActionResult ReceivesmsHistory()
        {
            return View();
        }
        public ActionResult ReceivesmsHistory_list(actionInputMModel input)
        {
            string smssavedPath = AppDomain.CurrentDomain.BaseDirectory + "smssaved.json";
            StreamReader file = new StreamReader(smssavedPath, Encoding.Default);
            string jsonstring = file.ReadToEnd();
            file.Close();
            file.Dispose();
            List<Sms> list = new List<Sms>();
            if (jsonstring.Length > 0)
            {
                Sms[] datas = JsonConvert.DeserializeObject<Sms[]>(jsonstring);
                foreach (Sms item in datas)
                {
                    if (!string.IsNullOrEmpty(input.tel) && !string.IsNullOrEmpty(input.text))
                    {
                        if (Contains(item.tel, input.tel)&& Contains(item.text, input.text))
                        {
                            list.Add(item);
                        }
                    }
                    else if (string.IsNullOrEmpty(input.tel) && !string.IsNullOrEmpty(input.text))
                    {
                        if (Contains(item.text, input.text))
                        {
                            list.Add(item);
                        }
                    }
                    else if (!string.IsNullOrEmpty(input.tel) && string.IsNullOrEmpty(input.text))
                    {
                        if (Contains(item.tel, input.tel))
                        {
                            list.Add(item);
                        }
                    }
                    else
                    {
                        list.Add(item);
                    }
                }
                var newdata = new { code = 0, msg = "", count = list.Count, data = list.OrderByDescending(b => b.sid).Skip((input.page - 1) * input.limit).Take(input.limit).ToList() };//构造出适配layui表格的json格式                                                                                                                                        //return Json(newdata, JsonRequestBehavior.AllowGet);//转为json并返回至前台
                return Json(newdata);
            }
            else
            {
                return Json(new { code = 0, msg = "", count = 0, data = "" });
            }
        }

        public ActionResult Emailfoward()
        {
            string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(orgCodePath);
            XmlNodeList topM = xmldoc.SelectNodes("//userSettings");
            foreach (XmlElement element in topM)
            {
                ViewBag.status = element.GetElementsByTagName("emailFowardStatus")[0].InnerText=="0" ? "" : "checked=\"\"";
                ViewBag.smtp = element.GetElementsByTagName("smtpHost")[0].InnerText;
                ViewBag.smtpPort = element.GetElementsByTagName("smtpPort")[0].InnerText;
                ViewBag.key = element.GetElementsByTagName("emailKey")[0].InnerText;
                ViewBag.sendEmial = element.GetElementsByTagName("sendEmial")[0].InnerText;
                ViewBag.reciveEmial = element.GetElementsByTagName("reciveEmial")[0].InnerText;
            }
            return View();
        }
        [HttpPost]
        public ActionResult EmailfowardStatusChange(string kg, string smtp,string smtpport, string key, string sendEmial, string reciveEmial)
        {
            string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
            XmlDocument MyXml = new XmlDocument();
            MyXml.Load(orgCodePath);
            //获取<Rule>节点的所有子节点
            XmlNodeList topM = MyXml.SelectNodes("//userSettings");
            //遍历<Rule>下的所有子节点
            foreach (XmlElement element in topM)
            {
                element.GetElementsByTagName("emailFowardStatus")[0].InnerText = kg == "false" ? "0" : "1";
                element.GetElementsByTagName("smtpHost")[0].InnerText = smtp;
                element.GetElementsByTagName("smtpPort")[0].InnerText = smtpport;
                element.GetElementsByTagName("emailKey")[0].InnerText = key;
                element.GetElementsByTagName("sendEmial")[0].InnerText = sendEmial;
                element.GetElementsByTagName("reciveEmial")[0].InnerText = reciveEmial;
            }
            MyXml.Save(orgCodePath);
            foreach (XmlElement element in topM)
            {
                ViewBag.status = element.GetElementsByTagName("emailFowardStatus")[0].InnerText == "0" ? "" : "checked=\"\"";
                ViewBag.smtp = element.GetElementsByTagName("smtpHost")[0].InnerText;
                ViewBag.smtpPort = element.GetElementsByTagName("smtpPort")[0].InnerText;
                ViewBag.key = element.GetElementsByTagName("emailKey")[0].InnerText;
                ViewBag.sendEmial = element.GetElementsByTagName("sendEmial")[0].InnerText;
                ViewBag.reciveEmial = element.GetElementsByTagName("reciveEmial")[0].InnerText;
            }
            return View("Emailfoward");
        }

        public ActionResult Wechatfoward()
        {
            string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(orgCodePath);
            XmlNodeList topM = xmldoc.SelectNodes("//userSettings");
            foreach (XmlElement element in topM)
            {
                ViewBag.status = element.GetElementsByTagName("WeChatQYFowardStatus")[0].InnerText == "0" ? "" : "checked=\"\"";
                ViewBag.qyid = element.GetElementsByTagName("WeChatQYID")[0].InnerText;
                ViewBag.apid = element.GetElementsByTagName("WeChatQYApplicationID")[0].InnerText;
                ViewBag.ApplicationSecret = element.GetElementsByTagName("WeChatQYApplicationSecret")[0].InnerText;
            }
            return View();
        }
        [HttpPost]
        public ActionResult WechatfowardStatusChange(string kg, string qyid, string apid,string ApplicationSecret)
        {
            string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
            XmlDocument MyXml = new XmlDocument();
            MyXml.Load(orgCodePath);
            //获取<Rule>节点的所有子节点
            XmlNodeList topM = MyXml.SelectNodes("//userSettings");
            //遍历<Rule>下的所有子节点
            foreach (XmlElement element in topM)
            {
                element.GetElementsByTagName("WeChatQYFowardStatus")[0].InnerText = kg == "false" ? "0" : "1";
                element.GetElementsByTagName("WeChatQYID")[0].InnerText = qyid;
                element.GetElementsByTagName("WeChatQYApplicationID")[0].InnerText = apid;
                element.GetElementsByTagName("WeChatQYApplicationSecret")[0].InnerText = ApplicationSecret;
            }
            MyXml.Save(orgCodePath);
            foreach (XmlElement element in topM)
            {
                ViewBag.status = element.GetElementsByTagName("WeChatQYFowardStatus")[0].InnerText == "0" ? "" : "checked=\"\"";
                ViewBag.qyid = element.GetElementsByTagName("WeChatQYID")[0].InnerText;
                ViewBag.apid = element.GetElementsByTagName("WeChatQYApplicationID")[0].InnerText;
                ViewBag.ApplicationSecret = element.GetElementsByTagName("WeChatQYApplicationSecret")[0].InnerText;
            }
            return View("Wechatfoward");
        }

        public ActionResult PushPlusfoward()
        {
            string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(orgCodePath);
            XmlNodeList topM = xmldoc.SelectNodes("//userSettings");
            foreach (XmlElement element in topM)
            {
                ViewBag.status = element.GetElementsByTagName("pushPlusFowardStatus")[0].InnerText == "0" ? "" : "checked=\"\"";
                ViewBag.pptoken = element.GetElementsByTagName("pushPlusToken")[0].InnerText;
            }
            return View();
        }
        [HttpPost]
        public ActionResult PushPlusStatusChange(string kg, string pptoken)
        {
            string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
            XmlDocument MyXml = new XmlDocument();
            MyXml.Load(orgCodePath);
            //获取<Rule>节点的所有子节点
            XmlNodeList topM = MyXml.SelectNodes("//userSettings");
            //遍历<Rule>下的所有子节点
            foreach (XmlElement element in topM)
            {
                element.GetElementsByTagName("pushPlusFowardStatus")[0].InnerText = kg == "false" ? "0" : "1";
                element.GetElementsByTagName("pushPlusToken")[0].InnerText = pptoken;
                
            }
            MyXml.Save(orgCodePath);
            foreach (XmlElement element in topM)
            {
                ViewBag.status = element.GetElementsByTagName("pushPlusFowardStatus")[0].InnerText == "0" ? "" : "checked=\"\"";
                ViewBag.pptoken = element.GetElementsByTagName("pushPlusToken")[0].InnerText;
            }
            return View("PushPlusfoward");
        }
        public ActionResult TGBotfoward()
        {
            string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(orgCodePath);
            XmlNodeList topM = xmldoc.SelectNodes("//userSettings");
            foreach (XmlElement element in topM)
            {
                ViewBag.status = element.GetElementsByTagName("tgBotFowardStatus")[0].InnerText == "0" ? "" : "checked=\"\"";
                ViewBag.tgbToken = element.GetElementsByTagName("tgBotToken")[0].InnerText;
                ViewBag.tgbChatID = element.GetElementsByTagName("tgBotChatID")[0].InnerText;
            }
            return View();
        }

        [HttpPost]
        public ActionResult TGBotfowardStatusChange(string kg, string tgbtoken, string tgbchatid)
        {
            string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
            XmlDocument MyXml = new XmlDocument();
            MyXml.Load(orgCodePath);
            //获取<Rule>节点的所有子节点
            XmlNodeList topM = MyXml.SelectNodes("//userSettings");
            //遍历<Rule>下的所有子节点
            foreach (XmlElement element in topM)
            {
                element.GetElementsByTagName("tgBotFowardStatus")[0].InnerText = kg == "false" ? "0" : "1";
                element.GetElementsByTagName("tgBotToken")[0].InnerText = tgbtoken;
                element.GetElementsByTagName("tgBotChatID")[0].InnerText = tgbchatid;
            }
            MyXml.Save(orgCodePath);
            foreach (XmlElement element in topM)
            {
                ViewBag.status = element.GetElementsByTagName("tgBotFowardStatus")[0].InnerText == "0" ? "" : "checked=\"\"";
                ViewBag.tgbToken = element.GetElementsByTagName("tgBotToken")[0].InnerText;
                ViewBag.tgbChatID = element.GetElementsByTagName("tgBotChatID")[0].InnerText;
            }
            return View("TGBotfoward");
        }




        public ActionResult EditPwd()
        {
            if (HttpContext.Session.GetString("uname") == null)
            {
                return Content("<script type='text/javascript'>alert('登录失效!');window.location.href='login';</script>");
            }
            else
            {
                return View();
            }
        }
        /// <summary>
        /// 处理修改管理员登录密码请求
        /// </summary>
        /// <param name="oapwd">旧密码</param>
        /// <param name="napwd">新密码</param>
        /// <param name="againnapwd">重复的新密码</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditPwd(string oapwd, string napwd, string againnapwd)
        {
            if (HttpContext.Session.GetString("uname") == null)
            {
                return Content("<script type='text/javascript'>alert('登录失效!');window.location.href='login';</script>");
            }
            else
            {
                string adminname = HttpContext.Session.GetString("uname");
                if (oapwd.Trim() == "" || napwd.Trim() == "" || againnapwd.Trim() == "")//判断是否为空
                {
                    ViewBag.js = "<script>alert('新密码或旧密码为空！');</script>";
                    return View();
                }
                else if (napwd != againnapwd)//判断两次新密码是否一致
                {
                    ViewBag.js = "<script>alert('新密码两次输入不一致！');</script>";
                    return View();
                }
                else
                {
                    string orgCodePath = AppDomain.CurrentDomain.BaseDirectory + "loginpassw.xml";
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(orgCodePath);
                    XmlNodeList topM = xmldoc.SelectNodes("//userSettings");
                    string username="", opass = "";
                    foreach (XmlElement element in topM)
                    {
                        username = element.GetElementsByTagName("username")[0].InnerText;
                        opass = element.GetElementsByTagName("password")[0].InnerText;
                    }
                    if (adminname== username&& opass== oapwd)//判断旧密码是否准确
                    {
                        foreach (XmlElement element in topM)
                        {
                            element.GetElementsByTagName("password")[0].InnerText = napwd;
                        }
                        xmldoc.Save(orgCodePath);
                        ViewBag.js = "<script type='text/javascript'>alert('修改成功！');layui.use(['form'], function() {var form = layui.form,layer = layui.layer;function close(){var iframeIndex = parent.layer.getFrameIndex(window.name);parent.layer.close(iframeIndex);window.parent.location.replace('LoginOut');}close();});</script>";
                        return View();
                    }
                    else
                    {
                        ViewBag.js = "<script>alert('旧的密码输入错误！');</script>";
                        return View();
                    }
                }
            }
        }
        public bool Contains(string source, string toCheck)
        {
            return source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public ActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
