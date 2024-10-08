using FamilyApp.Common;
using FamilyApp.Common.Databases.FamilyDb;
using FamilyApp.Common.Dtos;
using FamilyApp.Common.Enums;
using FamilyApp.Common.Repositories;
using FamilyApp.Communication.Services;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static FamilyApp.Common.Dtos.OuthGoogleDto;
using static System.Net.WebRequestMethods;

namespace FamilyApp.Auth.Services
{
    public interface IAuthenticationService
    {
        Task ConfirmEmailAsync(string email, string emailConfirmToken);
        Task ConfirmUserAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDto resetPassword);
        Task ResetPasswordRequestAsync(string email);
        Task SignUpAsync(SignUpDto signUp, OuthProvider outhProvider);
        Task<SuccessLoginDto> SingInAsync(SignInDto signIn, bool isOuth = false);
        Task<(OuthStatus status, string email)> ValidateGoogleOuthSignInAsync(string code);
    }

    public class AuthenticationService(UserManager<TblUser> userManager, IEmailService emailService, IConfiguration configuration, IGenericRepository genericRepository) : IAuthenticationService
    {
        public async Task SignUpAsync(SignUpDto signUp, OuthProvider outhProvider = OuthProvider.None)
        {
            var userExist = await userManager.FindByEmailAsync(signUp.Email);
            if (userExist != null)
            {
                throw new RecordAlreadyExistException("AKaun telah wujud.");
            }
            var toSave = signUp.Adapt<TblUser>();

            if (outhProvider != OuthProvider.None)
            {
                toSave.OuthProvider = outhProvider;
                toSave.EmailConfirmed = true;
                signUp.Password = "From_Outh_2"; //dummy password for outh user to pass the validation, since outh user do not have password
            }
            var result = await userManager.CreateAsync(toSave, signUp.Password);
            if (!result.Succeeded)
            {
                throw new BadRequestException(result.Errors.Select(e => e.Description).ToString()!);
            }
            if(outhProvider == OuthProvider.None) //only for non outh user
            {
                var emailConfirmToken = await userManager.GenerateEmailConfirmationTokenAsync(toSave);
                // Construct the URL with query string parameters
                var queryParams = new Dictionary<string, string>
            {
                { "token", emailConfirmToken },
                { "email", toSave.Email }
            };
                var confirmationUrl = QueryHelpers.AddQueryString($"{configuration["Host"]}/api/auth/confirm-email", queryParams!);

                await emailService.SendEmailAsync(toSave.Email, "Sahkan Emel anda", $"Sila sahkan emel ada dengan menekan pautan ini: <a href='{confirmationUrl}'>here</a>.");
            }
        }

        public async Task ConfirmEmailAsync(string email, string emailConfirmToken)
        {
            var user = await userManager.FindByEmailAsync(email) ?? throw new RecordNotFoundException("Akaun tidak dijumpai.");
            var result = await userManager.ConfirmEmailAsync(user, emailConfirmToken);
            if (!result.Succeeded)
            {
                throw new BadRequestException(result.Errors.Select(e => e.Description).ToString()!);
            }
        }

        public async Task ConfirmUserAsync(string email)
        {
            var user = await userManager.FindByEmailAsync(email) ?? throw new RecordNotFoundException("Akaun tidak dijumpai.");
            if (!user.EmailConfirmed) {
                throw new EmailNotConfirmedException();
            }
            if(user.UserConfirmed)
            {
                throw new AccountNotConfirmedException();
            }
            user.UserConfirmed = true;
            await userManager.UpdateAsync(user);
            await emailService.SendEmailAsync(user.Email, "Anda telah disahkan!", $"Identiti anda telah disahkan oleh pihak pentadbir, anda boleh log masuk dii sini {configuration["Host"]}");
        }
        public async Task<(OuthStatus status, string email)> ValidateGoogleOuthSignInAsync(string code)
        {
            var clientId = configuration["Outh:Google:ClientId"]!;
            var clientSecret = configuration["Outh:Google:ClientSecret"]!;
            var redirectUri = configuration["Outh:Google:RedirectUri"]!;

            using var httpClient = new HttpClient();
            var requestBody = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret }
        };
            var queryParams = new Dictionary<string, string>
            {
            { "code", code },
            { "grant_type", "authorization_code" },
                {"redirect_uri", redirectUri }
            };
            var requestContent = new FormUrlEncodedContent(requestBody);
            var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token".CreateQueryString(queryParams), requestContent);
            //response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(responseContent);

            // Validate the ID token
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(tokenResponse!.IdToken) as JwtSecurityToken;

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://accounts.google.com",
                ValidateAudience = true,
                ValidAudience = clientId,
                ValidateLifetime = true,
                IssuerSigningKeys = GetGoogleSigningKeys()
            };

            var validationResult = await handler.ValidateTokenAsync(tokenResponse.IdToken, validationParameters);
            OuthStatus status = OuthStatus.None;
            if (!validationResult.IsValid)
            {
                throw new BadRequestException("Token tidak sah!");
            }
            var email =  validationResult.Claims.First(c => c.Key == ClaimTypes.Email).Value.ToString();
            var userExist = await userManager.FindByEmailAsync(email!);
            if(userExist == null)
                status = OuthStatus.New;
            else if (userExist.OuthProvider != OuthProvider.Google)
                throw new BadRequestException("Email telah didaftarkan dengan kaedah lain.");
            else if (!userExist.UserConfirmed)
                throw new AccountNotConfirmedException();
            else
                status = OuthStatus.Verified;
            return (status!, email!);
        }
        public async Task<SuccessLoginDto> SingInAsync(SignInDto signIn, bool isOuth = false)
        {
            var user = await userManager.FindByEmailAsync(signIn.Email);
            if (user == null)
            {
                throw new EmailOrPasswordNotValidException();
            }
            if (!user.EmailConfirmed)
            {
                throw new EmailNotConfirmedException();
            }
            if (!user.UserConfirmed)
            {
                throw new AccountNotConfirmedException();
            }
            if (!isOuth)
            {
                if (user.OuthProvider != OuthProvider.None)
                {
                    throw new BadRequestException("Emel ini telah didaftarkan dengan kaedah lain.");
                }
                var result = await userManager.CheckPasswordAsync(user, signIn.Password);
                if (!result)
                {
                    throw new EmailOrPasswordNotValidException();
                }
            }
            var userRoles = await userManager.GetRolesAsync(user);
            var session = new TblLoginSession
            {
                UserId = user.Id,
            };
            genericRepository.Add(session);
            await genericRepository.SaveChangesAsync();
            var authClaims = new List<Claim>
                {
                    new(JwtTokenClaim.Email, user.Email),
                    new(JwtTokenClaim.UserId, user.Id.ToString()),
                    new(JwtTokenClaim.SessionId, session.Id.ToString()),
                    new(JwtTokenClaim.OuthProvider, user.OuthProvider.ToString()),
                };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new(ClaimTypes.Role, userRole));
            }
            var token = GetToken(authClaims);
            return new SuccessLoginDto(new JwtSecurityTokenHandler().WriteToken(token), token.ValidTo);
        }
        public async Task ResetPasswordRequestAsync(string email)
        {
            var user = await userManager.FindByEmailAsync(email) ?? throw new RecordNotFoundException("Akaun tidak dijumpai.");
            if (!user.EmailConfirmed)
            {
                throw new EmailNotConfirmedException();
            }
            if (!user.UserConfirmed)
            {
                throw new AccountNotConfirmedException();
            }
            if(user.OuthProvider != OuthProvider.None)
            {
                throw new BadRequestException("Akaun ini tidak boleh ditetap semula kata laluan.");
            }
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var queryParams = new Dictionary<string, string>
            {
                { "token", resetToken },
                { "email", user.Email }
            };
            var resetUrl = QueryHelpers.AddQueryString($"{configuration["FEHost"]}/reset-password", queryParams!);
            await emailService.SendEmailAsync(user.Email, "Tetapan semula kata laluan", $"Sila klik pada pautan ini untuk menetapkan semula kata laluan anda: <a href='{resetUrl}'>here</a>.");
        }
        public async Task ResetPasswordAsync(ResetPasswordDto resetPassword)
        {
            var user = await userManager.FindByEmailAsync(resetPassword.Email) ?? throw new RecordNotFoundException("AKaun tidak dijumpai.");
            var result = await userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
            if (!result.Succeeded)
            {
                throw new BadRequestException("Tidak berjaya menetap semula kata laluan. Sila cuba semula");
            }
        }

        private static IEnumerable<SecurityKey> GetGoogleSigningKeys()
        {
            using var httpClient = new HttpClient();
            var response = httpClient.GetStringAsync("https://www.googleapis.com/oauth2/v3/certs").Result;
            var keys = JsonSerializer.Deserialize<GoogleCertsResponse>(response);

            return keys!.Keys.Select(key => new JsonWebKey(JsonSerializer.Serialize(key)));
        }
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!));
            var token = new JwtSecurityToken(
                issuer: configuration["JWT:ValidIssuer"],
                audience: configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(Convert.ToInt32(configuration["JWT:ValidDays"])),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return token;
        }
    }
}
