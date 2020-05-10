Imports Aqua.TypeSystem
Imports Aqua.TypeSystem.Extensions
Imports Shouldly
Imports Xunit

Namespace Aqua.Test.TypeSystem.Extensions
    Public Class When_creating_constructor_info
        Private Class A
        End Class

        <Fact>
        Public Sub Should_create_from_memberinfo()
            Dim a = New A
            Dim types(-1) As Type
            Dim ctor = a.GetType().GetConstructor(types)
            Dim ctorInfo = New ConstructorInfo(ctor)
            ctorInfo.Name.ShouldBe(".ctor")
            ctorInfo.IsStatic.ShouldBeNull()
            ctorInfo.DeclaringType.Type.ShouldBeSameAs(a.GetType())
        End Sub

        <Fact>
        Public Sub Should_create_from_name()
            Dim a = New A
            Dim ctorInfo = New ConstructorInfo(".ctor", a.GetType())
            Dim types(-1) As Type
            Dim ctor = a.GetType().GetConstructor(types)
            ctorInfo.Constructor.ShouldBeSameAs(ctor)
        End Sub
    End Class
End Namespace