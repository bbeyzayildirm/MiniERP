using System;
using System.Linq;
using System.Web.Security;
using MiniERPprojesi.Models;

public class MyRoleProvider : RoleProvider
{
    public override string[] GetRolesForUser(string username)
    {
        using (MiniERPDBEntities6 db = new MiniERPDBEntities6())
        {
            var kullanici = db.Kullanicilar.FirstOrDefault(k => k.KullaniciAdi == username);
            if (kullanici != null)
            {
                return new string[] { kullanici.Roller.RolAdi };
            }
            return new string[] { };
        }
    }

    // 🔽 Zorunlu soyut metotlar - boş implementasyon veriyoruz (şimdilik kullanmıyorsan)
    public override bool IsUserInRole(string username, string roleName) => false;
    public override string[] GetUsersInRole(string roleName) => new string[] { };
    public override string[] GetAllRoles() => new string[] { };
    public override string[] FindUsersInRole(string roleName, string usernameToMatch) => new string[] { };
    public override void CreateRole(string roleName) => throw new NotImplementedException();
    public override void AddUsersToRoles(string[] usernames, string[] roleNames) => throw new NotImplementedException();
    public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames) => throw new NotImplementedException();
    public override bool RoleExists(string roleName) => false;

    // 🔧 Bu metot dönüş tipi bool olmalı!
    public override bool DeleteRole(string roleName, bool throwOnPopulatedRole) => throw new NotImplementedException();

    public override string ApplicationName { get; set; }
}
