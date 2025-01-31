// File: NamedPipe.cs
// Author: TrifingZW
// Created: 2025-01-03 at 02:01:59
// Description:
// Dependencies:

using System.IO.Pipes;

namespace WatcherCore;

public class NamedPipe(string pipeName)
{
    private NamedPipeServerStream? _pipeServer;
    private NamedPipeClientStream? _pipeClient;

    // 作为服务器端接收消息
    public void MonitorOutput(CancellationToken cancellationToken)
    {
        Task.Run(
            () =>
            {
                try
                {
                    _pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1);
                    _pipeServer.WaitForConnection();

                    using var reader = new StreamReader(_pipeServer);
                    while (!cancellationToken.IsCancellationRequested)
                        if (_pipeServer.IsConnected)
                        {
                            string? message = reader.ReadLine();
                            if (message != null)
                                Console.WriteLine($"tModLoader: {message}");
                        }
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
                }
            },
            cancellationToken
        );
    }


    // 作为客户端发送消息
    public async Task SendMessageAsync(string message)
    {
        _pipeClient = new NamedPipeClientStream(pipeName);
        await _pipeClient.ConnectAsync();

        await using var writer = new StreamWriter(_pipeClient);
        await writer.WriteLineAsync(message);
        await writer.FlushAsync();
        Console.WriteLine($"Sent to server: {message}");
    }

    // 关闭管道连接
    public void Close()
    {
        _pipeServer?.Dispose();
        _pipeClient?.Dispose();
    }
}