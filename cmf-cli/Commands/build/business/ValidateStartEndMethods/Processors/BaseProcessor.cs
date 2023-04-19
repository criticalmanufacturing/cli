using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Abstractions.Processors;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;
using Cmf.CLI.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Processors;

internal abstract class BaseProcessor : IBaseProcessor
{
    protected string _namespaceName = string.Empty;
    protected string _className = string.Empty;
    protected string _methodName = string.Empty;
    protected List<KeyValuePair<string, string>> _parameterNames = new();
    protected string _outputType = string.Empty;

    internal BaseProcessor()
    {
    }

    public abstract bool IsStartMethod { get; }

    public string StartEndMethodString => IsStartMethod ? "StartMethod" : "EndMethod";

    public void SetValues(string namespaceName, string className, string methodName, IEnumerable<ParameterSyntax> parameters, string outputType)
    {
        _namespaceName = namespaceName;
        _className = className;
        _methodName = methodName;
        _parameterNames = new();
        _parameterNames.AddRange(parameters.Where(x => x.IsKind(SyntaxKind.Parameter)).Select(x => x.GetParameterNameValuePair()));
        _outputType = outputType;
    }

    public abstract void ValidateParameterCount(ArgumentListSyntax statementArguments);

    public virtual void Process(SyntaxNode statement)
    {
        var statementArguments = statement.DescendantNodes().OfType<ArgumentListSyntax>().FirstOrDefault();

        if (statementArguments is null)
        {
            Log.Warning(string.Format(ErrorMessages.MethodHasNoParameters, StartEndMethodString, _namespaceName, _className, _methodName));
            return;
        }

        ValidateParameterCount(statementArguments);

        ValidateMethodParameters(statementArguments.Arguments.SelectMany(arg => arg.ChildNodes()));
    }

    private void ValidateMethodParameters(IEnumerable<SyntaxNode> arguments)
    {
        foreach (var parameter in _parameterNames)
        {
            var containsParameter = false;

            foreach (var argument in arguments)
            {
                if (argument.ChildNodes().Where(x => x.IsKind(SyntaxKind.ArgumentList)).FirstOrDefault() is ArgumentListSyntax ObjectArgumentList
                    && ObjectArgumentList.Arguments.Count == 2)
                {
                    var dictionaryKey = ObjectArgumentList?.Arguments[0].ToString().Trim('"');
                    var dictionaryValue = ObjectArgumentList?.Arguments[1].ToString();

                    if (dictionaryKey != null && dictionaryKey.EqualsToItselfOrNameOfItself(parameter.Key) && parameter.Value == dictionaryValue)
                    {
                        containsParameter = true;
                        break;
                    }
                }
            }

            if (!containsParameter)
            {
                Log.Warning(string.Format(ErrorMessages.MethodParameterMissingOrIncorrectlyNamed, StartEndMethodString, _namespaceName, _className, _methodName, parameter.Key));
            }
        }

        foreach (var objectArgument in arguments)
        {
            if (objectArgument.IsKind(SyntaxKind.ObjectCreationExpression))
            {
                var ObjectArgumentList = objectArgument.ChildNodes().Where(x => x.IsKind(SyntaxKind.ArgumentList)).FirstOrDefault() as ArgumentListSyntax;
                var dictionaryKey = ObjectArgumentList?.Arguments[0].ToString().Trim('"').ToLower(1);
                var dictionaryValue = ObjectArgumentList?.Arguments[1].ToString();

                if (_parameterNames.ContainsKeyValue(dictionaryKey, dictionaryValue))
                {
                    Log.Warning(string.Format(ErrorMessages.MethodParameterMissingOrIncorrectlyNamed, StartEndMethodString, _namespaceName, _className, _methodName, dictionaryKey));
                }
            }
        }
    }
}