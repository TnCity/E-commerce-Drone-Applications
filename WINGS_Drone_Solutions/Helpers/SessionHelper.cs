namespace WINGS.Web.Helpers
{
    public static class SessionHelper
    {
        public static bool IsLoggedIn(HttpContext context)
        {
            return context.Session.GetInt32("UserId") != null;
        }

        public static bool IsAdmin(HttpContext context)
        {
            return context.Session.GetString("Role") == "Admin";
        }

        public static bool IsCustomer(HttpContext context)
        {
            return context.Session.GetString("Role") == "Customer";
        }
    }
}