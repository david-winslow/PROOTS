using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OTS.DAL.Core;

namespace OTS.Web.Authorization
{
  public class AssignRolesAuthorizationRequirement : IAuthorizationRequirement
  {
  }


  public class AssignRolesAuthorizationHandler : AuthorizationHandler<AssignRolesAuthorizationRequirement,
          (string[] newRoles, string[] currentRoles)>
  {
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
      AssignRolesAuthorizationRequirement requirement, (string[] newRoles, string[] currentRoles) newAndCurrentRoles)
    {
      if (!GetIsRolesChanged(newAndCurrentRoles.newRoles, newAndCurrentRoles.currentRoles))
      {
        context.Succeed(requirement);
      }   
      else if (context.User.HasClaim(ClaimConstants.Permission, ApplicationPermissions.AssignRoles))
      {
        if (context.User.HasClaim(ClaimConstants.Permission, ApplicationPermissions.ViewRoles)) // If user has ViewRoles permission, then he can assign any roles
          context.Succeed(requirement);

        else if (GetIsUserInAllAddedRoles(context.User, newAndCurrentRoles)) // Else user can only assign roles they're part of
          context.Succeed(requirement);
      }


      return Task.CompletedTask;
    }


    private bool GetIsRolesChanged(string[] newRoles, string[] currentRoles)
    {
      if (newRoles == null)
        newRoles = new string[] { };

      if (currentRoles == null)
        currentRoles = new string[] { };


      var roleAdded = newRoles.Except(currentRoles).Any();
      var roleRemoved = currentRoles.Except(newRoles).Any();

      return roleAdded || roleRemoved;
    }


    private bool GetIsUserInAllAddedRoles(ClaimsPrincipal contextUser, (string[] newRoles, string[] currentRoles) roles )
    {
      if (roles.newRoles == null)
        roles.newRoles = new string[] { };

      if (roles.currentRoles == null)
        roles.currentRoles = new string[] { };


      var addedRoles = roles.newRoles.Except(roles.currentRoles);

      return addedRoles.All(contextUser.IsInRole);
    }
  }
}
