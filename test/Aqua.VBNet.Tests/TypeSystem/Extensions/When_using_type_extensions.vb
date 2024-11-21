Imports Aqua.TypeExtensions
Imports Shouldly
Imports Xunit

Namespace Aqua.Tests.TypeSystem.Extensions
    Public Class When_using_type_extensions
        Private Class CustomType
        End Class

        <Fact>
        Public Sub Is_anonymous_should_return_true_for_anonymous_type()
            Dim obj = New With {Key .X = 0}
            obj.GetType().IsAnonymousType().ShouldBeTrue()
        End Sub

        <Fact>
        Public Sub Is_anonymous_should_return_false_for_custom_type()
            Dim obj = New CustomType
            obj.GetType().IsAnonymousType().ShouldBeFalse()
        End Sub

        <Fact>
        Public Sub Is_anonymous_should_return_false_for_string_type()
            Dim obj = ""
            obj.GetType().IsAnonymousType().ShouldBeFalse()
        End Sub
    End Class
End Namespace
