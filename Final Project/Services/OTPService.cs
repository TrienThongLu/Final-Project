using Final_Project.Models;
using MongoDB.Driver;

namespace Final_Project.Services
{
    public class OTPService
    {
        private readonly IConfiguration _configuration;
        public IMongoCollection<OTPModel> otpCollection;

        public OTPService(IConfiguration configuration)
        {
            _configuration = configuration;
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString"));
            otpCollection = mongoClient.GetDatabase("FinalProject").GetCollection<OTPModel>("OTP");
        }

        public async Task<string> generateOTP(OTPModel otpObject)
        {
            var _otpExistence = await otpCollection.Find(o => o.PhoneNumber == otpObject.PhoneNumber).FirstOrDefaultAsync();
            if (_otpExistence != null)
            {
                await deleteOTP(_otpExistence.Id);
            }
            var _otpCode = "";
            Random rd = new Random();
            for (int i = 0; i < 6; i++)
            {
                _otpCode += rd.Next(0, 9).ToString();
            }
            otpObject.OTP = _otpCode;
            otpObject.ExpireAt = DateTime.UtcNow.AddMinutes(3);
            await otpCollection.InsertOneAsync(otpObject);

            return otpObject.OTP;
        }

        public async Task<OTPModel> getOTP(string otp, string phonenumber)
        {
            return await otpCollection.Find(o => o.OTP == otp && o.PhoneNumber == phonenumber).FirstOrDefaultAsync();
        }

        public async Task deleteOTP(string id)
        {
            await otpCollection.DeleteOneAsync(o => o.Id == id);
        }
    }
}
