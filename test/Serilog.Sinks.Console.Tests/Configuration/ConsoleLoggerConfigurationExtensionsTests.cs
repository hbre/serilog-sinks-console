using System;
using System.IO;
using System.Linq;
using Xunit;
using Serilog.Sinks.SystemConsole.Themes;

namespace Serilog.Sinks.Console.Tests.Configuration
{
    public class ConsoleLoggerConfigurationExtensionsTests
    {
        [Fact]
        public void OutputFormattingIsIgnored()
        {
            using (var stream = new MemoryStream())
            {
                var sw = new StreamWriter(stream);

                System.Console.SetOut(sw);
                var config = new LoggerConfiguration()
                    .WriteTo.Console(theme: AnsiConsoleTheme.Literate,
                        applyThemeToRedirectedOutput: false);

                var logger = config.CreateLogger();

                logger.Error("test");
                stream.Position = 0;

                using (var streamReader = new StreamReader(stream))
                {
                    var result = streamReader.ReadToEnd();
                    var controlCharacterCount = result.Count(c => Char.IsControl(c) && !Char.IsWhiteSpace(c));
                    Assert.Equal(0, controlCharacterCount);
                }
            }
        }
        
        [Fact]
        public void OutputFormattingIsPresent()
        {
            using (var stream = new MemoryStream())
            {
                var sw = new StreamWriter(stream);

                System.Console.SetOut(sw);
                var config = new LoggerConfiguration()
                    .WriteTo.Console(theme: AnsiConsoleTheme.Literate,
                        applyThemeToRedirectedOutput: true);

                var logger = config.CreateLogger();

                logger.Error("test");
                stream.Position = 0;

                using (var streamReader = new StreamReader(stream))
                {
                    var result = streamReader.ReadToEnd();
                    var controlCharacterCount = result.Count(c => Char.IsControl(c) && !Char.IsWhiteSpace(c));
                    Assert.NotEqual(0, controlCharacterCount);
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SkipNullValuesInOutputTests(bool skipNullValuesInOutput)
        {
            using (var stream = new MemoryStream())
            {
                var sw = new StreamWriter(stream);

                System.Console.SetOut(sw);
                var config = new LoggerConfiguration()
                    .WriteTo.Console(theme: ConsoleTheme.None,
                    outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    skipNullValuesInOutput: skipNullValuesInOutput);

                var logger = config.CreateLogger();

                logger.Error("Test: {@0}", new Dictionary<string, object>(){{"FirstKey", new DateTime(2020,12,24) }, {"SecondKey", null } });
                stream.Position = 0;

                using (var streamReader = new StreamReader(stream))
                {
                    var result = streamReader.ReadToEnd();

                    Assert.Equal($"[ERR] Test: {{\"FirstKey\": \"2020-12-24T00:00:00.0000000\"{(skipNullValuesInOutput ? "" : ", \"SecondKey\": null")}}}\r\n", result);
                }
            }
        }
    }
}