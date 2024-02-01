using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using Practiseproject.Data;
using Practiseproject.DTO;
using Practiseproject.GenericeRepository.Interface;
using Practiseproject.Model;
using Practiseproject.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Practiseproject.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _genericRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public AuthService(IGenericRepository<User> userRepository,IMapper mapper,IConfiguration configuration)
        {
            _configuration = configuration;
            _mapper = mapper;
            _genericRepository = userRepository;
        }
        
        public AddUser Add(AddUser adduser)
        {
            User user=_mapper.Map<User>(adduser);
            user=_genericRepository.Add(user);
            adduser=_mapper.Map<AddUser>(adduser);
            return adduser;
        }

        public string Login(LoginModel loginModel)
        {
            IQueryable<User> users=_genericRepository.GetDatas().Where(x=>x.Email==loginModel.UserName && x.Password==loginModel.Password);
            var user =users.FirstOrDefault();
            UserDTO userDTO=_mapper.Map<UserDTO>(user);
            if(user!=null)
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub,_configuration["JWT:Subject"]),
                    new Claim("Id",user.Id.ToString()),
                    new Claim("Username",user.Name.ToString()),
                    new Claim("Email",user.Email)
                    
                };
                var token = GetToken(claims);
                var jwtToken= new JwtSecurityTokenHandler().WriteToken(token);
                return jwtToken;
            }
            else
            {
                return "no userfound.";
            }

        }
        private JwtSecurityToken GetToken(List<Claim> claims) 
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["JWT:Issuer"],
                _configuration["JWT:Audience"],
                claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: signIn);
            return token;

        }
    }
}
