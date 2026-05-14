using System.Diagnostics;
using System.Text;

namespace TeachCodIT.Services
{
    public class CodeExecutionService
    {
        public async Task<string> RunCSharpCode(string userCode)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"code_{Guid.NewGuid()}.cs");

            var fullCode = $@"
using System;
class Program
{{
    static void Main()
    {{
        {userCode}
    }}
}}";

            await File.WriteAllTextAsync(filePath, fullCode);

            var exePath = filePath.Replace(".cs", ".exe");

            // компиляция
            var compile = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "csc",
                    Arguments = $"/out:{exePath} {filePath}",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            compile.Start();
            await compile.WaitForExitAsync();

            if (compile.ExitCode != 0)
                return "COMPILATION_ERROR";

            // запуск
            var run = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            run.Start();

            string output = await run.StandardOutput.ReadToEndAsync();
            await run.WaitForExitAsync();

            return output.Trim();
        }
    }
}