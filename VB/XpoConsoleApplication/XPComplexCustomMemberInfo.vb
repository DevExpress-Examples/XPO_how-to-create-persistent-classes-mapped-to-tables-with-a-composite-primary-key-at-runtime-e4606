Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports DevExpress.Xpo.Metadata

Namespace DevExpress.Xpo.Metadata

    Public Class XPComplexCustomMemberInfo
        Inherits XPCustomMemberInfo

        Public Sub New(ByVal owner As XPClassInfo, ByVal propertyName As String, ByVal propertyType As Type, ParamArray ByVal attributes() As Attribute)
            Me.New(owner, propertyName, propertyType, Nothing, False, False, attributes)
        End Sub
        Public Sub New(ByVal owner As XPClassInfo, ByVal propertyName As String, ByVal referenceType As XPClassInfo, ParamArray ByVal attributes() As Attribute)
            Me.New(owner, propertyName, Nothing, referenceType, False, False, attributes)
        End Sub
        Public Sub New(ByVal owner As XPClassInfo, ByVal propertyName As String, ByVal propertyType As Type, ByVal referenceType As XPClassInfo, ByVal nonPersistent As Boolean, ByVal nonPublic As Boolean, ParamArray ByVal attributes() As Attribute)
            MyBase.New(owner, propertyName, propertyType, referenceType, nonPersistent, nonPublic)
            If Equals(Me.subMembersArray, XPMemberInfo.EmptyList) Then
                Me.subMembersArray = New List(Of XPMemberInfo)()
            End If
            For i As Integer = 0 To attributes.Length - 1
                Dim attribute As Attribute = attributes(i)
                Me.AddAttribute(attribute)
            Next i
        End Sub
        Public Overrides ReadOnly Property IsStruct() As Boolean
            Get
                Return Me.SubMembers.Count > 0
            End Get
        End Property
        Public Sub AddSubMember(ByVal memeberInfo As XPComplexCustomMemberInfo)
            Me.SubMembers.Add(memeberInfo)
            memeberInfo.valueParent = Me
        End Sub
        Public Function AddSubMember(ByVal propertyName As String, ByVal propertyType As Type, ParamArray ByVal attributes() As Attribute) As XPComplexCustomMemberInfo
            Dim memeberInfo As New XPComplexCustomMemberInfo(Me.Owner, propertyName, propertyType, attributes)
            AddSubMember(memeberInfo)
            Return memeberInfo
        End Function
        Public Function AddSubMember(ByVal propertyName As String, ByVal referenceType As XPClassInfo, ParamArray ByVal attributes() As Attribute) As XPComplexCustomMemberInfo
            Dim memeberInfo As New XPComplexCustomMemberInfo(Me.Owner, propertyName, referenceType, attributes)
            AddSubMember(memeberInfo)
            Return memeberInfo
        End Function
        Public Overrides ReadOnly Property Name() As String
            Get
                If valueParent Is Nothing Then
                    Return MyBase.Name
                End If
                Return String.Concat(Me.valueParent.Name, "."c, MyBase.Name)
            End Get
        End Property
        Protected Overrides Function GetDefaultMappingField() As String
            If Me.valueParent Is Nothing Then
                Return MyBase.Name
            End If
            Return Me.valueParent.MappingField & MyBase.Name
        End Function
        Public Overrides Function GetValue(ByVal theObject As Object) As Object
            If IsStruct Then
                Dim idList As New DevExpress.Xpo.Helpers.IdList()
                For Each memberInfo As XPMemberInfo In Me.SubMembers
                    If memberInfo.IsPersistent Then
                        idList.Add(memberInfo.GetValue(theObject))
                    End If
                Next memberInfo
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
                    Next i
                End If
                Return
            End If
            MyBase.SetValue(theObject, theValue)
        End Sub
    End Class
End Namespace
