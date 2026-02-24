using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Utilities
{
    public class DatabaseScripts
    {
        /// GENERIC MANAGER
        public const string GetNextRowId = "sp_GetNextRowId";

        public const string OTPTable = "OTP";

        public const string AuditLogTable = "AuditLog";

        public const string OrganizationTable = "Organization";

        public const string CIUserTable = "CIUser";

        public const string SubscriptionTable = "Subscription";

        public const string PaymentTable = "Payment";

        public const string OrganizationCountryTable = "OrganizationCountry";

        public const string OrganizationFacilityTable = "OrganizationFacility";

        public const string OrganizationDepartmentTable = "OrganizationDepartment";

        public const string OperationalExcellenceTable = "OperationalExcellence";

        public const string OperationalExcellenceMonthlySavingTable = "OperationalExcellenceMonthlySaving";

        public const string StrategicInitiativeTable = "StrategicInitiative";

        public const string SISubProjectTable = "SISubProject";

        public const string OrganizationSoftSavingTable = "OrganizationSoftSaving";

        public const string ContinuousImprovementTable = "ContinuousImprovement";

        public const string CIProjectTeamMemberTable = "CIProjectTeamMember";

        public const string CIProjectToolTable = "CIProjectTool";

        public const string CIProjectCommentTable = "CIProjectComment";

        public const string CIProjectSavingTable = "CIProjectSaving";

        public const string OrganizationToolTable = "OrganizationTool";

        public const string OrganizationBOATable = "OrganizationBOA";
    }
}
