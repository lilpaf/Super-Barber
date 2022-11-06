namespace SuperBarber
{
    public class CustomRoles
    {
        public const string AdministratorRoleName = "Administrator";
        public const string BarberRoleName = "Barber";
        public const string BarberShopOwnerRoleName = "Owner";
        public const string BarberShopOwnerOrAdmin = BarberShopOwnerRoleName + "," + AdministratorRoleName;
    }
}
