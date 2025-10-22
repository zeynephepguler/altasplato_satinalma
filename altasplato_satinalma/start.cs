using altasplato_satinalma.Data;
using Microsoft.EntityFrameworkCore;
using altasplato_satinalma.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SignalRChat
{
    public class Startup
    {
       public void ConfigureServices(IServiceCollection services)
       {
            services.AddRazorPages();
            services.AddSignalR();
       }

       public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
                        {
       app.UseRouting();

       app.UseEndpoints(endpoints =>
       {
            endpoints.MapHub<ChatHub>("/chatHub");
            endpoints.MapRazorPages();
            });
       }
    }
}