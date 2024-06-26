﻿using LMS.Domain.User.Entities;
using LMS.Domain.User.Enums;
using LMS.Domain.User.Interfaces;

namespace LMS.Application.Common.Interfaces
{
    public interface IAccessPolicy
    {
        public Task<bool> IsAllowed(PermissionEnum action, object relation, IAccessUser? user = null);

        // usage: 
        // _accessPolicy.Role(UserRoles.Admin); 
        public Task<bool> Role(UserRoles role, IAccessUser? user = null);
        public Task<bool> Relationship(PermissionEnum action, object relation, Guid ownerId, IAccessUser? user = null);

        // usage: 
        // _accessPolicy.EnforceIsAllowed("read", _context.Notifications.EntityType); 
        public Task EnforceIsAllowed(PermissionEnum action, object relation, Guid? user = null);
        public Task EnforceRole(UserRoles role, Guid? userId = null);
        public Task EnforceRelationship(PermissionEnum action, object relation, Guid ownerId, Guid? user = null); 
        public Task<UserEntity> GetCurrentUser(); 
    }
}
