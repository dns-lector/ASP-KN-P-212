using ASP_KN_P_212.Data;
using ASP_KN_P_212.Data.DAL;
using ASP_KN_P_212.Middleware;
using ASP_KN_P_212.Services.Email;
using ASP_KN_P_212.Services.Hash;
using ASP_KN_P_212.Services.Kdf;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("emailconfig.json", false);

// Add services to the container.
builder.Services.AddControllersWithViews();

/* ������ ����� ������ �� ���������� builder.Services
 * �� ����� ������ � ��������� �������, ��� �� ������� 
 * var app = builder.Build();
 * ������, ������� � ����������� DIP ����������� �� 
 * ��'���� (binding) �� ����������� �� ������, �� ����
 * ������. ���������� ����� �������� �� "�� ����� 
 * �������� IHashService ��������� �� ��������� ��'���
 * ����� Md5HashService"
 */
// builder.Services.AddSingleton<IHashService, Md5HashService>();
// ������� �� ������ ����������� ������ ������ - ���� ����� ���
builder.Services.AddSingleton<IHashService, ShaHashService>();


// ��������� ��������� ����� (MS SQL)
builder.Services.AddDbContext<DataContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("LocalMSSQL"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(60),
            errorNumbersToAdd: null);
        }
    ),
    ServiceLifetime.Singleton);

/*
// ��������� ��������� ����� (MySQL)
String connectionString = builder.Configuration.GetConnectionString("LocalMySQL")!;
MySqlConnection connection = new(connectionString);
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(connection, ServerVersion.AutoDetect(connection)),
    ServiceLifetime.Singleton);
*/

builder.Services.AddSingleton<DataAccessor>();
builder.Services.AddSingleton<IKdfService, Pbkdf1Service>();
builder.Services.AddSingleton<IEmailService, GmailService>();

// ������������ Http-����
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(builder => 
    builder
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

app.UseAuthorization();

// ���������� Http-����
app.UseSession();

// ϳ��������� ������ Middleware
// app.UseMiddleware<AuthSessionMiddleware>();
app.UseAuthSession();
app.UseAuthToken();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
