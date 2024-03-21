using System.Net;
using Blazored.Modal;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using smpatients;
using smpatients.Client.Pages;
using smpatients.Components;
using smpatients.Components.Account;
using smpatients.Data;
using smpatients.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString),
    ServiceLifetime.Scoped);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
builder.Services.AddAuthorization(options =>
{
     options.AddPolicy("admin", policy =>
        policy.RequireRole("admin"));
    options.AddPolicy("AdminOrSmagDr", policy =>
        policy.RequireRole("admin", "partner"));
});
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddFluentUIComponents();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IToastService,ToastService>();
builder.Services.AddBlazoredModal();
builder.Services.AddScoped<PatientServices>();
builder.Services.AddScoped<BotApiService>();
builder.Services.AddScoped<AnalyticServices>();
builder.Services.AddScoped<PdfImageConverter>();
// builder.UseKestrel((context, options) =>
// {
//     if (context.HostingEnvironment.IsProduction())
//     {
//         // Specify the path to your certificate
//         options.ListenAnyIP(443, listenOptions =>
//         {
//             listenOptions.UseHttps("/path/to/your/certificate.pfx", "your-certificate-password");
//         });
//     }
// });
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

 app.UseStaticFiles(new StaticFileOptions
  {
    OnPrepareResponse = ctx =>
    {
         if (ctx.Context.Request.Path.StartsWithSegments("/images") ||
            ctx.Context.Request.Path.StartsWithSegments("/others") ||
            ctx.Context.Request.Path.StartsWithSegments("/videos") ||
            ctx.Context.Request.Path.StartsWithSegments("/files") ||
            ctx.Context.Request.Path.StartsWithSegments("/documents") )
        {
            if (!ctx.Context.User.Identity.IsAuthenticated)
            {
                // respond HTTP 401 Unauthorized.
                ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
        }
    }
  });
  app.UseFileServer(new FileServerOptions
    {
        EnableDefaultFiles = true
    });
//   app.Use(async (context, next) =>
// {
//     // Check if the request path requires authentication
  
//     if ( !context.User.Identity.IsAuthenticated)
//     {
//         // Redirect to the login page
//         context.Response.Redirect("/Login");
//         return;
//     }

//     // Call the next middleware in the pipeline
//     await next();
// });
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(smpatients.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
