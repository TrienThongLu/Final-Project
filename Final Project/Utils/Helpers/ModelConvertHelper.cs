using Microsoft.AspNetCore.Mvc;
using Newtonsoft;
using Final_Project.Models;

namespace Final_Project.Utils.Helpers
{
    public static class ModelConvertHelper
    {
        public static T Convert<T>(object needConvert)
        {
            var _temp = Newtonsoft.Json.JsonConvert.SerializeObject(needConvert);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(_temp);
        }
    }
}