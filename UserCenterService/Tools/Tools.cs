using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace UserCenterService.Tools
{
    internal class Tools
    {
        //JWT密钥，用于签名Token
        public static readonly string _jwtSecret = "UserCenter_JWT_SecretKey_081220";
        //Token有效期
        public static TimeSpan _tokenExpire
        {
            get
            {
                 return TimeSpan.FromHours(1);
            }
        }

        //MD5加密
        public static string ComputMd5Hash(string input)
        {
            using var md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
        //生成JWT Token
        public static string GenerateJWToken(string userID)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userID) };
            var keys = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds = new SigningCredentials(keys, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(claims: claims,
                                             expires: DateTime.UtcNow.Add(_tokenExpire),
                                             signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        //校验Token
        public static bool ValidateJWToken(string token, out long userID)
        {
            userID = 0;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSecret);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var JWToken = (JwtSecurityToken)validatedToken;
                //userID = JWToken.Claims.First(Claim => Claim.Type == ClaimTypes.NameIdentifier).Value;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
