using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Shift
    {
        public Guid Id { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Description { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public Guid DepartmentId { get; set; }

        public virtual Department Department { get; set; }

        public virtual ICollection<StaffShift> StaffShifts { get; set; } = new List<StaffShift>();
    }
}
