using FamilyApp.Auth.Services;
using FamilyApp.Common;
using FamilyApp.Common.Databases.FamilyDb;
using FamilyApp.Common.Dtos;
using FamilyApp.Common.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyApp.Auth.Endpoints
{
    public static class AuthenticationApi
    {
        public static void RegisterAuthenticationApi(this RouteGroupBuilder group)
        {
            // Register the authentication api here
            group.MapPost("/signup", async ([FromServices] IAuthenticationService authenticationService, [FromBody]SignUpDto signUp) =>
            {
                await authenticationService.SignUpAsync(signUp, OuthProvider.None);
                return Results.Ok(signUp);
            }).Validate<SignUpDto>();

            group.MapGet("/confirm-email", async ([FromServices] IAuthenticationService authenticationService, string email, string token) =>
            {
                await authenticationService.ConfirmEmailAsync(email, token);
                return Results.Ok("Berjaya mengesahkan emel anda!");
            });
            group.MapPut("/confirm-user", async ([FromServices] IAuthenticationService authenticationService, string email) =>
            {
                await authenticationService.ConfirmUserAsync(email);
                return Results.Ok("Berjaya mengesahkan pengguna anda!");
            });
            group.MapPut("/oauth/google", async ([FromServices] IAuthenticationService authenticationService, string code) =>
            {
               var (outhResult, email) = await authenticationService.ValidateGoogleOuthSignInAsync(code);
                if (outhResult == OuthStatus.New)
                {
                    await authenticationService.SignUpAsync(new SignUpDto { Email = email }, OuthProvider.Google);
                    return Results.Accepted();
                }
                else if (outhResult == OuthStatus.Verified)
                {
                    var token = await authenticationService.SingInAsync(new SignInDto { Email = email }, true);
                    return Results.Ok(token);
                }
                return Results.BadRequest();
            });
            group.MapPost("/signin", async ([FromServices] IAuthenticationService authenticationService, [FromBody] SignInDto signIn) =>
            {
                var result = await authenticationService.SingInAsync(signIn);
                return Results.Ok(result);
            }).Validate<SignInDto>();

            group.MapGet("/reset-password-request", async ([FromServices] IAuthenticationService authenticationService, string email) =>
            {
                await authenticationService.ResetPasswordRequestAsync(email);
                return Results.Ok("Emel telah dihantar untuk anda menetap semula kata laluan.");
            });

            group.MapPost("/reset-password", async ([FromServices] IAuthenticationService authenticationService, [FromBody] ResetPasswordDto resetPassword) =>
            {
                await authenticationService.ResetPasswordAsync(resetPassword);
                return Results.Ok("Kata laluan telah berjaya ditetapkan semula. Sila log masuk semula menggunakan kata laluan baharu.");
            });
        }
    }
}
