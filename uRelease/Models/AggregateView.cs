using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace uRelease.Models
{
    using System.Diagnostics;

    public class AggregateView
    {
        public string version { get; set; }

        public bool released { get; set; }

        public bool latestRelease { get; set; }

        public bool inProgressRelease { get; set; }

        public bool isPatch { get; set; }

        public string releaseDate { get; set; }

        public string releaseDescription { get; set; }

        public IEnumerable<IssueView> issues { get; set; }

        public IEnumerable<ActivityView> activities { get; set; }

        public bool currentRelease { get; set; }

        public bool plannedRelease { get; set; }
    }

    public class IssuesWrapper
    {
        public List<Issue> Issues { get; set; }
    }

    [DebuggerDisplay("Issue: {Id}")]
    public class Issue
    {
        public string Id { get; set; }
        //public string Id { get; set; }

        //public List<Field> Field { get; set; }
        public List<Field> Fields { get; set; }
    }

    [DebuggerDisplay("Field: {Name} {Value}")]
    public class Field
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Changes : List<Change>
    {
        //public List<Change> Change { get; set; }
        public Issue Issue { get; set; }
    }

    public class Change
    {
        public List<Field> Fields { get; set; }

        //public string Name { get; set; }
        //public string Value { get; set; }
        //public string NewValue { get; set; }
        //public string OldValue { get; set; }
        //public List<ChangedField> Fields { get; set; }

        public class Field
        {
            public string name { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public string NewValue { get; set; }
            public string OldValue { get; set; }
        }
    }



    public class IssueView
    {
        public string id { get; set; }

        public string title { get; set; }

        public string state { get; set; }

        public string type { get; set; }

        public Boolean breaking { get; set; }

    }

    public class ActivityView
    {
        /// <summary>
        /// Gets or sets the issue id for the activity
        /// </summary>
        /// <value>The id.</value>
        public string id { get; set; }

        public string username { get; set; }

        public long date { get; set; }

        public IEnumerable<ChangeView> changes { get; set; }
    }

    public class ChangeView
    {
        public string fieldName { get; set; }
        public string oldValue { get; set; }
        public string newValue { get; set; }
    }
}