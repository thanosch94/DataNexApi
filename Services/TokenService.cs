using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Models;
using Json.Net;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DataNexApi.Services
{
    public class TokenService
    {
        public static string GenerateToken(User user)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("=d*T-pAtiG-cEID=&8,^XVTSNE50.)|6Ch(PM~L&`A'y(mChC_.2mR|,h]-TM~9.Z$Pam.gz]ZH)HwP`!setATBPaV^2Wlq+~kdohCDo`H0BC8i[U}PY>V.fqHhZ#O"));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var userRoleName = string.Empty;
            using (var context = new ApplicationDbContext(AppBase.ConnectionString))
            {
                var userRole = context.UserRoles.Where(x => x.UserId == user.Id).FirstOrDefault();
                if (userRole != null)
                {
                    var role = context.Roles.Where(x => x.Id == userRole.RoleId).FirstOrDefault();
                    userRoleName = role.Name;
                }
            }
            var tokeOptions = new JwtSecurityToken(
                issuer: "http://localhost:5000",
                audience: "http://localhost:5000",
                claims: new List<Claim>()
                {
                       new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(user,new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        })),
                       new Claim(ClaimTypes.Role, userRoleName),

                },
                expires: DateTime.Now.AddHours(8),

                signingCredentials: signinCredentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);

            return tokenString;
        }
    }
}
