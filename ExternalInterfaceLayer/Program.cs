using BusinessLogicLayer.Services.Implements;
using BusinessLogicLayer.Services.Interface;
using BusinessLogicLayer.Viewmodels;
using CloudinaryDotNet;
using DataAccessLayer.Application;
using DataAccessLayer.Entity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplication();

var mailSetting = builder.Configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailSetting);
builder.Services.AddTransient<ICartProductDetailsService, CartProductDetailsService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddTransient<IManufacturerService, ManufacturerService>();
builder.Services.AddTransient<IVoucherUserService, VoucherUserService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<IOrderDetailsService, OrderDetailsService>();
builder.Services.AddTransient<IImageService, ImageService>();
builder.Services.AddTransient<IProductDetailsService, ProductDetailsService>();
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddTransient<ICartService, CartService>();
builder.Services.AddTransient<IVoucherService, VoucherService>();
builder.Services.AddTransient<IOptionsService, OptionsService>();
builder.Services.AddTransient<ISizeService, SizeService>();
builder.Services.AddTransient<IBrandService, BrandService>();
builder.Services.AddTransient<IMaterialService, MaterialService>();
builder.Services.AddTransient<IColorService, ColorService>();
builder.Services.AddTransient<IApplicationUserService, ApplicationUserService>();
builder.Services.AddTransient<ApplicationDBContext>();
builder.Services.AddTransient<IViewKHServiece, ViewKHServiece>();
builder.Services.AddTransient<ICartServieceM,CartServieceM>();
builder.Services.AddTransient<IOrderTQServiece,OrderTQServiece>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();
var cloudinaryAccount = new Account("dcv1hoalk", "729284788565283", "BGa7XmLy9BkF91j_gsMZQ-7uY2U");
var cloudinary = new Cloudinary(cloudinaryAccount);
builder.Services.AddSingleton(cloudinary);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ClientPolicy", policy => policy.RequireRole("Client"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
