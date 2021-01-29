using cw5.DTOs;
using cw5.Models;
using cw5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Nest;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace cw5.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
    private IStudentsDbService _service;
 
        public IConfiguration Configuration { get; set; }
        public StudentsController(IStudentsDbService service, IConfiguration configuration)
        {
         _service = service;
            Configuration = configuration;
         }

        [HttpGet]
        [Authorize(Roles="admin")]
        public IActionResult GetStudents() 
        { 

            try
            {
                List<Student> resp = _service.GetStudents();

                return CreatedAtRoute(new RouteValues(), resp);
            }
            catch(ArgumentException e)
            {
                return BadRequest(e.Message);
            }


            return Ok();
        }
    [HttpPost]
       public IActionResult Login(LoginRequestDto log)
            {
            var claim = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,"1"),
                new Claim(ClaimTypes.Name, "jan123"),
                new Claim(ClaimTypes.Role,"admin"),
                new Claim(ClaimTypes.Role, "student")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "Gakko",
                audience: "Students",
                claims: claim,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
                );
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = Guid.NewGuid()
            }
                ) ; 
            }

            }
    
}
