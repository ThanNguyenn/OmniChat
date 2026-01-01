using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class StaffShift
    {
        public Guid Id { get; set; }
        public Guid StaffId { get; set; }
        public virtual Staff Staff { get; set; }
        public Guid ShiftId { get; set; }
        public virtual Shift Shift { get; set; }

        public string Note { get; set; }

        public StaffShiftStatus Status { get; set; }

    }

    public enum StaffShiftStatus
    {
      Working = 0,
      Absent = 1,
    }
}
