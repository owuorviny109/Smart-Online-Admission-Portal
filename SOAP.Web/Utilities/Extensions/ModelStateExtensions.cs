using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SOAP.Web.Utilities.Extensions
{
    public static class ModelStateExtensions
    {
        public static List<string> GetErrorMessages(this ModelStateDictionary modelState)
        {
            var errors = new List<string>();

            foreach (var entry in modelState.Values)
            {
                foreach (var error in entry.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }

            return errors;
        }

        public static string GetFirstErrorMessage(this ModelStateDictionary modelState)
        {
            var errors = GetErrorMessages(modelState);
            return errors.FirstOrDefault() ?? string.Empty;
        }

        public static Dictionary<string, List<string>> GetErrorsByField(this ModelStateDictionary modelState)
        {
            var errorsByField = new Dictionary<string, List<string>>();

            foreach (var kvp in modelState)
            {
                var fieldName = kvp.Key;
                var fieldErrors = kvp.Value.Errors.Select(e => e.ErrorMessage).ToList();

                if (fieldErrors.Any())
                {
                    errorsByField[fieldName] = fieldErrors;
                }
            }

            return errorsByField;
        }
    }
}