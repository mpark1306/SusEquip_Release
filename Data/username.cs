using System.Security.Principal;


namespace SusEquip.Data
{
    public class Username
    {
        public string formattedUserName { get; set; } = string.Empty;

        public void FormatUsername()
        {
            WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
            string userName = currentUser.Name;
            formattedUserName = userName.Substring(4);
        }

    }

} //Commented out due to not being able to run on non-windows machines - ALLOW IN RELEASE
