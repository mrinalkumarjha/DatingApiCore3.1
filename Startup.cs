using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.Data;
using DatingApp.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(x =>
            x.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            
            services.AddControllers().AddJsonOptions(options => {
                options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
            });

            services.AddCors();// for enabling cors to client
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings")); // configuring CloudinarySettings class. so value from app setting with match in class


            // Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfiles());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);

            // added seed class in container
            services.AddTransient<Seed>();

            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IDatingRepository, DatingRepository>();
            // adding authen middleware
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            services.AddScoped<LogUserActivity>(); // for using LogUserActivity action filter
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Seed seeder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // handling exception globally
                app.UseExceptionHandler(builder =>
                {
                    // run is terminal middleware
                    builder.Run(async context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        //context.Response.ContentType = "text/html";
                        var error = context.Features.Get<IExceptionHandlerFeature>();
                        if(error !=null)
                        {
                            // added extension method to add cors header as cors error in api is misleading.
                            context.Response.AddApplicationError(error.Error.Message);
                            // writing error to http response 
                            await context.Response.WriteAsync(error.Error.Message).ConfigureAwait(false);
                        }
                    });
                });
            }
            //seeder.SeedUsers();
            app.UseRouting();
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()); // for cors
            app.UseAuthentication(); // authenticaltion support
            app.UseAuthorization(); // use this sequence to properly work authorization..
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseDefaultFiles(); // it will look for default files like index.html, default.html etc
            app.UseStaticFiles(); // enables ststic file serving 

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
             
                endpoints.MapFallbackToController("Index", "Fallback"); // for spa route
            });
        }
    }
}
