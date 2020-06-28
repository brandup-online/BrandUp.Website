namespace BrandUp.Website.ClientModels
{
    public class StartupModel
    {
        public EnvironmentModel Env { get; set; }
        public ApplicationModel Model { get; set; }
        public NavigationModel Nav { get; set; }
        public AntiforgeryModel Antiforgery { get; set; }
    }
}