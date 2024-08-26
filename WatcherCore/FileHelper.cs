namespace WatcherCore;

public static class FileHelper
{
    public static string GetCSharpFieldName(string path, bool snakeCase = true)
    {
        // 获取文件名和扩展名
        var fileName = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);

        // 处理文件名和扩展名
        var processedFileName = CapitalizeFirstLetter(fileName);
        var processedExtension = CapitalizeFirstLetter(extension.Replace(".", ""));

        // 合并文件名和扩展名
        var snake = snakeCase ? "_" : "";
        var result = $"{processedFileName}{snake}{processedExtension}";

        // 处理 C# 关键字冲突 (添加前缀)
        if (IsCSharpKeyword(result))
            result = "_" + result;

        // 如果第一个字符是数字，则添加下划线
        if (char.IsDigit(result, 0))
            result = "_" + result;

        return result;
    }

    private static string CapitalizeFirstLetter(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var firstChar = text[0];

        if (char.IsLetter(firstChar))
            return char.ToUpper(firstChar) + text[1..];

        return text;
    }

    // 检查字符串是否为 C# 关键字
    private static bool IsCSharpKeyword(string text)
    {
        string[] keywords =
        [
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal",
            "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null",
            "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
            "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
            "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
        ];
        return keywords.Contains(text);
    }
}