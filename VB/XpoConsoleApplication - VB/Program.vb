Imports System
Imports System.Collections.Generic
Imports System.Text
Imports DevExpress.Xpo
Imports DevExpress.Xpo.Metadata
Imports DevExpress.Data.Filtering

Namespace XpoConsoleApplication
	Friend Class Program
		Shared Sub Main(ByVal args() As String)

			Dim dict As XPDictionary = New ReflectionDictionary()

			'Create dynamic classes and members
			Dim ciEngineer As XPClassInfo = dict.CreateClass("Engineer")
			ciEngineer.CreateMember("UserID", GetType(String), New KeyAttribute(), New SizeAttribute(8))
			ciEngineer.CreateMember("PublicName", GetType(String), New SizeAttribute(32))

			Dim ciProject As XPClassInfo = dict.CreateClass("Project")
			ciProject.CreateMember("ProjectID", GetType(String), New KeyAttribute(), New SizeAttribute(4))
			ciProject.CreateMember("Title", GetType(String), New SizeAttribute(255))
			ciProject.CreateMember("Description", GetType(String), New SizeAttribute(SizeAttribute.Unlimited))
			ciProject.CreateMember("Assignments", GetType(XPCollection), True, New AssociationAttribute("R1", Nothing, "Assignment"))

			Dim ciAssignment As XPClassInfo = dict.CreateClass("Assignment")
			Dim miAssignmentKey As New XPComplexCustomMemberInfo(ciAssignment, "Key", GetType(Object), New KeyAttribute())
			miAssignmentKey.AddSubMember("Engineer", ciEngineer, New PersistentAttribute("Engineer"))
			miAssignmentKey.AddSubMember("Project", ciProject, New PersistentAttribute("Project"), New AssociationAttribute("R1"))
			ciAssignment.CreateMember("Role", GetType(String), New SizeAttribute(32))
			ciAssignment.CreateMember("Rank", GetType(Integer))

			Dim ciTask As XPClassInfo = dict.CreateClass("Task")
			Dim miTaskKey As New XPComplexCustomMemberInfo(ciTask, "Key", GetType(Object), New KeyAttribute())
			miTaskKey.AddSubMember("Project", ciProject, New PersistentAttribute("Project"))
			miTaskKey.AddSubMember("TaskNo", GetType(Integer), New PersistentAttribute("TaskNo"))
			ciTask.CreateMember("Description", GetType(String), New SizeAttribute(SizeAttribute.Unlimited))

			'Initialize the data layer
			'XpoDefault.DataLayer = XpoDefault.GetDataLayer(DevExpress.Xpo.DB.MSSqlConnectionProvider.GetConnectionString("(local)", "E4606"), dict, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
			XpoDefault.DataLayer = New SimpleDataLayer(dict, New DevExpress.Xpo.DB.InMemoryDataStore())

			'Create DB and some sample data
			Using uow As New UnitOfWork()
				uow.UpdateSchema()
				Dim engineers As IList(Of Object) = CreateObjects(uow, ciEngineer, New String() { "UserID", "PublicName" },
				New Object()() {
					New Object(){ "U1", "John" },
					New Object(){ "PAUL", "Paul" },
					New Object(){ "XFR", "Fred" }
				})
				Dim projects As IList(Of Object) = CreateObjects(uow, ciProject, New String() { "ProjectID", "Title", "Description" },
				New Object()() {
					New Object(){ "P1", "DemoApp", "bla-bla-bla" },
					New Object(){ "P2", "MegaApp", "duh!" },
					New Object(){ "PNEW", "KillerApp", "wow!" }
				})
				Dim assignments As IList(Of Object) = CreateObjects(uow, ciAssignment, New String() { "Key.Engineer", "Key.Project", "Role", "Rank" },
				New Object()() {
					New Object(){ engineers(1), projects(0), "Developer", 6 },
					New Object(){ engineers(2), projects(0), "Developer", 8 },
					New Object(){ engineers(0), projects(1), "Leader", 10 },
					New Object(){ engineers(2), projects(1), "Developer", 8 },
					New Object(){ engineers(1), projects(1), "Consultant", 3 },
					New Object(){ engineers(1), projects(2), "Developer", 8 },
					New Object(){ engineers(2), projects(2), "Developer", 7 },
					New Object(){ engineers(0), projects(2), "Tester", 5 }
				})
				Dim tasks As IList(Of Object) = CreateObjects(uow, ciTask, New String() { "Key.Project", "Key.TaskNo", "Description" },
				New Object()() {
					New Object(){ projects(0), 1, "Start" },
					New Object(){ projects(0), 2, "Finish" },
					New Object(){ projects(1), 0, "Framework" },
					New Object(){ projects(1), 1, "Feature1" },
					New Object(){ projects(1), 2, "Feature2" },
					New Object(){ projects(1), 3, "Feature3" },
					New Object(){ projects(1), 4, "Feature4" },
					New Object(){ projects(1), 5, "Feature5" },
					New Object(){ projects(1), 6, "Polish" },
					New Object(){ projects(2), 1, "ready" },
					New Object(){ projects(2), 2, "steady" },
					New Object(){ projects(2), 3, "go" }
				})
				uow.CommitChanges()
			End Using

			'read data
			Using uow As New UnitOfWork()
				'
				Dim xpv As New XPView(uow, ciTask)
				xpv.Sorting.Add(New SortProperty("Project", DevExpress.Xpo.DB.SortingDirection.Ascending))
				xpv.Sorting.Add(New SortProperty("No", DevExpress.Xpo.DB.SortingDirection.Ascending))
				xpv.AddProperty("Project", "Key.Project.Title")
				xpv.AddProperty("No", "Key.TaskNo")
				xpv.AddProperty("Task", "Description")
				For Each r As ViewRecord In xpv
					Dim sb As New StringBuilder()
					For Each p As ViewProperty In xpv.Properties
						If sb.Length > 0 Then
							sb.Append(", ")
						End If
						sb.Append(r(p.Name))
					Next p
					Console.WriteLine(sb.ToString())
				Next r
				Console.WriteLine()
			End Using
			Using uow As New UnitOfWork()
				'
				Dim xpc As New XPCollection(uow, ciProject)
				For Each o As Object In xpc
					Console.WriteLine(DumpValues(ciProject, o, New String() { "Title", "Description" }))
					Dim xpc2 As XPCollection = TryCast(ciProject.GetMember("Assignments").GetValue(o), XPCollection)
					For Each o2 As Object In xpc2
						Console.WriteLine("    " & DumpValues(ciAssignment, o2, New String() { "Role", "Rank", "Key.Engineer.PublicName" }))
					Next o2
				Next o
				Console.WriteLine()
			End Using
			Using uow As New UnitOfWork()
				'
				Dim project As Object = uow.FindObject(ciProject, CriteriaOperator.Parse("ProjectID=?", "P2"))
				Dim taskKey As New DevExpress.Xpo.Helpers.IdList()
				taskKey.Add(project)
				taskKey.Add(2)
				Dim task As Object = uow.GetObjectByKey(ciTask, taskKey)
				Console.WriteLine(DumpValues(ciTask, task, New String() { "Key.Project.Title", "Key.TaskNo", "Description" }))
				Console.WriteLine()
			End Using
			Console.ReadLine()
		End Sub
		Private Shared Function CreateObjects(ByVal s As Session, ByVal ci As XPClassInfo, ByVal props() As String, ByVal objValues()() As Object) As IList(Of Object)
			Dim list As IList(Of Object) = New List(Of Object)()
			For Each values As Object() In objValues
				Dim o As Object = ci.CreateNewObject(s)
				For i As Integer = 0 To props.Length - 1
					ci.GetMember(props(i)).SetValue(o, values(i))
				Next i
				s.Save(o)
				list.Add(o)
			Next values
			Return list
		End Function
		Private Shared Function DumpValues(ByVal ci As XPClassInfo, ByVal o As Object, ByVal props() As String) As String
			Dim sb As New StringBuilder()
			For Each p As String In props
				If sb.Length > 0 Then
					sb.Append(", ")
				End If
				Dim v As Object = (New DevExpress.Data.Filtering.Helpers.ExpressionEvaluator(ci.GetEvaluatorContextDescriptor(), New DevExpress.Data.Filtering.OperandProperty(p))).Evaluate(o)
				sb.Append(v)
			Next p
			Return sb.ToString()
		End Function
	End Class

	<NonPersistent>
	Public Class LiteDataObject
		Inherits XPLiteObject

		Public Sub New(ByVal s As Session)
			MyBase.New(s)
		End Sub
		Public Sub New(ByVal s As Session, ByVal ci As XPClassInfo)
			MyBase.New(s, ci)
		End Sub
	End Class


End Namespace
