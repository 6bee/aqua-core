Imports Aqua.TypeSystem
Imports Aqua.TypeSystem.Extensions
Imports Shouldly
Imports Xunit

Namespace Aqua.Tests.TypeSystem.Extensions
    Public Class When_creating_constructor_info
        Private Class A
        End Class

        <Fact>
        Public Sub Should_create_from_memberinfo()
            Dim a = New A
            Dim typeargs = Array.Empty(Of Type)()
            Dim ctor = a.GetType().GetConstructor(typeargs)
            Dim ctorInfo = New ConstructorInfo(ctor)
            ctorInfo.Name.ShouldBe(".ctor")
            ctorInfo.IsStatic.ShouldBeNull()
            ctorInfo.DeclaringType.ToType().ShouldBeSameAs(a.GetType())
        End Sub

        <Fact>
        Public Sub Should_create_from_name()
            Dim a = New A
            Dim ctorInfo = New ConstructorInfo(".ctor", a.GetType())
            Dim typeargs = Array.Empty(Of Type)()
            Dim ctor = a.GetType().GetConstructor(typeargs)
            ctorInfo.ToConstructorInfo.ShouldBeSameAs(ctor)
        End Sub
    End Class
End Namespace