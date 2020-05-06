using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Razor;
using Wunion.DataAdapter.NetCore.Test.Pages;

namespace Wunion.DataAdapter.NetCore.Test
{
    public class Startup
    {
        internal IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddJsonOptions((options) => {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            });
            // ���÷������ת�ӣ���ȷ����õĿͻ��˵�ַ��Ϣ����ȷ��.
            services.Configure<ForwardedHeadersOptions>(options => {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                // �������������εĴ�������ת��ͷ����ֹ�ܵ� IP ��ƭ��������
                IConfigurationSection section = Configuration.GetSection("ForwardedHeadersOptions").GetSection("KnownProxies");
                KeyValuePair<string, string>[] array = section.AsEnumerable().ToArray();
                if (array != null && array.Length > 0)
                {
                    foreach (KeyValuePair<string, string> item in array)
                    {
                        if (string.IsNullOrEmpty(item.Value))
                            continue;
                        options.KnownProxies.Add(System.Net.IPAddress.Parse(item.Value));
                    }
                }
            });
            services.AddSession((configure) => {
                configure.IdleTimeout = TimeSpan.FromMinutes(45);
                configure.IOTimeout = TimeSpan.FromMinutes(45);
            });
            services.AddHttpContextAccessor();
            services.AddSingleton(typeof(HtmlEncoder), HtmlEncoder.Create(UnicodeRanges.All)); // cshtml ��ͼ�� Unicode ���봦��.
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseForwardedHeaders();
            app.UseStaticFiles();            
            app.UseCookiePolicy();
            app.UseSession();
            app.UseRouting();
            app.UseEndpoints((endpoints) => { endpoints.MapRazorPages(); });
            app.Run(async (context) => {
                context.Response.Redirect("/main");
                await Task.FromResult(0);
            });
        }
    }
}
