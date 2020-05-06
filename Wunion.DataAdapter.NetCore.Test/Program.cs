using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Wunion.DataAdapter.NetCore.Test
{
    public class Program
    {
        private static string ContentRoot;

        public static void Main(string[] args)
        {
            IConfiguration configuration = GetConfiguration();
            IHostBuilder builder = Host.CreateDefaultBuilder(args);
            builder.ConfigureWebHostDefaults((webBuilder) => {
                webBuilder.UseContentRoot(ContentRoot);
                webBuilder.UseWebRoot(Path.Combine(ContentRoot, "wwwroot"));
                webBuilder.UseKestrel(options => { options.Limits.MaxRequestBodySize = null; });
                IConfigurationSection sectionUrl = configuration.GetSection("ServerUrls");
                if (sectionUrl != null && sectionUrl.GetValue<bool>("Enabled"))
                {
                    string[] urls = sectionUrl.GetValue<string>("Urls")?.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (urls != null && urls.Length > 0)
                        webBuilder.UseUrls(urls);
                }
                webBuilder.UseStartup<Startup>();
            });
            builder.Build().Run();
        }

        /// <summary>
        /// ��ȡӦ�ó������е���ʵ����Ŀ¼.
        /// </summary>
        /// <returns></returns>
        private static string GetBasePath()
        {
            string basePath = string.Empty;
            using (ProcessModule pm = Process.GetCurrentProcess().MainModule)
                basePath = Path.GetDirectoryName(pm?.FileName);
            return basePath;
        }

        /// <summary>
        /// ��ȡ appsettings.json ���õĶ�ȡ����
        /// </summary>
        /// <returns></returns>
        internal static IConfiguration GetConfiguration()
        {
            ContentRoot = GetBasePath();
            if (string.IsNullOrEmpty(ContentRoot))
                throw new DirectoryNotFoundException("Failed to determine application root.");
            // wwwroot �ڸ�Ŀ¼���� IDE ����ʱ��Ŀ¼���ܲ���ȷ��������ѭ���ҵ�����
            // �˴����಻Ӱ�췢��������������
            DirectoryInfo DirInfo = new DirectoryInfo(ContentRoot);
            do
            {
                if (DirInfo.GetDirectories("wwwroot").Length > 0)
                {
                    ContentRoot = DirInfo.FullName;
                    break;
                }
                else
                {
                    DirInfo = DirInfo.Parent;
                }
            } while (DirInfo.Parent != null);
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.SetBasePath(ContentRoot);
            builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }
    }
}
