using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class MessageIntentType
    {
        public Guid Id { get; set; }

        public Guid MessageId { get; set; }

        public virtual CustomerMessage CustomerMessage { get; set; }

        public Guid IntentTypeId { get; set; }

        public virtual IntentType IntentType { get; set; }
    
    }
}
