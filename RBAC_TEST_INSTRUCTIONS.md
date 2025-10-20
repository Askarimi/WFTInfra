# 🧪 RBAC Query Test - Step 7

## ✅ Test Endpoint Created

A new test endpoint has been added to verify that the RBAC junction tables and relationships are working correctly.

### 📍 Endpoint Details

**URL:** `GET /api/Users/test-rbac/{userId}`

**Purpose:** Tests the RBAC query from Step 7 of the guide to verify that:
- Junction tables (UserRoles, RolePermissions) are properly configured
- Navigation properties work correctly
- EF Core can traverse the relationships
- Permissions are correctly retrieved for a user

---

## 🚀 How to Test

### Option 1: Using Browser or Postman

1. **Start the application:**
   ```bash
   dotnet run --project src/WFT.Infra.WebApi
   ```

2. **Call the endpoint:**
   ```
   GET https://localhost:<port>/api/Users/test-rbac/1
   ```
   Replace `1` with any valid user ID in your database.

### Option 2: Using curl

```bash
curl -X GET "https://localhost:5001/api/Users/test-rbac/1" -k
```

### Option 3: Using .http file

If you're using Visual Studio or Rider, add this to your `.http` file:

```http
### Test RBAC Relationships
GET {{baseUrl}}/api/Users/test-rbac/1
```

---

## 📊 Expected Response

```json
{
  "userId": 1,
  "username": "admin",
  "email": "admin@example.com",
  "roles": [
    "Administrator",
    "Manager"
  ],
  "permissions": [
    "CreateUser",
    "EditUser",
    "DeleteUser",
    "ViewUser",
    "ViewUserList"
  ],
  "permissionsCount": 5,
  "permissionsJoined": "CreateUser, EditUser, DeleteUser, ViewUser, ViewUserList",
  "message": "RBAC relationships are working correctly! ✅"
}
```

---

## 🔍 What This Tests

The endpoint executes this exact query from Step 7:

```csharp
var permissions = await _context.Users
    .Where(u => u.Id == userId)
    .SelectMany(u => u.UserRoles)          // ← Junction table 1
    .SelectMany(ur => ur.Role.RolePermissions)  // ← Junction table 2
    .Select(rp => rp.Permission.Name)
    .ToListAsync();
```

### SQL Generated (approximate):

```sql
SELECT [Permission].[Name]
FROM [Users] AS [u]
INNER JOIN [UserRoles] AS [ur] ON [u].[Id] = [ur].[UserId]
INNER JOIN [RolePermissions] AS [rp] ON [ur].[RoleId] = [rp].[RoleId]
INNER JOIN [Permissions] AS [p] ON [rp].[PermissionId] = [p].[Id]
WHERE [u].[Id] = @userId
```

---

## ✅ Success Indicators

If the RBAC relationships are working correctly, you should see:

1. ✅ **Status 200 OK** - Endpoint returns successfully
2. ✅ **User information** - Username and email are populated
3. ✅ **Roles array** - Contains the user's assigned roles
4. ✅ **Permissions array** - Contains all permissions from all the user's roles
5. ✅ **No errors** - No navigation property or foreign key errors

---

## ❌ Common Issues

### User Not Found (404)
```json
{
  "message": "User with ID 1 not found."
}
```
**Solution:** Use a valid user ID from your database.

### Empty Permissions Array
```json
{
  "permissions": [],
  "permissionsCount": 0
}
```
**Causes:**
- User has no roles assigned (UserRoles table is empty)
- Roles have no permissions assigned (RolePermissions table is empty)
- User/Role/Permission is marked as inactive

**Solution:** Check your seed data and ensure:
1. User has roles in `UserRoles` table
2. Roles have permissions in `RolePermissions` table
3. All entities have `IsActive = true`

---

## 🗃️ Database Verification Queries

If you want to verify the data directly in the database:

### Check User's Roles:
```sql
SELECT u.Username, r.Name AS RoleName
FROM Users u
INNER JOIN UserRoles ur ON u.Id = ur.UserId
INNER JOIN Roles r ON ur.RoleId = r.Id
WHERE u.Id = 1;
```

### Check Role's Permissions:
```sql
SELECT r.Name AS RoleName, p.Name AS PermissionName, p.DisplayName
FROM Roles r
INNER JOIN RolePermissions rp ON r.Id = rp.RoleId
INNER JOIN Permissions p ON rp.PermissionId = p.Id
WHERE r.IsActive = 1 AND p.IsActive = 1;
```

### Check User's Full Permission Chain:
```sql
SELECT 
    u.Username,
    r.Name AS RoleName,
    p.Name AS PermissionName,
    p.DisplayName
FROM Users u
INNER JOIN UserRoles ur ON u.Id = ur.UserId
INNER JOIN Roles r ON ur.RoleId = r.Id
INNER JOIN RolePermissions rp ON r.Id = rp.RoleId
INNER JOIN Permissions p ON rp.PermissionId = p.Id
WHERE u.Id = 1 
  AND u.IsActive = 1 
  AND r.IsActive = 1 
  AND p.IsActive = 1;
```

---

## 🎯 Next Steps After Successful Test

Once you verify the RBAC relationships work correctly:

1. ✅ Junction tables are properly configured
2. ✅ Navigation properties work
3. ✅ EF Core can query across relationships
4. ✅ Remove or comment out the test endpoint (optional for production)

**The test endpoint can remain for development/debugging purposes.**

---

## 🔧 Cleanup (Optional)

If you want to remove the test endpoint after verification, simply delete or comment out the `TestRbacQuery` method in `UsersController.cs` (lines 141-178).

---

## 📝 Notes

- This endpoint does **not** require authentication for testing purposes
- Add `[Authorize]` attribute if you want to secure it
- The endpoint is read-only and doesn't modify any data
- It's safe to leave in development environments



