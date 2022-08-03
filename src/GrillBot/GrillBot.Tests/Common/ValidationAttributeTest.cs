using System;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Tests.Common;

public abstract class ValidationAttributeTest<TAttribute> : AttributeTest<TAttribute> where TAttribute : ValidationAttribute
{
    protected ValidationResult Execute(object value, Action<ValidationContext> contextConfiguration = null)
    {
        var context = CreateContext(contextConfiguration);
        return Attribute.GetValidationResult(value, context);
    }

    private ValidationContext CreateContext(Action<ValidationContext> contextConfiguration)
    {
        var context = CreateDefaultContext();
        contextConfiguration?.Invoke(context);

        return context;
    }

    private ValidationContext CreateDefaultContext()
    {
        return new ValidationContext(Attribute)
        {
            MemberName = "Value"
        };
    }

    protected void CheckSuccess(ValidationResult result)
        => Assert.AreEqual(ValidationResult.Success, result);

    protected void CheckFail(ValidationResult result, string expectedErrorMessage = null)
    {
        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));

        if (!string.IsNullOrEmpty(expectedErrorMessage))
            Assert.AreEqual(expectedErrorMessage, result.ErrorMessage);
    }
}
