using System.Text.Json;

namespace Final_Project.Utils.Resources.Commons
{
    public class ErrorDetail
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}