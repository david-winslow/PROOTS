﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using OTS.DAL.Models;
using OTS.DAL.Core.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using OTS.DAL.Core;
using IdentityServer4.AccessTokenValidation;
using OTS.Authorization;
using OTS.Helpers;
using OTS.ViewModels;

namespace OTS.Controllers
{
  [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
  [Route("api/[controller]")]
  public class AccountController : Controller
  {
    private const string GetUserByIdActionName = "GetUserById";
    private const string GetRoleByIdActionName = "GetRoleById";
    private readonly IAccountManager _accountManager;
    private readonly IAuthorizationService _authorizationService;

    public AccountController(IAccountManager accountManager, IAuthorizationService authorizationService)
    {
      _accountManager = accountManager;
      _authorizationService = authorizationService;
    }


    [HttpGet("users/me")]
    [Produces(typeof(UserViewModel))]
    public async Task<IActionResult> GetCurrentUser()
    {
      return await GetUserByUserName(User.Identity.Name);
    }


    [HttpGet("users/{id}", Name = GetUserByIdActionName)]
    [Produces(typeof(UserViewModel))]
    public async Task<IActionResult> GetUserById(string id)
    {
      if (!(await _authorizationService.AuthorizeAsync(User, id, AccountManagementOperations.Read)).Succeeded)
        return new ChallengeResult();


      var userVM = await GetUserViewModelHelper(id);

      if (userVM != null)
        return Ok(userVM);
      return NotFound(id);
    }


    [HttpGet("users/username/{userName}")]
    [Produces(typeof(UserViewModel))]
    public async Task<IActionResult> GetUserByUserName(string userName)
    {
      var appUser = await _accountManager.GetUserByUserNameAsync(userName);

      if (!(await _authorizationService.AuthorizeAsync(User, appUser?.Id ?? "", AccountManagementOperations.Read))
        .Succeeded)
        return new ChallengeResult();

      if (appUser == null)
        return NotFound(userName);

      return await GetUserById(appUser.Id);
    }


    [HttpGet("users")]
    [Produces(typeof(List<UserViewModel>))]
    [Authorize(Policies.ViewAllUsersPolicy)]
    public async Task<IActionResult> GetUsers()
    {
      return await GetUsers(-1, -1);
    }


    [HttpGet("users/{page:int}/{pageSize:int}")]
    [Produces(typeof(List<UserViewModel>))]
    [Authorize(Policies.ViewAllUsersPolicy)]
    public async Task<IActionResult> GetUsers(int page, int pageSize)
    {
      var usersAndRoles = await _accountManager.GetUsersAndRolesAsync(page, pageSize);

      var usersVM = new List<UserViewModel>();

      foreach (var item in usersAndRoles)
      {
        var userVM = Mapper.Map<UserViewModel>(item.Item1);
        userVM.Roles = item.Item2;

        usersVM.Add(userVM);
      }

      return Ok(usersVM);
    }


    [HttpPut("users/me")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UserEditViewModel user)
    {
      return await UpdateUser(Utilities.GetUserId(User), user);
    }


    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UserEditViewModel user)
    {
      var appUser = await _accountManager.GetUserByIdAsync(id);
      var currentRoles = appUser != null ? (await _accountManager.GetUserRolesAsync(appUser)).ToArray() : null;

      var manageUsersPolicy = _authorizationService.AuthorizeAsync(User, id, AccountManagementOperations.Update);
      var assignRolePolicy = _authorizationService.AuthorizeAsync(User, Tuple.Create(user.Roles, currentRoles),
        Policies.AssignAllowedRolesPolicy);


      if ((await Task.WhenAll(manageUsersPolicy, assignRolePolicy)).Any(r => !r.Succeeded))
        return new ChallengeResult();


      if (ModelState.IsValid)
      {
        if (user == null)
          return BadRequest($"{nameof(user)} cannot be null");

        if (!string.IsNullOrWhiteSpace(user.Id) && id != user.Id)
          return BadRequest("Conflicting user id in parameter and model data");

        if (appUser == null)
          return NotFound(id);


        if (Utilities.GetUserId(User) == id && string.IsNullOrWhiteSpace(user.CurrentPassword))
        {
          if (!string.IsNullOrWhiteSpace(user.NewPassword))
            return BadRequest("Current password is required when changing your own password");

          if (appUser.UserName != user.UserName)
            return BadRequest("Current password is required when changing your own username");
        }


        var isValid = true;

        if (Utilities.GetUserId(User) == id &&
            (appUser.UserName != user.UserName || !string.IsNullOrWhiteSpace(user.NewPassword)))
        {
          if (!await _accountManager.CheckPasswordAsync(appUser, user.CurrentPassword))
          {
            isValid = false;
            AddErrors(new[] {"The username/password couple is invalid."});
          }
        }

        if (isValid)
        {
          Mapper.Map<UserViewModel, ApplicationUser>(user, appUser);

          var result = await _accountManager.UpdateUserAsync(appUser, user.Roles);
          if (result.Item1)
          {
            if (!string.IsNullOrWhiteSpace(user.NewPassword))
            {
              if (!string.IsNullOrWhiteSpace(user.CurrentPassword))
                result = await _accountManager.UpdatePasswordAsync(appUser, user.CurrentPassword, user.NewPassword);
              else
                result = await _accountManager.ResetPasswordAsync(appUser, user.NewPassword);
            }

            if (result.Item1)
              return NoContent();
          }

          AddErrors(result.Item2);
        }
      }

      return BadRequest(ModelState);
    }


    [HttpPatch("users/me")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] JsonPatchDocument<UserPatchViewModel> patch)
    {
      return await UpdateUser(Utilities.GetUserId(User), patch);
    }


    [HttpPatch("users/{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] JsonPatchDocument<UserPatchViewModel> patch)
    {
      if (!(await _authorizationService.AuthorizeAsync(User, id, AccountManagementOperations.Update)).Succeeded)
        return new ChallengeResult();


      if (ModelState.IsValid)
      {
        if (patch == null)
          return BadRequest($"{nameof(patch)} cannot be null");


        var appUser = await _accountManager.GetUserByIdAsync(id);

        if (appUser == null)
          return NotFound(id);


        var userPVM = Mapper.Map<UserPatchViewModel>(appUser);
        patch.ApplyTo(userPVM, ModelState);


        if (ModelState.IsValid)
        {
          Mapper.Map(userPVM, appUser);

          var result = await _accountManager.UpdateUserAsync(appUser);
          if (result.Item1)
            return NoContent();


          AddErrors(result.Item2);
        }
      }

      return BadRequest(ModelState);
    }


    [HttpPost("users")]
    [Authorize(Policies.ManageAllUsersPolicy)]
    public async Task<IActionResult> Register([FromBody] UserEditViewModel user)
    {
      if (!(await _authorizationService.AuthorizeAsync(User, Tuple.Create(user.Roles, new string[] { }),
        Policies.AssignAllowedRolesPolicy)).Succeeded)
        return new ChallengeResult();


      if (ModelState.IsValid)
      {
        if (user == null)
          return BadRequest($"{nameof(user)} cannot be null");


        var appUser = Mapper.Map<ApplicationUser>(user);

        var result = await _accountManager.CreateUserAsync(appUser, user.Roles, user.NewPassword);
        if (result.Item1)
        {
          var userVM = await GetUserViewModelHelper(appUser.Id);
          return CreatedAtAction(GetUserByIdActionName, new {id = userVM.Id}, userVM);
        }

        AddErrors(result.Item2);
      }

      return BadRequest(ModelState);
    }


    [HttpDelete("users/{id}")]
    [Produces(typeof(UserViewModel))]
    public async Task<IActionResult> DeleteUser(string id)
    {
      if (!(await _authorizationService.AuthorizeAsync(User, id, AccountManagementOperations.Delete)).Succeeded)
        return new ChallengeResult();

      if (!await _accountManager.TestCanDeleteUserAsync(id))
        return BadRequest("User cannot be deleted. Delete all orders associated with this user and try again");


      UserViewModel userVM = null;
      var appUser = await _accountManager.GetUserByIdAsync(id);

      if (appUser != null)
        userVM = await GetUserViewModelHelper(appUser.Id);


      if (userVM == null)
        return NotFound(id);

      var result = await _accountManager.DeleteUserAsync(appUser);
      if (!result.Item1)
        throw new Exception("The following errors occurred whilst deleting user: " + string.Join(", ", result.Item2));


      return Ok(userVM);
    }


    [HttpPut("users/unblock/{id}")]
    [Authorize(Policies.ManageAllUsersPolicy)]
    public async Task<IActionResult> UnblockUser(string id)
    {
      var appUser = await _accountManager.GetUserByIdAsync(id);

      if (appUser == null)
        return NotFound(id);

      appUser.LockoutEnd = null;
      var result = await _accountManager.UpdateUserAsync(appUser);
      if (!result.Item1)
        throw new Exception("The following errors occurred whilst unblocking user: " + string.Join(", ", result.Item2));


      return NoContent();
    }


    [HttpGet("users/me/preferences")]
    [Produces(typeof(string))]
    public async Task<IActionResult> UserPreferences()
    {
      var userId = Utilities.GetUserId(User);
      var appUser = await _accountManager.GetUserByIdAsync(userId);

      if (appUser != null)
        return Ok(appUser.Configuration);
      return NotFound(userId);
    }


    [HttpPut("users/me/preferences")]
    public async Task<IActionResult> UserPreferences([FromBody] string data)
    {
      var userId = Utilities.GetUserId(User);
      var appUser = await _accountManager.GetUserByIdAsync(userId);

      if (appUser == null)
        return NotFound(userId);

      appUser.Configuration = data;
      var result = await _accountManager.UpdateUserAsync(appUser);
      if (!result.Item1)
        throw new Exception("The following errors occurred whilst updating User Configurations: " +
                            string.Join(", ", result.Item2));


      return NoContent();
    }


    [HttpGet("roles/{id}", Name = GetRoleByIdActionName)]
    [Produces(typeof(RoleViewModel))]
    public async Task<IActionResult> GetRoleById(string id)
    {
      var appRole = await _accountManager.GetRoleByIdAsync(id);

      if (!(await _authorizationService.AuthorizeAsync(User, appRole?.Name ?? "", Policies.ViewRoleByRoleNamePolicy))
        .Succeeded)
        return new ChallengeResult();

      if (appRole == null)
        return NotFound(id);

      return await GetRoleByName(appRole.Name);
    }


    [HttpGet("roles/name/{name}")]
    [Produces(typeof(RoleViewModel))]
    public async Task<IActionResult> GetRoleByName(string name)
    {
      if (!(await _authorizationService.AuthorizeAsync(User, name, Policies.ViewRoleByRoleNamePolicy)).Succeeded)
        return new ChallengeResult();


      var roleVM = await GetRoleViewModelHelper(name);

      if (roleVM == null)
        return NotFound(name);

      return Ok(roleVM);
    }


    [HttpGet("roles")]
    [Produces(typeof(List<RoleViewModel>))]
    [Authorize(Policies.ViewAllRolesPolicy)]
    public async Task<IActionResult> GetRoles()
    {
      return await GetRoles(-1, -1);
    }


    [HttpGet("roles/{page:int}/{pageSize:int}")]
    [Produces(typeof(List<RoleViewModel>))]
    [Authorize(Policies.ViewAllRolesPolicy)]
    public async Task<IActionResult> GetRoles(int page, int pageSize)
    {
      var roles = await _accountManager.GetRolesLoadRelatedAsync(page, pageSize);
      return Ok(Mapper.Map<List<RoleViewModel>>(roles));
    }


    [HttpPut("roles/{id}")]
    [Authorize(Policies.ManageAllRolesPolicy)]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] RoleViewModel role)
    {
      if (ModelState.IsValid)
      {
        if (role == null)
          return BadRequest($"{nameof(role)} cannot be null");

        if (!string.IsNullOrWhiteSpace(role.Id) && id != role.Id)
          return BadRequest("Conflicting role id in parameter and model data");


        var appRole = await _accountManager.GetRoleByIdAsync(id);

        if (appRole == null)
          return NotFound(id);


        Mapper.Map(role, appRole);

        var result = await _accountManager.UpdateRoleAsync(appRole, role.Permissions?.Select(p => p.Value).ToArray());
        if (result.Item1)
          return NoContent();

        AddErrors(result.Item2);
      }

      return BadRequest(ModelState);
    }


    [HttpPost("roles")]
    [Authorize(Policies.ManageAllRolesPolicy)]
    public async Task<IActionResult> CreateRole([FromBody] RoleViewModel role)
    {
      if (ModelState.IsValid)
      {
        if (role == null)
          return BadRequest($"{nameof(role)} cannot be null");


        var appRole = Mapper.Map<ApplicationRole>(role);

        var result = await _accountManager.CreateRoleAsync(appRole, role.Permissions?.Select(p => p.Value).ToArray());
        if (result.Item1)
        {
          var roleVM = await GetRoleViewModelHelper(appRole.Name);
          return CreatedAtAction(GetRoleByIdActionName, new {id = roleVM.Id}, roleVM);
        }

        AddErrors(result.Item2);
      }

      return BadRequest(ModelState);
    }


    [HttpDelete("roles/{id}")]
    [Produces(typeof(RoleViewModel))]
    [Authorize(Policies.ManageAllRolesPolicy)]
    public async Task<IActionResult> DeleteRole(string id)
    {
      if (!await _accountManager.TestCanDeleteRoleAsync(id))
        return BadRequest("Role cannot be deleted. Remove all users from this role and try again");


      RoleViewModel roleVM = null;
      var appRole = await _accountManager.GetRoleByIdAsync(id);

      if (appRole != null)
        roleVM = await GetRoleViewModelHelper(appRole.Name);


      if (roleVM == null)
        return NotFound(id);

      var result = await _accountManager.DeleteRoleAsync(appRole);
      if (!result.Item1)
        throw new Exception("The following errors occurred whilst deleting role: " + string.Join(", ", result.Item2));


      return Ok(roleVM);
    }


    [HttpGet("permissions")]
    [Produces(typeof(List<PermissionViewModel>))]
    [Authorize(Policies.ViewAllRolesPolicy)]
    public IActionResult GetAllPermissions()
    {
      return Ok(Mapper.Map<List<PermissionViewModel>>(ApplicationPermissions.AllPermissions));
    }


    private async Task<UserViewModel> GetUserViewModelHelper(string userId)
    {
      var userAndRoles = await _accountManager.GetUserAndRolesAsync(userId);
      if (userAndRoles == null)
        return null;

      var userVM = Mapper.Map<UserViewModel>(userAndRoles.Item1);
      userVM.Roles = userAndRoles.Item2;

      return userVM;
    }


    private async Task<RoleViewModel> GetRoleViewModelHelper(string roleName)
    {
      var role = await _accountManager.GetRoleLoadRelatedAsync(roleName);
      if (role != null)
        return Mapper.Map<RoleViewModel>(role);


      return null;
    }


    private void AddErrors(IEnumerable<string> errors)
    {
      foreach (var error in errors)
      {
        ModelState.AddModelError(string.Empty, error);
      }
    }
  }
}