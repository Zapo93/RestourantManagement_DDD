﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RestaurantManagement.Common.Application;
using RestaurantManagement.Common.Application.Contracts;
using RestaurantManagement.Application.Identity;
using RestaurantManagement.Infrastructure.Common;
using RestaurantManagement.Infrastructure.Common.Persistence;
using RestaurantManagement.Infrastructure.Hosting;
using RestaurantManagement.Infrastructure.Identity;
using RestaurantManagement.Serving.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using RestaurantManagement.Common.Infrastructure;
using RestaurantManagement.Kitchen.Infrastructure.Configuration;
using RestaurantManagement.Common.Infrastructure.Persistence;
using RestaurantManagement.Serving.Infrastructure.Configuration;

namespace RestaurantManagement.Infrastructure
{
    public static class InfrastructureConfiguration
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            return services
                    .AddCommonInfrastructure<RestaurantManagementDbContext>(configuration)
                    .AddServingInfrastructure(configuration)
                    .AddDatabase(configuration)
                    .AddIdentity(configuration);
        }

        public static IServiceCollection AddInfrastructure(//For Test Purposes
            this IServiceCollection services,
            string dbConnectionString,
            string secret)
        {
            return services
                    .AddCommonInfrastructure<RestaurantManagementDbContext>(dbConnectionString, secret)
                    .AddServingInfrastructure(dbConnectionString, secret)
                    .AddDatabase(dbConnectionString)
                    .AddIdentity(secret);
        }

        private static IServiceCollection AddDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
            => services
                .AddScoped<IHostingDbContext>(provider => provider.GetService<RestaurantManagementDbContext>())
                .AddTransient<IInitializer, DatabaseInitializer<RestaurantManagementDbContext>>();

        private static IServiceCollection AddDatabase(//For Test Purposes
            this IServiceCollection services,
            string dbConnectionString)
            => services
                .AddTransient<IHostingDbContext>(provider => provider.GetService<RestaurantManagementDbContext>())//Must be transient
                .AddTransient<IInitializer, DatabaseInitializer<RestaurantManagementDbContext>>();

        private static IServiceCollection AddIdentity(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var secret = configuration
                .GetSection(nameof(ApplicationSettings))
                .GetValue<string>(nameof(ApplicationSettings.Secret));

            SetUpIdentity(services, secret);

            return services;
        }

        private static IServiceCollection AddIdentity(
            this IServiceCollection services,
            string secret)
        {
            SetUpIdentity(services, secret);

            return services;
        }

        private static void SetUpIdentity(IServiceCollection services, string secret) 
        {
            services
                .AddIdentity<User, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                })
                .AddEntityFrameworkStores<RestaurantManagementDbContext>();

            var key = Encoding.ASCII.GetBytes(secret);

            services
                .AddAuthentication(authentication =>
                {
                    authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(bearer =>
                {
                    bearer.RequireHttpsMetadata = false;
                    bearer.SaveToken = true;
                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            services.AddTransient<IIdentity, IdentityService>();
            services.AddTransient<IJwtTokenGenerator, JwtTokenGeneratorService>();
        }
    }
}
