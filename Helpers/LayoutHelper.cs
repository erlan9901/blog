using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myblog.Helpers
{
    public static class LayoutHelper
    {
        public static IEnumerable<(string text, string color)> Tags
        {
            get {
                return
            [
                ("C#", "text-blue-600 bg-blue-300/50"),
                ("后端", "text-stone-600 bg-stone-400/50"),
                (".NET", "text-indigo-600 bg-indigo-300/50"),
                ("Javascript", "text-amber-600 bg-amber-300/50"),
                ("HTML", "text-red-600 bg-red-300/50"),
                ("CSS", "text-yellow-600 bg-yellow-300/50"),
                ("前端", "text-lime-600 bg-lime-300/50"),
                ("React", "text-cyan-600 bg-cyan-300/50"),
                ("Vue", "text-green-600 bg-green-400/50"),
                ("数据库", "text-zinc-600 bg-zinc-300/50"),
                ("MSSQL", "text-slate-600 bg-slate-300/50"),
                ("MySQL", "text-teal-600 bg-teal-300/50"),
                ("Python", "text-gray-600 bg-gray-300/50"),
                ("Redux", "text-pink-600 bg-pink-300/50")
            ];
            }
        }
    }
}
