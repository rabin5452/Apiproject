using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Practiseproject.Data;
using Practiseproject.DTO;
using Practiseproject.GenericeRepository.Interface;
using Practiseproject.Model;
using Practiseproject.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Practiseproject.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _genericRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        public AuthService(IGenericRepository<User> userRepository, IMapper mapper, IConfiguration configuration, UserManager<User> userManager)
        {
            _userManager = userManager;

            _configuration = configuration;
            _mapper = mapper;
            _genericRepository = userRepository;
        }

        public async Task<AddUser> Add(AddUser adduser)
        {
            var userexist = await _userManager.FindByNameAsync(adduser.Username);
            if (userexist == null)
            {
                User user = new()
                {
                    Email = adduser.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = adduser.Username,
                    PasswordHash = adduser.Password

                };
                var result = await _userManager.CreateAsync(user, adduser.Password);
                adduser = _mapper.Map<AddUser>(user);
                return adduser;

            }
            else
            {
                adduser = null;
                return adduser;
            }
        }

        public async Task<TokenModel> RefreshToken(TokenModel tokenModel)
        {
            if (tokenModel is null)
            {
                return tokenModel;
            }
            string? accessToken = tokenModel.AccessToken;
            string? refreshToken = tokenModel.RefreshToken;

            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                tokenModel = null;
                return tokenModel;
            }
            string username = principal.Identity.Name;
            var user = await _userManager.FindByNameAsync(username);
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                tokenModel = null;
                return tokenModel;
            }
            var newAccessToken = GetToken(principal.Claims.ToList());
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);
            tokenModel.AccessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken);
            tokenModel.RefreshToken = newRefreshToken;
            return tokenModel;
            
        }

        public async Task<TokenResponse> Login(LoginModel loginModel)
        {
            var user = await _userManager.FindByNameAsync(loginModel.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, loginModel.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

                var token = GetToken(claims);
                var refreshToken = GenerateRefreshToken();
                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

                await _userManager.UpdateAsync(user);

                TokenResponse tokenResponse = new TokenResponse
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo
                };

                return tokenResponse;
            }

            // Handle invalid user or password
            return null;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private JwtSecurityToken GetToken(List<Claim> claims)
        {
            _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["JWT:Issuer"],
                _configuration["JWT:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
                signingCredentials: signIn);
            return token;

        }
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"])),
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
