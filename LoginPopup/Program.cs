using RRReddit.Data;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register MongoDatabase as a singleton service
builder.Services.AddSingleton<MongoDatabase>();

// Configure Firestore as a singleton service
/*builder.Services.AddSingleton(provider =>
{
    // Path to your service account key JSON file
    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "rrreddit-6ed19-firebase-adminsdk-amdyd-b89dd14407.json");
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
    return FirestoreDb.Create("rrreddit-6ed19");
});*/

// Add Sessions
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Info}/{id?}");

app.Run();
