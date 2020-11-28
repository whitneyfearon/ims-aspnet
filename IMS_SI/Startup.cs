using Microsoft.Owin;
using Owin;


[assembly: OwinStartupAttribute(typeof(IMS_SI.Startup))]
namespace IMS_SI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
