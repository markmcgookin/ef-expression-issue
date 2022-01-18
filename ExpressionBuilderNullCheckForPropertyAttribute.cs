using System;

namespace simple.dbapp;

[System.AttributeUsage(System.AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
public class ExpressionBuilderNullCheckForPropertyAttribute : Attribute
{
    public ExpressionBuilderNullCheckForPropertyAttribute(string propertyName)
    {
        this.PropertyName = propertyName;
    }
    public string PropertyName { get; set; }
}