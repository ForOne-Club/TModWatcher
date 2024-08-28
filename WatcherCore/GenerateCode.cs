using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WatcherCore;

public class GenerateCode(TreeItem treeItem, string assemblyName, bool snakeCase = true, bool generateExtension = true)
{
    public string Generate()
    {
        //声明静态类声明
        ClassDeclarationSyntax classDeclaration = SyntaxFactory.ClassDeclaration("R").AddModifiers(
            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
            SyntaxFactory.Token(SyntaxKind.StaticKeyword));

        GenerateClass(ref classDeclaration, treeItem);
        return classDeclaration.NormalizeWhitespace().ToFullString();
    }

    private void GenerateClass(ref ClassDeclarationSyntax parent, TreeItem parentItem)
    {
        if (parentItem.Directory)
        {
            if (!parentItem.HasFile()) return;

            //声明静态类声明
            ClassDeclarationSyntax classDeclaration = SyntaxFactory.ClassDeclaration(parentItem.FileName).AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            //遍历
            foreach (TreeItem item in parentItem.TreeItems)
                GenerateClass(ref classDeclaration, item);

            //添加并更新Parent
            parent = parent.AddMembers(classDeclaration);
        }
        else
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(parentItem.RelativePath);
            // 拼接目录路径和去掉扩展名的文件名
            var directoryPath = Path.GetDirectoryName(parentItem.RelativePath);
            if (directoryPath == null || fileNameWithoutExtension == null) return;
            var resultPath = Path.Combine(directoryPath, fileNameWithoutExtension);

            // 指定 string 类型
            PredefinedTypeSyntax type = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
            // 指定变量名称并设置初始值
            VariableDeclaratorSyntax variableDeclarator = SyntaxFactory
                .VariableDeclarator(SyntaxFactory.Identifier(GetCSharpFieldName(parentItem.FilePath)))
                .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal($"{assemblyName}/{resultPath.Replace("\\", "/")}"))));
            // 创建 VariableDeclarationSyntax
            VariableDeclarationSyntax variableDeclaration = SyntaxFactory.VariableDeclaration(type)
                .WithVariables(SyntaxFactory.SingletonSeparatedList(variableDeclarator));
            // 创建 FieldDeclarationSyntax
            FieldDeclarationSyntax fieldDeclaration = SyntaxFactory.FieldDeclaration(variableDeclaration).AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.ConstKeyword));

            //更新
            parent = parent.AddMembers(fieldDeclaration);
        }
    }

    private string GetCSharpFieldName(string path)
    {
        // 获取文件名和扩展名
        var fileName = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);

        // 处理文件名和扩展名
        var processedFileName = CapitalizeFirstLetter(fileName);
        var processedExtension = CapitalizeFirstLetter(extension.Replace(".", ""));

        // 合并文件名和扩展名
        var result = processedFileName;
        if (generateExtension)
        {
            if (snakeCase)
                result += "_";
            result += processedExtension;
        }

        // 将非字母数字字符替换为下划线
        result = Regex.Replace(result, @"[^\w\d\u4e00-\u9fa5]", "_");

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