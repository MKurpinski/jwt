using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthorizationSample.Controllers
{
    [Route("api/[controller]")]
    public class AuthController:Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly PasswordHasher<IdentityUser> _passwordHasher;
        private readonly IConfigurationRoot _configuration;

        public AuthController(UserManager<IdentityUser> userManager, PasswordHasher<IdentityUser> passwordHasher, IConfigurationRoot configuration)
        {
            _userManager = userManager;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }
        [HttpPost, Route("")]
        public async Task<IActionResult> Register([FromBody]UserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                return Ok();
            }
            return BadRequest();

        }
        [HttpPost("token")]
        public async Task<IActionResult> Token([FromBody] UserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByNameAsync(model.Email);

            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) != PasswordVerificationResult.Success)
            {
                return BadRequest();
            }

            var token = CreateToken(user);

            return Ok(
                new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
        }

        private JwtSecurityToken CreateToken(IdentityUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Tokens:Issuer"],
                audience: _configuration["Tokens:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );
            return token;
        }
    }
}
