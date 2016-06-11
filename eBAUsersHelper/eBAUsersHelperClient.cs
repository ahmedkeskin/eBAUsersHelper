using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBAUsersHelper
{
    public class eBAUsersHelperClient
    {
        public eBAUsersHelperClient(string dataSource, string instanceName, string userName, string password)
        {
            connectionString = CreateConnStr(dataSource, instanceName, userName, password);
        }

        private string connectionString { get; set; }

        private List<string> errorList = new List<string>();

        public static string CreateConnStr(string dataSource, string instanceName, string userName, string password)
        {
            string connectionString = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder
            {
                Metadata = "res://*/EbaModel.csdl|res://*/EbaModel.ssdl|res://*/EbaModel.msl",
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = new System.Data.SqlClient.SqlConnectionStringBuilder
                {
                    InitialCatalog = instanceName,
                    DataSource = dataSource,
                    IntegratedSecurity = false,
                    UserID = userName,
                    Password = password,
                }.ConnectionString
            }.ConnectionString;

            return connectionString;
        }

        public eBAUser GetUser(string userID)
        {
            using (var dbContext = new EBAEntities(connectionString))
            {
                dbContext.Configuration.AutoDetectChangesEnabled = false;
                List<OSPROPERTYVALUES> pValues = (from x in dbContext.OSPROPERTYVALUES where x.ID == userID select x).ToList();

                eBAUser userProperties = new eBAUser();


                OSUSERS users = (from x in dbContext.OSUSERS where x.ID == userID select x).FirstOrDefault();

                if (users != null)
                {
                    userProperties.FirstName = users.FIRSTNAME;
                    userProperties.UserId = users.ID;
                    userProperties.LastName = users.LASTNAME;
                    userProperties.FullName = users.FIRSTNAME + " " + users.LASTNAME;
                    userProperties.Password = users.PASSWORD;
                    userProperties.Email = users.EMAIL;
                    userProperties.Status = users.STATUS;
                    userProperties.Category = users.CATEGORY;
                    userProperties.Sex = users.SEX;
                    userProperties.DepartmentId = users.DEPARTMENT;



                    userProperties.Manager = (from x in dbContext.OSMANAGERS where x.USERID == userID && x.MANAGERKEY == "default" select x.MANAGERUSERID).FirstOrDefault();
                    userProperties.DepartmentName = (from x in dbContext.OSDEPARTMENTS where x.ID == users.DEPARTMENT select x.DESCRIPTION).FirstOrDefault();
                    userProperties.ProfessionId = users.PROFESSION;
                    userProperties.ProfessionName = (from x in dbContext.OSPROFESSIONS where x.ID == users.PROFESSION select x.DESCRIPTION).FirstOrDefault();
                    userProperties.PositionId = (from x in dbContext.OSPOSITIONS where x.USERID == userID select x.ID).FirstOrDefault();
                    userProperties.PositionName = (from x in dbContext.OSPOSITIONS where x.USERID == userID select x.DESCRIPTION).FirstOrDefault();
                    userProperties.CompanyId = (from x in dbContext.COMPANYOBJECTS where x.ID == userID select x.COMPANY).FirstOrDefault();

                    userProperties.DepartmentName = (from x in dbContext.OSDEPARTMENTS where x.ID == users.DEPARTMENT select x.DESCRIPTION).FirstOrDefault();


                    List<OSGROUPCONTENT> groupContent = (from x in dbContext.OSGROUPCONTENT where x.USERID == userProperties.UserId select x).ToList();

                    foreach (var groupCont in groupContent)
                    {
                        eBAUser.GroupInfo gInfo = new eBAUser.GroupInfo();

                        OSGROUPS grp = (from x in dbContext.OSGROUPS where x.ID == groupCont.GROUPID select x).FirstOrDefault();

                        gInfo.GroupId = groupCont.GROUPID;
                        gInfo.GroupDescription = grp.DESCRIPTION;

                        userProperties.Groups.Add(gInfo);

                    }

                }

                return userProperties;
            }

        }

        public void CreateUser(eBAUser userProperties)
        {
            using (var dbContext = new EBAEntities(connectionString))
            {

                OSUSERS user = new OSUSERS() { ID = userProperties.UserId, FIRSTNAME = userProperties.FirstName, LASTNAME = userProperties.LastName, PASSWORD = userProperties.Password, EMAIL = userProperties.Email, STATUS = userProperties.Status, TYPE = 0, IMPORTSTATUS = 0, CATEGORY = 0, SEX = userProperties.Sex, DEPARTMENT = userProperties.DepartmentId, PROFESSION = userProperties.ProfessionId };


                OSPOSITIONS position = (from x in dbContext.OSPOSITIONS where x.ID == userProperties.PositionId select x).FirstOrDefault();

                if (position.ID != null)
                    position.USERID = userProperties.UserId;
                else
                {
                    OSPOSITIONS addPosition = new OSPOSITIONS { ID = userProperties.PositionId, DESCRIPTION = userProperties.PositionName, IMPORTEDPOSCODE = "", IMPORTSTATUS = 0, STATUS = 1, TYPE = 0, USERID = userProperties.UserId };
                    dbContext.Entry(addPosition).State = System.Data.Entity.EntityState.Added;
                }

                List<eBAUser.GroupInfo> groups = userProperties.Groups;

                if (groups.Count > 0)
                {
                    foreach (var grp in groups)
                    {
                        string tableGroup = (from c in dbContext.OSGROUPS where c.ID == grp.GroupId select c.ID).FirstOrDefault();
                        if (tableGroup != null)
                        {
                            OSGROUPCONTENT addGroup = new OSGROUPCONTENT() { GROUPID = grp.GroupId, USERID = userProperties.UserId };
                            try
                            {
                                dbContext.Entry(addGroup).State = System.Data.Entity.EntityState.Added;
                            }
                            catch (Exception ex)
                            {
                                errorList.Add(ex.Message);
                            }
                        }
                        else
                        {
                            errorList.Add("Veritabanında " + grp.GroupId + " isminde bir grup tanımlaması yok!");
                        }
                    }
                }

                COMPANYOBJECTS company = new COMPANYOBJECTS() { ID = userProperties.UserId, COMPANY = userProperties.CompanyId, TYPE = 1, IMPORTSTATUS = 0 };

                OSMANAGERS manager = new OSMANAGERS() { USERID = userProperties.UserId, MANAGERKEY = "default", MANAGERUSERID = userProperties.Manager };

                dbContext.Entry(user).State = System.Data.Entity.EntityState.Added;
                dbContext.Entry(company).State = System.Data.Entity.EntityState.Added;
                dbContext.Entry(manager).State = System.Data.Entity.EntityState.Added;



                string checkUser = (from x in dbContext.OSUSERS where x.ID == userProperties.UserId select x.ID).FirstOrDefault();

                if (checkUser != null)
                {
                    errorList.Add(userProperties.UserId + " kullanıcısı zaten mevcut!");
                }               
                else
                {
                    try
                    {
                        dbContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        errorList.Add(ex.Message);
                    }
                }
            }
        }

        public void UpdateUser(eBAUser userProperties)
        {
            using (var dbContext = new EBAEntities(connectionString))
            {
                OSUSERS user = (from x in dbContext.OSUSERS where x.ID == userProperties.UserId select x).FirstOrDefault();

                if (user != null)
                {
                    user.ID = userProperties.UserId;
                    user.FIRSTNAME = userProperties.FirstName;
                    user.LASTNAME = userProperties.LastName;
                    user.PASSWORD = userProperties.Password;
                    user.EMAIL = userProperties.Email;
                    user.STATUS = userProperties.Status;
                    user.TYPE = 0;
                    user.IMPORTSTATUS = 0;
                    user.CATEGORY = 0;
                    user.SEX = userProperties.Sex;
                    user.DEPARTMENT = userProperties.DepartmentId;
                    user.PROFESSION = userProperties.ProfessionId;
                }
                else
                {
                    errorList.Add(userProperties.UserId + " kullanıcısı bulunamadı!");
                }


                OSMANAGERS manager = (from x in dbContext.OSMANAGERS where x.USERID == userProperties.UserId select x).FirstOrDefault();
                if (manager != null)
                {
                    manager.MANAGERKEY = "default";
                    manager.MANAGERUSERID = userProperties.Manager;
                }
                else
                {
                    OSMANAGERS addManager = new OSMANAGERS { USERID = userProperties.UserId, MANAGERKEY = "default", MANAGERUSERID = userProperties.Manager };
                    dbContext.Entry(addManager).State = System.Data.Entity.EntityState.Added;
                }

                List<OSGROUPCONTENT> currGroups = (from x in dbContext.OSGROUPCONTENT where x.USERID == userProperties.UserId select x).ToList();
                foreach (var grp in currGroups)
                {
                    dbContext.Entry(grp).State = System.Data.Entity.EntityState.Deleted;
                    dbContext.SaveChanges();
                }

                foreach (var newGroup in userProperties.Groups)
                {
                    var groupCheck = (from x in dbContext.OSGROUPS where x.ID == newGroup.GroupId select x).FirstOrDefault();

                    if (groupCheck != null)
                    {
                        OSGROUPCONTENT cnt = new OSGROUPCONTENT() { GROUPID = newGroup.GroupId, USERID = userProperties.UserId };
                        dbContext.Entry(cnt).State = System.Data.Entity.EntityState.Added;
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        errorList.Add("Veritabanında " + newGroup.GroupId + " ID'si ile bir grup bulunmadığından kullanıcıya atama gerçekleştirilemedi.");
                    }

                }

                try
                {
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    errorList.Add(ex.Message);
                }
            }
        }

        public void UpdatePassword(string userId, string newPassword)
        {
            var dbContext = new EBAEntities(connectionString);

            OSUSERS user = (from x in dbContext.OSUSERS where x.ID == userId select x).FirstOrDefault();

            if (user != null)
            {
                user.PASSWORD = newPassword;

                try
                {
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    errorList.Add(ex.Message);
                }
            }
            else
            {
                errorList.Add(userId + " kullanıcısı bulunamadı!");
            }
        }

        public string GetLastError()
        {

            if (errorList.Count != 0)
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i <= errorList.Count() - 1; i++)
                {
                    sb.Append(errorList[i] + "\n");
                }

                return sb.ToString();
            }
            else
            {
                return null;
            }

        }

        public void EnableUser(string userId)
        {
            var dbContext = new EBAEntities(connectionString);

            OSUSERS user = (from x in dbContext.OSUSERS where x.ID == userId select x).FirstOrDefault();

            if (user.ID != null)
            {
                user.STATUS = 1;

                try
                {
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    errorList.Add(ex.Message + " " + ex.InnerException.Message + ex.InnerException.InnerException.Message);
                }
            }
            else
            {
                errorList.Add(userId + " kullanıcısı bulunamadı!");
            }
        }

        public void DisableUser(string userId)
        {
            var dbContext = new EBAEntities(connectionString);

            OSUSERS user = (from x in dbContext.OSUSERS where x.ID == userId select x).FirstOrDefault();

            if (user.ID != null)
            {
                user.STATUS = 0;

                try
                {
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    errorList.Add(ex.Message + " " + ex.InnerException.Message + ex.InnerException.InnerException.Message);
                }
            }
            else
            {
                errorList.Add(userId + " kullanıcısı bulunamadı!");
            }
        }

        public bool IsUserExists(string UserId)
        {
            var dbContext = new EBAEntities(connectionString);

            string userid = (from x in dbContext.OSUSERS where x.ID == UserId select x.ID).FirstOrDefault();

            if (userid != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<eBAUser> GetAllActiveUsers()
        {
            using (var dbContext = new EBAEntities(connectionString))
            {
                dbContext.Configuration.AutoDetectChangesEnabled = false;

                List<OSUSERS> activeUserList = (from x in dbContext.OSUSERS where x.TYPE == 0 && x.STATUS == 1 select x).ToList();

                List<eBAUser> userList = new List<eBAUser>();

                foreach (var user in activeUserList)
                {
                    eBAUser usr = new eBAUser();
                    usr.UserId = user.ID;
                    usr.FirstName = user.FIRSTNAME;
                    usr.LastName = user.LASTNAME;
                    usr.Email = user.EMAIL;
                    usr.DepartmentName = user.DEPARTMENT;
                    usr.ProfessionName = user.PROFESSION;
                    userList.Add(usr);
                }

                return userList;
            }
        }

        public int GetActiveUsersCount()
        {
            using(var dbContext = new EBAEntities(connectionString))
            {
                int userCount = dbContext.OSUSERS.Where(x => x.TYPE == 0 && x.STATUS == 1).Count();

                return userCount;
            }
        }
    }
}
