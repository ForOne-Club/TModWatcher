// File: ConsoleInteraction.cs
// Author: TrifingZW
// Created: 2025-01-02 at 23:01:02
// Description:
// Dependencies:

using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace WatcherCore;

public class ConsoleAttacher(int processId)
{
    // 导入kernel32.dll以使用AttachConsole和FreeConsole
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool AttachConsole(int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool FreeConsole();

    // 导入user32.dll以模拟键盘事件
    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

    // 定义用于模拟输入的结构体
    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint type; // 输入类型（1表示键盘输入）
        public MOUSEKEYBDHARDWAREINPUT mi; // 键盘或鼠标输入
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MOUSEKEYBDHARDWAREINPUT
    {
        [FieldOffset(0)] public KEYBDINPUT ki; // 键盘输入
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk; // 虚拟键码
        public ushort wScan; // 扫描码
        public uint dwFlags; // 键盘事件标志
        public uint time; // 时间戳
        public IntPtr dwExtraInfo; // 额外信息
    }

    // 定义键盘输入事件的常量
    const uint KEYEVENTF_KEYDOWN = 0x0000; // 按下键
    const uint KEYEVENTF_KEYUP = 0x0002; // 松开键
    const int ATTACH_PARENT_PROCESS = -1; // 附加到父进程控制台

    // 目标进程ID

    // 构造函数，传入目标进程ID

    // 尝试附加到目标进程的控制台
    public bool Attach()
    {
        // 尝试附加到目标进程的控制台
        if (AttachConsole(processId))
        {
            Console.WriteLine("成功附加到目标进程的控制台。");
            return true;
        }
        else
        {
            // 如果附加失败，输出错误信息
            Console.WriteLine("附加到目标进程的控制台失败，错误代码：" + Marshal.GetLastWin32Error());
            return false;
        }
    }

    // 释放控制台附加
    public void Detach()
    {
        FreeConsole(); // 释放控制台附加
        Console.WriteLine("控制台已释放附加。");
    }

    // 模拟按下某个键
    public void SimulateKeyPress(ushort key)
    {
        // 创建键盘输入事件
        INPUT input = new INPUT
        {
            type = 1, // 键盘输入
            mi = new MOUSEKEYBDHARDWAREINPUT
            {
                ki = new KEYBDINPUT
                {
                    wVk = key, // 设置虚拟键码
                    dwFlags = KEYEVENTF_KEYDOWN // 设置按下键
                }
            }
        };

        // 发送按键按下事件
        SendInput(1, ref input, Marshal.SizeOf(typeof(INPUT)));

        // 发送按键松开事件
        input.mi.ki.dwFlags = KEYEVENTF_KEYUP; // 设置松开键
        SendInput(1, ref input, Marshal.SizeOf(typeof(INPUT)));
    }
}