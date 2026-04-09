using Microsoft.Extensions.Logging;
using System.Text;

namespace Shared.Logging;

public sealed class FileErrorLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly object _lock = new();

    public FileErrorLoggerProvider(string filePath)
    {
        _filePath = Path.GetFullPath(filePath);
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
    }

    public ILogger CreateLogger(string categoryName) => new FileErrorLogger(categoryName, _filePath, _lock);

    public void Dispose() { }

    private sealed class FileErrorLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _filePath;
        private readonly object _lock;

        public FileErrorLogger(string categoryName, string filePath, object @lock)
        {
            _categoryName = categoryName;
            _filePath = filePath;
            _lock = @lock;
        }

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            var sb = new StringBuilder()
                .AppendLine($"[{DateTimeOffset.Now:O}] [{logLevel}] {_categoryName}")
                .AppendLine(message);

            if (exception is not null)
            {
                sb.AppendLine(exception.ToString());
            }

            sb.AppendLine();

            lock (_lock)
            {
                File.AppendAllText(_filePath, sb.ToString());
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}