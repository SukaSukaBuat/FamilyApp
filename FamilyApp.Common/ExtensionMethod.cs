using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace FamilyApp.Common
{
    public static class  ExtensionMethod
    {
        public static IndexBuilder<T> IsUniqueSoftDelete<T>(this IndexBuilder<T> indexBuilder)
        {
            //unique key with filter, to exclude the data that fullfill the filter, in this case the soft deleted record
            //so if the soft deleted record has unique field with value "a", new record with same value in the same field can still be inserted
            return indexBuilder.IsUnique().HasFilter("is_deleted = false");
        }
        public static (List<ValidationResult> Results, bool IsValid) DataAnnotationsValidate(this object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);

            var isValid = Validator.TryValidateObject(model, context, results, true);

            return (results, isValid);
        }
        //refer https://stackoverflow.com/a/78014554
        public static RouteHandlerBuilder Validate<T>(this RouteHandlerBuilder builder, bool firstErrorOnly = false)
        {
            builder.AddEndpointFilter(async (invocationContext, next) =>
            {
                var argument = invocationContext.Arguments.OfType<T>().FirstOrDefault();
                var response = argument!.DataAnnotationsValidate();

                if (!response.IsValid)
                {
                    string? errorMessage = firstErrorOnly ?
                                            response.Results.FirstOrDefault()!.ErrorMessage :
                                            string.Join("|", response.Results.Select(x => x.ErrorMessage));

                    return Results.Problem(errorMessage, statusCode: 400);
                }

                return await next(invocationContext);
            });

            return builder;
        }
        public static bool IsSystemException(this Exception e)
        {
            return e.GetType().IsSubclassOf(typeof(SystemException));
        }
        public static string CreateQueryString(this string baseUrl, Dictionary<string, string> queryParams)
        {
            return QueryHelpers.AddQueryString(baseUrl, queryParams);
        }
    }
}
