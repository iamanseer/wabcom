using AutoMapper;
using DinkToPdf.Contracts;
using DinkToPdf;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PB.DatabaseFramework;
using PB.IdentityServer;
using PB.Model;
using PB.Server.Helper;
using PB.Server.Repository;
using PB.Shared.Helpers;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Web.Http.Versioning;
using Microsoft.Web.Http;
using PB.EntityFramework;
using PB.Server.Repository.eCommerce;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });

});

builder.Services.AddSignalR();

builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration["ConnectionStrings:DefaultConnection"]));
builder.Services.AddHangfireServer();

#region JWT Based Authentication

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               ValidIssuer = builder.Configuration["Jwt:Issuer"],
               ValidAudience = builder.Configuration["Jwt:Audience"],
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecurityKey"]))
           };
       });

#endregion

#region Automapper

var mappingConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new AutoMapperProfile());
});
IMapper mapper = mappingConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

#endregion

#region Common Repository DI

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IDbConnection>((sp) => new SqlConnection(builder.Configuration["ConnectionStrings:DefaultConnection"]));
builder.Services.AddScoped<IDbContext, DbContext>();
builder.Services.AddTransient<IIdentityDatabaseInitializer, IdentityDatabaseInitializer>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IMediaRepository, MediaRepository>();
builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository2>();
builder.Services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();
builder.Services.AddTransient<INotificationRepository, NotificationRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IWhatsappRepository, WhatsappRepository>();
builder.Services.AddTransient<IAccountRepository, AccountRepository>();
builder.Services.AddTransient<ICommonRepository, CommonRepository>();
builder.Services.AddTransient<IPaymentGatewayRepository, PaymentGatewayRepository>();
builder.Services.AddTransient<IInventoryRepository, InventoryRepository>();

//changed here
builder.Services.AddTransient<ICRMRepository, CRMRepository>();
builder.Services.AddTransient<ICommonRepository, CommonRepository>();
builder.Services.AddTransient<ISuperAdminRepository, SuperAdminRepository>();
builder.Services.AddTransient<IPDFRepository, PDFRepository>();
builder.Services.AddTransient<ICourtPackageRepository, CourtPackageRepository>();
builder.Services.AddTransient<ISpinRepository, SpinRepository>();
builder.Services.AddTransient<IWhiteLabelRepository, WhiteLabelRepository>();
builder.Services.AddTransient<IItemRepository, ItemRepository>();
builder.Services.AddTransient<ICustomerRepository, CustomerRepository>();
builder.Services.AddTransient<ISupplierRepository, SupplierRepository>();
builder.Services.AddTransient<IEntityRepository, EntityRepository>();


//E Commerce related repository

builder.Services.AddTransient<IEC_CommonRepository, EC_CommonRepository>();

var context = new CustomAssemblyLoadContext();
context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

#endregion

#region API versioning

builder.Services.Configure<ApiVersioningOptions>(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0); // Default API version
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new HeaderApiVersionReader("api-version"); // Header name for versioning
});

#endregion

//
builder.Services.AddEndpointsApiExplorer();
//
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


#region Our Settings

app.UseMiddleware<Middleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("CorsPolicy");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
});
app.UseHangfireDashboard();

#endregion

app.MapRazorPages();
app.MapControllers();


app.MapHub<NotificationHub>("/notificationhub");
app.MapFallbackToFile("index.html");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var databaseInitializer = services.GetRequiredService<IDatabaseInitializer>();
        databaseInitializer.InsertDefaultEntries().Wait();
    }
    catch (Exception ex)
    {

    }
}

app.Run();
