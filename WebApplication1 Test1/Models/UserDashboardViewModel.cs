using System;
using System.Collections.Generic;

namespace WebApplication1_Test1.Models
{
    public class UserDashboardViewModel
    {
        public int LockersActive { get; set; }
        public int ActivitiesUpcoming { get; set; }
        public int GatePassesActive { get; set; }

        public int LockerPending { get; set; }
        public int ActivityPending { get; set; }
        public int GatePassPending { get; set; }

        public List<Expense> RecentExpenses { get; set; } = new();
    }
}