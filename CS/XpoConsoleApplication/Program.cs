using System;
using System.Collections.Generic;
using System.Text;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;

namespace XpoConsoleApplication {
    class Program {
        static void Main(string[] args) {

            XPDictionary dict = new ReflectionDictionary();

            //Create dynamic classes and members
            XPClassInfo ciEngineer = dict.CreateClass("Engineer");
            ciEngineer.CreateMember("UserID", typeof(string), new KeyAttribute(), new SizeAttribute(8));
            ciEngineer.CreateMember("PublicName", typeof(string), new SizeAttribute(32));

            XPClassInfo ciProject = dict.CreateClass("Project");
            ciProject.CreateMember("ProjectID", typeof(string), new KeyAttribute(), new SizeAttribute(4));
            ciProject.CreateMember("Title", typeof(string), new SizeAttribute(255));
            ciProject.CreateMember("Description", typeof(string), new SizeAttribute(SizeAttribute.Unlimited));
            ciProject.CreateMember("Assignments", typeof(XPCollection), true, new AssociationAttribute("R1", null, "Assignment"));

            XPClassInfo ciAssignment = dict.CreateClass("Assignment");
            XPComplexCustomMemberInfo miAssignmentKey = new XPComplexCustomMemberInfo(ciAssignment, "Key", typeof(object), new KeyAttribute());
            miAssignmentKey.AddSubMember("Engineer", ciEngineer, new PersistentAttribute("Engineer"));
            miAssignmentKey.AddSubMember("Project", ciProject, new PersistentAttribute("Project"), new AssociationAttribute("R1"));
            ciAssignment.CreateMember("Role", typeof(string), new SizeAttribute(32));
            ciAssignment.CreateMember("Rank", typeof(int));

            XPClassInfo ciTask = dict.CreateClass("Task");
            XPComplexCustomMemberInfo miTaskKey = new XPComplexCustomMemberInfo(ciTask, "Key", typeof(object), new KeyAttribute());
            miTaskKey.AddSubMember("Project", ciProject, new PersistentAttribute("Project"));
            miTaskKey.AddSubMember("TaskNo", typeof(int), new PersistentAttribute("TaskNo"));
            ciTask.CreateMember("Description", typeof(string), new SizeAttribute(SizeAttribute.Unlimited));

            //Initialize the data layer
            //XpoDefault.DataLayer = XpoDefault.GetDataLayer(DevExpress.Xpo.DB.MSSqlConnectionProvider.GetConnectionString("(local)", "E4606"), dict, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
            XpoDefault.DataLayer = new SimpleDataLayer(dict, new DevExpress.Xpo.DB.InMemoryDataStore());

            //Create DB and some sample data
            using (UnitOfWork uow = new UnitOfWork()) {
                uow.UpdateSchema();
                IList<object> engineers = CreateObjects(uow, ciEngineer, new string[] { "UserID", "PublicName" }, new object[][] {
                    new object[]{ "U1", "John" },
                    new object[]{ "PAUL", "Paul" },
                    new object[]{ "XFR", "Fred" },
                });
                IList<object> projects = CreateObjects(uow, ciProject, new string[] { "ProjectID", "Title", "Description" }, new object[][] {
                    new object[]{ "P1", "DemoApp", "bla-bla-bla" },
                    new object[]{ "P2", "MegaApp", "duh!" },
                    new object[]{ "PNEW", "KillerApp", "wow!" },
                });
                IList<object> assignments = CreateObjects(uow, ciAssignment, new string[] { "Key.Engineer", "Key.Project", "Role", "Rank" }, new object[][] {
                    new object[]{ engineers[1], projects[0], "Developer", 6 },
                    new object[]{ engineers[2], projects[0], "Developer", 8 },
                    new object[]{ engineers[0], projects[1], "Leader", 10 },
                    new object[]{ engineers[2], projects[1], "Developer", 8 },
                    new object[]{ engineers[1], projects[1], "Consultant", 3 },
                    new object[]{ engineers[1], projects[2], "Developer", 8 },
                    new object[]{ engineers[2], projects[2], "Developer", 7 },
                    new object[]{ engineers[0], projects[2], "Tester", 5 },
                });
                IList<object> tasks = CreateObjects(uow, ciTask, new string[] { "Key.Project", "Key.TaskNo", "Description" }, new object[][] {
                    new object[]{ projects[0], 1, "Start" },
                    new object[]{ projects[0], 2, "Finish" },
                    new object[]{ projects[1], 0, "Framework" },
                    new object[]{ projects[1], 1, "Feature1" },
                    new object[]{ projects[1], 2, "Feature2" },
                    new object[]{ projects[1], 3, "Feature3" },
                    new object[]{ projects[1], 4, "Feature4" },
                    new object[]{ projects[1], 5, "Feature5" },
                    new object[]{ projects[1], 6, "Polish" },
                    new object[]{ projects[2], 1, "ready" },
                    new object[]{ projects[2], 2, "steady" },
                    new object[]{ projects[2], 3, "go" },
                });
                uow.CommitChanges();
            }

            //read data
            using (UnitOfWork uow = new UnitOfWork()) {
                //
                XPView xpv = new XPView(uow, ciTask);
                xpv.Sorting.Add(new SortProperty("Project", DevExpress.Xpo.DB.SortingDirection.Ascending));
                xpv.Sorting.Add(new SortProperty("No", DevExpress.Xpo.DB.SortingDirection.Ascending));
                xpv.AddProperty("Project", "Key.Project.Title");
                xpv.AddProperty("No", "Key.TaskNo");
                xpv.AddProperty("Task", "Description");
                foreach(ViewRecord r in xpv){
                    StringBuilder sb = new StringBuilder();
                    foreach (ViewProperty p in xpv.Properties) {
                        if (sb.Length > 0) sb.Append(", ");
                        sb.Append(r[p.Name]);
                    }
                    Console.WriteLine(sb.ToString());
                }
                Console.WriteLine();
            }
            using (UnitOfWork uow = new UnitOfWork()) {
                //
                XPCollection xpc = new XPCollection(uow, ciProject);
                foreach (object o in xpc) {
                    Console.WriteLine(DumpValues(ciProject, o, new string[] { "Title", "Description" }));
                    XPCollection xpc2 = ciProject.GetMember("Assignments").GetValue(o) as XPCollection;
                    foreach (object o2 in xpc2) {
                        Console.WriteLine("    " + DumpValues(ciAssignment, o2, new string[] { "Role", "Rank", "Key.Engineer.PublicName" }));
                    }
                }
                Console.WriteLine();
            }
            using (UnitOfWork uow = new UnitOfWork()) {
                //
                object project = uow.FindObject(ciProject, CriteriaOperator.Parse("ProjectID=?", "P2"));
                DevExpress.Xpo.Helpers.IdList taskKey = new DevExpress.Xpo.Helpers.IdList();
                taskKey.Add(project);
                taskKey.Add(2);
                object task = uow.GetObjectByKey(ciTask, taskKey);
                Console.WriteLine(DumpValues(ciTask, task, new string[] { "Key.Project.Title", "Key.TaskNo", "Description" }));
                Console.WriteLine();
            }
            Console.ReadLine();
        }
        private static IList<object> CreateObjects(Session s, XPClassInfo ci, string[] props, object[][] objValues) {
            IList<object> list = new List<object>();
            foreach (object[] values in objValues) {
                object o = ci.CreateNewObject(s);
                for (int i = 0; i < props.Length; i++) {
                    ci.GetMember(props[i]).SetValue(o, values[i]);
                }
                s.Save(o);
                list.Add(o);
            }
            return list;
        }
        private static string DumpValues(XPClassInfo ci, object o, string[] props) {
            StringBuilder sb = new StringBuilder();
            foreach (string p in props) {
                if (sb.Length > 0) sb.Append(", ");
                object v = new DevExpress.Data.Filtering.Helpers.ExpressionEvaluator(ci.GetEvaluatorContextDescriptor(),
                    new DevExpress.Data.Filtering.OperandProperty(p)).Evaluate(o);
                sb.Append(v);
            }
            return sb.ToString();
        }
    }

    [NonPersistent]
    public class LiteDataObject : XPLiteObject {
        public LiteDataObject(Session s) : base(s) { }
        public LiteDataObject(Session s, XPClassInfo ci) : base(s, ci) { }
    }


}
