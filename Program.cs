using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Statiq.App;
using Statiq.Web;
namespace Myblog
{
    public class Program
    {
        static async Task<int> Main(string[] args) =>
            await Bootstrapper
                .Factory
                .CreateWeb(args)
                .ConfigureSettings(s =>
                {
                    var linkRootEnv = Environment.GetEnvironmentVariable("SITE_LINKROOT");
                    if (!string.IsNullOrWhiteSpace(linkRootEnv))
                    {
                        var linkRoot = linkRootEnv.Trim();
                        if (!linkRoot.StartsWith('/'))
                            linkRoot = "/" + linkRoot;

                        linkRoot = linkRoot.TrimEnd('/');
                        s.Add(Keys.LinkRoot, linkRoot);
                    }
                    else
                        s.Add(Keys.LinkRoot, string.Empty);
                })
                .RunAsync();
    }
}