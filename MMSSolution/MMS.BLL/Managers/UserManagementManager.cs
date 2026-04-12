
using Intalio.Tools.Common;
using Intalio.Tools.Common.Encryptions;
using Intalio.Tools.Common.Extensions.StringExtensions;
using Intalio.Tools.Common.JwtToken;
using Microsoft.AspNetCore.Http;
using Intalio.Tools.Common.Ldap;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using MMS.BLL.Constants;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO.Permissions;
using MMS.DTO.Users.Auth;
using MMS.DTO.Users;
using MMS.DTO;
using System.DirectoryServices;
using System.Linq.Expressions;
using System.Security.Claims;
using Task = System.Threading.Tasks.Task;
using MMS.DTO.Notifications;
using MMS.BLL.Storage;
using Microsoft.AspNetCore.Http;
using Intalio.Tools.Common.Extensions.FileExtensions;
using Intalio.Tools.Common.Storage;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;
using Intalio.Tools.Common.Pincode;
using StackExchange.Redis;

namespace MMS.BLL.Managers;

public class UserManagementManager
{
	private readonly IMapper _mapper;
	private readonly JwtSettings _jwtSettings;
	private readonly LdapSettings _ldapSettings;
	private readonly string _defaultPassword;
	private readonly int _totalCountForAutoComplete;
	private readonly IUserManagementUnitOfWork _userManagementUnitOfWork;
	private readonly IProcessUnitOfWork _processUnitOfWork;
	private readonly IMMSUnitOfWork _mmsUnitOfWork;
	private readonly bool _twoFactorAuthEnabled;
	private readonly bool _enableLogin;
	private readonly SmsManager _smsManager;
	private readonly IStorage _storage;
	private readonly IHostEnvironment _hostEnvironment;
	private readonly StorageSettings _storageSettings;
	private readonly PincodeSetting _pincodeSetting;
	private readonly StorageManager _storageManager;
	private readonly IConnectionMultiplexer _redis;
	private readonly IConfiguration _configuration;

	public UserManagementManager(IConfiguration configuration,
		StorageSettings storageSettings,
		IMapper mapper,
		IUserManagementUnitOfWork userManagementUnitOfWork,
		IMMSUnitOfWork mmsUnitOfWork,
		IProcessUnitOfWork processUnitOfWork,
		SmsManager smsManager,
		StorageFactory storageFactory,
		IHostEnvironment hostEnvironment,
		IConnectionMultiplexer redis,
		StorageManager storageManager)
	{
		_redis = redis;
		_mapper = mapper;
		_storageSettings = storageSettings;
		_storage = storageFactory.GetStorage();
		_jwtSettings = configuration.GetSection(AppSettingsConstants.JwtSectionName).Get<JwtSettings>() ?? new();
		_pincodeSetting = configuration.GetSection(AppSettingsConstants.PincodeSectionName).Get<PincodeSetting>() ?? new();
		_ldapSettings = configuration.GetSection(AppSettingsConstants.LdapSectionName).Get<LdapSettings>() ?? new();
		_defaultPassword = configuration.GetValue<string>(AppSettingsConstants.FBAPassword) ?? string.Empty;
		_totalCountForAutoComplete = configuration.GetValue<int>(AppSettingsConstants.TotalCountForAutoComplete);
		_twoFactorAuthEnabled = configuration.GetValue<bool>(AppSettingsConstants.Enable2FA);
		_enableLogin = configuration.GetValue<bool>(AppSettingsConstants.EnableLogin);
		_userManagementUnitOfWork = userManagementUnitOfWork;
		_processUnitOfWork = processUnitOfWork;
		_mmsUnitOfWork = mmsUnitOfWork;
		_smsManager = smsManager;
		_hostEnvironment = hostEnvironment;
		_storageManager = storageManager;
		_configuration = configuration;

	}

	public async Task<(LoggedInUserDto? userDto, bool locked)> AuthenticateAsync(string username, string password)
	{
		var user = await _userManagementUnitOfWork.Users.GetIncludeCredentialsAndLanguageAndStructuresAsync((x) => x.Username == username || x.Email == username);
		if (user != null)
		{
			var locked = !user.Approved;
			if (locked)
			{
				return (null, true);
			}

			// Verify password against stored BCrypt hash
			if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
			{
				return (null, false);
			}

			user.LastLoginDate = DateTime.Now;
			await _userManagementUnitOfWork.SaveChangesAsync();

			return (await GetLoggedInUserAsync(user), locked: false);
		}

		return (null, false);
	}


	public async Task UpdateDefaultLanguageAsync(string userId, string language)
	{
		int languageId = language == "ar" ? (int)LanguageDbEnum.Arabic : (int)LanguageDbEnum.English;

		var user = await _userManagementUnitOfWork.Users.GetAsync(x => x.Id == userId);
		if (user != null)
		{
			user.DefaultLanguageId = languageId;
			await _userManagementUnitOfWork.SaveChangesAsync();
		}
	}

	public async Task<LoggedInUserDto?> RefreshUser(string userId)
	{
		var user = await _userManagementUnitOfWork.Users.GetIncludeCredentialsAndLanguageAndStructuresAsync(x => x.Id == userId);
		if (user != null)
		{
			return await GetLoggedInUserAsync(user);
		}

		return null;
	}

	public async Task<List<ListItemDto>> ListStructuresAsync(LanguageDbEnum language)
	{
		var structures = await _userManagementUnitOfWork.Structures.ListAsync(x => x.Active == true && x.ExternalStructure == false);

		return structures.Select(structure => _mapper.Map<ListItemDto>((structure, language))).ToList();
	}

	public async Task<List<ListItemDto>> ListExternalStructuresAsync(LanguageDbEnum language)
	{
		var structures = await _userManagementUnitOfWork.Structures.ListAsync(x => x.ExternalStructure == true);

		return structures.Select(structure => _mapper.Map<ListItemDto>((structure, language))).ToList();
	}

	public async Task SetSignatureAsync(string userId, byte[] fileBytes, int pincode)
	{
		UserSignature userSignature = new();
		userSignature.Signature = fileBytes;
		userSignature.TypeId = (int)SignatureTypeDbEnum.Signature;
		userSignature.UserId = userId;
		userSignature.Pincode = pincode;
		await _userManagementUnitOfWork.UserSignature.AddAsync(userSignature);

		await _userManagementUnitOfWork.SaveChangesAsync();
	}

	public async Task<List<ListItemDto>> ListSignaturesAsync(string userId)
	{
		var signatures = await _userManagementUnitOfWork.UserSignature.ListAsync(x => x.UserId == userId);
		return _mapper.Map<List<ListItemDto>>((signatures));
	}

	public async Task DeleteSignatureAsync(int signatureId)
	{
		var userSignature = await _userManagementUnitOfWork.UserSignature.GetAsync(x => x.Id == signatureId);
		if (userSignature != null)
		{
			_userManagementUnitOfWork.UserSignature.Remove(userSignature);
			await _userManagementUnitOfWork.SaveChangesAsync();
		}
	}

	public async Task<bool> HasUserPermissionAsync(string userId, PermissionDbEnum permission, PermissionLevelDbEnum permissionLevel)
	{
		return await HasUserPermissionAsync(userId, permission, permissionLevel, null, null);
	}

	public async Task<bool> HasUserPermissionAsync(string userId, PermissionDbEnum permission, PermissionLevelDbEnum permissionLevel,
		string? groupIdsCsv, List<string>? roleNames)
	{
		// Step 1: Check user-level permissions (PermissionMatrix)
		var userPermissionLevel = await _userManagementUnitOfWork.PermissionMatrices.GetAsync(x => x.UserId == userId &&
				( (x.PermissionId == (int)permission && x.Value)||
				(permissionLevel== PermissionLevelDbEnum.Read&& x.PermissionId==(int)PermissionDbEnum.SuperAdmin &&x.Value)));
		if (userPermissionLevel != null)
		{
			switch (permissionLevel)
			{
				case PermissionLevelDbEnum.Full:
					return userPermissionLevel.LevelId == (int)PermissionLevelDbEnum.Full ;
				case PermissionLevelDbEnum.Read:
					return true;
				case PermissionLevelDbEnum.Write:
					return userPermissionLevel.LevelId == (int)PermissionLevelDbEnum.Full || userPermissionLevel.LevelId == (int)PermissionLevelDbEnum.Write;
			}
		}

		// Step 2: Check role-based permissions from DB
		// Roles come from BOTH direct UserRole assignments (RBAC) and UserStructure assignments
		var userRoles = await _mmsUnitOfWork.UserRoles.ListAsync(ur => ur.UserId == userId);
		var userStructures = await _userManagementUnitOfWork.UserStructures.ListAsync(us => us.UserId == userId);
		var roleIds = userRoles.Select(ur => ur.RoleId)
			.Union(userStructures.Select(us => us.RoleId))
			.Distinct().ToList();
		if (roleIds.Any())
		{
			var roles = await _userManagementUnitOfWork.Roles.ListAsync(r => roleIds.Contains(r.Id));
			var dbRoleNames = roles.Select(r => r.RoleNameEn).Distinct().ToList();
			if (dbRoleNames.Any())
			{
				var hasRolePerm = await _mmsUnitOfWork.RoleMenuPermissions
					.HasPermissionForRolesAsync(dbRoleNames, (int)permission);
				if (hasRolePerm) return true;
			}
		}

		// Step 3: Check group-based permissions from DB (via UserGroup)
		var userGroups = await _mmsUnitOfWork.UserGroups.ListAsync(ug => ug.UserId == userId);
		var groupIds = userGroups.Select(ug => ug.GroupId.ToString()).ToList();
		if (groupIds.Any())
		{
			var hasGroupPerm = await _mmsUnitOfWork.GroupMenuPermissions
				.HasPermissionForGroupsAsync(groupIds, (int)permission);
			if (hasGroupPerm) return true;
		}

		return false;
	}
	private string GenerateToken(User user)
	{
		string jwtUserId = EncryptionService.Encrypt(StringManipulation.ExtendStringValue(user.Id.ToString()));
		int primaryStructureId = 0;
		if (user.UserStructures.Any())
		{
			primaryStructureId = user.UserStructures.FirstOrDefault(x => x.IsPrimary)?.StrucutreId ?? user.UserStructures.FirstOrDefault().StrucutreId;
		}
		string jwtStructureId = EncryptionService.Encrypt(StringManipulation.ExtendStringValue(primaryStructureId.ToString()));

		string userFullName = EncryptionService.Encrypt(StringManipulation.ExtendStringValue(
			((LanguageDbEnum)user.DefaultLanguage.Id == LanguageDbEnum.Arabic) ? user.FullnameAr.ToString() : user.FullnameEn.ToString()));

		var claims = new List<System.Security.Claims.Claim>
		{
			new(JwtTokenGenerator.CommonClaimNames.UserId, jwtUserId),
			new(JwtTokenGenerator.CommonClaimNames.StructureId, jwtStructureId),
			new(JwtTokenGenerator.CommonClaimNames.FullName, userFullName),
			new(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
		};
		if (!string.IsNullOrWhiteSpace(user.DefaultLanguage?.Code))
			claims.Add(new(JwtTokenGenerator.CommonClaimNames.Language, user.DefaultLanguage.Code));

		return new JwtTokenGenerator(_jwtSettings).GenerateToken(claims.ToArray());
	}
	private async Task<LoggedInUserDto> GetLoggedInUserAsync(User user)
	{
		var db = _redis.GetDatabase();
		var jwtToken = GenerateToken(user);
		await Logout(user.Id);//delete exist refresh tokens to prevent Simultaneous login
		await db.StringSetAsync("token_" + user.Id, jwtToken);
		string refreshToken = await GenerateRefreshTokenAsync(user.Id, _jwtSettings.RefreshExpiryMinutes);

		if (refreshToken != null)
		{
			return _mapper.Map<LoggedInUserDto>((user, jwtToken, refreshToken));

		}
		return null;
	}

	private async Task<string> GenerateRefreshTokenAsync(string UserId, double? refreshExpiryMinutes)
	{
		// Generate a random string for the refresh token
		refreshExpiryMinutes ??= 60;
		var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)); // Secure random bytes
		var refreshToken = new RefreshToken()
		{
			Id = 0,
			Token = token,
			Expiration = DateTime.Now.AddMinutes(refreshExpiryMinutes.Value),
			UserId = UserId,
		};
		await _userManagementUnitOfWork.RefreshTokens.AddAsync(refreshToken);
		var added = await _userManagementUnitOfWork.SaveChangesAsync() > 0;
		if (added)
		{
			return token;
		}
		return null;

	}

	public static int GetClaimValue(ClaimsPrincipal claimPrincipal, string claimName)
	{
		string? claimCypher = claimPrincipal.FindFirst(claimName)?.Value;
		if (!string.IsNullOrEmpty(claimCypher))
		{
			string plainText = EncryptionService.Decrypt(claimCypher);
			int.TryParse(StringManipulation.ContractStringValue(plainText), out int id);
			return id;
		}
		return 0;
	}

	public static string GetStringClaimValue(ClaimsPrincipal claimPrincipal, string claimName)
	{
		string? claimCypher = claimPrincipal.FindFirst(claimName)?.Value;
		if (!string.IsNullOrEmpty(claimCypher))
		{
			string plainText = EncryptionService.Decrypt(claimCypher);
			return StringManipulation.ContractStringValue(plainText);
		}
		return string.Empty;
	}

	public async Task<string?> GetSignature(string userId)
	{
		var signature = await _userManagementUnitOfWork.UserSignature.GetAsync(x => x.UserId == userId && x.TypeId == (int)SignatureTypeDbEnum.Signature);
		if (signature != null)
		{
			return Convert.ToBase64String(signature.Signature);
		}
		return null;
	}

	public async Task<List<ListItemDto>> ListUsersForAutoComplete(string search, LanguageDbEnum language, bool active)
	{
		var users = await _userManagementUnitOfWork.Users.ListAsync(x => x.Approved == active && (x.FullnameEn.Contains(search) || x.FullnameAr.Contains(search) || x.Username.Contains(search) || x.NationalId.Contains(search) || x.Mobile.Contains(search)));
		var retVal = users.Take(_totalCountForAutoComplete).ToList();
		return retVal.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
	}

	public async Task<List<ListItemDto>> ListUsers(LanguageDbEnum language)
	{
		var users = await _userManagementUnitOfWork.Users.ListAsync();
		return users.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
	}

	public async Task<List<ListItemDto>> ListUsersByIds(List<string> userIds, LanguageDbEnum language)
	{
		var users = await _userManagementUnitOfWork.Users.ListAsync(x => userIds.Contains(x.Id));
		return users.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
	}

	public async Task<UserAdminListItemDto?> GetUserAsync(string userId)
	{
		var user = await _userManagementUnitOfWork.Users.GetAsync(x => x.Id == userId);
		if (user != null)
		{
			return _mapper.Map<UserAdminListItemDto?>(user);
		}
		return null;
	}

	public async Task<GenericPaginationListDto<UserAdminListItemDto>> ListUsersForAdminAsync(int page, int pageSize, bool active)
	{
		var totalUsers = await _userManagementUnitOfWork.Users.CountAsync(x => x.Approved == active);
		var users = await _userManagementUnitOfWork.Users.ListAsync(
				page,
				pageSize,
				x => x.Approved == active,
				orderBy: x => x.Id);

		var retval = users.Select(x => _mapper.Map<UserAdminListItemDto>(x)).ToList();
		return new GenericPaginationListDto<UserAdminListItemDto>(totalUsers, retval);
	}

	public async Task AddUserAsync(UserDto userObj)
	{
		User newUser = new User
		{
			Id = Guid.NewGuid().ToString(),
			Username = userObj.Username,
			Email = userObj.Email,
			FullnameAr = userObj.FullNameAr,
			FullnameEn = userObj.FullNameEn,
			Mobile = userObj.Mobile,
			NationalId = userObj.NationalId,
			Approved = userObj.Approved,
			DefaultLanguageId = userObj.DefaultLanguageId,
			CreatedDate = DateTime.Now,
			PasswordHash = !string.IsNullOrEmpty(userObj.Password) ? BCrypt.Net.BCrypt.HashPassword(userObj.Password) : null
		};

		await _userManagementUnitOfWork.Users.AddAsync(newUser);
		await _userManagementUnitOfWork.SaveChangesAsync();
	}

	public async Task<bool> RegisterUserAsync(RegisterUserDto userObj)
	{
		if (await CheckEmailIfExsits(userObj.Email))
		{
			return false;
		}
		if (await CheckUsernameIfExsits(userObj.Username))
		{
			return false;
		}

		User newUser = new User
		{
			Username = userObj.Username,
			Email = userObj.Email,
			FullnameAr = userObj.FullNameAr,
			FullnameEn = userObj.FullNameEn,
			Mobile = userObj.Mobile,
			NationalId = userObj.NationalId,
			Approved = false,
			DefaultLanguageId = userObj.DefaultLanguageId,
			CreatedDate = DateTime.Now
		};

		await _userManagementUnitOfWork.Users.AddAsync(newUser);
		await _userManagementUnitOfWork.SaveChangesAsync();
		return true;
	}

	public async Task UpdateUserAsync(UserDto userObj)
	{
		var user = await _userManagementUnitOfWork.Users.GetAsync(x => x.Id == userObj.Id);
		if (user != null)
		{
			user.FullnameAr = userObj.FullNameAr;
			user.FullnameEn = userObj.FullNameEn;
			user.Email = userObj.Email;
			user.Username = userObj.Username;
			user.DefaultLanguageId = userObj.DefaultLanguageId;
			user.Approved = userObj.Approved;
			user.NationalId = userObj.NationalId;
			user.Mobile = userObj.Mobile;
			await _userManagementUnitOfWork.SaveChangesAsync();
		}
	}

	public async Task EnableSmsAsync(string userId, bool activated)
	{
		var user = await _userManagementUnitOfWork.Users.GetAsync(x => x.Id == userId);
		if (user != null)
		{
			user.SmsEnabled = activated;
			await _userManagementUnitOfWork.SaveChangesAsync();
		}
	}

	public async Task EnableEmailAsync(string userId, bool activated)
	{
		var user = await _userManagementUnitOfWork.Users.GetAsync(x => x.Id == userId);
		if (user != null)
		{
			user.EmailNotificationEnabled = activated;
			await _userManagementUnitOfWork.SaveChangesAsync();
		}
	}

	public async Task<List<PermissionListItemDto>?> ListPermissionsAsync(string userId)
	{
		var permissions = await _userManagementUnitOfWork.Permissions.ListIncludeTypeAsync(x => x.IsSpecific == false && x.TypeId == (int)PermissionTypeDbEnum.Menu);
		var userMatrix = await _userManagementUnitOfWork.PermissionMatrices.ListAsync(x => x.UserId == userId);
		
		return permissions.OrderBy(x => x.Order).GroupBy(item => item.TypeId, (key, group) =>
		new PermissionListItemDto
		{
			Id = key,
			Items = group.OrderBy(x => x.GroupName).GroupBy(x => x.GroupName, (key, group) => new SecondLevelPermissionDto
			{
				GroupName = key,
				Items = group.OrderBy(x => x.GroupItemOrder).Select(per => new PermissionAccessListItemDto
				{
					Id = per.Id,
					Name = per.Name,
					HasLevel = per.ShowLevel,
                    GroupName = per.GroupName,
					HasAccess =  userMatrix.Any(p => p.PermissionId == per.Id),
					LevelId = userMatrix.FirstOrDefault(p => p.PermissionId == per.Id) != null ? userMatrix.FirstOrDefault(p => p.PermissionId == per.Id).LevelId : (int)PermissionLevelDbEnum.Read
				}).ToList()
			}).ToList()
		}).ToList();
	}

	public async Task EditUserPermissionsAsync(string userId, int permissionId, int permissionTypeId, bool enabled)
	{
		if (!enabled)
		{
			var permissionMatrix = await _userManagementUnitOfWork.PermissionMatrices.GetAsync(x => x.UserId == userId && x.PermissionId == permissionId);
			if (permissionMatrix != null)
			{
				_userManagementUnitOfWork.PermissionMatrices.Remove(permissionMatrix);
				await _userManagementUnitOfWork.SaveChangesAsync();
			}
		}
		else
		{
			PermissionMatrix newMatrix = new PermissionMatrix { UserId = userId, PermissionId = permissionId, LevelId = (int)PermissionLevelDbEnum.Full, Value = true };
			await _userManagementUnitOfWork.PermissionMatrices.AddAsync(newMatrix);
			await _userManagementUnitOfWork.SaveChangesAsync();
		}
	}

	public async Task<List<PermissionAccessListItemDto>?> ListUserMenuPermissionsAsync(string userId)
	{
		var isSuperAdmin = await _userManagementUnitOfWork.PermissionMatrices.AnyAsync(p => p.UserId==userId&& p.PermissionId == (int)PermissionDbEnum.SuperAdmin);

		// If SuperAdmin, return all menu permissions
		if (isSuperAdmin)
		{
			var allPermissions = await _userManagementUnitOfWork.Permissions.ListIncludeTypeAsync(
				x => x.TypeId == (int)PermissionTypeDbEnum.Menu && x.Id != (int)PermissionDbEnum.CreateMeeting);
			allPermissions = allPermissions.OrderBy(x => x.Order).ThenBy(x => x.GroupItemOrder).ToList();
			return allPermissions.Select(x => new PermissionAccessListItemDto
			{
				Id = x.Id,
				Name = x.Name,
				HasAccess = true
			}).ToList();
		}

		// Step 1: Get user-level permission IDs from PermissionMatrix
		var userPerm = await _userManagementUnitOfWork.PermissionMatrices.ListAsync(
			p => p.UserId == userId && p.Permission.TypeId == (int)PermissionTypeDbEnum.Menu);
		var userPermIds = userPerm.Select(p => p.PermissionId).ToHashSet();

		// Step 2: Get group-level permission IDs from DB
		var userGroups = await _mmsUnitOfWork.UserGroups.ListAsync(ug => ug.UserId == userId);
		var groupIds = userGroups.Select(ug => ug.GroupId.ToString()).ToList();
		if (groupIds.Any())
		{
			var groupPermIds = await _mmsUnitOfWork.GroupMenuPermissions
				.GetPermissionIdsForGroupsAsync(groupIds);
			foreach (var id in groupPermIds) userPermIds.Add(id);
		}

		// Step 3: Get role-level permission IDs from DB
		// Roles come from BOTH direct UserRole assignments (RBAC) and UserStructure assignments
		var userRoles = await _mmsUnitOfWork.UserRoles.ListAsync(ur => ur.UserId == userId);
		var userStructures = await _userManagementUnitOfWork.UserStructures.ListAsync(us => us.UserId == userId);
		var roleIds = userRoles.Select(ur => ur.RoleId)
			.Union(userStructures.Select(us => us.RoleId))
			.Distinct().ToList();
		if (roleIds.Any())
		{
			var roles = await _userManagementUnitOfWork.Roles.ListAsync(r => roleIds.Contains(r.Id));
			var roleNames = roles.Select(r => r.RoleNameEn).Distinct().ToList();
			if (roleNames.Any())
			{
				var rolePermIds = await _mmsUnitOfWork.RoleMenuPermissions
					.GetPermissionIdsForRolesAsync(roleNames);
				foreach (var id in rolePermIds) userPermIds.Add(id);
			}
		}

		// Step 4: Query Permission table for the unioned IDs where TypeId == Menu
		var permissions = await _userManagementUnitOfWork.Permissions.ListIncludeTypeAsync(
			x => x.TypeId == (int)PermissionTypeDbEnum.Menu && userPermIds.Contains(x.Id));
		permissions = permissions.OrderBy(x => x.Order).ThenBy(x => x.GroupItemOrder).ToList();

		return permissions.Select(x => new PermissionAccessListItemDto
		{
			Id = x.Id,
			Name = x.Name,
			HasAccess = true
		}).ToList();
	}

	public async Task<List<UserStructureRoleLstItemDto>?> ListUserStructuresRolesAsync(string userId, LanguageDbEnum language)
	{
		var userStructureRoles = await _userManagementUnitOfWork.UserStructures.ListIncludeStructureAndRoleAsync(x => x.User.Id == userId);
		if (userStructureRoles != null && userStructureRoles.Any())
		{
			return userStructureRoles.Select(x => _mapper.Map<UserStructureRoleLstItemDto>((x, language))).ToList();
		}
		return null;
	}

	public bool ValidateWindowsUser(string userName, string password, string domain, string url)
	{
		try
		{
			string username = string.Format(@"{0}@{1}", userName, domain);
			using (DirectoryEntry dirEnt = new DirectoryEntry(url, username, password))
			{
				using (DirectorySearcher directorySearcher = new DirectorySearcher(dirEnt))
				{
					Console.WriteLine("starting to connect");
					directorySearcher.Filter = string.Format("(&(objectClass=user)({0}={1}))", "samaccountname", "a.aljuaidan");
					SearchResult searchResult = directorySearcher.FindOne();
					if (searchResult == null)
					{
						return false;
					}
					else
					{
						return true;
					}
				}
			}
			/* 
			 DirectoryEntry de = new DirectoryEntry(url, username, password);
			 DirectorySearcher ds = new DirectorySearcher(de);
			 ds.SearchScope = SearchScope.OneLevel;
			 ds.FindOne();
			 return true;*/
		}
		catch (DirectoryServicesCOMException ex)
		{
			return false;
		}
	}

	public async Task<int> CountUnclaimedTasksAsync(string userId)
	{
		var roles = await _userManagementUnitOfWork.Roles.ListAsync(x => x.UserStructures.Any(u => u.UserId == userId));
		var roleIds = roles.Select(x => x.Id).Distinct().ToList();
		var userStructures = await _userManagementUnitOfWork.UserStructures.ListAsync(x => x.UserId == userId);
		var structureIds = userStructures.Select(x => x.StrucutreId);
		Expression<Func<VActiveActivityInstance, bool>> filter = x => (x.UserId == userId || x.RoleId.HasValue && roleIds.Contains(x.RoleId.Value) || (x.StructureId.HasValue && structureIds.Contains(x.StructureId.Value))) && x.Claimed == null;
		return await _processUnitOfWork.VActiveActivityInstances.CountAsync(filter);
	}

	public bool TwoFactorAuthEnabled()
	{
		return _twoFactorAuthEnabled;
	}

	private async Task<bool> CheckEmailIfExsits(string email)
	{
		return await _userManagementUnitOfWork.Users.AnyAsync(x => x.Email == email);
	}

	private async Task<bool> CheckUsernameIfExsits(string username)
	{
		return await _userManagementUnitOfWork.Users.AnyAsync(x => x.Username == username);
	}

	public async Task<(bool Success, string Message)> RequestVerificationCodeAsync(UserVerificationCodePostDto userDto)
	{
		var user = await _userManagementUnitOfWork.Users.GetAsync(x => x.Id == userDto.User.Id);
		if (user != null)
		{
			if (!string.IsNullOrEmpty(user.Mobile) && user.Mobile.TryCleanMobileSA(out string cleanedNumber))
			{
				string last4Digits = cleanedNumber.Substring(cleanedNumber.Length - 4, 4);
				if (last4Digits != userDto.Last4MobileDigits)
				{
					return (false, MessageConstants.InvalidMobile);
				}

				(bool success, string validation) = await _smsManager.SendOtpAsync(cleanedNumber);
				if (success)
				{
					// Store OTP hash in Redis
					var db = _redis.GetDatabase();
					await db.StringSetAsync($"otp_hash:{user.Id}", validation, TimeSpan.FromMinutes(10));
					return (true, MessageConstants.InvalidMobile);
				}
				return (false, MessageConstants.ErrorSendingSms);
			}
			return (false, MessageConstants.InvalidMobile);
		}
		return (false, MessageConstants.ErrorOccured);
	}

	public async Task<LoggedInUserDto?> CheckVerificationCodeAsync(UserVerificationCodePostDto userDto)
	{
		var user = await _userManagementUnitOfWork.Users.GetIncludeCredentialsAndLanguageAndStructuresAsync(x => x.Id == userDto.User.Id);
		if (user != null && !string.IsNullOrEmpty(user?.Mobile))
		{
			// Retrieve OTP hash from Redis
			var db = _redis.GetDatabase();
			var storedHash = await db.StringGetAsync($"otp_hash:{user.Id}");
			if (!storedHash.HasValue) return null;

			if (!string.IsNullOrEmpty(user.Mobile) && user.Mobile.TryCleanMobileSA(out string cleanedNumber))
			{
				// NCA Compliance: Use SHA-384 for OTP verification hash (NCS-1:2020 Section 4.1)
				if (storedHash == StringManipulation.SHA_384(userDto.Last4MobileDigits, cleanedNumber, DateTime.Now.ToString("yyyyMMdd")))
				{
					await db.KeyDeleteAsync($"otp_hash:{user.Id}");
					return await GetLoggedInUserAsync(user);
				}
			}
		}
		return null;
	}

	public async Task<List<NotificationListItemDto>?> ListNotificationsAsync(string userId)
	{
		List<NotificationListItemDto>? retVal = null;
		var pendingTasks = await _mmsUnitOfWork.Tasks.CountAsync(x => x.UserId == userId && (x.Claimed == null || x.Claimed == false));
		if (pendingTasks > 0)
		{
			retVal = new List<NotificationListItemDto>();
			NotificationListItemDto notificationListItemDto = new NotificationListItemDto();
			notificationListItemDto.TypeName = NotificationTypeConstants.Task;
			notificationListItemDto.Count = pendingTasks;
			retVal.Add(notificationListItemDto);
		}
		return retVal;
	}

	public async Task UpdateProfilePicture(string userId, IFormFileCollection files)
	{
		var image = files[0];

		var extension = System.IO.Path.GetExtension(image.FileName);
		var imageType = image.ContentType;
		await _storageManager.UpdateProfileImage(image.ToBytes(), userId, extension, imageType);
		var user = await _userManagementUnitOfWork.Users.Find(userId);
		user.HasProfilePicture = true;
		await _userManagementUnitOfWork.SaveChangesAsync();

	}


	public async Task<(bool approved, string message)> CheckSignaturePincode(int id, string pincode)
	{

		var userSignature = await _userManagementUnitOfWork.UserSignature.Find(id);
		if (userSignature.IsLocked)
		{
			if (userSignature.LastAttempt.GetValueOrDefault().AddMinutes(_pincodeSetting.LockMinutes.GetValueOrDefault()) > DateTime.Now)
			{
				return (false, "pincode_locked"); // //dont change message (related to translations in viewer)
			}
			else
			{
				// Reset lock
				userSignature.IsLocked = false;
				userSignature.FailedAttempts = 0;
			}
		}

		if (userSignature.Pincode.GetValueOrDefault().ToString() == pincode)
		{
			userSignature.LastSuccessfulAttempt = DateTime.Now;
			userSignature.LastAttempt = DateTime.Now;
			userSignature.FailedAttempts = 0;

			await _userManagementUnitOfWork.SaveChangesAsync();
			return (true, "");
		}
		else
		{
			userSignature.FailedAttempts++;
			userSignature.LastAttempt = DateTime.Now;

			if (userSignature.FailedAttempts >= _pincodeSetting.MaxAttempts)
			{
				userSignature.IsLocked = true;
			}

			await _userManagementUnitOfWork.SaveChangesAsync();
			return (false, "pincode_error"); //dont change message (related to translations in viewer)
		}
	}

	public async Task<LoggedInUserDto?> AuthenticateMoiUserAsync(string nationalId)
	{
		User? user = await _userManagementUnitOfWork.Users.GetIncludeCredentialsAndLanguageAndStructuresAsync((x) => x.NationalId == nationalId);
		if (user == null)
		{
			user = new User()
			{
				Id = Guid.NewGuid().ToString(),
				NationalId = nationalId,
				CreatedDate = DateTime.Now,
				Approved = true,
				DefaultLanguageId = 1,
				Username = string.Empty,
				Email = string.Empty,
				FullnameAr = string.Empty,
				FullnameEn = string.Empty,

			};
			await AddUserWithDefaultPermissions(user);

		}
		if (user != null)
		{
			return await GetLoggedInUserAsync(user);
		}

		return null;
	}

	private async Task AddUserWithDefaultPermissions(User user)
	{
		await _userManagementUnitOfWork.Users.AddAsync(user);
		await _userManagementUnitOfWork.SaveChangesAsync();

		var defaultPermissions = await _userManagementUnitOfWork.Permissions.ListAsync(x => x.IsDefault);

		var userDefaultPermissions = defaultPermissions.Select(
			permission => new PermissionMatrix()
			{
				Id = 0,
				UserId = user.Id,
				PermissionId = permission.Id,
				LevelId = (int)PermissionLevelDbEnum.Full,
				Value = true

			}).ToList();
		if (userDefaultPermissions.Count > 0)
		{
			await _userManagementUnitOfWork.PermissionMatrices.AddRangeAsync(userDefaultPermissions);
			await _userManagementUnitOfWork.SaveChangesAsync();
		}



	}

	private bool isUserDataComplete(User user)
	{
		if (user == null) return false;
		if (string.IsNullOrEmpty(user.Id)) return false;
		if (string.IsNullOrEmpty(user.FullnameEn.Trim()) || string.IsNullOrEmpty(user.FullnameAr.Trim())) return false;
		if (string.IsNullOrEmpty(user.Email.Trim()) || string.IsNullOrEmpty(user.Mobile)) return false;
		return true;
	}

	public LoginOptionsDto GetLoginOptions()
	{
		return new LoginOptionsDto()
		{
			EnableLogin = _enableLogin,
			EnableExternalLogin = false,
			ExternalTokenName = null
		};
	}

	public async Task<(byte[]? bytes, string mimeType)> GetUserProfileImage(string userId)
	{
		return await _storageManager.GetProfileImage(userId);
	}

	public async Task<bool> checkSignatureAccess(string userId, int signatureId)
	{
		return await _userManagementUnitOfWork.UserSignature.AnyAsync(x => x.UserId == userId && x.Id == signatureId);
	}

	public async Task<RefreshTokenResponseDto?> RefreshToken(RefreshTokenPostDto refreshTokenPostDto)
	{
		var refreshToken = await _userManagementUnitOfWork.RefreshTokens.GetAsync(x => x.Token == refreshTokenPostDto.RefreshToken && x.Expiration >= DateTime.Now);
		if (refreshToken != null)
		{
			var user = await _userManagementUnitOfWork.Users.GetIncludeCredentialsAndLanguageAndStructuresAsync(x => x.Id == refreshToken.UserId);
			var db = _redis.GetDatabase();
			var token = GenerateToken(user);
			await db.StringSetAsync("token_" + refreshToken.UserId, token);
			return new RefreshTokenResponseDto() { Token = token };
		}
		return null;
	}

	public async Task<bool> Logout(string userId)
	{
		var db = _redis.GetDatabase();
		await db.KeyDeleteAsync("token_" + userId);
		return await _userManagementUnitOfWork.RefreshTokens.DeleteRefreshToken(userId);
	}


	public string DerptyUiPassword(string newPassword)
	{
		return EncryptionService.DecryptFromFrontend(newPassword);
	}
}