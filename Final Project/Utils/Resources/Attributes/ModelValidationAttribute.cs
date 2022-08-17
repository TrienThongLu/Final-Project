﻿using System.ComponentModel.DataAnnotations;
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

    [AttributeUsage(AttributeTargets.Property)]
    public class DateOfBirthAttribute : ValidationAttribute
    {
        public int MinAge { get; set; }
        public int MaxAge { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _value = (new DateTime(1970, 1, 1)).AddMilliseconds(Double.Parse(value.ToString()));
            var _yearObject = _value.Year;
            var _yearNow = DateTime.UtcNow.Year;

            if (_yearNow - _yearObject < MinAge || _yearNow - _yearObject > MaxAge)
            {
                return new ValidationResult("The age is not acceptable. It must between 12 and 100.");
            }

            return ValidationResult.Success;
        }
    }
}
