using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using smsmanager.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

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
        //public HomeController(IWebHostEnvironment hostEnvironment)
        //{
        //    _hostEnvironment = hostEnvironment;
        //}
        public IActionResult login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult login(User user)
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
        public IActionResult GetLogo()
        {
            var strPath = Path.Combine(_hostEnvironment.WebRootPath, "layuimini/images/logo.png");
            using (var sw = new FileStream(strPath, FileMode.Open))
            {
                var bytes = new byte[sw.Length];
                sw.Read(bytes, 0, bytes.Length);
                sw.Close();
                return new FileContentResult(bytes, "image/png");
            }
        }
        public IActionResult Welcome()
        {
            var psi = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 --messaging-list-sms");
            psi.RedirectStandardOutput = true;
            using (var process = System.Diagnostics.Process.Start(psi))
            {
                var output = process.StandardOutput.ReadToEnd();
                if (output != string.Empty && output.Trim() != "No sms messages were found")
                {
                    //int count = 0;
                    string[] qline = output.Split(Environment.NewLine.ToCharArray());
                    int sendcount = 0;
                    int tempcount = 0;
                    int receivecount = 0;
                    for (int i = 0; i < qline.Count() - 1; i++)
                    {
                        string[] theRow = qline[i].Split("(");
                        if (theRow[1].Trim() == "sent)")
                        {
                            sendcount++;
                        }
                        if (theRow[1].Trim() == "unknown)")
                        {
                            tempcount++;
                        }
                        if (theRow[1].Trim() == "received)")
                        {
                            receivecount++;
                        }
                    }
                    ViewBag.smscount = sendcount+ tempcount+ receivecount;
                    ViewBag.sendcount = sendcount;
                    ViewBag.tempcount = tempcount;
                    ViewBag.receivecount = receivecount;
                    return View();
                }
                else
                {
                    ViewBag.smscount = 0;
                    ViewBag.sendcount = 0;
                    ViewBag.tempcount = 0;
                    ViewBag.receivecount = 0;
                    return View();
                }
            }
        }
        public IActionResult Index()
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
        public IActionResult LoginOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("login");
        }

        public IActionResult Sendedsms()
        {
            return View();
        }
        public IActionResult Sms_list(int page, int limit)
        {
            var psi = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 --messaging-list-sms");
            psi.RedirectStandardOutput = true;
            using (var process = System.Diagnostics.Process.Start(psi))
            {
                var output = process.StandardOutput.ReadToEnd();
                if (output != string.Empty&& output.Trim() != "No sms messages were found")
                {
                    //int count = 0;
                    List<Sms> list=new List<Sms>();
                    string[] qline = output.Split(Environment.NewLine.ToCharArray());
                    for (int i = 0; i < qline.Count() - 1; i++)
                    {
                        string[] theRow = qline[i].Split("(");
                        if (theRow[1].Trim()== "sent)") 
                        {
                            Sms s = new Sms();
                            s.sid = theRow[0].Trim().Split("SMS/")[1].ToString().Trim();
                            var psi2 = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 -s "+ s.sid);
                            psi2.RedirectStandardOutput = true;
                            using (var process2 = System.Diagnostics.Process.Start(psi2))
                            {
                                var output2 = process2.StandardOutput.ReadToEnd();
                                if (output2 != string.Empty)
                                {
                                    string[] qline2 = output2.Split(Environment.NewLine.ToCharArray());
                                    s.tel = qline2[3].Split("|")[1].Trim().Split(":")[1].Trim();
                                    s.text = qline2[4].Split("|")[1].Trim().Split(":")[1].Trim();
                                }
                            }
                            list.Add(s);
                            //count++;
                        }
                    }
                    var newdata = new { code = 0, msg = "", count = list.Count, data = list.OrderBy(b => b.sid).Skip((page - 1) * limit).Take(limit).ToList()};//构造出适配layui表格的json格式
                    //return Json(newdata, JsonRequestBehavior.AllowGet);//转为json并返回至前台
                    return Json(newdata);
                }
                else
                {
                    return Json(new { code = 0, msg = "", count = 0, data = "" });
                }
            }
            
        }
        public IActionResult ViewSms(string tel, string smstext)
        {
            ViewBag.tel = tel;
            ViewBag.smstext = smstext;
            return View(); 
        }
        public IActionResult DeleteSms(int id,string from)
        {
            var psi = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 --messaging-delete-sms="+ id);
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            using (var process = System.Diagnostics.Process.Start(psi))
            {
                var output = process.StandardOutput.ReadToEnd();
                var error= process.StandardError.ReadToEnd();
                if (output.Trim() == "successfully deleted SMS from modem")
                {
                    return RedirectToAction(from);
                }
                else if (error.Trim() == "error: couldn't delete SMS: 'GDBus.Error:org.freedesktop.ModemManager1.Error.Core.Failed: Couldn't delete 1 parts from this SMS'")
                {
                    doubleDeleteSms(id);
                    return RedirectToAction(from);
                }
                else
                {
                    return RedirectToAction(from);
                }
            }
        }

        public void doubleDeleteSms(int id) 
        {
            var psi = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 --messaging-delete-sms=" + id);
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            using (var process = System.Diagnostics.Process.Start(psi))
            {
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                if (error.Trim() == "couldn't delete SMS: 'GDBus.Error:org.freedesktop.ModemManager1.Error.Core.Failed: Couldn't delete 1 parts from this SMS'")
                {
                    doubleDeleteSms(id);
                }
            }
        }

        public IActionResult Tempdsms()
        {
            return View();
        }
        public IActionResult TempSms_list(int page, int limit)
        {
            var psi = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 --messaging-list-sms");
            psi.RedirectStandardOutput = true;
            using (var process = System.Diagnostics.Process.Start(psi))
            {
                var output = process.StandardOutput.ReadToEnd();
                if (output != string.Empty && output.Trim() != "No sms messages were found")
                {
                    //int count = 0;
                    List<Sms> list = new List<Sms>();
                    string[] qline = output.Split(Environment.NewLine.ToCharArray());
                    for (int i = 0; i < qline.Count() - 1; i++)
                    {
                        string[] theRow = qline[i].Split("(");
                        if (theRow[1].Trim() == "unknown)")
                        {
                            Sms s = new Sms();
                            s.sid = theRow[0].Trim().Split("SMS/")[1].ToString().Trim();
                            var psi2 = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 -s " + s.sid);
                            psi2.RedirectStandardOutput = true;
                            using (var process2 = System.Diagnostics.Process.Start(psi2))
                            {
                                var output2 = process2.StandardOutput.ReadToEnd();
                                if (output2 != string.Empty)
                                {
                                    string[] qline2 = output2.Split(Environment.NewLine.ToCharArray());
                                    s.tel = qline2[3].Split("|")[1].Trim().Split(":")[1].Trim();
                                    s.text = qline2[4].Split("|")[1].Trim().Split(":")[1].Trim();
                                }
                            }
                            list.Add(s);
                            //count++;
                        }
                    }
                    var newdata = new { code = 0, msg = "", count = list.Count, data = list.OrderBy(b => b.sid).Skip((page - 1) * limit).Take(limit).ToList() };//构造出适配layui表格的json格式
                    //return Json(newdata, JsonRequestBehavior.AllowGet);//转为json并返回至前台
                    return Json(newdata);
                }
                else
                {
                    return Json(new { code = 0, msg = "", count = 0, data = "" });
                }
            }

        }
        public IActionResult EditSms(int id, string tel, string smstext)
        {
            ViewBag.sid = id;
            ViewBag.tel = tel;
            ViewBag.smstext = smstext;
            return View();
        }
        [HttpPost]
        public IActionResult EditingSms(int id,string tel, string text)
        {
            var psi = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 --messaging-delete-sms=" + id);
            psi.RedirectStandardOutput = true;
            using (var process = System.Diagnostics.Process.Start(psi))
            {
                var output = process.StandardOutput.ReadToEnd();
                if (output.Trim() == "successfully deleted SMS from modem")
                {
                    var psi1 = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 --messaging-create-sms=\"text ='" + text + "', number = '" + Regex.Replace(tel, @"\s", "") + "'\"");
                    psi1.RedirectStandardOutput = true;
                    using (var process1 = System.Diagnostics.Process.Start(psi1))
                    {
                        var output1 = process1.StandardOutput.ReadToEnd();
                        if (output1.Split(":")[0].Trim() == "Successfully created new SMS")
                        {
                            ViewBag.js = "<script type='text/javascript'>alert('修改成功！');layui.use(['form'], function() {var form = layui.form,layer = layui.layer,$ = layui.$;function close(){var iframeIndex = parent.layer.getFrameIndex(window.name);parent.layer.close(iframeIndex);window.parent.location.reload();}close();});</script>";
                            return View("EditSms");
                        }
                        else
                        {
                            ViewBag.js = "<script type='text/javascript'>alert('修改失败！');layui.use(['form'], function() {var form = layui.form,layer = layui.layer,$ = layui.$;function close(){var iframeIndex = parent.layer.getFrameIndex(window.name);parent.layer.close(iframeIndex);window.parent.location.reload();}close();});</script>";
                            return View("EditSms");
                        }
                    }
                }
                else
                {
                    ViewBag.js = "<script type='text/javascript'>alert('修改失败！');layui.use(['form'], function() {var form = layui.form,layer = layui.layer,$ = layui.$;function close(){var iframeIndex = parent.layer.getFrameIndex(window.name);parent.layer.close(iframeIndex);window.parent.location.reload();}close();});</script>";
                    return View();
                }
            }
        }
        public IActionResult SendSms(int id)
        {
            var psi = new System.Diagnostics.ProcessStartInfo("mmcli", "-s " + id + " --send");
            psi.RedirectStandardOutput = true;
            using (var process = System.Diagnostics.Process.Start(psi))
            {
                var output = process.StandardOutput.ReadToEnd();
                if (output.Trim() == "successfully sent the SMS")
                {
                    return RedirectToAction("Sendedsms");
                }
                else
                {
                    return RedirectToAction("Tempdsms");
                }
            }
        }
        [HttpPost]
        public IActionResult TempSaveSms(string tel, string text)
        {
            var psi = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 --messaging-create-sms=\"text ='" + text + "', number = '" + Regex.Replace(tel, @"\s", "") + "'\"");
            psi.RedirectStandardOutput = true;
            using (var process = System.Diagnostics.Process.Start(psi))
            {
                var output = process.StandardOutput.ReadToEnd();
                if (output.Split(":")[0].Trim() == "Successfully created new SMS")
                {
                    return RedirectToAction("Tempdsms");
                }
                else
                {
                    return RedirectToAction("SendingSms");
                }
            }
        }
        public IActionResult SendingSms()
        {
            return View(); 
        }
        [HttpPost]
        public IActionResult SendingSms(string tel,string text)
        {
            var psi = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 --messaging-create-sms=\"text ='"+ text + "', number = '"+ Regex.Replace(tel, @"\s", "") + "'\"");
            psi.RedirectStandardOutput = true;
            using (var process = System.Diagnostics.Process.Start(psi))
            {
                var output = process.StandardOutput.ReadToEnd();
                if (output.Split(":")[0].Trim() == "Successfully created new SMS")
                {
                    string sid = output.Split(":")[1].Split("SMS/")[1].Trim();
                    SendSms(Convert.ToInt32(sid));
                    return View("Sendedsms");
                }
                else
                {
                    ViewBag.js = "<script>alert('发送失败！');</script>";
                    return View("SendingSms");
                }
            }
        }
        public IActionResult Receivesms()
        {
            return View();
        }
        public IActionResult ReceiveSms_list(int page, int limit)
        {
            var psi = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 --messaging-list-sms");
            psi.RedirectStandardOutput = true;
            using (var process = System.Diagnostics.Process.Start(psi))
            {
                var output = process.StandardOutput.ReadToEnd();
                if (output != string.Empty && output.Trim() != "No sms messages were found")
                {
                    //int count = 0;
                    List<Sms> list = new List<Sms>();
                    string[] qline = output.Split(Environment.NewLine.ToCharArray());
                    for (int i = 0; i < qline.Count() - 1; i++)
                    {
                        string[] theRow = qline[i].Split("(");
                        if (theRow[1].Trim() == "received)")
                        {
                            Sms s = new Sms();
                            s.sid = theRow[0].Trim().Split("SMS/")[1].ToString().Trim();
                            var psi2 = new System.Diagnostics.ProcessStartInfo("mmcli", " -m 0 -s " + s.sid);
                            psi2.RedirectStandardOutput = true;
                            using (var process2 = System.Diagnostics.Process.Start(psi2))
                            {
                                var output2 = process2.StandardOutput.ReadToEnd();
                                if (output2 != string.Empty)
                                {
                                    string[] qline2 = output2.Split(Environment.NewLine.ToCharArray());
                                    s.tel = qline2[3].Split("|")[1].Trim().Split(":")[1].Trim();
                                    s.text = qline2[4].Split("|")[1].Trim().Split(":")[1].Trim();
                                }
                            }
                            list.Add(s);
                            //count++;
                        }
                    }
                    var newdata = new { code = 0, msg = "", count = list.Count, data = list.OrderBy(b => b.sid).Skip((page - 1) * limit).Take(limit).ToList() };//构造出适配layui表格的json格式
                    //return Json(newdata, JsonRequestBehavior.AllowGet);//转为json并返回至前台
                    return Json(newdata);
                }
                else
                {
                    return Json(new { code = 0, msg = "", count = 0, data = "" });
                }
            }
        }
        public IActionResult Emailfoward()
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
        public IActionResult EmailfowardStatusChange(string kg, string smtp,string smtpport, string key, string sendEmial, string reciveEmial)
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

        public IActionResult Wechatfoward()
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
        public IActionResult WechatfowardStatusChange(string kg, string qyid, string apid,string ApplicationSecret)
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

        public IActionResult EditPwd()
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
        public IActionResult EditPwd(string oapwd, string napwd, string againnapwd)
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
