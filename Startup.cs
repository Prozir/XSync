using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Configuration;
using Microsoft.Owin.Host.SystemWeb;

[assembly: OwinStartup(typeof(XSync.Startup))]

namespace XSync
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {            
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    TokenValidationParameters = new TokenValidationParameters()
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings.Get("apiSecret"))),
                        //ValidateIssuer = true,
                        //ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = ConfigurationManager.AppSettings.Get("tokenIssuer"), //some string, normally web url,
                        ValidAudience = ConfigurationManager.AppSettings.Get("tokenAudience")
                        //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("V84mvs"))                        
                    }
                });
        }
    }
}
