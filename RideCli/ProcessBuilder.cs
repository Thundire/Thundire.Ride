using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideCli;
internal class ProcessBuilder {

    Process? _process;

    List<string> _arguments = [];
    string? _workingDirectory;
    string? _fileName;

    Encoding? _inputEncoding;
    Encoding? _outputEncoding;
    Encoding? _errorEncoding;

    List<Action<string>> _outputActions = [];

    event Action<string?>? Print;

    bool _outPutSet;

    public ProcessBuilder()
    {
        
    }

    public ProcessBuilder(string fileName) {
        _fileName = fileName;
    }

    public async Task Execute(CancellationToken token) {
        if (string.IsNullOrEmpty(_fileName)) throw new InvalidOperationException("You not set filename of launching app");

        ProcessStartInfo startInfo = new(_fileName);
        if (_workingDirectory is not null) startInfo.WorkingDirectory = _workingDirectory;
        if (_arguments.Count > 0) {
            foreach (var argument in _arguments) {
                startInfo.ArgumentList.Add(argument);
            }
        }

        if (_inputEncoding is not null) startInfo.StandardInputEncoding = _inputEncoding;
        if (_outputEncoding is not null) startInfo.StandardOutputEncoding = _outputEncoding;
        if (_errorEncoding is not null) startInfo.StandardErrorEncoding = _errorEncoding;

        _process = Process.Start(startInfo);
        if (_process is null) return;

        _process.OutputDataReceived += OutputDataReceived;
        _process.ErrorDataReceived += ErrorDataReceived;

        _outPutSet = _outputActions.Count > 0; 

        if(_outPutSet) {
            foreach (var action in _outputActions)
            {
                Print += action;
            }
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        await _process.WaitForExitAsync(token);
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
    
    private void ErrorDataReceived(object sender, DataReceivedEventArgs e) => Print?.Invoke(e.Data);
    private void OutputDataReceived(object sender, DataReceivedEventArgs e) => Print?.Invoke(e.Data);

    public static ProcessBuilder Create(string? fileName = default) => fileName is { Length: > 0 } ? new (fileName) : new ();

    public static ProcessBuilder operator | (ProcessBuilder builder, Action<string> outputAction) {
        return builder.WithOutput(outputAction);
    }
}
