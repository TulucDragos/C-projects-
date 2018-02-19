using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FirmeIDS.Startup))]
namespace FirmeIDS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
