using OmniChat.Infrastructure.Dtos.Requests.Intent;
using OmniChat.Infrastructure.Dtos.Responses.Intent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface ITaskAssignmentService
{
    Task<bool> ProcessTask(PredictRequest predictRequest, Guid conversationId);
    Task ProcessWaitingQueueAsync();

}
