namespace BeyondResponsiveDesign.Menus
{
    using Owin;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseStaticFiles();
        }
    }
}