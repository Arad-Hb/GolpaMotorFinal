namespace GolpaMotorFinal.Models.ViewModels.UserManagement
{
    public class UserStatBoxesViewModel
    {
        public int TotalEarnedPoints { get; set; }
        public int TotalSettledPoints { get; set; }
        public int RemainedPoints { get; set; }
        public int TotalRegisteredCards { get; set; }

        public static UserStatBoxesViewModel From(
            int totalEarnedPoints,
            int totalSettledPoints,
            int remainedPoints,
            int totalRegisteredCards)
        {
            return new UserStatBoxesViewModel
            {
                TotalEarnedPoints = totalEarnedPoints,
                TotalSettledPoints = totalSettledPoints,
                RemainedPoints = remainedPoints,
                TotalRegisteredCards = totalRegisteredCards
            };
        }
    }
}
