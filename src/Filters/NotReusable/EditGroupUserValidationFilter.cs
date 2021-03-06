﻿using System.Linq;
using CallGate.ApiModels.GroupUser;
using CallGate.DependencyInjection;
using CallGate.Services.Group;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CallGate.Filters.NotReusable
{
    public class EditGroupUserValidationFilter : IActionFilterDependency
    {
        private readonly IGroupValidationService _groupValidationService;

        public EditGroupUserValidationFilter(
            IGroupValidationService groupValidationService
        ){
            _groupValidationService = groupValidationService;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            foreach(var argument in context.ActionArguments.Values.Where(v => v is EditGroupUserRequest))
            {
                EditGroupUserRequest command = argument as EditGroupUserRequest;
                
                _groupValidationService.RequireUsernameIsGroupMember(command.Username, command.GroupId);
            }
        }
        
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}