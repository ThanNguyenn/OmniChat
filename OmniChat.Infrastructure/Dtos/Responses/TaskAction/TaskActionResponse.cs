using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.TaskAction
{
    public class TaskActionResponse
    {
        public Guid Id { get; set; }

        public Guid SupportTaskId { get; set; }

        public TaskActionType Action { get; set; }

        public string Reason { get; set; }

        public Guid ActionById { get; set; }

        public Guid ActionToId { get; set; }
    }
}
