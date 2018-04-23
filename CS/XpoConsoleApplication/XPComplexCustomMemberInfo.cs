using System;
using System.Collections;
using System.Collections.Generic;
using DevExpress.Xpo.Metadata;

namespace DevExpress.Xpo.Metadata {

    public class XPComplexCustomMemberInfo : XPCustomMemberInfo {
        public XPComplexCustomMemberInfo(XPClassInfo owner, string propertyName, Type propertyType, params Attribute[] attributes)
            : this(owner, propertyName, propertyType, null, false, false, attributes) { }
        public XPComplexCustomMemberInfo(XPClassInfo owner, string propertyName, XPClassInfo referenceType, params Attribute[] attributes)
            : this(owner, propertyName, null, referenceType, false, false, attributes) { }
        public XPComplexCustomMemberInfo(XPClassInfo owner, string propertyName, Type propertyType, XPClassInfo referenceType, bool nonPersistent, bool nonPublic, params Attribute[] attributes)
            : base(owner, propertyName, propertyType, referenceType, nonPersistent, nonPublic) {
            if (Equals(this.subMembersArray, XPMemberInfo.EmptyList)) {
                this.subMembersArray = new List<XPMemberInfo>();
            }
            for (int i = 0; i < attributes.Length; i++) {
                Attribute attribute = attributes[i];
                this.AddAttribute(attribute);
            }
        }
        public override bool IsStruct { get { return this.SubMembers.Count > 0; } }
        public void AddSubMember(XPComplexCustomMemberInfo memeberInfo) {
            this.SubMembers.Add(memeberInfo);
            memeberInfo.valueParent = this;
        }
        public XPComplexCustomMemberInfo AddSubMember(string propertyName, Type propertyType, params Attribute[] attributes) {
            XPComplexCustomMemberInfo memeberInfo = new XPComplexCustomMemberInfo(this.Owner, propertyName, propertyType, attributes);
            AddSubMember(memeberInfo);
            return memeberInfo;
        }
        public XPComplexCustomMemberInfo AddSubMember(string propertyName, XPClassInfo referenceType, params Attribute[] attributes) {
            XPComplexCustomMemberInfo memeberInfo = new XPComplexCustomMemberInfo(this.Owner, propertyName, referenceType, attributes);
            AddSubMember(memeberInfo);
            return memeberInfo;
        }
        public override string Name {
            get {
                if (valueParent == null) return base.Name;
                return string.Concat(this.valueParent.Name, '.', base.Name);
            }
        }
        protected override string GetDefaultMappingField() {
            if (this.valueParent == null) return base.Name;
            return this.valueParent.MappingField + base.Name;
        }
        public override object GetValue(object theObject) {
            if (IsStruct) {
                DevExpress.Xpo.Helpers.IdList idList = new DevExpress.Xpo.Helpers.IdList();
                foreach (XPMemberInfo memberInfo in this.SubMembers) {
                    if (memberInfo.IsPersistent) {
                        idList.Add(memberInfo.GetValue(theObject));
                    }
                }
                return idList;
            }
            return base.GetValue(theObject);
        }
        public override void SetValue(object theObject, object theValue) {
            if (IsStruct) {
                IList list = theValue as IList;
                if (list != null) {
                    for (int i = 0; i < list.Count; i++) {
                        XPMemberInfo memberInfo = (XPMemberInfo)this.SubMembers[i];
                        if (memberInfo.IsPersistent) {
                            memberInfo.SetValue(theObject, list[i]);
                        }
                    }
                }
                return;
            }
            base.SetValue(theObject, theValue);
        }
    }
}
