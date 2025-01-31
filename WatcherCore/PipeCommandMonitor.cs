// File: PipeCommandMonitor.cs
// Author: TrifingZW
// Created: 2025-01-03 at 03:01:32
// Description:
// Dependencies:

using System.IO.Pipes;

namespace WatcherCore;

public class PipeCommandMonitor(string pipeName)
{
    private NamedPipeClientStream? _pipeClient;
    private CancellationTokenSource? _cts;
    private StreamReader? _commandReader;

    public void Start(CancellationToken cancellationToken, Stream inputStream)
    {
        _commandReader = new StreamReader(inputStream);
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task.Run(() => MonitorClient(_cts.Token), _cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _pipeClient?.Dispose();
    }

    private void MonitorClient(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _pipeClient = new NamedPipeClientStream(pipeName);
                _pipeClient.Connect();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("已连接到命令管道服务 资源热重载已启用");
                
                using var writer = new StreamWriter(_pipeClient);
                writer.AutoFlush = true;

                while (_pipeClient.IsConnected && !cancellationToken.IsCancellationRequested)
                {
                    string? command = _commandReader?.ReadLine();

                    if (string.IsNullOrEmpty(command) || writer == null)
                        continue;

                    writer.WriteLine(command);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PipeCommandMonitor: {ex.Message}");
            }
            finally
            {
                // 连接断开后，释放资源
                _pipeClient?.Dispose();
                _pipeClient = null;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("命令管道服务已断开 资源热重载不可用");
            }
        }
    }
}