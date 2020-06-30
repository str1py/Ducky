using Ducky.Helpers.Socials.Telegram;
using Ducky.Model;
using Ducky.ViewModel;
using MihaZupan;
using Starksoft.Aspen.Proxy;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ducky.Helpers
{
    public class ProxyHelper
    {
    
        public ProxyHelper()
        {
        }

        public TcpClient TcpHandler(string address, int port)
        {
            var proxy = Properties.Settings.Default.ProxyInUse.Split(';');
            if (proxy[0] == "Socks5")
            {
                Socks5ProxyClient SocksProxyClient;
                if (proxy[3] == "")
                    SocksProxyClient = new Socks5ProxyClient(proxy[1], Int32.Parse(proxy[2]));
                else
                    SocksProxyClient = new Socks5ProxyClient(proxy[1], Int32.Parse(proxy[2]), proxy[3], proxy[4]);

                try { TcpClient client = SocksProxyClient.CreateConnection(address, port); return client; }
                catch { return null; }
            }
            else
            {
                HttpProxyClient webProxyClient;
                if (proxy[3] == "")
                    webProxyClient = new HttpProxyClient(proxy[1], Int32.Parse(proxy[2]));
                else
                    webProxyClient = new HttpProxyClient(proxy[1], Int32.Parse(proxy[2]), proxy[3], proxy[4]);

                try
                {
                    TcpClient client = webProxyClient.CreateConnection(address, port);
                    return client;
                }
                catch
                {
                    return null;
                }
            }
        }
        public HttpToSocks5Proxy Socks5Proxy()
        {
            var proxysplit = Properties.Settings.Default.ProxyInUse.Split(';');
            if (proxysplit[3] != "")
                return new HttpToSocks5Proxy(proxysplit[1], Int32.Parse(proxysplit[2]), proxysplit[3], proxysplit[4]);
            else
                return new HttpToSocks5Proxy(proxysplit[1], Int32.Parse(proxysplit[2]));
        }
        public WebProxy HttpProxy()
        {
            var proxysplit = Properties.Settings.Default.ProxyInUse.Split(';');
            var httpProxy = new WebProxy(proxysplit[1], Int32.Parse(proxysplit[2]));
            if (proxysplit[3] != "")
                httpProxy.Credentials = new NetworkCredential(proxysplit[5], proxysplit[4]);

            return httpProxy;
        }

        public HttpToSocks5Proxy TestSocks5Proxy(string proxy)
        {
            var proxysplit = proxy.Split(';');
            if (proxysplit[3] != "")
                return new HttpToSocks5Proxy(proxysplit[1], Int32.Parse(proxysplit[2]), proxysplit[3], proxysplit[4]);
            else
                return new HttpToSocks5Proxy(proxysplit[1], Int32.Parse(proxysplit[2]));
        }
        public WebProxy TestHttpProxy(string proxy)
        {
            var proxysplit = proxy.Split(';');
            var httpProxy = new WebProxy(proxysplit[1], Int32.Parse(proxysplit[2]));
            if (proxysplit[3] != "")
                httpProxy.Credentials = new NetworkCredential(proxysplit[5], proxysplit[4]);

            return httpProxy;
        }
    }
}
