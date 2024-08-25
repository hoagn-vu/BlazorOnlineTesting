using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using MongoDB.Driver;
using testQuiz.Authentication;
using testQuiz.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServerSideBlazor();
//builder.Services.AddRazorComponents();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// v
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.Cookie.Name = "auth_token";
//        options.LoginPath = "/login";
//        options.Cookie.MaxAge = TimeSpan.FromMinutes(30);
//        options.AccessDeniedPath = "/access-denied";
//    });
//builder.Services.AddAuthorization();
//builder.Services.AddCascadingAuthenticationState();
// v



// Configure MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDbConnection");
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("SchoolDB"); // Replace with your actual database name
});

// Register QuizService
builder.Services.AddSingleton<QuestionService>();
builder.Services.AddScoped<SubjectService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<ExamService>();
//builder.Services.AddScoped<Microsoft.JSInterop.IJSRuntime, Microsoft.JSInterop.JSRuntime>();


var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

// v
//app.UseAuthentication();
//app.UseAuthorization();
// v

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

