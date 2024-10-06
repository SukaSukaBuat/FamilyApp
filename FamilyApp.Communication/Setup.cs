using FamilyApp.Communication.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyApp.Communication
{
    public static class Setup
    {
        public static void ConfigureCommunicationServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IEmailService, EmailService>();
        }
    }
}
