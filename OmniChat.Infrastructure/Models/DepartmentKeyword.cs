using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class DepartmentKeyword
    {
        public Guid Id { get; set; }
        public Guid DepartmentId { get; set; }
        public virtual Department Department { get; set; }
        public Guid KeywordId { get; set; }
        public virtual Keyword Keyword { get; set; }
    }
}
