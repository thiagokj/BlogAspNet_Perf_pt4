using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BlogAspNet_Performance.Extensions
{
    public static class ModelStateExtension
    {
        public static List<string> GetErrors(this ModelStateDictionary modelState)
        {
            var result = new List<string>();

            foreach (var item in modelState.Values)
                result.AddRange(from error in item.Errors
                                select error.ErrorMessage);

            return result;
        }
    }
}
