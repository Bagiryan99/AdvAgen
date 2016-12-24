using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AdvAgen.Startup))]
namespace AdvAgen
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
