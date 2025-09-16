using System.Threading.Tasks;
using Statiq.App;
using Statiq.Web;
using YamlDotNet.Core.Tokens;
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
                    var linkRoot = Environment.GetEnvironmentVariable("SITE_LINKROOT");
                    if (!string.IsNullOrEmpty(linkRoot))
                        s.Add(Keys.LinkRoot, "/myblog");
                    else
                        s.Add(Keys.LinkRoot, "");
                    
                })
                .RunAsync();
    }
}