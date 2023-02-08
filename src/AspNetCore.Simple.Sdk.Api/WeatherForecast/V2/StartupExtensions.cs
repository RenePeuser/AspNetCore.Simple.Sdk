using AspNetCore.Simple.Sdk.AutoDependencyRegistration;

namespace AspNetCore.Simple.Sdk.Api.WeatherForecast.V2
{
    public static class AddWeatherForecastExtension
    {
        public static void AddWeatherForecast(this AutoRegistration autoRegistration)
        {
            autoRegistration.DoAutoRegistrationFor<WeatherForecastController>();
        }
    }
}
