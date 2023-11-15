
using CodeGen.Services;

namespace CodeGen {
    public static class Program {
        public static async Task Main(string[] args) {
            if (args.Length > 0) {
                var converter = new CsToTsService(args[0]);
                await converter.ConvertToTypeScriptAsync();
            }
        }
    }
}