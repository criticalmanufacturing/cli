using Autofac.Extras.Moq;
using Cmf.CLI.Commands.build.business.BusinessLinter.Abstractions;
using Cmf.CLI.Commands.build.business.BusinessLinter.Rules;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using Xunit;

namespace tests.Specs;

public class BusinessLinter
{
	[Fact]
	public void NoLoadInForeachRule_WhenLoadCalledInForeach_ShouldLogWarning()
	{
		// Arrange
		using var mock = AutoMock.GetLoose();
		var logger = mock.Mock<ILintLogger>();

		var code = @"
using System.Data;

public class TestClass
{
	public void ProcessMaterials(DataRowCollection rows)
	{
		foreach (DataRow dataRow in rows)
		{
			var material = CreateMaterial();
			material.Name = (string)dataRow[""Name""];
			material.Load();
		}
	}

	private dynamic CreateMaterial() => null;
}";

		var syntaxTree = CSharpSyntaxTree.ParseText(code);
		var root = syntaxTree.GetRoot();
		var methodNode = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

		var rule = mock.Create<NoLoadInForeachRule>();

		// Act
		rule.Analyze(methodNode, "TestFile.cs", "TestClass");

		// Assert
		logger.Verify(x => x.Warning(It.Is<string>(s => s.Contains("NoLoadInForeach"))), Times.Once);
	}

	[Fact]
	public void NoLoadInForeachRule_WhenChainedLoadCalledInForeach_ShouldLogWarning()
	{
		// Arrange
		using var mock = AutoMock.GetLoose();
		var logger = mock.Mock<ILintLogger>();

		var code = @"
using System.Data;

public class TestClass
{
	public void ProcessMaterials(DataRowCollection rows)
	{
		foreach (DataRow dataRow in rows)
		{
			var material = CreateMaterial();
			material.Facility.Load();
		}
	}

	private dynamic CreateMaterial() => null;
}";

		var syntaxTree = CSharpSyntaxTree.ParseText(code);
		var root = syntaxTree.GetRoot();
		var methodNode = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

		var rule = mock.Create<NoLoadInForeachRule>();

		// Act
		rule.Analyze(methodNode, "TestFile.cs", "TestClass");

		// Assert
		logger.Verify(x => x.Warning(It.Is<string>(s => s.Contains("NoLoadInForeach"))), Times.Once);
	}

	[Fact]
	public void NoLoadInForeachRule_WhenMultipleLoadCallsInForeach_ShouldLogMultipleWarnings()
	{
		// Arrange
		using var mock = AutoMock.GetLoose();
		var logger = mock.Mock<ILintLogger>();

		var code = @"
using System.Data;

public class TestClass
{
	public void ProcessMaterials(DataRowCollection rows)
	{
		foreach (DataRow dataRow in rows)
		{
			var material = CreateMaterial();
			material.Load();
			material.Facility.Load();
		}
	}

	private dynamic CreateMaterial() => null;
}";

		var syntaxTree = CSharpSyntaxTree.ParseText(code);
		var root = syntaxTree.GetRoot();
		var methodNode = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

		var rule = mock.Create<NoLoadInForeachRule>();

		// Act
		rule.Analyze(methodNode, "TestFile.cs", "TestClass");

		// Assert
		logger.Verify(x => x.Warning(It.Is<string>(s => s.Contains("NoLoadInForeach"))), Times.Exactly(2));
	}

	[Fact]
	public void NoLoadInForeachRule_WhenLoadCalledOutsideForeach_ShouldNotLogWarning()
	{
		// Arrange
		using var mock = AutoMock.GetLoose();
		var logger = mock.Mock<ILintLogger>();

		var code = @"
using System.Data;

public class TestClass
{
	public void ProcessMaterial()
	{
		var material = CreateMaterial();
		material.Load();
	}

	private dynamic CreateMaterial() => null;
}";

		var syntaxTree = CSharpSyntaxTree.ParseText(code);
		var root = syntaxTree.GetRoot();
		var methodNode = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

		var rule = mock.Create<NoLoadInForeachRule>();

		// Act
		rule.Analyze(methodNode, "TestFile.cs", "TestClass");

		// Assert
		logger.Verify(x => x.Warning(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void NoLoadInForeachRule_WhenNoLoadCallsInForeach_ShouldNotLogWarning()
	{
		// Arrange
		using var mock = AutoMock.GetLoose();
		var logger = mock.Mock<ILintLogger>();

		var code = @"
using System.Data;

public class TestClass
{
	public void ProcessMaterials(DataRowCollection rows)
	{
		foreach (DataRow dataRow in rows)
		{
			var material = CreateMaterial();
			material.Name = (string)dataRow[""Name""];
		}
	}

	private dynamic CreateMaterial() => null;
}";

		var syntaxTree = CSharpSyntaxTree.ParseText(code);
		var root = syntaxTree.GetRoot();
		var methodNode = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

		var rule = mock.Create<NoLoadInForeachRule>();

		// Act
		rule.Analyze(methodNode, "TestFile.cs", "TestClass");

		// Assert
		logger.Verify(x => x.Warning(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void NoLoadInForeachRule_WhenNestedForeachWithLoad_ShouldLogWarning()
	{
		// Arrange
		using var mock = AutoMock.GetLoose();
		var logger = mock.Mock<ILintLogger>();

		var code = @"
using System.Collections.Generic;

public class TestClass
{
	public void ProcessData(List<List<dynamic>> data)
	{
		foreach (var outer in data)
		{
			foreach (var item in outer)
			{
				item.Load();
			}
		}
	}
}";

		var syntaxTree = CSharpSyntaxTree.ParseText(code);
		var root = syntaxTree.GetRoot();
		var methodNode = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

		var rule = mock.Create<NoLoadInForeachRule>();

		// Act
		rule.Analyze(methodNode, "TestFile.cs", "TestClass");

		// Assert
		logger.Verify(x => x.Warning(It.Is<string>(s => s.Contains("NoLoadInForeach"))), Times.Once);
	}
}
