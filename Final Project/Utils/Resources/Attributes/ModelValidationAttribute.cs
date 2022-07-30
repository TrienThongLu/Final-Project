using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Final_Project.Utils.Resources.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PhoneNumber : ValidationAttribute
    {
        private Regex _regex = new Regex(@"[0]([35789]|(28))[0-9]{8}");

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (_regex.IsMatch(value.ToString()))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("The provided Phone number is not vaild");
        }
    }
}
