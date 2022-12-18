using SuperBarber.Infrastructure.Data.Models;

namespace SuperBarber.Core.Extensions
{
    public static class ExeptionErrors
    {
        public const string BarberNonExistent = "This barber does not exist.";

        public const string BarberShopNonExistent = "This barbershop does not exist.";

        //{0} - barbershop name
        public const string UserIsNotOwnerOfBarberShop = "You are not owner of {0}.";

        public const string InvalidDateInput = "Date input is incorect.";

        public const string InvalidHourInput = "Hour input is incorect.";

        public const string InvalidCity = "Invalid city.";

        public const string InvalidDistrict = "Invalid district.";

        public static class BarberServiceErrors
        {
            //{0} - barbershop name
            public const string UserIsTheOnlyOwnerOfBarberShop = "You are the only owner of {0}. If you want to unassign yoursef as barber from {0} you have to transfer the ownership to someone else from the barbershop manegment or delete the barbershop from the button 'Delete'.";

            public const string UserIsBarber = "User is already a barber.";

            public const string UserIsAsignedToBarberShop = "User is already asigned to this barbershop.";

            //{0} - barbershop name
            public const string UserIsAlreadyOwnerOfBarberShop = "This barber is already owner of {0}.";

            public const string BarberShopHasToHaveAtLeastOneOwner = "Every barbershop has to have at least one owner.";

            //{0} - barbershop name
            public const string UserIsOwnerOfTheBarberShop = "Use the resign function next to your name in the {0} manegment menu.";

            //{0} - barbershop name
            public const string BarberIsNotOwnerOfBarberShop = "This barber is not an owner of {0}.";
        }

        public static class BarberShopServiceErrors
        {
            //{0} - barbershop name
            public const string UserIsTheOnlyOwnerOfBarberShop = "You are not the only owner of {0}. If you want to unasign only yourself as owner and barber from {0} press the 'resign' button in the barbershop manage menu. If you want to delete {0} you have to be the only owner.";

            public const string BarberShopIsNonExistentOrUserIsNotOwner = "This barbershop does not exist or you are not the owner of it.";

            public const string ErrorWhenSaveingImage = "Something went wrong with saveing the image.";

            public const string ErrorWhenDeleteingImage = "Something went wrong with deleteing the old image.";

            public const string StartHourInputIsIncorrect = "Start hour input is incorrect.";

            public const string FinishHourInputIsIncorrect = "Finish hour input is incorrect.";

            public const string StartHourMustBeSmallerThanFinishHour = "Opening hour must be smaller that the closing hour. And can not be the same as the closing hour.";

            public const string BarberShopAlreadyExist = "This barbershop already exist.";

            public const string UserNonExistent = "User does not exist.";

            public const string UserMustBeABarber = "You need to be a barber in order to do this action.";
        }

        public static class CartServiceErrors
        {
            public const string BarberShopNotContaingService = "This barbershop does not contain this service.";

            //{0} - service name; {1} - barbershop name
            public const string BookHourIsOutOfWorkingTime =
                "Your desired book hour for {0} at {1} is out of the working time of {1}. If you had any previous orders in your cart and they are gone they are already proccesed you can find all the orders that you have made in the menu 'My orders'. Remove {0} at {1} from the cart to continue.";

            //{0} - service name; {1} - barbershop name
            public const string BarberShopDoesNotContainService =
                "{1} does not contain service {0}. If you had any previous orders in your cart and they are gone they are already proccesed you can find all the orders that you have made in the menu 'My orders'.";

            //{0} - minimumBookDate.ToLocalTime().Hour; {1} - minimumBookDate.ToLocalTime().Minute
            public const string BookHourMustBeBiggerOrEqualToTheMinimumHour =
                "Your desired book hour must be at least at {0}:{1}. If you had any previous orders in your cart and they are gone they are already proccesed you can find all the orders that you have made in the menu 'My orders'.";

            //{0} - service name; {1} - barbershop name
            public const string NoneAvalibleBarbers =
                "There are no avalible barbers at {1} for the desired time right now. If you had any previous orders in your cart and they are gone they are already proccesed you can find all the orders that you have made in the menu 'My orders'. Remove {0} at {1} from the cart to continue.";
        }

        public static class HomeServiceErrors
        {
            public const string NoneAvalibleBarberShops = "Right now we do not have any avalible barbershops matching your criteria.";
        }

        public static class OrderServiceErrors
        {
            public const string OrderNonExistent = "Invalid order.";

            public const string UserIsNotTheAssignedBarber = "You are not the assigned barber.";

            public const string CancelOrderCanNoLongerBeDone = "You can no longer cancel this order.";

            public const string UserDidNotMakeTheOrder = "You did not make this order.";
        }

        public static class ServiceServiceErrors
        {
            public const string CategoryNonExistent = "Category does not exist.";

            //{0} - barbershop name
            public const string ServiceAlreadyExistInBarberShop = "This service already exists in {0}.";

            //{0} - barbershop name
            public const string ServiceNonExistentInBarberShop = "This service does not exist in {0}.";
        }
    }
}
