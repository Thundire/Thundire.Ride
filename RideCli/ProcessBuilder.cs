﻿using System.Diagnostics;
using System.Text;

namespace RideCli;
internal class ProcessBuilder {
	readonly List<string> _arguments = [];
    string? _workingDirectory;
    string? _fileName;

    Encoding? _inputEncoding;
    Encoding? _outputEncoding;
    Encoding? _errorEncoding;

	readonly List<Action<string>> _outputActions = [];
	readonly List<Action<string>> _errorActions = [];

    event Action<string?>? Error;
    event Action<string?>? Print;

    bool _outputSet;
    bool _errorSet;
    bool _asAdmin;

    private ProcessBuilder()
    {
        
    }

    private ProcessBuilder(string fileName) {
        _fileName = fileName;
    }

    public async ValueTask Execute(CancellationToken token) {
        if (string.IsNullOrEmpty(_fileName)) throw new InvalidOperationException("You not set filename of launching app");

        ProcessStartInfo startInfo = new(_fileName);
        if (_workingDirectory is not null) startInfo.WorkingDirectory = _workingDirectory;
        if (_arguments.Count > 0) {
            startInfo.Arguments = string.Join(" ", _arguments);
        }

        if (_inputEncoding is not null) startInfo.StandardInputEncoding = _inputEncoding;
        if (_outputEncoding is not null) startInfo.StandardOutputEncoding = _outputEncoding;
        if (_errorEncoding is not null) startInfo.StandardErrorEncoding = _errorEncoding;

        _outputSet = _outputActions.Count > 0;
        _errorSet = _errorActions.Count > 0;

        if (_outputSet) startInfo.RedirectStandardOutput = true;
        if (_errorSet) startInfo.RedirectStandardError = true;

        if (_asAdmin) {
            startInfo.Verb = "runas";
            startInfo.UseShellExecute = true;
            Process.Start(startInfo);
            return;
        }

        using Process? process = Process.Start(startInfo);
        if (process is null) return;

        if(_outputSet) {
            foreach (var action in _outputActions)
            {
                Print += action;
            }
            process.OutputDataReceived += OutputDataReceived;
            process.BeginOutputReadLine();
        }
        
        if(_errorSet) {
            foreach (var action in _errorActions)
            {
                Error += action;
            }
            process.ErrorDataReceived += ErrorDataReceived;
            process.BeginErrorReadLine();
        }

        try {
            await process.WaitForExitAsync(token);
        }
        catch (OperationCanceledException) {
            return;
        }
    }

    public ProcessBuilder WithWorkingDirectory(string workingDirectory) {
        _workingDirectory = workingDirectory;
        return this;
    }
    
    public ProcessBuilder WithFile(string fileName) {
        _fileName = fileName;
        return this;
    }
    
    public ProcessBuilder WithArguments(params string[] arguments) {
        _arguments.AddRange(arguments);
        return this;
    }
    
    public ProcessBuilder WithArgument(string argument) {
        _arguments.Add(argument);
        return this;
    }
    
    public ProcessBuilder WithInputEncoding(Encoding encoding) {
        _inputEncoding = encoding;
        return this;
    }
    
    public ProcessBuilder WithOutputEncoding(Encoding encoding) {
        _outputEncoding = encoding;
        return this;
    }
    
    public ProcessBuilder WithErrorEncoding(Encoding encoding) {
        _errorEncoding = encoding;
        return this;
    }
    
    public ProcessBuilder WithEncoding(Encoding encoding) {
        _errorEncoding = _inputEncoding = _outputEncoding = encoding;
        return this;
    }
    
    public ProcessBuilder WithOutput(Action<string> output) {
        _outputActions.Add(output);
        return this;
    }
    
    public ProcessBuilder WithError(Action<string> output) {
        _errorActions.Add(output);
        return this;
    }

    public ProcessBuilder AsAdmin() {
        _asAdmin = true;
        return this;
    }

    
    private void ErrorDataReceived(object sender, DataReceivedEventArgs e) => Error?.Invoke(e.Data ?? string.Empty);
    private void OutputDataReceived(object sender, DataReceivedEventArgs e) => Print?.Invoke(e.Data ?? string.Empty);

    public static ProcessBuilder Create(string? fileName = default) => fileName is { Length: > 0 } ? new (fileName) : new ();
	public static void Open(string path) => Process.Start("explorer", path).Dispose();

	public static ProcessBuilder operator | (ProcessBuilder builder, Action<string> outputAction) {
        return builder.WithOutput(outputAction);
    }
}
