using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Practiseproject.Constraint;
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
        private readonly RoleManager<IdentityRole> _roleManager;
        public AuthService(IGenericRepository<User> userRepository, IMapper mapper, IConfiguration configuration, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _mapper = mapper;
            _genericRepository = userRepository;
            _roleManager = roleManager;
        }
        public async Task<AddUser> AddAdmin(AddUser adduser)
        {
            var userExist = await _userManager.FindByNameAsync(adduser.Username);

            if (userExist == null)
            {
                var user = new User
                {
                    Email = adduser.Email,
                    UserName = adduser.Username,
                    // Password should not be set here; let Identity handle hashing
                };

                var result = await _userManager.CreateAsync(user, adduser.Password);

                if (result.Succeeded)
                {
                    // Check if the role exists, and create it if not
                    var roleExists = await _roleManager.RoleExistsAsync(Roles.Admin.ToString());
                    if (!roleExists)
                    {
                        // Create the role if it doesn't exist
                        await _roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
                    }

                    // Assign the user to the "User" role
                    await _userManager.AddToRoleAsync(user, Roles.Admin.ToString());

                    adduser = _mapper.Map<AddUser>(user);
                    return adduser;
                }
                else
                {
                    // Handle errors from _userManager.CreateAsync if needed
                    adduser = null;
                    return adduser;
                }
            }
            else
            {
                adduser = null;
                return adduser;
            }
        }
        public async Task<AddUser> Add(AddUser adduser)
        {
            var userExist = await _userManager.FindByNameAsync(adduser.Username);

            if (userExist == null)
            {
                var user = new User
                {
                    Email = adduser.Email,
                    UserName = adduser.Username,
                    // Password should not be set here; let Identity handle hashing
                };

                var result = await _userManager.CreateAsync(user, adduser.Password);

                if (result.Succeeded)
                {
                    // Check if the role exists, and create it if not
                    var roleExists = await _roleManager.RoleExistsAsync(Roles.User.ToString());
                    if (!roleExists)
                    {
                        // Create the role if it doesn't exist
                        await _roleManager.CreateAsync(new IdentityRole(Roles.User.ToString()));
                    }

                    // Assign the user to the "User" role
                    await _userManager.AddToRoleAsync(user, Roles.User.ToString());

                    adduser = _mapper.Map<AddUser>(user);
                    return adduser;
                }
                else
                {
                    // Handle errors from _userManager.CreateAsync if needed
                    adduser = null;
                    return adduser;
                }
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
                var userRoles = await _userManager.GetRolesAsync(user);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, loginModel.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                foreach (var userRole in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole));
                }

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
