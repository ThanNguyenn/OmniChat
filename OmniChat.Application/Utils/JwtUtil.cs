using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Claim = System.Security.Claims.Claim;

namespace OmniChat.Application.Utils;

public class JwtUtil
{
    private readonly string _jwtkey, _issuer, _audience;
    private readonly double _expired;

    public JwtUtil(IConfiguration configuration)
    {
        _jwtkey = configuration["Jwt:Key"];
        _issuer = configuration["Jwt:Issuer"];
        _audience = configuration["Jwt:Audience"];
        _expired = double.Parse(configuration["Jwt:TokenValidityInMinutes"]);
    }

    public string GenerateJwtToken(Account user, Tuple<string, Guid> guidClaimer, string sessionId)
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SymmetricSecurityKey secrectKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtkey));
        var credentials = new SigningCredentials(secrectKey, SecurityAlgorithms.HmacSha256Signature);
        string issuer = _issuer;

        List<Claim> securityClaims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Staff.Id.ToString()),  
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("session_id",sessionId)
            };

        if (guidClaimer != null)
            securityClaims.Add(new Claim(guidClaimer.Item1, guidClaimer.Item2.ToString()));

        var expires = DateTime.UtcNow.AddMinutes(_expired);
        var token = new JwtSecurityToken(issuer, _audience, securityClaims, DateTime.UtcNow, expires, credentials);

        return tokenHandler.WriteToken(token);
    }
}