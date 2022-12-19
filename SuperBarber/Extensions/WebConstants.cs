namespace SuperBarber.Extensions
{
    public static class WebConstants
    {
        public const string GlobalMessageKey = "GlobalMessage";

        public static class CartControllerConstants
        {
            public const string InvalidServiceOrBarbershop = "Invalid service or barbershop.";
            
            public const string InvalidService = "Invalid service.";
            
            public const string ItemAddedSuccsessMessege = "Item was added to the cart.";
            
            public const string ItemRemovedSuccsessMessege = "Item was removed from the cart.";
            
            public const string OrderBookedSuccsessMessege = "Your order was booked! You can find all the orders you have made in the menu 'My Orders'.";
        }

        public static class BarberControllerConstants
        {
            public const string AddBarberSuccsessMessege = "You are barber now!";
            
            //{0} - barbershop name
            public const string AssignBarberSuccsessMessege = "You started working at {0}!";

            //{0} - barbershop name; {1} - barber name
            public const string UnassignBarberSuccsessMessege = "{1} stoped working at {0}!";

            //{0} - barbershop name; {1} - barber name
            public const string MakeOwnerSuccsessMessege = "{1} is owner at {0}!";

            //{0} - barbershop name; {1} - barber name
            public const string RemoveOwnerSuccsessMessege = "{1} is no longer owner at {0}!";

            //{0} - barbershop name; {1} - barber name
            public const string MakeUnavailableSuccsessMessege = "{1} is unavailable at {0}!";
            
            //{0} - barbershop name; {1} - barber name
            public const string MakeAvailableSuccsessMessege = "{1} is available at {0}!";
        }

        public static class BarberShopControllerConstants
        {
            public const string AddSuccsessMessege = "Your barbershop was added and is waiting for approval!";

            //{0} - barbershop name
            public const string EditSuccsessMessege = "{0} was edited and is waiting for approval!";

            //{0} - barbershop name
            public const string DeleteSuccsessMessege = "{0} was deleted!";
        }

        public static class OrderControllerConstants
        {
            public const string RemoveSuccsessMessege = "Order was canceled!";
        }
        
        public static class ServiceControllerConstants
        {
            //{0} - barbershop name
            public const string AddSuccsessMessege = "Service was added to {0}!";

            //{0} - barbershop name
            public const string RemoveSuccsessMessege = "Service was removed from {0}!";
        }
    }
}
