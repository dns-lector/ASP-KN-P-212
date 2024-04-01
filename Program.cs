using ASP_KN_P_212.Services.Hash;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

/* Додаємо власні сервіси до контейнера builder.Services
 * Це можна робити у довільному порядку, але до команди 
 * var app = builder.Build();
 * Сервіси, створені з дотриманням DIP реєструються як 
 * зв'язка (binding) між інтерфейсом та класом, що його
 * реалізує. Інструкцію можна пояснити як "на запит 
 * інжекції IHashService контейнер має повернути об'єкт
 * класу Md5HashService"
 */
// builder.Services.AddSingleton<IHashService, Md5HashService>();
// перехід між різними реалізаціями одного сервісу - один рядок змін
builder.Services.AddSingleton<IHashService, ShaHashService>();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
