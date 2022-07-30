using Final_Project.Utils.Resources.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Utils.Helpers
{
    public static class ObjectValidateHelper
    {
        public static List<String> ValidateObject(object Object)
        {
            var _validationResults = new List<ValidationResult>();
            var _validationContext = new ValidationContext(Object);
            Validator.TryValidateObject(Object, _validationContext, _validationResults, true);

            if (_validationResults.Count() > 0)
            {
                List<String> errors = new List<String>();
                foreach (ValidationResult validationResult in _validationResults)
                {
                    errors.Add(validationResult.ErrorMessage);
                }
                return errors;
            }
            return null;
        }
    }
}
