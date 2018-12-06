using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using WebApi.Helpers;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebApi.Services;
using WebApi.Dtos;
using WebApi.Entities;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class LocationsController : ControllerBase
    {
        private ILocationService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;
        private static Producer<Null, string> KafkaProducer;

        public LocationsController(
            ILocationService userService,
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {

            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;

        }

        [AllowAnonymous]
        [HttpPost("radioLocation")]
        public IActionResult RadioLocation([FromBody]LocationDto location)
        {
            var locationText = location.Location;


            KafkaProducer.OnLog += (_, msg) =>
                       {
                           Console.WriteLine($"LOG: {msg.Message}");
                       };

            KafkaProducer.OnError += (_, ex) =>
                       {
                           Console.WriteLine($"ERROR: {ex}");
                       };


            KafkaProducer.ProduceAsync("incomming_radio_messages", null, locationText);

            return Ok(new {
                Id = "Location received"
            });
        }

        private void ValidateToken(string tokenString, string secret)
        {
            // CLIENT CODE ----------------------------

            TokenValidationParameters validationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    //ValidIssuer = this._appSettings.Token.Issuer,
                    //ValidAudiences = new string[] { },
                    IssuerSigningKeys = new[] { new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)) }
                };

            // Now validate the token. If the token is not valid for any reason, an exception will be thrown by the method
            SecurityToken validatedToken;
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            var nonValidated = handler.ReadJwtToken(tokenString);

            foreach (var item in nonValidated.Claims)
            {
                Console.WriteLine($"{item.Type} - {item.Value}");
            }

            var usr = handler.ValidateToken(tokenString, validationParameters, out validatedToken);
            // END CLIENT CODE ------------------------

        }


        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]UserDto userDto)
        {
            var user = _userService.Authenticate(userDto.Username, userDto.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),             
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info (without password) and token to store client side
            return Ok(new {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = tokenString
            });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]UserDto userDto)
        {
            // map dto to entity
            var user = _mapper.Map<User>(userDto);

            try 
            {
                // save 
                _userService.Create(user, userDto.Password);
                return Ok();
            } 
            catch(AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users =  _userService.GetAll();
            var userDtos = _mapper.Map<IList<UserDto>>(users);
            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user =  _userService.GetById(id);
            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]UserDto userDto)
        {
            // map dto to entity and set id
            var user = _mapper.Map<User>(userDto);
            user.Id = id;

            try 
            {
                // save 
                _userService.Update(user, userDto.Password);
                return Ok();
            } 
            catch(AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            return Ok();
        }
    }
}
