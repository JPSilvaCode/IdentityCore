﻿using ICWebAPI.Models;
using ICWebAPI.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ICWebAPI.Configurations
{
    public static class EmailConfig
    {
        public static void AddEmailConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddTransient<IEmailService, EmailService>();
            services.Configure<EmailSetting>(configuration.GetSection("EmailSetting"));
        }
    }
}