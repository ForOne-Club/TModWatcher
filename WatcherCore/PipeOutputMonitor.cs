// File: PipeOutputMonitor.cs
// Author: TrifingZW
// Created: 2025-01-03 at 03:01:37
// Description:
// Dependencies:

using System.IO.Pipes;

namespace WatcherCore;

public class PipeOutputMonitor(string pipeName)
{
    private NamedPipeServerStream? _pipeServer;
    private CancellationTokenSource? _cts;

    public void Start(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task.Run(() => MonitorOutput(_cts.Token), _cts.Token);
    }

    public void Stop() => _cts?.Cancel();

    private void MonitorOutput(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1);
                _pipeServer.WaitForConnection(); 

                using var reader = new StreamReader(_pipeServer);
                while (!cancellationToken.IsCancellationRequested && _pipeServer.IsConnected)
                    if (reader.ReadLine() is {} message)
                        Console.WriteLine($"tModLoader: {message}");
                    else
                        break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MonitorOutput: {ex.Message}");
            }
            finally
            {
                _pipeServer?.Dispose();
                _pipeServer = null;
            }
        }
    }
}