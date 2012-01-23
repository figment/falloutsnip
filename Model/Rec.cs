using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Linq;
using System.Drawing;
using System.Text;
using RTF;
using TESVSnip.Data;

namespace TESVSnip
{
    [Persistable(Flags = PersistType.DeclaredOnly), Serializable]
    public abstract class Rec : BaseRecord
    {
        BaseRecord parent;

        protected Rec() { }

        protected Rec(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        [Persistable]
        protected string descriptiveName;
        public override string DescriptiveName
        {
            get { return descriptiveName == null ? Name : (Name + descriptiveName); }
            //set { descriptiveName = value; }
        }
        public override void SetDescription(string value)
        {
            this.descriptiveName = value;
        }
        public override void UpdateShortDescription() { this.descriptiveName = ""; }

        public override BaseRecord Parent
        {
            get { return parent; }
            internal set { this.parent = value; }
        }
    }
}
