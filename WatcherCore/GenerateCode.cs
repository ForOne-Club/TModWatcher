using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WatcherCore;

public class GenerateCode(TreeItem treeItem, string assemblyName, bool snakeCase)
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
            // 创建静态字段声明
            // 指定 string 类型
            PredefinedTypeSyntax type = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
            // 指定变量名称并设置初始值
            VariableDeclaratorSyntax variableDeclarator = SyntaxFactory
                .VariableDeclarator(SyntaxFactory.Identifier(FileHelper.GetCSharpFieldName(parentItem.FilePath, snakeCase)))
                .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal($"{assemblyName}/{parentItem.RelativePath.Replace("\\", "/")}"))));
            // 创建 VariableDeclarationSyntax
            VariableDeclarationSyntax variableDeclaration = SyntaxFactory.VariableDeclaration(type)
                .WithVariables(SyntaxFactory.SingletonSeparatedList(variableDeclarator));
            // 创建 FieldDeclarationSyntax
            FieldDeclarationSyntax fieldDeclaration = SyntaxFactory.FieldDeclaration(variableDeclaration).AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            //更新
            parent = parent.AddMembers(fieldDeclaration);
        }
    }
}