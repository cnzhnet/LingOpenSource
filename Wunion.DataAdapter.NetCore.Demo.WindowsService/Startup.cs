using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wunion.DataAdapter.NetCore.Demo.Controllers;

namespace Wunion.DataAdapter.NetCore.Demo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            BaseController.Configuration = Configuration; // ��ӵ��������������ڿ������л�ȡ�����ļ���

            ResourceFilesController.EnabledMemoryCached = Configuration.GetValue<bool>("ResourceFilesMemoryCached");
            ResourceFilesController.InitializeHttpContentTypes();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            // ��� Session ֧�֣���Ҫ�ֶ����NuGet����Microsoft.AspNetCore.Session��
            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseSession(); // ���� Session ֧�֡�
            // �� appsettings.json ��ʼ���������档
            AppServices.UseDataEngine(env, Configuration);

            app.UseMvc(routes =>
            {
                // ��չ Url ·��
                routes.MapRoute(
                    name: "download",
                    template: "Download",
                    defaults: new { controller = "Download", action = "Content" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
