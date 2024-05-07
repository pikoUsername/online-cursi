﻿using LMS.Domain.Study.Events;
using LMS.Domain.User.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Domain.Study.Entities
{
    public class InstitutionMemberEntity : BaseAuditableEntity
    {
        [ForeignKey(nameof(UserEntity)), Required]
        public Guid UserId { get; set; }
        public List<InstitutionRolesEntity> Roles { get; private set; } = [];
        [ForeignKey(nameof(UserEntity)), Required]
        public Guid InstitutionId { get; set; }

        public static InstitutionMemberEntity Create(Guid userId, Guid institutionId)
        {
            var member = new InstitutionMemberEntity() { 
                UserId = userId, 
                Roles = [],
                InstitutionId = institutionId
            };

            member.AddDomainEvent(new MemberAdded(member)); 

            return member; 
        }

        public void SetRoles(params InstitutionRolesEntity[] roles)
        {
            Roles = [.. roles]; 

            AddDomainEvent(new MemberRolesChanged(this, roles));
        }

        public void AddRoles(params InstitutionRolesEntity[] roles)
        {
            Roles.AddRange(roles);

            AddDomainEvent(new MemberRolesChanged(this, roles));
        }
    }
}
