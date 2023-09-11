Imports System
Imports System.Collections
Imports System.Collections.Generic

Namespace DevExpress.Xpo.Metadata

    Public Class XPComplexCustomMemberInfo
        Inherits XPCustomMemberInfo

        Public Sub New(ByVal owner As XPClassInfo, ByVal propertyName As String, ByVal propertyType As Type, ParamArray attributes As Attribute())
            Me.New(owner, propertyName, propertyType, Nothing, False, False, attributes)
        End Sub

        Public Sub New(ByVal owner As XPClassInfo, ByVal propertyName As String, ByVal referenceType As XPClassInfo, ParamArray attributes As Attribute())
            Me.New(owner, propertyName, Nothing, referenceType, False, False, attributes)
        End Sub

        Public Sub New(ByVal owner As XPClassInfo, ByVal propertyName As String, ByVal propertyType As Type, ByVal referenceType As XPClassInfo, ByVal nonPersistent As Boolean, ByVal nonPublic As Boolean, ParamArray attributes As Attribute())
            MyBase.New(owner, propertyName, propertyType, referenceType, nonPersistent, nonPublic)
            If Equals(subMembersArray, EmptyList) Then
                subMembersArray = New List(Of XPMemberInfo)()
            End If

            For i As Integer = 0 To attributes.Length - 1
                Dim attribute As Attribute = attributes(i)
                Me.AddAttribute(attribute)
            Next
        End Sub

        Public Overrides ReadOnly Property IsStruct As Boolean
            Get
                Return Me.SubMembers.Count > 0
            End Get
        End Property

        Public Sub AddSubMember(ByVal memeberInfo As XPComplexCustomMemberInfo)
            Me.SubMembers.Add(memeberInfo)
            memeberInfo.valueParent = Me
        End Sub

        Public Function AddSubMember(ByVal propertyName As String, ByVal propertyType As Type, ParamArray attributes As Attribute()) As XPComplexCustomMemberInfo
            Dim memeberInfo As XPComplexCustomMemberInfo = New XPComplexCustomMemberInfo(Owner, propertyName, propertyType, attributes)
            AddSubMember(memeberInfo)
            Return memeberInfo
        End Function

        Public Function AddSubMember(ByVal propertyName As String, ByVal referenceType As XPClassInfo, ParamArray attributes As Attribute()) As XPComplexCustomMemberInfo
            Dim memeberInfo As XPComplexCustomMemberInfo = New XPComplexCustomMemberInfo(Owner, propertyName, referenceType, attributes)
            AddSubMember(memeberInfo)
            Return memeberInfo
        End Function

        Public Overrides ReadOnly Property Name As String
            Get
                If valueParent Is Nothing Then Return MyBase.Name
                Return String.Concat(valueParent.Name, "."c, MyBase.Name)
            End Get
        End Property

        Protected Overrides Function GetDefaultMappingField() As String
            If valueParent Is Nothing Then Return MyBase.Name
            Return valueParent.MappingField & MyBase.Name
        End Function

        Public Overrides Function GetValue(ByVal theObject As Object) As Object
            If IsStruct Then
                Dim idList As Xpo.Helpers.IdList = New Xpo.Helpers.IdList()
                For Each memberInfo As XPMemberInfo In Me.SubMembers
                    If memberInfo.IsPersistent Then
                        idList.Add(memberInfo.GetValue(theObject))
                    End If
                Next

                Return idList
            End If

            Return MyBase.GetValue(theObject)
        End Function

        Public Overrides Sub SetValue(ByVal theObject As Object, ByVal theValue As Object)
            If IsStruct Then
                Dim list As IList = TryCast(theValue, IList)
                If list IsNot Nothing Then
                    For i As Integer = 0 To list.Count - 1
                        Dim memberInfo As XPMemberInfo = CType(Me.SubMembers(i), XPMemberInfo)
                        If memberInfo.IsPersistent Then
                            memberInfo.SetValue(theObject, list(i))
                        End If
                    Next
                End If

                Return
            End If

            MyBase.SetValue(theObject, theValue)
        End Sub
    End Class
End Namespace
