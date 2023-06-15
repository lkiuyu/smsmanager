# smsmanager
用于部分随身wifi刷了Debian后的短信管理图形化界面，支持实时短信Email、企业微信、pushplus、tgbot转发

使用.net 6开发，已集成运行时

食用方法：在debian输入

sudo apt install libicu67

安装libicu，接着下载本程序的独立DBUS版本(smsmanager_independent_DBus.zip)，解压上传至debian，打开本程序根目录，输入

sudo chmod -R 777 smsmanager

sudo ./smsmanager

即可运行程序，当然你也可以安装screen，在screen内运行程序从而达到后台运行的效果

程序运行后，访问设备的ip:8080即可进入管理页面，默认用户密码均为admin

需要修改运行端口的，可以在程序运行并自动生成配置文件loginpassw.xml后，修改该配置文件内的urlport节点的值，改成你需要的端口保存后再重新运行即可
