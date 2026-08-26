using System.Collections.Generic;

namespace PanopticonAuditHistorySearch.Model
{
    public static class AuditLabels
    {
        private static readonly Dictionary<int, string> Actions = new Dictionary<int, string>
        {
            {0,"Unknown"},{1,"Create"},{2,"Update"},{3,"Delete"},{4,"Activate"},{5,"Deactivate"},
            {6,"Upsert"},{11,"Cascade"},{12,"Merge"},{13,"Assign"},{14,"Share"},{15,"Retrieve"},
            {16,"Close"},{17,"Cancel"},{18,"Complete"},{20,"Resolve"},{21,"Reopen"},{22,"Fulfill"},
            {23,"Paid"},{24,"Qualify"},{25,"Disqualify"},{26,"Submit"},{27,"Reject"},{28,"Approve"},
            {29,"Invoice"},{30,"Hold"},{31,"Add Member"},{32,"Remove Member"},{33,"Associate Entities"},
            {34,"Disassociate Entities"},{35,"Add Members"},{36,"Remove Members"},{37,"Add Item"},
            {38,"Remove Item"},{39,"Add Substitute"},{40,"Remove Substitute"},{41,"Set State"},
            {42,"Renew"},{43,"Revise"},{44,"Win"},{45,"Lose"},{46,"Internal Processing"},
            {47,"Reschedule"},{48,"Modify Share"},{49,"Unshare"},{50,"Book"},
            {51,"Generate Quote From Opportunity"},{52,"Add To Queue"},{53,"Assign Role To Team"},
            {54,"Remove Role From Team"},{55,"Assign Role To User"},{56,"Remove Role From User"},
            {57,"Add Privileges to Role"},{58,"Remove Privileges From Role"},{59,"Replace Privileges In Role"},
            {60,"Import Mappings"},{61,"Clone"},{62,"Send Direct Email"},{63,"Enabled for organization"},
            {64,"User Access via Web"},{65,"User Access via Web Services"},{100,"Delete Entity"},
            {101,"Delete Attribute"},{102,"Audit Change at Entity Level"},{103,"Audit Change at Attribute Level"},
            {104,"Audit Change at Org Level"},{105,"Entity Audit Started"},{106,"Attribute Audit Started"},
            {107,"Audit Enabled"},{108,"Entity Audit Stopped"},{109,"Attribute Audit Stopped"},
            {110,"Audit Disabled"},{111,"Audit Log Deletion"},{112,"User Access Audit Started"},
            {113,"User Access Audit Stopped"},{115,"Archive"},{116,"Retain"},{117,"RollbackRetain"},
            {118,"IP Firewall Access Denied"},{119,"IP Firewall Access Allowed"},{120,"Restore"},
            {121,"Application Based Access Denied"},{122,"Application Based Access Allowed"},
            {123,"Create - AI assisted"},{124,"Update - AI assisted"},{125,"Read Unmasked"}
        };

        private static readonly Dictionary<int, string> Operations = new Dictionary<int, string>
        {
            {1,"Create"},{2,"Update"},{3,"Delete"},{4,"Access"},{5,"Upsert"},
            {115,"Archive"},{116,"Retain"},{117,"RollbackRetain"},{118,"Restore"},{200,"CustomOperation"}
        };

        public static string Action(int value)
        {
            string label;
            return Actions.TryGetValue(value, out label) ? label : value.ToString();
        }

        public static string Operation(int value)
        {
            string label;
            return Operations.TryGetValue(value, out label) ? label : value.ToString();
        }

        public static IEnumerable<KeyValuePair<int, string>> AllActions { get { return Actions; } }
        public static IEnumerable<KeyValuePair<int, string>> AllOperations { get { return Operations; } }
    }
}
