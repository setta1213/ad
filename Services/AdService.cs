using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using StudentAdWindowsApp.Models;
using StudentAdWindowsApp.Api.Models;
using System.Text.RegularExpressions;



namespace StudentAdWindowsApp.Services
{
    public class AdService
    {
        public void TestConnection(AdConfig config)
        {
            // -----------------------------
            // 1️⃣ ตรวจว่ากรอกข้อมูลครบ
            // -----------------------------
            if (string.IsNullOrWhiteSpace(config.Domain) ||
                string.IsNullOrWhiteSpace(config.OuPath) ||
                string.IsNullOrWhiteSpace(config.AdminUser) ||
                string.IsNullOrWhiteSpace(config.AdminPassword))
            {
                throw new Exception("ต้องกรอก Domain, OU, Username และ Password ให้ครบ");
            }

            // -----------------------------
            // 2️⃣ Validate Username / Password จริง
            // -----------------------------
            using (var domainContext = new PrincipalContext(
                ContextType.Domain,
                config.Domain))
            {
                bool isValid = domainContext.ValidateCredentials(
                    config.AdminUser,
                    config.AdminPassword,
                    ContextOptions.Negotiate
                );

                if (!isValid)
                {
                    throw new Exception("Username หรือ Password ไม่ถูกต้อง");
                }
            }

            // -----------------------------
            // 3️⃣ ตรวจว่า OU มีอยู่จริง
            // -----------------------------
            try
            {
                using var entry = new DirectoryEntry(
                    $"LDAP://{config.OuPath}",
                    config.AdminUser,
                    config.AdminPassword
                );

                // บังคับ bind จริง
                var native = entry.NativeObject;
            }
            catch
            {
                throw new Exception("ไม่พบ OU นี้ใน Active Directory หรือไม่มีสิทธิ์เข้าถึง");
            }

            // -----------------------------
            // 4️⃣ (Optional) ทดสอบ query user ใน OU
            // -----------------------------
            using (var pc = new PrincipalContext(
                ContextType.Domain,
                config.Domain,
                config.OuPath,
                config.AdminUser,
                config.AdminPassword
            ))
            {
                // query เบา ๆ 1 ครั้ง
                var user = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, config.AdminUser);
                // ไม่จำเป็นต้องพบ user แค่ไม่ throw = ผ่าน
            }
        }
        public void CreateStudent(AdConfig config, string studentId, string password, string firstName, string lastName)
        {
            using var pc = new PrincipalContext(
                ContextType.Domain,
                config.Domain,
                config.OuPath,
                config.AdminUser,
                config.AdminPassword
            );

            var exist = UserPrincipal.FindByIdentity(pc, studentId);
            if (exist != null)
                throw new Exception("มี Student ID นี้อยู่แล้ว");

            using var user = new UserPrincipal(pc);
            user.SamAccountName = studentId;
            user.UserPrincipalName = $"{studentId}@{config.Domain}";
            user.GivenName = firstName;
            user.Surname = lastName;
            user.DisplayName = $"{firstName} {lastName}";
            user.Enabled = true;

            user.SetPassword(password);
            user.Save();
        }
            public void ResetPasswordAndForceChange(
            AdConfig config,
            string username
        )
        {
            using var context = new PrincipalContext(
                ContextType.Domain,
                config.Domain,
                config.OuPath,
                config.AdminUser,
                config.AdminPassword
            );

            var user = UserPrincipal.FindByIdentity(context, username);
            if (user == null)
                throw new Exception("ไม่พบผู้ใช้งานใน Active Directory");

            // 1️⃣ ตั้งรหัสผ่านใหม่
            user.SetPassword("12345678");

            // 2️⃣ บังคับเปลี่ยนรหัสผ่านเมื่อ login
            user.ExpirePasswordNow();

            user.Save();
        }
        public void DeleteStudent(
            AdConfig config,
            string studentId
        )
                {
            using var context = new PrincipalContext(
                ContextType.Domain,
                config.Domain,
                config.OuPath,
                config.AdminUser,
                config.AdminPassword
            );

            var user = UserPrincipal.FindByIdentity(context, studentId);
            if (user == null)
                throw new Exception("ไม่พบ Student ID นี้ใน Active Directory");

            user.Delete(); // 🗑️ ลบ user
        }
        public StudentInfoDto GetStudentInfo(AdConfig config, string studentId)
        {
            using var context = new PrincipalContext(
                ContextType.Domain,
                config.Domain,
                config.OuPath,
                config.AdminUser,
                config.AdminPassword
            );

            var user = UserPrincipal.FindByIdentity(context, studentId);
            if (user == null)
                throw new Exception("ไม่พบผู้ใช้งานใน Active Directory");

            var entry = (DirectoryEntry)user.GetUnderlyingObject();

            return new StudentInfoDto
            {
                StudentId = user.SamAccountName ?? "",
                FirstName = user.GivenName ?? "",
                LastName = user.Surname ?? "",
                DisplayName = user.DisplayName ?? "",
                
                Email = entry.Properties["mail"]?.Value?.ToString() ?? "",
                Phone = entry.Properties["telephoneNumber"]?.Value?.ToString() ?? "",
                Office = entry.Properties["physicalDeliveryOfficeName"]?.Value?.ToString() ?? "",

                // ⭐ Profile tab
                ProfilePath = entry.Properties["profilePath"]?.Value?.ToString() ?? "",
                LogonScript = entry.Properties["scriptPath"]?.Value?.ToString() ?? "",
                HomeDirectory = entry.Properties["homeDirectory"]?.Value?.ToString() ?? "",
                HomeDrive = entry.Properties["homeDrive"]?.Value?.ToString() ?? "",

                Enabled = user.Enabled ?? false
            };
        }


        public void UpdateStudent(
            AdConfig config,
            UpdateStudentDto dto
        )
        {
            using var context = new PrincipalContext(
                ContextType.Domain,
                config.Domain,
                config.OuPath,
                config.AdminUser,
                config.AdminPassword
            );

            var user = UserPrincipal.FindByIdentity(context, dto.StudentId);
            if (user == null)
                throw new Exception("ไม่พบผู้ใช้งานใน Active Directory");

            // -------------------------
            // UserPrincipal fields
            // -------------------------
            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                user.GivenName = dto.FirstName;

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                user.Surname = dto.LastName;

            if (!string.IsNullOrWhiteSpace(dto.DisplayName))
                user.DisplayName = dto.DisplayName;

            user.Enabled = dto.Enabled;

            // -------------------------
            // DirectoryEntry fields
            // -------------------------
            var entry = (DirectoryEntry)user.GetUnderlyingObject();

            void SetIfNotEmpty(string attr, string? value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    entry.Properties[attr].Value = value;
            }

            SetIfNotEmpty("mail", dto.Email);
            SetIfNotEmpty("telephoneNumber", dto.Phone);
            SetIfNotEmpty("physicalDeliveryOfficeName", dto.Office);
            SetIfNotEmpty("profilePath", dto.ProfilePath);
            SetIfNotEmpty("scriptPath", dto.LogonScript);
            SetIfNotEmpty("homeDirectory", dto.HomeDirectory);

            // homeDrive ต้อง strict มาก
            if (!string.IsNullOrWhiteSpace(dto.HomeDrive))
            {
                if (!Regex.IsMatch(dto.HomeDrive, "^[A-Z]:$"))
                    throw new Exception("HomeDrive ต้องเป็นรูปแบบ H:");

                entry.Properties["homeDrive"].Value = dto.HomeDrive;
            }

            entry.CommitChanges();
            user.Save();
        }
        //---------------------------------------------------------------------------------
    }




}


