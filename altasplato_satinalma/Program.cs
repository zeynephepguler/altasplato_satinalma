using altasplato_satinalma.Data;
using Microsoft.EntityFrameworkCore;
using altasplato_satinalma.Hubs; // ChatHub için namespace

var builder = WebApplication.CreateBuilder(args);

// Servisler
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();  // Razor Pages ekledik
builder.Services.AddSignalR();      // SignalR servisi eklendi

// DbContext
builder.Services.AddDbContext<AltasPlatoDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqlconnection"));
});

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ...
app.MapHub<ChatHub>("/chatHub");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Route ve hub
app.MapRazorPages(); // Razor Pages için
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<ChatHub>("/chatHub");
// ChatHub için rota

app.Run();