using Final_Project.Models;
using Final_Project.Utils.Resources.Exceptions;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Final_Project.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        public IMongoCollection<TokenModel> tokenCollection;
        public IMongoClient jwtTokenClient;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionString"));
            jwtTokenClient = mongoClient;
            tokenCollection = mongoClient.GetDatabase("FinalProject").GetCollection<TokenModel>("Token");
        }

        public JwtSecurityToken ReadJwt(string token)
        {
            return new JwtSecurityTokenHandler()
                        .ReadJwtToken(token);
        }

        public TokenModel GenerateJwt(UserModel user, string roleName)
        {
            var _claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Id),
                new Claim(ClaimTypes.Role, roleName)
            };
            var _token = new JwtSecurityToken(
                claims: _claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: new SigningCredentials
                (
                    new SymmetricSecurityKey
                    (
                        Encoding.UTF8.GetBytes(_configuration.GetValue<string>("TokenKey"))), SecurityAlgorithms.HmacSha256Signature
                    )
                );
            var _tokenModel = new TokenModel()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(_token),
                ExpireAt = _token.ValidTo,
                UserId = user.Id,
            };
            return _tokenModel;
        }

        public async Task<bool> ValidateTokenAsync(string Token)
        {
            if (!Token.StartsWith("Bearer "))
                throw new HttpReturnException(HttpStatusCode.Unauthorized, "Missing Bearer inside token");
            Token = Token.Split("Bearer ")[1];
            TokenModel currentToken = await tokenCollection.Find(x => x.Token == Token).FirstOrDefaultAsync();
            JwtSecurityToken processeModelken = ReadJwt(Token);
            if (currentToken == null || currentToken.UserId != processeModelken.Claims.FirstOrDefault().Value) throw new HttpReturnException(HttpStatusCode.Unauthorized, "Token has been revoked or unauthorized");
            if (DateTime.UtcNow > currentToken.ExpireAt) throw new HttpReturnException(HttpStatusCode.Unauthorized, "Require refresh token");
            return true;
        }

        public async Task<IEnumerable<Claim>> DecryptToken(string Token)
        {
            Token = Token.Split("Bearer ")[1];
            JwtSecurityToken processeModelken = ReadJwt(Token);
            var x = processeModelken.Claims;
            return x;
        }

        public async Task RefreshTokenAsync(string Token)
        {
            TokenModel currentToken = await tokenCollection.Find(x => x.Token == Token).FirstOrDefaultAsync();
            TokenModel refresheModelken = new TokenModel() { Token = currentToken.Token, UserId = currentToken.UserId, ExpireAt = DateTime.UtcNow.AddMinutes(30) };
            if (currentToken != null)
            {
                await CreateAsync(refresheModelken);
            }
        }

        public async Task CreateAsync(TokenModel newToken)
        {
            await tokenCollection.FindOneAndDeleteAsync(x => x.UserId == newToken.UserId);
            await tokenCollection.InsertOneAsync(newToken);
        }

        public async Task DeleteAsync(string currentToken) =>
              await tokenCollection.FindOneAndDeleteAsync(x => x.Token == currentToken);
    }
}
