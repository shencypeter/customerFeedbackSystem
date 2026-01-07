using Microsoft.AspNetCore.Mvc;

namespace CustomerFeedbackSystem.Models
{
    public static class FormValidator
    {
        public static IActionResult CheckRequiredFields(
        IFormCollection collection,
        Dictionary<string, string> requiredFields,
        Controller controller,
        Func<IActionResult> returnAction,
        string tempDataKey)
        {
            foreach (var field in requiredFields)
            {
                string value = collection[field.Key].ToString().Trim();
                if (string.IsNullOrEmpty(value))
                {
                    controller.TempData[tempDataKey] = field.Value;
                    return returnAction();
                }
            }

            return null;
        }

        public static IActionResult CheckDateFormat(
            IFormCollection collection,
            string fieldKey,
            string errorMessage,
            Controller controller,
            Func<IActionResult> returnAction,
            string tempDataKey)
        {
            var value = collection[fieldKey].ToString().Trim();
            if (!string.IsNullOrEmpty(value) && !DateOnly.TryParse(value, out _))
            {
                controller.TempData[tempDataKey] = errorMessage;
                return returnAction();
            }
            return null;
        }
    }

}
