# eBAUsersHelper
eBAUsersHelper is user account helper library for Bimser eBA Document Management & Workflow system. Coded by C# .NET 4.5 with Visual Studio 2013 and Entity Framework 6.

How to use:

Firstly you should initialize eBA database with eBAUsersHelperClient method. Pass eBA database login credentials to eBAUsersHelperClient method.

```cs
eBAUsersHelperClient sampleClient = new eBAUsersHelperClient("myDataSource", "eBAInstanceName", "username", "password");
```

After initializing client, you will able to do various operations with it.

eBAUser object: Stores information about an eBA user. User Id, department, position etc. All CRUD operations works with this object.

CreateUser(): Creates an eBA system user.
DisableUser(): Disables eBA user by given user id.
EnableUser(): Enables eBA user by given user id.
GetActiveUsersCount(): Gets the only active users count of eBA users.
GetAllActiveUsers(): Gets the all active users on eBA System. Returns List<eBAUser>.
GetLastError(): Gets the last occured error on eBAUsersHelperClient object.
GetUser(string): Returns eBAUser object by given user ID.
IsUserExists(string): Queries an username if is exist or not in the eBA System. Returns boolean.
UpdatePassword(string, string): Updates an user's password with given user ID and new Password.
UpdateUser(eBAUsersHelper.eBAUser): Updates an user's properties with given eBAUser object.

